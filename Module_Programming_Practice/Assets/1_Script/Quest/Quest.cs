using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

using Debug = UnityEngine.Debug;

public enum QuestState
{
    Inactive, // 비활성화
    Running,
    Complete,
    Cancel,
    WaitingForComletion, // 미션은 이미 깻고 유저가 따로 완료하기를 누르기를 기다리는 상태
}

[CreateAssetMenu(menuName = "Quest/Quest", fileName = "Quest_")]
public class Quest : ScriptableObject
{
    #region event handlers
    public delegate void TaskSuccessChangeHandler(Quest _quest, Task _task, int _currentSuccess, int _prevSuccess);
    public delegate void CompleteHandler(Quest _quest);
    public delegate void CanceledHandler(Quest _quest);
    public delegate void NewTaskGroupHandler(Quest _quest, TaskGroup _currentTaskGroup, TaskGroup _prevTaskGroup);
    #endregion

    [SerializeField] Category category;
    public Category Category => category;
    [SerializeField] Sprite icon;
    public Sprite Icon => icon;

    public QuestState State { get; private set; }
    public bool IsRegistered => State != QuestState.Inactive;
    public bool IsCompletable => State == QuestState.WaitingForComletion;
    public bool IsComplete => State == QuestState.Complete;
    public bool IsCancel => State == QuestState.Cancel;


    [Header("Text")]
    [SerializeField] string codeName;
    public string CodeName => codeName;
    [SerializeField] string displayName;
    public string DisplayName => displayName;

    [SerializeField, TextArea] string description;
    public string Description => description;


    [Header("Taks")]
    [SerializeField] TaskGroup[] taskGroups;
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;

    int currentTaskGroupIndex = 0;
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];


    [Header("Reward")]
    [SerializeField] Reward[] rewards;
    public IReadOnlyList<Reward> Rewards => rewards;


    [Header("Options")]
    [SerializeField] bool useAutoComplete;

    [SerializeField] bool isSaveable;
    public virtual bool IsSaveable => isSaveable;

    [SerializeField] bool isCancelable;
    public virtual bool IsCancelable => isCancelable && cancelConditions.All(_condition => _condition.IsPass(this));


    [Header("Condition")]
    [SerializeField] QuestCondition[] acceptionConditions;
    // 모든 요소가 true이거나 배열이 비어 있으면 true인 프로퍼티
    public bool IsAcceptionable => acceptionConditions.All(_condition => _condition.IsPass(this));

    [SerializeField] QuestCondition[] cancelConditions;


    // 외부에서는 Quest의 이벤트만 구독해두면 여러 task의 성공 변화를 감지할 수 있음
    public event TaskSuccessChangeHandler OnTaskSuccessChanged;
    void Raise_OnTaskSuccessChanged(Task _task, int _currentSuccess, int _prevSuccess) 
        => OnTaskSuccessChanged?.Invoke(this, _task, _currentSuccess, _prevSuccess);

    public event CompleteHandler OnCompleted;
    public event CanceledHandler OnCanceled;
    public event NewTaskGroupHandler OnNewTaskGroup;

    // 퀘스트가 등록되면 실행되는 함수로 사실상 Setup, Init과 같은 초기화 함수처럼 사용됨
    public void OnRegister()
    {
        // Debug.Assert() : 인수가 false면 뒤에 문장을 Error문으로 출력. 디버깅 코드로 빌드하면 무시됨
        Debug.Assert(!IsRegistered, "this quest has already been registered.");

        foreach(TaskGroup _taskGroup in taskGroups)
        {
            _taskGroup.Setup(this);
            // Quest의 OnTaskSuccessChanged를 구독하면 Task의 OnSuccessChanged를 일일이 구독하지 않아도 변화를 감지할 수 있음
            foreach (Task _task in _taskGroup.Tasks)
                _task.OnSuccessChanged += Raise_OnTaskSuccessChanged;
        }

        State = QuestState.Running;
        CurrentTaskGroup.Start();
    }

    public void ReceiveReport(string _category, object _target, int _successCount)
    {
        Debug.Assert(IsRegistered, "this quest not registered.");
        Debug.Assert(!IsCancel, "this quest has been canceled");
        if (IsCompletable) return;

        CurrentTaskGroup.ReceiveReport(_category, _target, _successCount);
        
        if (CurrentTaskGroup.IsAllComplete)
        {
            if (currentTaskGroupIndex + 1 == taskGroups.Length) // 다음 퀘스트 그룹이 없다면
            {
                State = QuestState.WaitingForComletion;
                if (useAutoComplete) Complete();
            }
            else
            {
                TaskGroup _prevTaskGroup = TaskGroups[currentTaskGroupIndex++]; // 현재 TaskGroup을 가져오면서 index 증가
                _prevTaskGroup.End();
                CurrentTaskGroup.Start();
                OnNewTaskGroup?.Invoke(this, CurrentTaskGroup, _prevTaskGroup);
            }
        }
        else State = QuestState.Running; // 이미 깬 상태에서 물건 버리는 등 트롤짓을 방지하기 위해 완료하지 않으면 무조건 Running으로 세팅
    }

    public void Complete()
    {
        CheckIsRunngin();

        foreach (TaskGroup _taskGroup in taskGroups) _taskGroup.Complete();
        State = QuestState.Complete;

        foreach (Reward _reward in rewards) _reward.Give(this);
        OnCompleted?.Invoke(this);

        OnTaskSuccessChanged = null;
        OnCompleted = null;
        OnCanceled = null;
        OnNewTaskGroup = null;
    }

    public virtual void Cancel()
    {
        CheckIsRunngin();
        Debug.Assert(isCancelable, "this quest can not canceled");

        State = QuestState.Cancel;
        OnCanceled?.Invoke(this);
    }

    public bool ContainsTarget(object _target) => taskGroups.Any(x => x.ContainsTaskWithTarget(_target));
    public bool ContainsTarget(TaskTarget _target) => ContainsTarget(_target.Value);

    public Quest Clone()
    {
        Quest _clone = Instantiate(this);
        _clone.taskGroups = taskGroups.Select(x => new TaskGroup(x)).ToArray();
        return _clone;
    }

    public QuestSaveData ToSaveData()
    {
        return new QuestSaveData
        {
            codeName = CodeName,
            state = State,
            currentTaskGroupIndex = currentTaskGroupIndex,
            taskSuccessCounts = CurrentTaskGroup.Tasks.Select(x => x.CurrentSuccess).ToArray(),
        };
    }

    public void LoadFrom(QuestSaveData _saveData)
    {
        codeName = _saveData.codeName;
        State = _saveData.state;
        currentTaskGroupIndex = _saveData.currentTaskGroupIndex;

        // 이미 클리어한 TaskGroup들은 Clear 해줌
        for (int i = 0; i < _saveData.currentTaskGroupIndex; i++)
        {
            TaskGroup _group = taskGroups[i];
            _group.Start();
            _group.Complete();
        }

        // 현재 TaskGroup 시작 및 Task들 successCount 세팅
        for (int i = 0; i < _saveData.taskSuccessCounts.Length; i++)
        {
            CurrentTaskGroup.Start();
            CurrentTaskGroup.Tasks[i].CurrentSuccess = _saveData.taskSuccessCounts[i];
        }
    }

    // 사용자가 선언하는 디버그용 함수로 Unity Editor가 아닌 환경에서는 실행되지 않고 무시됨
    [Conditional("UNITY_EDITOR")]
    void CheckIsRunngin()
    {
        Debug.Assert(IsRegistered, "this quest not registered.");
        Debug.Assert(!IsCancel, "this quest has been canceled");
        Debug.Assert(!IsCancel, "this quest has already beed completed");
    }
}

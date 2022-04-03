using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

using Debug = UnityEngine.Debug;

public enum QuestState
{
    Inactive, // 비활성
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
    [SerializeField] string description;
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
    [SerializeField] bool isCancelable;
    public bool IsCancelable => isCancelable && cancelConditions.All(_condition => _condition.IsPass(this));


    [Header("Condition")]
    [SerializeField] QuestCondition[] acceptionConditions;
    public bool IsAcceptionable => acceptionConditions.All(_condition => _condition.IsPass(this));

    [SerializeField] QuestCondition[] cancelConditions;


    // code

    // 외부에서는 Quest의 이벤트만 구독해두면 여러 task의 성공 변화를 감지할 수 있음
    public event TaskSuccessChangeHandler OnTaskSuccessChanged;
    public void Raise_OnTaskSuccessChanged(Task _task, int _currentSuccess, int _prevSuccess) 
        => OnTaskSuccessChanged?.Invoke(this, _task, _currentSuccess, _prevSuccess);

    public event CompleteHandler OnCompleted;
    public event CanceledHandler OnCanceled;
    public event NewTaskGroupHandler OnNewTaskGroup;

    // Debug.Assert() 인수가 false면 뒤에 문장을 Error문으로 출력. 디버깅 코드로 빌드하면 무시됨
    public void OnRegister()
    {
        Debug.Assert(!IsRegistered, "this quest has already been registered.");

        foreach(TaskGroup _taskGroup in taskGroups)
        {
            _taskGroup.Setup(this);
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
            if (currentTaskGroupIndex == taskGroups.Length + 1) // 다음 퀘스트 그룹이 없다면
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
        else State = QuestState.Running; // 이미 깬 상태에서 물건 버리는 등 트롤짓하면 다시 Running로 돌아감
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

    public void Cancel()
    {
        CheckIsRunngin();
        Debug.Assert(isCancelable, "this quest can not canceled");

        State = QuestState.Cancel;
        OnCanceled?.Invoke(this);
    }


    // 사용자가 선언하는 디버그용 함수로 Unity Editor가 아닌 환경에서는 실행되지 않고 무시됨
    [Conditional("UNITY_EDITOR")]
    void CheckIsRunngin()
    {
        Debug.Log("22");
        Debug.Assert(IsRegistered, "this quest not registered.");
        Debug.Assert(!IsCancel, "this quest has been canceled");
        Debug.Assert(!IsCancel, "this quest has already beed completed");
    }
}

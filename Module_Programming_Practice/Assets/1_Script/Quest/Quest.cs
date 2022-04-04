using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

using Debug = UnityEngine.Debug;

public enum QuestState
{
    Inactive, // ��Ȱ��
    Running,
    Complete,
    Cancel,
    WaitingForComletion, // �̼��� �̹� ���� ������ ���� �Ϸ��ϱ⸦ �����⸦ ��ٸ��� ����
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
    public virtual bool IsCancelable => isCancelable && cancelConditions.All(_condition => _condition.IsPass(this));


    [Header("Condition")]
    [SerializeField] QuestCondition[] acceptionConditions;
    public bool IsAcceptionable => acceptionConditions.All(_condition => _condition.IsPass(this));

    [SerializeField] QuestCondition[] cancelConditions;


    // code

    // �ܺο����� Quest�� �̺�Ʈ�� �����صθ� ���� task�� ���� ��ȭ�� ������ �� ����
    public event TaskSuccessChangeHandler OnTaskSuccessChanged;
    public void Raise_OnTaskSuccessChanged(Task _task, int _currentSuccess, int _prevSuccess) 
        => OnTaskSuccessChanged?.Invoke(this, _task, _currentSuccess, _prevSuccess);

    public event CompleteHandler OnCompleted;
    public event CanceledHandler OnCanceled;
    public event NewTaskGroupHandler OnNewTaskGroup;

    // Debug.Assert() �μ��� false�� �ڿ� ������ Error������ ���. ����� �ڵ�� �����ϸ� ���õ�
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
            if (currentTaskGroupIndex == taskGroups.Length + 1) // ���� ����Ʈ �׷��� ���ٸ�
            {
                State = QuestState.WaitingForComletion;
                if (useAutoComplete) Complete();
            }
            else
            {
                TaskGroup _prevTaskGroup = TaskGroups[currentTaskGroupIndex++]; // ���� TaskGroup�� �������鼭 index ����
                _prevTaskGroup.End();
                CurrentTaskGroup.Start();
                OnNewTaskGroup?.Invoke(this, CurrentTaskGroup, _prevTaskGroup);
            }
        }
        else State = QuestState.Running; // �̹� �� ���¿��� ���� ������ �� Ʈ�����ϸ� �ٽ� Running�� ���ư�
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


    // ����ڰ� �����ϴ� ����׿� �Լ��� Unity Editor�� �ƴ� ȯ�濡���� ������� �ʰ� ���õ�
    [Conditional("UNITY_EDITOR")]
    void CheckIsRunngin()
    {
        Debug.Log("22");
        Debug.Assert(IsRegistered, "this quest not registered.");
        Debug.Assert(!IsCancel, "this quest has been canceled");
        Debug.Assert(!IsCancel, "this quest has already beed completed");
    }
}

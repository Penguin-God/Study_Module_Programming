using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TaskState
{
    Inactive,
    Running,
    Complete,
}

[CreateAssetMenu(menuName = "Quest/Task/Task", fileName = "new task")]
public class Task : ScriptableObject
{
    #region Event Handler
    public delegate void StateChangedHandler(Task _task, TaskState _currentTaskState, TaskState _prevTaskState);
    public delegate void SuccessChangedHandler(Task _task, int _currentSuccess, int _prevSuccess);
    #endregion


    [SerializeField] Category category;
    public Category Category => category;

    [Header("Text")]
    [SerializeField] string codeName;
    public string CodeName => codeName;
    [SerializeField] string description;
    public string Description => description;

    [Header("Action")]
    [SerializeField] TaskAction taskAction = null;

    [Header("Target")]
    [SerializeField] TaskTarget[] targets = null;

    [Header("Setting")]
    [SerializeField] InitialSuccessValue initialSuccessValue = null;
    [SerializeField] int needSuccessToComplete;
    public int NeedSuccessToComplete => needSuccessToComplete;

    public event SuccessChangedHandler OnSuccessChanged = null;
    private int currentSuccess;
    public int CurrentSuccess
    {
        get => currentSuccess;
        set
        {
            int _prevSuccess = currentSuccess;
            currentSuccess = Mathf.Clamp(value, 0 ,needSuccessToComplete);
            if(_prevSuccess != currentSuccess)
            {
                state = (currentSuccess == needSuccessToComplete) ? TaskState.Complete : TaskState.Running;
                OnSuccessChanged?.Invoke(this, currentSuccess, _prevSuccess);
            }
        }
    }

    public event StateChangedHandler OnStateChanged = null;
    TaskState state;
    public TaskState State
    {
        get => state;
        set
        {
            TaskState _prevState = state;
            state = value;
            OnStateChanged?.Invoke(this, state, _prevState);
        }
    }

    // �̼� �ϷῩ�� ��� ���� ���� ���ΰ�? (�̼� ������ �� ������ ���� ����ó����)
    [SerializeField] bool canReceiveReportsDuringCompletion;

    public bool IsComplete => state == TaskState.Complete;

    public Quest Owner { get; private set; }
    public void Setup(Quest _owner) => Owner = _owner;

    public void Start()
    {
        state = TaskState.Running;
        if (initialSuccessValue != null) CurrentSuccess = initialSuccessValue.GetValue(this);
    }

    public void End()
    {
        OnSuccessChanged = null;
        OnStateChanged = null;
    }

    public void ReceiveReport(int _successCount) => CurrentSuccess = taskAction.Run(this, CurrentSuccess, _successCount);

    public void Complete() => CurrentSuccess = needSuccessToComplete;

    // targets �ȿ� _target�� ���� ���� �ִ��� ��ȯ�ϴ� �Լ�
    public bool IsTarget(string _category, object _target) => 
        Category == _category && 
        targets.Any(x => x.IsTarget(_target)) && 
        (!IsComplete || IsComplete && canReceiveReportsDuringCompletion);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    int currentTaskGroupIndex;
    public TaskGroup currentTaskGroup => taskGroups[currentTaskGroupIndex];


    [Header("Options")]
    [SerializeField] bool useAutoComplete;


    public event TaskSuccessChangeHandler OnTaskSuccessChanged;
    public event CompleteHandler OnCompleted;
    public event CanceledHandler OnCanceled;
    public event NewTaskGroupHandler OnNewTaskGroup;

    // Debug.Assert() 인수가 false면 뒤에 문장을 Error문으로 출력. 디버깅 코드로 빌드하면 무시됨
    public void OnRegister()
    {
        Debug.Assert(!IsRegistered, "this quest has already been registered.");
    }

    public void ReceiveReport(string _category, object _target, int _successCount)
    {

    }

    public void Complete()
    {

    }

    public void Cancel()
    {

    }
}

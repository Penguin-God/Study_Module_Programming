using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
struct MarkerMaterialData
{
    public Category category;
    public Material markerMaterial;
}

public class QuestTargetMarker : MonoBehaviour
{
    Dictionary<Quest, Task> targetTaskByQuest = new Dictionary<Quest, Task>();
    int currentRunningTargetTaskCount;

    private void Awake()
    {
        cameraTansform = Camera.main.transform;
        myRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        gameObject.SetActive(false);

        QuestSystem.Instance.OnQuestRegistered += TryAddTargetQuest;
        foreach (Quest _quest in QuestSystem.Instance.ActiveQuests)
            TryAddTargetQuest(_quest);
    }

    Transform cameraTansform; // 마커가 플레이어를 바라보게 하기 위해 필요
    private void Update()
    {
        LookCamera();

        void LookCamera()
        {
            Quaternion rotation = Quaternion.LookRotation((cameraTansform.position - transform.position).normalized);
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y + 180f, 0);
        }
    }

    private void OnDestroy()
    {
        QuestSystem.Instance.OnQuestRegistered -= TryAddTargetQuest;
        foreach(KeyValuePair<Quest, Task> _pair in targetTaskByQuest)
        {
            _pair.Key.OnNewTaskGroup -= UpdateTargetTask;
            _pair.Key.OnCompleted -= RemoveTargetQuest;
            _pair.Value.OnStateChanged -= UpdateRunningTargetTaskCount;
        }
    }


    [SerializeField] TaskTarget target; // 이 target을 가진 task들을 가져와 감시함
    void TryAddTargetQuest(Quest _quest)
    {
        if(target != null && _quest.ContainsTarget(target))
        {
            _quest.OnNewTaskGroup += UpdateTargetTask;
            _quest.OnCompleted += RemoveTargetQuest;

            if(_quest.CurrentTaskGroup.FindTaskWithTarget(target))
                UpdateTargetTask(_quest, _quest.CurrentTaskGroup.FindTaskWithTarget(target));
        }
    }

    void UpdateTargetTask(Quest _quest, Task targetTask)
    {
        if (targetTask != null)
        {
            if (targetTaskByQuest.ContainsKey(_quest) == false)
                targetTaskByQuest.Add(_quest, targetTask);
            targetTask.OnStateChanged += UpdateRunningTargetTaskCount;

            UpdateRunningTargetTaskCount(targetTask, targetTask.State);
        }
    }

    Renderer myRenderer;
    [SerializeField] MarkerMaterialData[] markerMaterialDatas;
    void UpdateRunningTargetTaskCount(Task _task, TaskState _currentState, TaskState _prevState = TaskState.Inactive)
    {
        if (_currentState == TaskState.Running)
        {
            myRenderer.material = markerMaterialDatas.First(x => x.category == _task.Category).markerMaterial;
            currentRunningTargetTaskCount++;
        }
        else currentRunningTargetTaskCount--;

        gameObject.SetActive(currentRunningTargetTaskCount != 0);
    }

    void RemoveTargetQuest(Quest _quest)
    {
        if (targetTaskByQuest.ContainsKey(_quest))
            targetTaskByQuest.Remove(_quest);
    }

    #region Callback function
    void UpdateTargetTask(Quest _quest, TaskGroup _currentTaskGroup, TaskGroup _prevTaskGroup = null)
        => UpdateTargetTask(_quest, _currentTaskGroup.FindTaskWithTarget(target));
    #endregion
}

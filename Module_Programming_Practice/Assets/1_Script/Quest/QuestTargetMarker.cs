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
    List<Quest> trackQuests = new List<Quest>();
    Dictionary<Quest, Task> targetTaskByQuest = new Dictionary<Quest, Task>();

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

        foreach (Quest quest in trackQuests)
        {
            quest.OnNewTaskGroup -= UpdateTargetTask;
            quest.OnCompleted -= RemoveTargetQuest;
        }
        
        foreach (Task task in trackTasks)
            task.OnCompleted -= RemoveTargetTask;
    }

    [SerializeField] TaskTarget target; // 이 target을 가진 task들을 가져와 감시함
    void TryAddTargetQuest(Quest _quest)
    {
        if(target != null && _quest.ContainsTarget(target))
        {
            _quest.OnNewTaskGroup += UpdateTargetTask;
            _quest.OnCompleted += RemoveTargetQuest;

            if(_quest.CurrentTaskGroup.FindTaskWithTarget(target))
                UpdateTargetTask(_quest);
        }
    }

    void UpdateTargetTask(Quest _quest)
    {
        Task targetTask = _quest.CurrentTaskGroup.FindTaskWithTarget(target);
        if (targetTask != null)
        {
            if (trackQuests.Contains(_quest) == false)
                trackQuests.Add(_quest);

            AddTargetTask(targetTask);
            targetTask.OnCompleted += RemoveTargetTask;
        }
    }

    [SerializeField] MarkerMaterialData[] markerMaterialDatas;
    Renderer myRenderer;
    List<Task> trackTasks = new List<Task>();
    void AddTargetTask(Task _task)
    {
        if (_task.State == TaskState.Running)
        {
            myRenderer.material = markerMaterialDatas.First(x => x.category == _task.Category).markerMaterial;
            trackTasks.Add(_task);
            gameObject.SetActive(true);
        }
    }

    #region Only Callback function
    void UpdateTargetTask(Quest _quest, TaskGroup _currentTaskGroup, TaskGroup _prevTaskGroup = null)
        => UpdateTargetTask(_quest);

    void RemoveTargetTask(Task _task)
    {
        trackTasks.Remove(_task);
        gameObject.SetActive(trackTasks.Count != 0);
        if (trackTasks.Count == 0) gameObject.SetActive(false);
    }

    void RemoveTargetQuest(Quest _quest)
    {
        if (trackQuests.Contains(_quest))
            trackQuests.Remove(_quest);
    }
    #endregion
}

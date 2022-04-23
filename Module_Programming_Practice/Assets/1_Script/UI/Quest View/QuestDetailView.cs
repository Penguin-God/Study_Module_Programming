using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestDetailView : MonoBehaviour
{
    [SerializeField] GameObject displayGroup;
    [SerializeField] Button cancelButton;

    [Header("퀘스트 설명")]
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;

    [Header("테스크 설명")]
    [SerializeField] RectTransform taskDescriptorGroup;
    [SerializeField] TaskDescriptor taskDescriptorPrefab;
    [SerializeField] int taskDescriptorPoolCount;

    [Header("보상 설명")]
    [SerializeField] RectTransform rewardDescriptionGroup;
    [SerializeField] TextMeshProUGUI rewardDescriptionPrefab;
    [SerializeField] int rewardDescriptionPoolCount;

    [SerializeField] List<TaskDescriptor> taskDescriptorPool;
    [SerializeField] List<TextMeshProUGUI> rewardDescriptionPool;

    public Quest Target { get; private set; }

    private void Awake()
    {
        taskDescriptorPool = CreatePool(taskDescriptorPrefab, taskDescriptorPoolCount, taskDescriptorGroup);
        rewardDescriptionPool = CreatePool(rewardDescriptionPrefab, rewardDescriptionPoolCount, rewardDescriptionGroup);
        displayGroup.SetActive(false);
    }

    private void Start()
    {
        cancelButton.onClick.AddListener(CancelQuest);
    }

    List<T> CreatePool<T>(T _prefab, int _count, RectTransform _parent) where T : MonoBehaviour
    {
        // () 안에 int 값은 list가 가지고 있는 Capacity라는 프로퍼티 값을 설정하는 것
        // List의 내부 구현은 일정 수의 Capacity 만큼 메모리를 잡고 있다가 Count가 Capacity를 초과하면 다시
        // Capacity늘려 메모리를 확장하는데 리스트의 초기 크기를 알고 있으면 Capacity을 설정해서 메모리를 확보할 수 있음
        List<T> _pool = new List<T>(_count);
        for (int i = 0; i < _count; i++)
            _pool.Add(Instantiate(_prefab, _parent));

        return _pool;
    }

    private void CancelQuest()
    {
        if (Target.IsCancelable)
            Target.Cancel();
    }

    public void Show(Quest _quest)
    {
        displayGroup.SetActive(true);
        Target = _quest;

        title.text = _quest.DisplayName;
        description.text = _quest.Description;

        int _taskIndex = 0;
        foreach(TaskGroup _taskGroup in _quest.TaskGroups)
        {
            foreach (Task _task in _taskGroup.Tasks)
            {
                TaskDescriptor _poolObject = taskDescriptorPool[_taskIndex++];
                _poolObject.gameObject.SetActive(true);

                if (_taskGroup.IsComplete) _poolObject.UpdateTextUsingStrikeThrough(_task);
                else if (_taskGroup == _quest.CurrentTaskGroup) _poolObject.UpdateText(_task);
                else _poolObject.UpdateText("● ??????????");
            }
        }

        // 미사용 pool 오브젝트 비활성화
        for (int i = _taskIndex; i < taskDescriptorPool.Count; i++)
            taskDescriptorPool[i].gameObject.SetActive(false);

        IReadOnlyList<Reward> _rewards = _quest.Rewards;
        int _rewardCount = _rewards.Count;
        for (int i = 0; i < rewardDescriptionPoolCount; i++)
        {
            TextMeshProUGUI _poolObject = rewardDescriptionPool[i];
            if (i < _rewardCount)
            {
                Reward _reward = _rewards[i];
                _poolObject.text = $"● {_reward.Description} +{_reward.Quantity}";
                _poolObject.gameObject.SetActive(true);
            }
            else
                _poolObject.gameObject.SetActive(false);
        }

        cancelButton.gameObject.SetActive(_quest.IsCancelable && !_quest.IsComplete);
    }

    public void Hide()
    {
        Target = null;
        displayGroup.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }
}

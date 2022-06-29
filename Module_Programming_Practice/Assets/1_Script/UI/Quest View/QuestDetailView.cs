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

    [SerializeField] ToggleGroup tabGroup;

    public Quest Target { get; private set; }

    private void OnEnable()
    {
        if (Target != null)
            Show(Target); // 내용 업데이트
    }

    private void Awake()
    {
        taskDescriptorPool = CreatePool(taskDescriptorPrefab, taskDescriptorPoolCount, taskDescriptorGroup);
        rewardDescriptionPool = CreatePool(rewardDescriptionPrefab, rewardDescriptionPoolCount, rewardDescriptionGroup);

        displayGroup.SetActive(false);
    }

    private void Start()
    {
        QuestSystem.Instance.OnQuestCompleted += HideDetail;
        QuestSystem.Instance.OnQuestCanceled += HideDetail;

        foreach (var _tab in tabGroup.ActiveToggles())
            _tab.onValueChanged.AddListener(HideDetail);

        cancelButton.onClick.AddListener(CancelQuest);
    }

    void OnDestroy()
    {
        if (QuestSystem.Instance == null) return;
        QuestSystem.Instance.OnQuestCompleted -= HideDetail;
        QuestSystem.Instance.OnQuestCanceled -= HideDetail;
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
        SetQuestStatus();

        int _taskIndex = 0;
        ShowTaskStatus();
        InActivePoolObject();

        ShowRewards();
        cancelButton.gameObject.SetActive(_quest.IsCancelable && !_quest.IsComplete);


        void SetQuestStatus()
        {
            Target = _quest;
            title.text = _quest.DisplayName;
            description.text = _quest.Description;
        }

        void ShowTaskStatus()
        {
            foreach (TaskGroup _taskGroup in _quest.TaskGroups)
            {
                foreach (Task _task in _taskGroup.Tasks)
                    SetTaskDescriptor(_taskGroup, _task, taskDescriptorPool[_taskIndex++]);
            }

            void SetTaskDescriptor(TaskGroup _taskGroup, Task _task, TaskDescriptor descriptor)
            {
                descriptor.gameObject.SetActive(true);

                if (_taskGroup.IsComplete) descriptor.UpdateTextUsingStrikeThrough(_task);
                else if (_taskGroup == _quest.CurrentTaskGroup) descriptor.UpdateText(_task);
                else descriptor.UpdateText("● ??????????");
            }
        }

        void InActivePoolObject()
        {
            for (int i = _taskIndex; i < taskDescriptorPool.Count; i++)
                taskDescriptorPool[i].gameObject.SetActive(false);
        }

        void ShowRewards()
        {
            for (int i = 0; i < rewardDescriptionPoolCount; i++)
                SetRewardText(rewardDescriptionPool[i], i);

            void SetRewardText(TextMeshProUGUI rewardText, int i)
            {
                if (i < _quest.Rewards.Count)
                {
                    rewardText.text = $"● {_quest.Rewards[i].Description} +{_quest.Rewards[i].Quantity}";
                    rewardText.gameObject.SetActive(true);
                }
                else
                    rewardText.gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        Target = null;
        displayGroup.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }

    #region Only Callback Function
    void HideDetail(Quest _quest)
    {
        if (Target == _quest) Hide();
    }

    void HideDetail(bool click) => Hide();

    #endregion
}

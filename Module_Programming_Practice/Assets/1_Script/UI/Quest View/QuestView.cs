using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestView : MonoBehaviour
{
    [SerializeField] QuestListViewController questListViewController;
    [SerializeField] QuestDetailView questDetailView;

    private void Start()
    {
        QuestSystem _questSystem = QuestSystem.Instance;


        foreach (Quest _quest in _questSystem.ActiveQuests)
            AddQuest_To_ActiveQuestListView(_quest);

        foreach (Quest _quest in _questSystem.CompleteQuests)
            AddQuest_To_completedQuestListView(_quest);

        _questSystem.OnQuestRegistered += AddQuest_To_ActiveQuestListView;

        _questSystem.OnQuestCompleted += RemoveQuest_From_ActiveQuestListView;
        _questSystem.OnQuestCompleted += AddQuest_To_completedQuestListView;
        _questSystem.OnQuestCompleted += HideDetail_If_QuestCanceled;

        _questSystem.OnQuestCanceled += HideDetail_If_QuestCanceled;
        _questSystem.OnQuestCanceled += RemoveQuest_From_ActiveQuestListView;

        foreach (var _tab in questListViewController.Tabs)
            _tab.onValueChanged.AddListener(HideDetail);

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        QuestSystem _questSystem = QuestSystem.Instance;
        if (_questSystem == null) return;

        _questSystem.OnQuestRegistered -= AddQuest_To_ActiveQuestListView;

        _questSystem.OnQuestCompleted -= RemoveQuest_From_ActiveQuestListView;
        _questSystem.OnQuestCompleted -= AddQuest_To_completedQuestListView;
        _questSystem.OnQuestCompleted -= HideDetail_If_QuestCanceled;

        _questSystem.OnQuestCanceled -= HideDetail_If_QuestCanceled;
        _questSystem.OnQuestCanceled -= RemoveQuest_From_ActiveQuestListView;
    }

    private void OnEnable()
    {
        if (questDetailView.Target != null)
            questDetailView.Show(questDetailView.Target); // 내용 업데이트
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gameObject.SetActive(false);
    }

    private void ShowDetail(bool _isOn, Quest _quest)
    {
        if (_isOn)
            questDetailView.Show(_quest);
    }

    private void HideDetail(bool _isOn)
    {
        questDetailView.Hide();    
    }

    private void AddQuest_To_ActiveQuestListView(Quest _quest)
        => questListViewController.AddQuest_To_ActiveQuestListView(_quest, (_isOn) => ShowDetail(_isOn, _quest));

    private void AddQuest_To_completedQuestListView(Quest _quest)
        => questListViewController.AddQuest_To_completedQuestListView(_quest, (_isOn) => ShowDetail(_isOn, _quest));

    private void HideDetail_If_QuestCanceled(Quest _quest)
    {
        if (questDetailView.Target == _quest)
            questDetailView.Hide();
    }

    private void RemoveQuest_From_ActiveQuestListView(Quest _quest)
    {
        questListViewController.RemoveQuest_From_ActiveQuestListView(_quest);
        if (questDetailView.Target == null)
            questDetailView.Hide();
    }
}

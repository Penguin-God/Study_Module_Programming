using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuestView : MonoBehaviour
{
    [SerializeField] QuestListViewController questListViewController;
    // [SerializeField] QuestDetailView questDetailView;

    [SerializeField] ToggleGroup tabGroup;
    [SerializeField] QuestListView activeQuestListView;
    [SerializeField] QuestListView completedQuestListView;
    
    public void _RemoveQuest_From_ActiveQuestListView(Quest _quest)
        => activeQuestListView.RemoveElement(_quest);

    private void Start()
    {
        QuestSystem _questSystem = QuestSystem.Instance;
        SetupElement();

        _questSystem.OnQuestRegistered += AddActiveQuestElement;

        _questSystem.OnQuestCompleted += RemoveQuest_From_ActiveQuestListView;
        _questSystem.OnQuestCompleted += AddCompleteQuestElement;
        //_questSystem.OnQuestCompleted += HideDetail;

        //_questSystem.OnQuestCanceled += HideDetail;
        _questSystem.OnQuestCanceled += RemoveQuest_From_ActiveQuestListView;

        //foreach (var _tab in questListViewController.Tabs)
        //    _tab.onValueChanged.AddListener(HideDetail);

        gameObject.SetActive(false);

        // 추가
        //_questSystem.OnQuestCompleted += activeQuestListView.RemoveElement;

        void SetupElement()
        {
            foreach (Quest _quest in _questSystem.ActiveQuests)
                AddActiveQuestElement(_quest);

            foreach (Quest _quest in _questSystem.CompleteQuests)
                AddCompleteQuestElement(_quest);
        }
    }

    private void OnDestroy()
    {
        QuestSystem _questSystem = QuestSystem.Instance;
        if (_questSystem == null) return;

        _questSystem.OnQuestRegistered -= AddActiveQuestElement;

        _questSystem.OnQuestCompleted -= RemoveQuest_From_ActiveQuestListView;
        _questSystem.OnQuestCompleted -= AddCompleteQuestElement;
        //_questSystem.OnQuestCompleted -= HideDetail;

        //_questSystem.OnQuestCanceled -= HideDetail;
        _questSystem.OnQuestCanceled -= RemoveQuest_From_ActiveQuestListView;
    }

    private void OnEnable()
    {
        // if (questDetailView.Target != null)
        //    questDetailView.Show(questDetailView.Target); // 내용 업데이트
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gameObject.SetActive(false);
    }

    private void AddActiveQuestElement(Quest _quest)
        => questListViewController.AddQuest_To_ActiveQuestListView(_quest);

    private void AddCompleteQuestElement(Quest _quest)
        => questListViewController.AddQuest_To_CompletedQuestListView(_quest);


    #region Only Callback Function
    //private void HideDetail(Quest _quest)
    //{
    //    if (questDetailView.Target == _quest)
    //        questDetailView.Hide();
    //}

    // private void HideDetail(bool _isOn) => questDetailView.Hide();

    private void RemoveQuest_From_ActiveQuestListView(Quest _quest)
    {
        // activeQuestListView.RemoveElement(_quest);

        questListViewController.RemoveQuest_From_ActiveQuestListView(_quest);
        //if (questDetailView.Target == null)
        //    questDetailView.Hide();
    }

    #endregion
}

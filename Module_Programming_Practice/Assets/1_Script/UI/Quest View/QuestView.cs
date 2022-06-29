using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuestView : MonoBehaviour
{
    [SerializeField] QuestListView activeQuestListView;
    [SerializeField] QuestListView completedQuestListView;
    
    private void Start()
    {
        QuestSystem _questSystem = QuestSystem.Instance;
        SetupElement();

        _questSystem.OnQuestRegistered += AddActiveQuestElement;
        
        _questSystem.OnQuestCompleted += RemoveActiveQuestElement;
        _questSystem.OnQuestCompleted += AddCompleteQuestElement;

        _questSystem.OnQuestCanceled += RemoveActiveQuestElement;

        gameObject.SetActive(false);

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

        _questSystem.OnQuestCompleted -= RemoveActiveQuestElement;
        _questSystem.OnQuestCompleted -= AddCompleteQuestElement;

        _questSystem.OnQuestCanceled -= RemoveActiveQuestElement;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gameObject.SetActive(false);
    }

    #region Callback Function
    private void AddActiveQuestElement(Quest _quest) => activeQuestListView.AddElement(_quest);
    private void AddCompleteQuestElement(Quest _quest) => completedQuestListView.AddElement(_quest);
    private void RemoveActiveQuestElement(Quest _quest) => activeQuestListView.RemoveElement(_quest);
    #endregion
}

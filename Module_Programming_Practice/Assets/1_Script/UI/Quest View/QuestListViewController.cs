using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuestListViewController : MonoBehaviour
{
    [SerializeField] QuestListView activeQuestListView;
    [SerializeField] QuestListView completedQuestListView;

    public void AddQuest_To_ActiveQuestListView(Quest _quest) => activeQuestListView.AddElement(_quest);

    public void RemoveQuest_From_ActiveQuestListView(Quest _quest)
        => activeQuestListView.RemoveElement(_quest);

    public void AddQuest_To_CompletedQuestListView(Quest _quest) => completedQuestListView.AddElement(_quest);
}

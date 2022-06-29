using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuestListViewController : MonoBehaviour
{
    [SerializeField] ToggleGroup tabGroup;
    [SerializeField] QuestListView activeQuestListView;
    [SerializeField] QuestListView completedQuestListView;

    public IEnumerable<Toggle> Tabs => tabGroup.ActiveToggles();

    public void AddQuest_To_ActiveQuestListView(Quest _quest, UnityAction<bool> _OnClicked)
        => activeQuestListView.AddElement(_quest, _OnClicked);

    public void AddQuest_To_ActiveQuestListView(Quest _quest) => activeQuestListView.AddElement(_quest);

    public void RemoveQuest_From_ActiveQuestListView(Quest _quest)
        => activeQuestListView.RemoveElement(_quest);

    public void AddQuest_To_completedQuestListView(Quest _quest, UnityAction<bool> _OnClicked)
        => completedQuestListView.AddElement(_quest, _OnClicked);

    public void AddQuest_To_CompletedQuestListView(Quest _quest) => completedQuestListView.AddElement(_quest);
}

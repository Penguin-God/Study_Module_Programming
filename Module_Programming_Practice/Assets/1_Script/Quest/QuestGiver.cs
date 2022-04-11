using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [SerializeField] Quest[] quests;

    private void Start()
    {
        foreach(Quest _quest in quests)
        {
            if (_quest.IsAcceptionable && !QuestSystem.Instance.ContainsInCompleteQuests(_quest))
                QuestSystem.Instance.Register(_quest);
        }
    }
}

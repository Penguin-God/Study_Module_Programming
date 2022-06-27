using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class QuestGiver : MonoBehaviour
{
    [SerializeField] Quest[] quests;

    private void Awake()
    {
        foreach(Quest _quest in quests)
        {
            if (_quest.IsAcceptionable && !QuestSystem.Instance.ContainsInCompleteQuests(_quest) && 
                            QuestSystem.Instance.ActiveQuests.Any(x => x.CodeName == _quest.CodeName) == false)
            {
                QuestSystem.Instance.Register(_quest);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Condition/IsQuestComplete", fileName = "IsComplete_")]
public class IsQuestComplete : QuestCondition
{
    [SerializeField] Quest target;
    public override bool IsPass(Quest _quest)
        => QuestSystem.Instance.ContainsInCompleteQuests(target);
}
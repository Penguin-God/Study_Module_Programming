using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestCondition : ScriptableObject
{
    [SerializeField] string description;

    public abstract bool IsPass(Quest _quest);
}

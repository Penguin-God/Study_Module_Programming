using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Reward : ScriptableObject
{
    [SerializeField] string description;

    [SerializeField] Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] int quantity;
    public int Quantity => quantity;

    public abstract void Give(Quest _quest);
}

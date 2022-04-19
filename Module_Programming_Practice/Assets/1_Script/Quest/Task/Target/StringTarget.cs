using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/String Target", fileName = "Target Name Is ")]
public class StringTarget : TaskTarget
{
    [SerializeField] string value;
    public override object Value => value;

    public override bool IsEqual(object _target)
    {
        string _targetAsString = _target as string;
        if (_targetAsString == null) return false;
        return value == _targetAsString;
    }
}

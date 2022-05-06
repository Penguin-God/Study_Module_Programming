using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskTarget : ScriptableObject
{
    // object 타입으로 변수만 선언하고 자료형은 자식마다 다름
    public abstract object Value { get; }

    public abstract bool IsEqual(object _target);
}

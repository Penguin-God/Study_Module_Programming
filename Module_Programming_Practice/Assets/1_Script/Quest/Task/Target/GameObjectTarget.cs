using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/GameObject Target", fileName = "Target ")]
public class GameObjectTarget : TaskTarget
{
    [SerializeField] GameObject value;
    public override object Value => value;

    public override bool IsEqual(object _target)
    {
        GameObject _targetAsGameObject = _target as GameObject;
        if (_targetAsGameObject == null) return false;
        
        // 인자값으로 들어온 오브젝트가 씬 오브젝트라면 이름 뒤에 (clone) 과 같은 값들이 추가되 있을 수 있음.
        // 그러므로 인자값의 이름에 value의 이름이 포함되어 있는지 확인하는 방식으로 비교함
        return _targetAsGameObject.name.Contains(value.name);
    }
}

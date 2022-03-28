using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/GameObject Target", fileName = "Target ")]
public class GameObjectTarget : TaskTarget
{
    [SerializeField] GameObject value;
    public override object Value => value;

    public override bool IsTarget(object _target)
    {
        GameObject _targetAsGameObject = _target as GameObject;
        if (_targetAsGameObject == null) return false;
        
        // ���ڰ����� ���� ������Ʈ�� �� ������Ʈ��� �̸� �ڿ� (clone) �� ���� ������ �߰��� ���� �� ����.
        // �׷��Ƿ� ���ڰ��� �̸��� value�� �̸��� ���ԵǾ� �ִ��� Ȯ���ϴ� ������� ����
        return _targetAsGameObject.name.Contains(value.name);
    }
}

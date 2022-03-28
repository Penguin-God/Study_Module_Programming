using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/TaskAction/Set Count", fileName = "new task set count")]
public class SetCount : TaskAction
{
    public override int Run(Task _task, int _currentSuccess, int _successCount) => _successCount;
}

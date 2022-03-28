using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/TaskAction/Simple Count", fileName = "new task simple count")]
public class SimpleCount : TaskAction
{
    public override int Run(Task _task, int _currentSuccess, int _successCount) => _currentSuccess + _successCount;
}

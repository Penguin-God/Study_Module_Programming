using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/TaskAction/Positive Count", fileName = "new task positive count")]
public class PositiveCount : TaskAction
{
    public override int Run(Task _task, int _currentSuccess, int _successCount) => (_successCount > 0) ? _currentSuccess + _successCount : _currentSuccess;
}

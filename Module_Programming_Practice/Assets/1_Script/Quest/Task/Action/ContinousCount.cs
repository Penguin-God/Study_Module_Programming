using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/TaskAction/Continous Count", fileName = "new task continous count")]
public class ContinousCount : TaskAction
{
    public override int Run(Task _task, int _currentSuccess, int _successCount) => (_successCount > 0) ? _currentSuccess + _successCount : 0;
}

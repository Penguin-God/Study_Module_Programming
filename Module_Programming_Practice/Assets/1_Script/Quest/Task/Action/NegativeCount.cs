using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/TaskAction/Negative Count", fileName = "new task negative count")]
public class NegativeCount : TaskAction
{
    public override int Run(Task _task, int _currentSuccess, int _successCount) => 
        (_successCount < 0) ? _currentSuccess + Mathf.Abs(_successCount) : _currentSuccess;
}

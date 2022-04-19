using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TaskGroupState
{
    Inactive,
    Running,
    Complete,
}

[System.Serializable]
public class TaskGroup
{
    [SerializeField] Task[] tasks;
    public IReadOnlyList<Task> Tasks => tasks;

    public Quest Owner { get; private set; }

    public bool IsAllComplete => tasks.All(_task => _task.IsComplete);
    public bool IsComplete => State == TaskGroupState.Complete;

    public TaskGroupState State { get; private set; }

    public TaskGroup(TaskGroup _copyTarget)
    {
        tasks = _copyTarget.tasks.Select(x => Object.Instantiate(x)).ToArray();
    }

    public void Setup(Quest _owner)
    {
        Owner = _owner;
        foreach (Task _task in tasks) _task.Setup(_owner);
    }

    public void Start()
    {
        State = TaskGroupState.Running;
        foreach (Task _task in tasks) _task.Start();
    }

    public void End()
    {
        foreach (Task _task in tasks) _task.End();
    }

    public void ReceiveReport(string _category, object _target, int _successCount)
    {
        foreach(Task _task in tasks)
        {
            if (_task.IsTarget(_category, _target))
                _task.ReceiveReport(_successCount);
        }
    }

    public void Complete()
    {
        if (IsComplete) return;

        State = TaskGroupState.Complete;
        foreach (Task _task in tasks)
        {
            if(!_task.IsComplete)
                _task.Complete();
        }
    }

    public Task FindTaskWithTarget(object _target) => tasks.FirstOrDefault(x => x.ContainsTarget(_target));
    public Task FindTaskWithTarget(TaskTarget _target) => FindTaskWithTarget(_target.Value);

    public bool ContainsTaskWithTarget(object _target) => tasks.Any(x => x.ContainsTarget(_target));
    public bool ContainsTaskWithTarget(TaskTarget _target) => ContainsTaskWithTarget(_target.Value);
}

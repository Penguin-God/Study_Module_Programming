using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestTracker : MonoBehaviour
{
    // task�� ������ ������ִ� TaskDescriptor, Quest ���� ��°� TaskDescriptor�� ��� ����ϴ� QuestTrack
    // QuestTrack�� �����ϴ� QuestTrackerView 3���� ��ũ��Ʈ�� ����

    [SerializeField] TextMeshProUGUI questTitleText;
    [SerializeField] TaskDescriptor taskDescriptorPrefab;

    Dictionary<Task, TaskDescriptor> taskDescriptorByTask = new Dictionary<Task, TaskDescriptor>();
    Quest targetQeust;

    public void Setup(Quest _targetQeust, Color _titleColor)
    {
        targetQeust = _targetQeust;

        questTitleText.text = (_targetQeust.Category == null) ?
            _targetQeust.DisplayName :
            $"[{_targetQeust.Category.DisplayName}] {_targetQeust.DisplayName}";
        questTitleText.color = _titleColor;

        _targetQeust.OnNewTaskGroup += UpdateTaskDescriptor;
        _targetQeust.OnCompleted += DestroySelf;

        // ���� Update
        IReadOnlyList<TaskGroup> _taskGroups = _targetQeust.TaskGroups;
        UpdateTaskDescriptor(_targetQeust, _taskGroups[0], null);

        // ���� ������ �÷����� �� �̸� ��� Task�� Ŭ�����ߴٸ�
        if(_taskGroups[0] != _targetQeust.CurrentTaskGroup)
        {
            for (int i = 1; i < _taskGroups.Count; i++)
            {
                TaskGroup _taskGroup = _taskGroups[i];
                UpdateTaskDescriptor(_targetQeust, _taskGroup, _taskGroups[i - 1]);

                if (_taskGroup == _targetQeust.CurrentTaskGroup) break;
            }
        }
    }

    private void OnDestroy()
    {
        if(targetQeust != null)
        {
            targetQeust.OnNewTaskGroup -= UpdateTaskDescriptor;
            targetQeust.OnCompleted -= DestroySelf;
        }

        foreach(var _pair in taskDescriptorByTask) _pair.Key.OnSuccessChanged -= UpdateText;
    }

    #region call back
    void UpdateTaskDescriptor(Quest _quest, TaskGroup _currentTaskGroup, TaskGroup _prevTaskGroup)
    {
        foreach(Task _task in _currentTaskGroup.Tasks)
        {
            TaskDescriptor _newDescriptor = Instantiate(taskDescriptorPrefab);
            _newDescriptor.UpdateText(_task);
            _task.OnSuccessChanged += UpdateText;

            taskDescriptorByTask.Add(_task, _newDescriptor);
        }

        if(_prevTaskGroup != null)
        {
            foreach (Task _task in _prevTaskGroup.Tasks)
            {
                TaskDescriptor _taskDescriptor = taskDescriptorByTask[_task];
                _taskDescriptor.UpdateTextUsingStrikeThrough(_task);
            }
        }
    }
    void UpdateText(Task _task, int _currentSuccessCount, int _prevSuccessCount) => taskDescriptorByTask[_task].UpdateText(_task);
    private void DestroySelf(Quest _quest) => Destroy(gameObject);
    #endregion
}

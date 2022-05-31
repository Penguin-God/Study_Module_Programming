using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class QuestTracker : MonoBehaviour
{
    // task의 정보를 출력해주는 TaskDescriptor, Quest 정보 출력과 TaskDescriptor의 제어를 담당하는 QuestTrack
    // QuestTrack를 제어하는 QuestTrackerView 3가지 스크립트로 구성되어 있음

    [SerializeField] TextMeshProUGUI questTitleText;
    [SerializeField] TaskDescriptor taskDescriptorPrefab;

    Dictionary<Task, TaskDescriptor> taskDescriptorByTask = new Dictionary<Task, TaskDescriptor>();
    Quest targetQeust;

    public void Setup(Quest _targetQeust, Color _titleColor)
    {
        targetQeust = _targetQeust;
        SetTitleText(_targetQeust, _titleColor);
        SubscribeQuest(_targetQeust);
        UpdateTaskDescriptor(_targetQeust, _targetQeust.TaskGroups[0], null);
        UpdatePreviousTask(_targetQeust, _targetQeust.TaskGroups);

        // 중첩 함수....
        void SetTitleText(Quest _targetQeust, Color _titleColor)
        {
            questTitleText.text = (_targetQeust.Category == null) ?
                _targetQeust.DisplayName :
                $"[{_targetQeust.Category.DisplayName}] {_targetQeust.DisplayName}";

            questTitleText.color = _titleColor;
        }
        void SubscribeQuest(Quest _targetQeust)
        {
            _targetQeust.OnNewTaskGroup += UpdateTaskDescriptor;
            _targetQeust.OnCompleted += DestroySelf;
        }
        void UpdatePreviousTask(Quest _targetQeust, IReadOnlyList<TaskGroup> _taskGroups)
        {
            // 이전 게임을 플레이할 때 이미 클리어한 테스크가 있다면
            if (_taskGroups[0] != _targetQeust.CurrentTaskGroup)
            {
                for (int i = 1; i < _taskGroups.Count; i++)
                {
                    TaskGroup _taskGroup = _taskGroups[i];
                    UpdateTaskDescriptor(_targetQeust, _taskGroup, _taskGroups[i - 1]);

                    if (_taskGroup == _targetQeust.CurrentTaskGroup) break;
                }
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

        foreach(var _pair in taskDescriptorByTask)
        {
            _pair.Key.OnSuccessChanged -= UpdateTaskText;
            _pair.Key.OnCompleted -= UpdateCompleteTask;
        }
    }

    private void AddTaskDescriptorByTask(Task _task, TaskDescriptor _newDescriptor)
    {
        taskDescriptorByTask.Add(_task, _newDescriptor);
        _newDescriptor.UpdateText(_task);
    }

    void UpdateTaskDescriptor(Quest _quest, TaskGroup _currentTaskGroup, TaskGroup _prevTaskGroup)
    {
        AddNewTaskByDescriptorPair(_currentTaskGroup);
        SubscribeTaskChanged(_currentTaskGroup.Tasks.ToList());
        SubscribeTaskCompleted(_currentTaskGroup.Tasks.ToList());

        // 중첩 함수...
        void SubscribeTaskChanged(List<Task> tasks) => tasks.ForEach(x => x.OnSuccessChanged += UpdateTaskText);
        void SubscribeTaskCompleted(List<Task> tasks) => tasks.ForEach(x => x.OnCompleted += UpdateCompleteTask);
        void AddNewTaskByDescriptorPair(TaskGroup _currentTaskGroup)
            => _currentTaskGroup.Tasks.ToList().ForEach(x => AddTaskDescriptorByTask(x, Instantiate(taskDescriptorPrefab, transform)));
    }

    #region only call back
    void UpdateTaskText(Task _task, int _currentSuccessCount, int _prevSuccessCount) => taskDescriptorByTask[_task].UpdateText(_task);
    void UpdateCompleteTask(Task task) => taskDescriptorByTask[task].UpdateTextUsingStrikeThrough(task);

    private void DestroySelf(Quest _quest) => Destroy(gameObject);
    #endregion
}

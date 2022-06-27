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
        _targetQeust.OnNewTaskGroup += CreateTaskDescriptor;
        _targetQeust.OnNewTaskGroup += UpdateCompleteTask;
        _targetQeust.OnCompleted += DestroySelf;
        Create_FirstTaskDescriptor_InQuest(_targetQeust);
        Create_PreviousClear_TaskDescriptor(_targetQeust, _targetQeust.TaskGroups);


        void SetTitleText(Quest _targetQeust, Color _titleColor)
        {
            questTitleText.text = (_targetQeust.Category == null) ?
                _targetQeust.DisplayName :
                $"[{_targetQeust.Category.DisplayName}] {_targetQeust.DisplayName}";

            questTitleText.color = _titleColor;
        }
        void Create_FirstTaskDescriptor_InQuest(Quest _targetQeust) => CreateTaskDescriptor(_targetQeust, _targetQeust.TaskGroups[0]);
        void Create_PreviousClear_TaskDescriptor(Quest _targetQeust, IReadOnlyList<TaskGroup> _taskGroups)
        {
            // 이전 게임을 플레이할 때 이미 클리어한 테스크가 있다면
            if (_taskGroups[0] != _targetQeust.CurrentTaskGroup)
            {
                for (int i = 1; i < _taskGroups.Count; i++)
                {
                    TaskGroup _taskGroup = _taskGroups[i];
                    CreateTaskDescriptor(_targetQeust, _taskGroup, _taskGroups[i - 1]);

                    if (_taskGroup == _targetQeust.CurrentTaskGroup) break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if(targetQeust != null)
        {
            targetQeust.OnNewTaskGroup -= CreateTaskDescriptor;
            targetQeust.OnNewTaskGroup -= UpdateCompleteTask;
            targetQeust.OnCompleted -= DestroySelf;
        }

        foreach(var _pair in taskDescriptorByTask)
        {
            _pair.Key.OnSuccessChanged -= UpdateTaskText;
        }
    }

    void CreateTaskDescriptor(Quest _quest, TaskGroup _currentTaskGroup, TaskGroup _prevTaskGroup = null)
    {
        _currentTaskGroup.Tasks.ToList().ForEach(x => taskDescriptorByTask.Add(x, InstantiateTaskDescriptor(x)));
        SubscribeTaskChanged(_currentTaskGroup.Tasks.ToList());

        TaskDescriptor InstantiateTaskDescriptor(Task _task)
        {
            TaskDescriptor descriptor = Instantiate(taskDescriptorPrefab, transform);
            descriptor.UpdateText(_task); // Setup
            return descriptor;
        }
        void SubscribeTaskChanged(List<Task> tasks) => tasks.ForEach(x => x.OnSuccessChanged += UpdateTaskText);
    }

    #region only call back
    void UpdateTaskText(Task _task, int _currentSuccessCount, int _prevSuccessCount) => taskDescriptorByTask[_task].UpdateText(_task);
    void UpdateCompleteTask(Quest _quest, TaskGroup _currentTaskGroup, TaskGroup _prevTaskGroup) 
        => _prevTaskGroup.Tasks.ToList().ForEach(x => taskDescriptorByTask[x].UpdateTextUsingStrikeThrough(x));

    private void DestroySelf(Quest _quest) => Destroy(gameObject);
    #endregion
}

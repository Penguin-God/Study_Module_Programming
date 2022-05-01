using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_QuestSystem : MonoBehaviour
{
    [SerializeField] Quest quest;
    [SerializeField] Category category;
    [SerializeField] TaskTarget target;

    private void Start()
    {
        QuestSystem _questSystem = QuestSystem.Instance;

        _questSystem.OnQuestRegistered += (_quest) =>
        {
            print($"New Quest : {_quest.CodeName} Registered");
            print($"Active Quest Count : {_questSystem.ActiveQuests.Count}");
            print($"Complete System Count : {_questSystem.CompleteQuests.Count}");
        };

        _questSystem.OnQuestCompleted += (_quest) =>
        {
            print($"Complete Quest : {_quest.CodeName}");
            print($"Active Quest Count : {_questSystem.ActiveQuests.Count}");
            print($"Complete System Count : {_questSystem.CompleteQuests.Count}");
        };

        // Quest 등록
        Quest _newQuest = _questSystem.Register(quest);
        _newQuest.OnTaskSuccessChanged += (Quest _quest, Task _task, int _currentSuccess, int _prevSuccess) =>
        {
            print($"Quest : {_quest.CodeName}, Task : {_task.CodeName}, CurrentSuccess : {_currentSuccess}");
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            QuestSystem.Instance.ReceiveReport(category, target, 1); // successCount의 1은 성공했음을 의미함
    }
}

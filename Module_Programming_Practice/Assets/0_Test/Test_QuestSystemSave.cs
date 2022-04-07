using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_QuestSystemSave : MonoBehaviour
{
    [SerializeField] Quest quest;
    [SerializeField] Category category;
    [SerializeField] TaskTarget target;

    private void Start()
    {
        QuestSystem _questSystem = QuestSystem.Instance;

        if (_questSystem.ActiveQuests.Count == 0)
        {
            Debug.Log("등록");
            var _newQuest = _questSystem.Register(quest);
        }
        else
        {
            _questSystem.OnQuestCompleted += (_quest) =>
            {
                Debug.Log("Complete");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            };
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            QuestSystem.Instance.ReceiveReport(category, target, 1); // successCount의 1은 성공했음을 의미함
    }
}

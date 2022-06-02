using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
class QuestReporters
{
    public List<QuestReporter> reporters;
}
public class TestUtility : MonoBehaviour
{
    [SerializeField] List<QuestReporters> mainQuestReporters;
    [SerializeField] float reportDelay = 0.3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            TestQuest(mainQuestReporters);
    }

    void TestQuest(List<QuestReporters> questReporters) => StartCoroutine(QuestClearWithDelay(questReporters));

    IEnumerator QuestClearWithDelay(List<QuestReporters> questReporters)
    {
        for (int i = 0; i < questReporters.Count; i++)
        {
            List<QuestReporter> reporters = questReporters[i].reporters;
            for (int j = 0; j < reporters.Count; j++)
            {
                reporters[j].Report();
                yield return new WaitForSeconds(reportDelay);
            }
            yield return new WaitForSeconds(reportDelay);
        }
    }
}

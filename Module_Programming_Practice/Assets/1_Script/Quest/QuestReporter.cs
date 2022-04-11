using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class QuestReporter : MonoBehaviour
{
    [SerializeField] Category category;
    [SerializeField] TaskTarget target;
    [SerializeField] int successCount;
    [SerializeField] string[] colliderTags;

    public void Report() => QuestSystem.Instance.ReceiveReport(category, target, successCount);

    private void ReportIfPassCondition(Component _component)
    {
        if (colliderTags.Any(x => _component.CompareTag(x)))
            Report();
    }

    private void OnTriggerEnter(Collider other) => ReportIfPassCondition(other);
    private void OnTriggerEnter2D(Collider2D collision) => ReportIfPassCondition(collision);
}

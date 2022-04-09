using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;



public class QuestTrackerView : MonoBehaviour
{
    [SerializeField] QuestTracker questTrackerPrefab;
    [SerializeField] CategoryColor[] categoryColors;

    private void Start()
    {
        QuestSystem.Instance.OnQuestRegistered += CreateQuestTracker;
        
        foreach (Quest _quest in QuestSystem.Instance.ActiveQuests) CreateQuestTracker(_quest);
    }

    private void OnDestroy()
    {
        if (QuestSystem.Instance)
            QuestSystem.Instance.OnQuestRegistered -= CreateQuestTracker;
    }

    void CreateQuestTracker(Quest _quest)
    {
        CategoryColor _categoryColor = categoryColors.FirstOrDefault(x => x.category == _quest.Category);
        Color _color = (_categoryColor.category == null) ? Color.white : _categoryColor.color;
        Instantiate(questTrackerPrefab, transform).Setup(_quest, _color);
    }

    [System.Serializable]
    private struct CategoryColor
    {
        public Category category;
        public Color color;
    }
}

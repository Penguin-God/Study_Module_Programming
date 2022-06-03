using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class QuestTrackerView : MonoBehaviour
{
    [SerializeField] QuestTracker questTrackerPrefab;
    [SerializeField] CategoryColor[] categoryColors;

    private void Awake()
    {
        QuestSystem.Instance.OnQuestRegistered += CreateQuestTracker;
        
        // foreach (Quest _quest in QuestSystem.Instance.ActiveQuests) CreateQuestTracker(_quest);
    }

    private void OnDestroy()
    {
        if (QuestSystem.Instance)
            QuestSystem.Instance.OnQuestRegistered -= CreateQuestTracker;
    }

    void CreateQuestTracker(Quest _quest)
        => Instantiate(questTrackerPrefab, transform).Setup(_quest, GetCategoryColor(_quest));

    private Color GetCategoryColor(Quest _quest)
    {
        CategoryColor _categoryColor = categoryColors.FirstOrDefault(x => x.category == _quest.Category);
        return (_categoryColor.category == null) ? Color.white : _categoryColor.color;
    }

    [Serializable]
    private struct CategoryColor
    {
        public Category category;
        public Color color;
    }
}

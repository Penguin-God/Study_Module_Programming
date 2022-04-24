using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementView : MonoBehaviour
{
    [SerializeField] RectTransform achievementGroup;
    [SerializeField] AchievementDetailView achievementDetailViewPrefab;

    private void Start()
    {
        var _questSystem = QuestSystem.Instance;
        CreateDetailViews(QuestSystem.Instance.ActiveAchievements);
        CreateDetailViews(QuestSystem.Instance.CompleteAchievements);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gameObject.SetActive(false);
    }

    private void CreateDetailViews(IReadOnlyList<Quest> _achievements)
    {
        foreach (Quest _achievement in _achievements)
            Instantiate(achievementDetailViewPrefab, achievementGroup).Setup(_achievement);
    }
}

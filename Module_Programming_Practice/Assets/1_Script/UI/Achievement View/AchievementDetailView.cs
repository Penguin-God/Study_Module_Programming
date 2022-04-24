using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementDetailView : MonoBehaviour
{
    [SerializeField] Image achievementIcon;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Image rewardIcon;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] GameObject completionScreen;

    private Quest target;

    public void Setup(Quest _achievement)
    {
        target = _achievement;
        achievementIcon.sprite = _achievement.Icon;
        titleText.text = _achievement.DisplayName;

        Task _task = _achievement.CurrentTaskGroup.Tasks[0];
        description.text = BuildTaskDescription(_task);

        Reward _reward = _achievement.Rewards[0];
        rewardIcon.sprite = _reward.Icon;
        rewardText.text = $"{_reward.Description} +{_reward.Quantity}";

        if (_achievement.IsComplete)
            completionScreen.SetActive(true);
        else
        {
            completionScreen.SetActive(false);
            _achievement.OnTaskSuccessChanged += UpdateDescription;
            _achievement.OnCompleted += ShowCompletionScreen;
        }
    }

    private void OnDestroy()
    {
        if(target != null)
        {
            target.OnTaskSuccessChanged -= UpdateDescription;
            target.OnCompleted -= ShowCompletionScreen;
        }
    }

    private void UpdateDescription(Quest _achievement, Task _task, int _currentSuccess, int _prevSuccess)
        => description.text = BuildTaskDescription(_task);

    private void ShowCompletionScreen(Quest _achievement)
        => completionScreen.SetActive(true);

    private string BuildTaskDescription(Task _task) => $"{_task.Description} {_task.CurrentSuccess} / {_task.NeedSuccessToComplete}"; 
}

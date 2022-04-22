using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class QuestCompletionNotifier : MonoBehaviour
{
    [SerializeField] string titleDescription;

    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI rewardText;
    [SerializeField] float showTime = 3f;

    Queue<Quest> reservedQuests = new Queue<Quest>();
    StringBuilder stringBuilder = new StringBuilder();

    private void Start()
    {
        var _questSystem = QuestSystem.Instance;
        _questSystem.OnQuestCompleted += Notify;
        _questSystem.OnAchievementCompleted += Notify;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        var _questSystem = QuestSystem.Instance;
        if (_questSystem == null) return;

        _questSystem.OnQuestCompleted -= Notify;
        _questSystem.OnAchievementCompleted -= Notify;
    }

    void Notify(Quest _quest)
    {
        reservedQuests.Enqueue(_quest);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            StartCoroutine(Co_ShowNotice());
        }
    }

    // Clear한 Quest의 정보 보여줌
    IEnumerator Co_ShowNotice()
    {
        var _waitSceond = new WaitForSeconds(showTime);

        while (reservedQuests.Count > 0)
        {
            Quest _quest = reservedQuests.Dequeue();
            // %{dn} : 그냥 기호, text가 나왔으면 하는 곳에 적으면 text가 Replace되는 기법
            titleText.text = titleDescription.Replace("%{dn}", _quest.DisplayName);

            // stringBuilder이 성능 좋다네요~ for문에 문자열 연산하면 준내 느리답니다~
            foreach(Reward _reward in _quest.Rewards)
            {
                stringBuilder.Append(_reward.Description);
                stringBuilder.Append(" ");
                stringBuilder.Append(_reward.Quantity);
                stringBuilder.Append(" ");
            }
            rewardText.text = stringBuilder.ToString();
            stringBuilder.Clear();
            yield return _waitSceond;
        }

        gameObject.SetActive(false);
    }
}

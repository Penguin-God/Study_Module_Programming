using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class QuestSystem : MonoBehaviour
{
    private static QuestSystem instance;
    private static bool isApplicationQuitting;

    public static QuestSystem Instance
    {
        get
        {
            if(!isApplicationQuitting && instance == null)
            {
                instance = FindObjectOfType<QuestSystem>();
                if(instance == null)
                {
                    instance = new GameObject("QuestSystem").AddComponent<QuestSystem>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completeQuests = new List<Quest>();

    private List<Quest> activeAchievement = new List<Quest>();
    private List<Quest> completeAchievement = new List<Quest>();

    public IReadOnlyList<Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<Quest> CompleteQuests => completeQuests;
    public IReadOnlyList<Quest> ActiveAchievement => activeAchievement;
    public IReadOnlyList<Quest> CompleteAchievement => completeAchievement;


    private QuestDatabase questDatabase;
    private QuestDatabase achievementDatabase;

    private void Awake()
    {
        questDatabase = Resources.Load<QuestDatabase>("Quest_Database");
        achievementDatabase = Resources.Load<QuestDatabase>("Achievement_Database");

        // 업적들은 처음에 다 등록해둠
        foreach (Quest _quest in achievementDatabase.Quests) Register(_quest);
    }

    public event Action<Quest> OnQuestRegistered;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest> OnQuestCanceled;

    public event Action<Quest> OnAchievementRegistered;
    public event Action<Quest> OnAchievementCompleted;
    //public event Action<Quest> OnAchievementCanceled;

    public Quest Register(Quest _quest)
    {
        // 복사본 생성
        // quest 내부에 있는 task도 scriptable object이기 때문에 task와 같은 모듈도 복사본을 만들어줘야함
        // 하나하나 만들기에는 너무 지저분하니 지들이 알아서 복사본을 만들어주는 함수를 사용하는 쪽으로 구현함
        // 이러면 후에 복사본 생성 내부 코드에 변화가 생겨도 QuestSystem은 수정할 필요가 없음
        Quest _newQuest = _quest.Clone();
        
        if(_newQuest is Achievement)
        {
            activeAchievement.Add(_newQuest);
            _newQuest.OnCompleted += Raise_OnAchievementCompleted;
        }
        else
        {
            activeQuests.Add(_newQuest);
            _newQuest.OnCompleted += Raise_OnQuestCompleted;
            _newQuest.OnCanceled += Raise_OnQuestCanceled;
        }

        _newQuest.OnRegister();
        OnQuestRegistered?.Invoke(_newQuest);
        return _newQuest;
    }


    public void ReceiveReport(string _category, object _target, int _successCount)
    {
        ReceiveReport(activeQuests, _category, _target, _successCount);
        ReceiveReport(activeAchievement, _category, _target, _successCount);
    }

    public void ReceiveReport(Category _category, TaskTarget _target, int _successCount)
        => ReceiveReport(_category.CodeName, _target.Value, _successCount);

    private void ReceiveReport(List<Quest> _quests, string _category, object _target ,int _successCount)
    {
        // 리스트의 사본을 만들어 돌리는 이유는 complete 시 파괴되도록 이벤트에 등록해두어 반복문 중 quest가 없어지면서 에러가 날 수도 있어서
        foreach (Quest _quest in _quests.ToArray()) 
            _quest.ReceiveReport(_category, _target, _successCount);
    }


    #region Quest Event에 등록해놓을 CallBack 함수들
    private void Raise_OnQuestCompleted(Quest _quest)
    {
        activeQuests.Remove(_quest);
        completeQuests.Add(_quest);
        OnQuestCompleted?.Invoke(_quest);
    }

    private void Raise_OnQuestCanceled(Quest _quest)
    {
        activeQuests.Remove(_quest);
        OnQuestCanceled?.Invoke(_quest);

        Destroy(_quest, Time.deltaTime);
    }


    private void Raise_OnAchievementCompleted(Quest _quest)
    {
        activeAchievement.Remove(_quest);
        completeAchievement.Add(_quest);
        OnAchievementCompleted?.Invoke(_quest);
    }
    #endregion


    #region quest가 있는지 확인하는 함수들
    public bool ContainsInActiveQuests(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteQuests(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInActiveAchievement(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteAchievement(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);
    #endregion
}

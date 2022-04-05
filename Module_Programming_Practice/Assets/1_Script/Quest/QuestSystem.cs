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

        // �������� ó���� �� ����ص�
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
        // ���纻 ����
        // quest ���ο� �ִ� task�� scriptable object�̱� ������ task�� ���� ��⵵ ���纻�� ����������
        // �ϳ��ϳ� ����⿡�� �ʹ� �������ϴ� ������ �˾Ƽ� ���纻�� ������ִ� �Լ��� ����ϴ� ������ ������
        // �̷��� �Ŀ� ���纻 ���� ���� �ڵ忡 ��ȭ�� ���ܵ� QuestSystem�� ������ �ʿ䰡 ����
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
        // ����Ʈ�� �纻�� ����� ������ ������ complete �� �ı��ǵ��� �̺�Ʈ�� ����صξ� �ݺ��� �� quest�� �������鼭 ������ �� ���� �־
        foreach (Quest _quest in _quests.ToArray()) 
            _quest.ReceiveReport(_category, _target, _successCount);
    }


    #region Quest Event�� ����س��� CallBack �Լ���
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


    #region quest�� �ִ��� Ȯ���ϴ� �Լ���
    public bool ContainsInActiveQuests(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteQuests(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInActiveAchievement(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteAchievement(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);
    #endregion
}

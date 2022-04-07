using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

public class QuestSystem : MonoBehaviour
{
    #region Save Key
    private const string key_SaveRoot = "questSystem";
    private const string key_ActiveQuestst = "activeQuests";
    private const string key_CompleteQuests = "completeQuests";
    private const string key_ActiveAchievement = "activeAchievements";
    private const string key_CompleteAchievement = "completeAchievements";
    #endregion

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

    [SerializeField] List<Quest> activeQuests = new List<Quest>();
    [SerializeField] List<Quest> completeQuests = new List<Quest>();

    [SerializeField] List<Quest> activeAchievements = new List<Quest>();
    [SerializeField] List<Quest> completeAchievements = new List<Quest>();

    public IReadOnlyList<Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<Quest> CompleteQuests => completeQuests;
    public IReadOnlyList<Quest> ActiveAchievement => activeAchievements;
    public IReadOnlyList<Quest> CompleteAchievement => completeAchievements;


    private QuestDatabase questDatabase;
    private QuestDatabase achievementDatabase;

    private void Awake()
    {
        questDatabase = Resources.Load<QuestDatabase>("Quest_Database");
        achievementDatabase = Resources.Load<QuestDatabase>("Achievement_Database");

        bool _isFirst = !Load(); // �ε��ϸ鼭 bool �� ����
        // �������� ���� ó�� ���� �� �� ����ص�
        if (_isFirst)
        {
            foreach (Quest _quest in achievementDatabase.Quests) 
                Register(_quest);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        Save();
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
            activeAchievements.Add(_newQuest);
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
        ReceiveReport(activeAchievements, _category, _target, _successCount);
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
        activeAchievements.Remove(_quest);
        completeAchievements.Add(_quest);
        OnAchievementCompleted?.Invoke(_quest);
    }
    #endregion



    #region quest�� �ִ��� Ȯ���ϴ� �Լ���
    public bool ContainsInActiveQuests(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteQuests(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInActiveAchievement(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteAchievement(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);
    #endregion



    #region Save And Load
    private void Save()
    {
        JObject _root = new JObject();
        _root.Add(key_ActiveQuestst, CreateSaveDatas(activeQuests));
        _root.Add(key_CompleteQuests, CreateSaveDatas(completeQuests));
        _root.Add(key_ActiveAchievement, CreateSaveDatas(activeAchievements));
        _root.Add(key_CompleteAchievement, CreateSaveDatas(completeAchievements));
        PlayerPrefs.SetString(key_SaveRoot, _root.ToString());
        PlayerPrefs.Save();
    }

    private bool Load()
    {
        if (PlayerPrefs.HasKey(key_SaveRoot))
        {
            JObject _root = JObject.Parse(PlayerPrefs.GetString(key_SaveRoot));

            LoadSaveDatas(_root[key_ActiveQuestst], questDatabase, LoadActiveQuest);
            LoadSaveDatas(_root[key_CompleteQuests], questDatabase, LoadCompletedQuest);

            LoadSaveDatas(_root[key_ActiveAchievement], achievementDatabase, LoadActiveQuest);
            LoadSaveDatas(_root[key_CompleteAchievement], achievementDatabase, LoadCompletedQuest);
            return true;
        }
        return false;
    }

    private JArray CreateSaveDatas(IReadOnlyList<Quest> _quests)
    {
        JArray _saveDatas = new JArray();
        foreach (Quest quest in _quests)
        {
            if(quest.IsSaveable)
                _saveDatas.Add(JObject.FromObject(quest.ToSaveData()));
        }

        return _saveDatas;
    }

    private void LoadSaveDatas(JToken _token, QuestDatabase _database, Action<QuestSaveData, Quest> OnSuccess)
    {
        JArray _datas = _token as JArray;
        foreach (var _data in _datas)
        {
            QuestSaveData _saveData = _data.ToObject<QuestSaveData>();
            Quest _quest = _database.FindQuestBy(_saveData.codeName);
            OnSuccess?.Invoke(_saveData, _quest);
        }
    }

    private void LoadActiveQuest(QuestSaveData _saveData, Quest _quest)
    {
        Quest _newQuest = Register(_quest);
        _newQuest.LoadFrom(_saveData);
    }

    private void LoadCompletedQuest(QuestSaveData _saveData, Quest _quest)
    {
        Quest _newQuest = _quest.Clone();
        _newQuest.LoadFrom(_saveData);

        if (_newQuest is Achievement) completeAchievements.Add(_newQuest);
        else completeQuests.Add(_newQuest);
    }
    #endregion
}

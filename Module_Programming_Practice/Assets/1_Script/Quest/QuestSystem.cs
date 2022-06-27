using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
    public IReadOnlyList<Quest> ActiveAchievements => activeAchievements;
    public IReadOnlyList<Quest> CompleteAchievements => completeAchievements;


    private QuestDatabase questDatabase;
    private QuestDatabase achievementDatabase;

    private void Awake()
    {
        questDatabase = Resources.Load<QuestDatabase>("Quest_Database");
        achievementDatabase = Resources.Load<QuestDatabase>("Achievement_Database");

        bool _isFirst = !Load(); // 로드하면서 bool 값 세팅
        // 업적들은 게임 처음 시작 시 다 등록해둠
        if (_isFirst)
        {
            foreach (Quest _quest in achievementDatabase.Quests) 
                Register(_quest);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        //Save();
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
        // 여기에서 하나하나 복사본을 만들기에는 너무 지저분하니 지들이 알아서 복사본을 만들어주는 함수를 사용하는 쪽으로 구현함
        // 이러면 후에 복사본 생성 내부 코드에 변화가 생겨도 QuestSystem은 수정할 필요가 없음
        Quest _newQuest = _quest.Clone();
        
        if(_newQuest is Achievement)
        {
            activeAchievements.Add(_newQuest);
            _newQuest.OnCompleted += Raise_OnAchievementCompleted;

            _newQuest.OnRegister();
            OnAchievementRegistered?.Invoke(_newQuest);
        }
        else
        {
            activeQuests.Add(_newQuest);
            _newQuest.OnCompleted += Raise_OnQuestCompleted;
            _newQuest.OnCanceled += Raise_OnQuestCanceled;

            _newQuest.OnRegister();
            OnQuestRegistered?.Invoke(_newQuest);
        }

        return _newQuest;
    }

    public void ReceiveReport(Category _category, TaskTarget _target, int _successCount)
        => ReceiveReport(_category.CodeName, _target.Value, _successCount);

    public void ReceiveReport(string _category, object _target, int _successCount)
    {
        ReceiveReport(activeQuests, _category, _target, _successCount);
        ReceiveReport(activeAchievements, _category, _target, _successCount);
    }

    // TODO : 리스트를 전부 돌리는 것이 아니라 해당되는 퀘스트만 돌리기
    private void ReceiveReport(List<Quest> _quests, string _category, object _target ,int _successCount)
    {
        // 리스트의 사본을 만들어 돌리는 이유는 complete 시 파괴되도록 이벤트에 등록해두어 반복문 중 quest가 없어지면서 에러가 날 수도 있어서
        foreach (Quest _quest in _quests.ToArray()) 
            _quest.ReceiveReport(_category, _target, _successCount);
    }

    public void CompleteWatingQuest()
    {
        foreach(Quest _quest in activeQuests.ToList())
        {
            if (_quest.IsCompletable)
                _quest.Complete();
        }
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
        activeAchievements.Remove(_quest);
        completeAchievements.Add(_quest);
        OnAchievementCompleted?.Invoke(_quest);
    }
    #endregion



    #region quest가 있는지 확인하는 함수들
    public bool ContainsInActiveQuests(Quest _quest) => activeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteQuests(Quest _quest) => completeQuests.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInActiveAchievement(Quest _quest) => activeAchievements.Any(x => x.CodeName == _quest.CodeName);

    public bool ContainsInCompleteAchievement(Quest _quest) => completeAchievements.Any(x => x.CodeName == _quest.CodeName);
    #endregion



    #region Save And Load
    [ContextMenu("Test")]
    public void Save()
    {
        JObject _root = new JObject();
        _root.Add(key_ActiveQuestst, CreateSaveDatas(activeQuests));
        _root.Add(key_CompleteQuests, CreateSaveDatas(completeQuests));
        _root.Add(key_ActiveAchievement, CreateSaveDatas(activeAchievements));
        _root.Add(key_CompleteAchievement, CreateSaveDatas(completeAchievements));

        PlayerPrefs.SetString(key_SaveRoot, _root.ToString());
        PlayerPrefs.Save(); // 원래는 Application Quit시에 자동으로 작동하지만 튕길 수도 있으니 여기에서 사용해줌

        print(_root.ToString());
        print(JsonConvert.SerializeObject(_root, Formatting.Indented));
        print(JsonConvert.SerializeObject( activeQuests.Select( x => x.ToSaveData() ).ToArray() , Formatting.Indented));
    }

    private bool Load()
    {
        // JObject는 Dictionary, JArray는 List 
        // Dictionary에 List넣을 수 있고 반대도 가능한 것처럼 json클래스들도 서로 넣는거 가능
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

    // 리스트만 할 때는 JsonConvert.SerializeObject()로 해도 잘 되는데 JObject나 JArray에 Add()하니까 이스케이프문이 너무 많이 들어가서 SerializeObject 안 쓰는 듯?
    // 솔직히 자세한거는 모름
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
            // 모든 Quest들이 저장되어 있는 DB에서 찾은 후 Action실행
            Quest _quest = _database.FindQuestBy(_saveData.codeName);
            OnSuccess?.Invoke(_saveData, _quest);
        }
    }

    private void LoadActiveQuest(QuestSaveData _saveData, Quest _quest)
    {
        // Register() 함수 내에서 업적이랑 퀘스트 구분 되므로 따로 조건문 안 넣어도 됨
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

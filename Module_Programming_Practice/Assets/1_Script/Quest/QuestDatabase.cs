using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Quest/Database")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField] List<Quest> quests;
    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string _codeName) => quests.FirstOrDefault(_quest => _quest.CodeName == _codeName);

    
    
    #if UNITY_EDITOR
    [ContextMenu("Find Quests")]
    private void FindQuests()
    {
        FindQuestsBy<Quest>();
    }

    [ContextMenu("Find Achievements")]
    private void FindAchievements()
    {
        FindQuestsBy<Achievement>();
    }

    public void FindQuestsBy<T>() where T : Quest
    {
        quests = new List<Quest>();
        // T type인 Asset들의 guid를 가져옴
        string[] _guids = AssetDatabase.FindAssets($"t:{typeof(T)}");

        foreach(string _guid in _guids)
        {
            string _assetPath = AssetDatabase.GUIDToAssetPath(_guid);
            Quest _quest = AssetDatabase.LoadAssetAtPath<T>(_assetPath);

            if (_quest.GetType() == typeof(T)) quests.Add(_quest);
            EditorUtility.SetDirty(this); // asset 저장 시 반영
            AssetDatabase.SaveAssets(); // asset 저장
        }
    }
    #endif
}

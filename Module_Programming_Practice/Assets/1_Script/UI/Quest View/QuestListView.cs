using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class QuestListView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI elementPrefab; // Quest 이름을 Text Button으로 보여줌
    Dictionary<Quest, GameObject> elementByQuest = new Dictionary<Quest, GameObject>();
    [SerializeField] ToggleGroup toggleGroup;

    public void AddElement(Quest _quest, UnityAction<bool> _OnCilcked)
    {
        TextMeshProUGUI _element = Instantiate(elementPrefab, transform);
        _element.text = _quest.DisplayName;
        SetToggle(_element.GetComponent<Toggle>());
        elementByQuest.Add(_quest, _element.gameObject);

        void SetToggle(Toggle toggle)
        {
            toggle.group = toggleGroup;
            toggle.onValueChanged.AddListener(_OnCilcked);
        }
    }

    public void RemoveElement(Quest _quest)
    {
        Destroy(elementByQuest[_quest]);
        elementByQuest.Remove(_quest);
    }
}

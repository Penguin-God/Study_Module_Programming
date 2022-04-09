using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskDescriptor : MonoBehaviour
{
    // TextMeshProUGUI : TextMeshPro 컴포넌트
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] Color normalColor;
    [SerializeField] Color taskSuccessCountColor;
    [SerializeField] Color taskCompleteColor;
    [SerializeField] Color strikeThroughColor; // 착선( 문자를 가로지르는 선, 나무위키에 많이 나오는 그거 )


    public void UpdateText(string _text)
    {
        text.fontStyle = FontStyles.Normal;
        text.text = _text;
    }

    public void UpdateText(Task _task)
    {
        if (!_task.IsComplete) BuildText(_task, ColorUtility.ToHtmlStringRGB(normalColor), ColorUtility.ToHtmlStringRGB(taskSuccessCountColor));
        else
        {
            string _completeColorCode = ColorUtility.ToHtmlStringRGB(taskCompleteColor);
            BuildText(_task, _completeColorCode, _completeColorCode);
        }
    }

    public void UpdateTextUsingStrikeThrough(Task _task)
    {
        text.fontStyle = FontStyles.Strikethrough;
        string _strikeThroughColorCode = ColorUtility.ToHtmlStringRGB(strikeThroughColor);
        BuildText(_task, _strikeThroughColorCode, _strikeThroughColorCode);
    }

    private string BuildText(Task _task, string _textColorCode, string _successCountColorCode)
        => $"<color=#{_textColorCode}>● {_task.Description} <color=#{_successCountColorCode}>{_task.CurrentSuccess} / </color>{_task.NeedSuccessToComplete}</color>";
    
}

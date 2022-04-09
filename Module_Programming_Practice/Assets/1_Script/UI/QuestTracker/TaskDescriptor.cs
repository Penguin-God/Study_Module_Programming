using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskDescriptor : MonoBehaviour
{
    // TextMeshProUGUI : TextMeshPro ������Ʈ
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] Color normalColor;
    [SerializeField] Color taskSuccessCountColor;
    [SerializeField] Color taskCompleteColor;
    [SerializeField] Color strikeThroughColor; // ����( ���ڸ� ���������� ��, ������Ű�� ���� ������ �װ� )


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
        => $"<color=#{_textColorCode}>�� {_task.Description} <color=#{_successCountColorCode}>{_task.CurrentSuccess} / </color>{_task.NeedSuccessToComplete}</color>";
    
}

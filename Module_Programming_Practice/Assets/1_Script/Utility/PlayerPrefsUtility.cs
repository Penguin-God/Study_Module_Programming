using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsUtility : MonoBehaviour
{
    [ContextMenu("Delete Save Data")]
    private void DeleteSaveData()
    {
        PlayerPrefs.DeleteAll();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingPanel : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void OpenSettings()
    {
        gameObject.SetActive(true);
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    public void LoadScene(Define.Scene type)
    {
        Debug.Log("Starting LoadScene");

        Managers.Clear();
        Debug.Log("Managers cleared");

        string sceneName = GetSceneName(type);
        Debug.Log("Scene name: " + sceneName);

        SceneManager.LoadScene(sceneName);
        Debug.Log("Scene loaded");
    }


    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}

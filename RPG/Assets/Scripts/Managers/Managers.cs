using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance; // 유일성이 보장된다
    public static Define.Scene NextScene = Define.Scene.Unknown;
    public static Action NextAction = null;
    public static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    #region Contents
    InventoryManager _inven = new InventoryManager();
    NetworkManager _network = new NetworkManager();
    WebManager _web = new WebManager();
    ObjectManager _obj = new ObjectManager();
    QuestManager _qst = new QuestManager();
    public static InventoryManager Inven { get { return Instance._inven; } }
    public static NetworkManager Network { get { return Instance._network; } }
	public static WebManager Web { get { return Instance._web; } }
    public static ObjectManager Object { get { return Instance._obj; } }
    public static QuestManager Quest { get { return Instance._qst; } }
    #endregion

    #region Core
    DataManager _data = new DataManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    UIManager _ui = new UIManager();

    public static DataManager Data { get { return Instance._data; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._ui; } }
    #endregion
    public float aspectWidth = 16f;
    public float aspectHeight = 9f;

    void UpdateResolution()
    {
        int width = Screen.width;
        int height = (int)(width / (aspectWidth / aspectHeight));
        Screen.SetResolution(width, height, false);
    }
    void Start()
    {
        Init();
        UpdateResolution();
    }

    void Update()
    {
        _network.Update();
        if (Screen.width != (int)(Screen.height * (aspectWidth / aspectHeight)))
        {
            UpdateResolution();
        }
    }

    static void Init()
    {
        if (s_instance == null)
        {
			GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._data.Init();
            s_instance._pool.Init();
            s_instance._sound.Init();
        }		
	}

    public static void Clear()
    {
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
    }
    private void OnApplicationQuit()
    {
        SaveGameData();
        if (Network.ServInfo == null) { return; }
        C_SaveQuickSlot saveSlot = new C_SaveQuickSlot();
        if ((UI.SceneUI as UI_GameScene) != null)
        {
            foreach (var slot in (UI.SceneUI as UI_GameScene).QuickSlotSkill)
            {
                QuickSlotInfo info = new QuickSlotInfo();
                info.SlotName = slot.Key;
                info.TemplateId = slot.Value;
                saveSlot.Info.Add(info);
            }
            foreach (var slot in (UI.SceneUI as UI_GameScene).QuickSlotItem)
            {
                QuickSlotInfo info = new QuickSlotInfo();
                info.SlotName = slot.Key;
                info.TemplateId = slot.Value;
                saveSlot.Info.Add(info);
            }
            Network.Send(saveSlot);
        }
        Network._session.Disconnect();
    }
    public void BgmSoundChange(AudioSource audioSource, AudioClip newClip, float pitch = 1.0f, float fadeTime = 1.0f)
    {
        StartCoroutine(FadeInNewBGM(audioSource, newClip, pitch, fadeTime));
    }
    IEnumerator FadeOutOldBGM(AudioSource audioSource, float fadeTime = 1.0f)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = startVolume * (1 - t / fadeTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;  // reset volume
    }
    IEnumerator FadeInNewBGM(AudioSource audioSource, AudioClip newClip, float pitch = 1.0f, float fadeTime = 1.0f)
    {
        yield return StartCoroutine(FadeOutOldBGM(audioSource, fadeTime));

        audioSource.clip = newClip;
        audioSource.pitch = pitch;
        audioSource.Play();
        audioSource.volume = 0;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = t / fadeTime;
            if (audioSource.volume >= data.bgmVolume)
                break;
            yield return null;
        }

        audioSource.volume = data.bgmVolume;  
    }

    string GameDataFileName = "SoundData.json";
    public SoundData data = new SoundData();
    public void LoadGameData()
    {
        string filePath = Application.persistentDataPath + "/" + GameDataFileName;
        if (File.Exists(filePath))
        {
            string FromJsonData = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<SoundData>(FromJsonData);
        }
    }
    public void SaveGameData()
    {
        string ToJsonData = JsonUtility.ToJson(data, true);
        string filePath = Application.persistentDataPath + "/" + GameDataFileName;

        File.WriteAllText(filePath, ToJsonData);
    }
}

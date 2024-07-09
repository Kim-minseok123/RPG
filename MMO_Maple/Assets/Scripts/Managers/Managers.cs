using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance; // 유일성이 보장된다
    public static Managers Instance { get { Init(); return s_instance; } } // 유일한 매니저를 갖고온다

    #region Contents
    InventoryManager _inven = new InventoryManager();
    NetworkManager _network = new NetworkManager();
    WebManager _web = new WebManager();
    ObjectManager _obj = new ObjectManager();
    public static InventoryManager Inven { get { return Instance._inven; } }
    public static NetworkManager Network { get { return Instance._network; } }
	public static WebManager Web { get { return Instance._web; } }
    public static ObjectManager Object { get { return Instance._obj; } }
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

	void Start()
    {
        Init();
	}

    void Update()
    {
        _network.Update();
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
}

using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class ObjectManager 
{
    public MyPlayerController MyPlayer { get; set; }
    public int ClassType { get; set; }
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        if (MyPlayer != null && MyPlayer.Id == info.ObjectId)
            return;
        if (_objects.ContainsKey(info.ObjectId)) return;
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                GameObject go = Managers.Resource.Instantiate("Creature/Player/MyPlayer");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.ClassType = ClassType;
                MyPlayer.Id = info.ObjectId;
                MyPlayer.SetInfo(info);
                MyPlayer.SetPos(info.PosInfo.Pos, info.PosInfo.Rotate);
                MyPlayer.RefreshAdditionalStat();
                MyPlayer.GetComponent<NavMeshAgent>().enabled = true;
                if(Managers.UI.SceneUI == null)
                    Managers.UI.ShowSceneUI<UI_GameScene>();
                else
                {
                    UI_GameScene ui = Managers.UI.SceneUI as UI_GameScene;
                    ui.RefreshUI();
                }
                MyPlayer.GetComponent<AudioListener>().enabled = true;
                if (Camera.main.GetComponent<AudioListener>() != null)
                {
                    Camera.main.GetComponent<AudioListener>().enabled = false;
                }
#if UNITY_SERVER
                Managers.Quest.Init();
#endif
                Quest quest = Quest.MakeQuest(2);
                Managers.Quest.FinishQuest(quest);
            }
            else
            {
                GameObject go = Managers.Resource.Instantiate("Creature/Player/Player");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.SetInfo(info);
                pc.SetPos(info.PosInfo.Pos, info.PosInfo.Rotate);
                NavMeshAgent agent = pc.GetComponent<NavMeshAgent>();

                if(!agent.isOnNavMesh)
                {
                    C_RequestLeaveGame requestLeaveGame = new C_RequestLeaveGame();
                    requestLeaveGame.ObjectId = info.ObjectId;
                    Managers.Network.Send(requestLeaveGame);
                    return;
                }

                agent.enabled = true;
            }
        }
        else if (objectType == GameObjectType.Monster)
        {
            GameObject go = Managers.Resource.Instantiate($"Creature/Monster/{info.Name}");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            MonsterController mc = go.GetComponent<MonsterController>();
            mc.Id = info.ObjectId;
            mc.SetInfo(info);
            mc.SetPos(info.PosInfo.Pos, info.PosInfo.Rotate);
            if(mc.GetComponent<NavMeshAgent>() != null)
                mc.GetComponent<NavMeshAgent>().enabled = true;
        }
        
        else if (objectType == GameObjectType.Dropitem)
        {
            GameObject go = Managers.Resource.Instantiate($"Item/DropItem/{info.Name}");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            DropItem dropItem = go.GetComponent<DropItem>();
            dropItem.Id = info.ObjectId;
            dropItem.itemName = info.Name;
            dropItem.SetPos(info.PosInfo.Pos);
        }
        else if(objectType == GameObjectType.Npc)
        {
            GameObject go = Managers.Resource.Instantiate($"Creature/NPC/{info.Name}");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            NPCController npcController = go.GetComponent<NPCController>();
            npcController.Id = info.ObjectId;
            npcController.SetPos(info.PosInfo.Pos, info.PosInfo.Rotate);
        }
    }

    public void Remove(int id, bool isDestroy = true)
    {
        if (MyPlayer != null && MyPlayer.Id == id)
            return;

        if (_objects.ContainsKey(id) == false) return;

        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        if(isDestroy)
            Managers.Resource.Destroy(go);
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject FindCreature(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objects.Values)
        {
            CreatureController cc = obj.GetComponent<CreatureController>();
            if (cc == null)
                continue;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objects.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Managers.Resource.Destroy(obj);
        _objects.Clear();
        MyPlayer = null;
        if (Camera.main.GetComponent<AudioListener>() != null)
        {
            Camera.main.GetComponent<AudioListener>().enabled = true;
        }
    }
}

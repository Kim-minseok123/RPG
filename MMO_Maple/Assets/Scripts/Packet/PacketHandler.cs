using EasyTransition;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
        Managers.Network.Send(pongPacket);
    }
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();
        loginPacket.AccountId = Managers.Network.AccountId;
        loginPacket.Token = Managers.Network.Token;
        Managers.Network.Send(loginPacket);
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
        Managers.Object.Clear();
    }
    public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = (S_Login)packet;
		Debug.Log($"LoginOk({loginPacket.LoginOk})");

        var lobbyPlayers = loginPacket.Players;

        foreach (var player in lobbyPlayers)
        {
            Managers.Network.LobbyPlayerInfos.Add(player);
        }
#if UNITY_SERVER
        C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
        createPlayerPacket.Name = "Master";
        createPlayerPacket.IsMale = true;
        Managers.Network.Send(createPlayerPacket);

#else
        TransitionSettings ts = Managers.Resource.Load<TransitionSettings>("Trans/LinearWipe");
        TransitionManager.Instance().Transition(Define.Scene.Lobby, ts, 0);
        Managers.UI.CloseAllPopupUI();
#endif
    }
    public static void S_BanishHandler(PacketSession session, IMessage packet)
    {
        Managers.Network._session.Disconnect();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;
#if UNITY_SERVER
            Managers.Scene.LoadScene(Define.Scene.Game);
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = "Master";
            Managers.Network.Send(enterGamePacket);
#else
        if (createOkPacket.Player == null)
        {
            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("닉네임이 중복됩니다.\n 다른 닉네임을 사용해주세요.");
        }
        else
        {
            Managers.Network.LobbyPlayerInfos.Add(createOkPacket.Player);
            Managers.UI.FindPopupUI<UI_CreatePlayer_Popup>().EndCreate();
        }
#endif

    }
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = (S_EnterGame)packet;
        
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = (S_Spawn)packet;
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
        }
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = (S_Move)packet;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        GameObject mosterTarget = null;
        if (movePacket.TargetId != -1) mosterTarget = Managers.Object.FindById(movePacket.TargetId);
        Vector3 target = new Vector3(movePacket.DestPosInfo.Pos.PosX, movePacket.DestPosInfo.Pos.PosY, movePacket.DestPosInfo.Pos.PosZ);
        bc.MoveTarget(target, mosterTarget);
    }
    public static void S_StopMoveHandler(PacketSession session, IMessage packet)
    {
        S_StopMove stopMovePacket = (S_StopMove)packet;

        GameObject go = Managers.Object.FindById(stopMovePacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        if (stopMovePacket.PosOk == false) return;
        bc.StopMove(
            new Vector3(stopMovePacket.Rotate.RotateX, stopMovePacket.Rotate.RotateY, stopMovePacket.Rotate.RotateZ),
            new Vector3(stopMovePacket.Pos.PosX, stopMovePacket.Pos.PosY, stopMovePacket.Pos.PosZ)
            );
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changeHpPacket = (S_ChangeHp)packet;

        GameObject go = Managers.Object.FindById(changeHpPacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        bc.ChangeHp(changeHpPacket.Hp, changeHpPacket.IsHeal, changeHpPacket.ChangeHp);
    }
    public static void S_SkillMotionHandler(PacketSession session, IMessage packet)
    {
        S_SkillMotion skillMotionPacket = (S_SkillMotion)packet;   

        GameObject go = Managers.Object.FindById(skillMotionPacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        bc.OnAttack(skillMotionPacket.Info);
    }
    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem itemList = (S_AddItem)packet;

        // 메모리에 아이템 정보 적용
        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Add(item);
        }
        if (itemList.Money != 0)
            Managers.Inven.AddMoney(itemList.Money);
        Debug.Log("아이템을 획득했습니다!");

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();

        /*if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();*/
    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePakcet = (S_Die)packet;

        GameObject go = Managers.Object.FindById(diePakcet.ObjectId);
        GameObject attacker = Managers.Object.FindById(diePakcet.AttackerId);
        if (go == null || attacker == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        bc.OnDead(attacker);
    }
    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;

        Managers.Inven.Clear();

        // 메모리에 아이템 정보 적용
        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            if(item.Equipped == true)
            {
                Managers.Inven.EquipAdd(item.Slot, item);
            }
            else
            {
                Managers.Inven.Add(item);
            }
        }
        
        Managers.Inven.Money = itemList.Money;
        /*if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();*/
    }
    public static void S_GetDropItemMotionHandler(PacketSession session, IMessage packet)
    {
        S_GetDropItemMotion motionPacket = (S_GetDropItemMotion)packet;

        GameObject go = Managers.Object.FindById(motionPacket.ObjectId);
        if (go == null)
            return;
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;
        pc.PickUpItem();
    }
    public static void S_EquipItemListHandler(PacketSession session, IMessage packet)
    {
        S_EquipItemList equipItemList = (S_EquipItemList)packet;

        GameObject go = Managers.Object.FindById(equipItemList.ObjectId);
        if (go == null) return;
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;
        foreach (var id in equipItemList.TemplateIds)
        {
            pc.EquipItem(id);
        }
    }
}



using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

public class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
        session.Disconnect();
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changePacket = packet as S_ChangeHp;
    }

    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;
    }

    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();

        ServerSession serverSession = (ServerSession)session;
        loginPacket.AccountId = 1000 + serverSession.DummyId;
        //loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
        serverSession.Send(loginPacket);
    }

    // 로그인 OK + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        ServerSession serverSession = (ServerSession)session;

        // TODO : 로비 UI에서 캐릭터 보여주고, 선택할 수 있도록
        if (loginPacket.LoginOk == 1)
        {
            C_EnterGame enterDummy = new C_EnterGame();
            enterDummy.Name = $"Dummy_{serverSession.DummyId.ToString("0000")}";
            serverSession.Send(enterDummy);
        }
    }
    
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;
        ServerSession serverSession = (ServerSession)session;

        if (createOkPacket.Player == null)
        {
           
        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPacket.Player.Player.Name;
            serverSession.Send(enterGamePacket);
        }
    }
    public static void S_BanishHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;
        ServerSession serverSession = (ServerSession)session;

        if (createOkPacket.Player == null)
        {

        }
        else
        {
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = createOkPacket.Player.Player.Name;
            serverSession.Send(enterGamePacket);
        }
    }
    public static void S_StopMoveHandler(PacketSession session, IMessage packet)
    {
        
    }
    public static void S_SkillMotionHandler(PacketSession session, IMessage packet)
    {
        
    }
    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;
    }

    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem itemList = (S_AddItem)packet;
    }
    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItemOk = (S_EquipItem)packet;
    }
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat itemList = (S_ChangeStat)packet;

    }
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        S_Ping pingPacket = new S_Ping();
    }

    public static void S_MotionOrEffectHandler(PacketSession session, IMessage packet)
    {
        S_MotionOrEffect getDropItemMotionPacket = new S_MotionOrEffect();
    }
    public static void S_EquipItemListHandler(PacketSession session, IMessage packet)
    {
        S_EquipItemList equipItemListPacket = new S_EquipItemList();
    }
    public static void S_ChangeConsumableItemHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_UseItemHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_SkillListHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_SkillLevelUpHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_QuickSlotHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_ChangeItemSlotHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_MessageHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_ChangeMapHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_SetMasterClientHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_MakeMeteorObjectHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_BossItemCutSceneHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_EndBossItemCutSceneHandler(PacketSession session, IMessage packet)
    {

    }
}
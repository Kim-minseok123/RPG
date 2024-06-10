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
        S_Skill skillPacket = packet as S_Skill;
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
        //loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
        serverSession.Send(loginPacket);
    }

    // 로그인 OK + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;
        ServerSession serverSession = (ServerSession)session;

        // TODO : 로비 UI에서 캐릭터 보여주고, 선택할 수 있도록
        if (loginPacket.Players == null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new C_CreatePlayer();
            createPacket.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
            serverSession.Send(createPacket);
        }
        else
        {
            // 무조건 첫번째 로그인
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = info.Name;
            serverSession.Send(enterGamePacket);
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
            enterGamePacket.Name = createOkPacket.Player.Name;
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
            enterGamePacket.Name = createOkPacket.Player.Name;
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

    public static void S_GetDropItemMotionHandler(PacketSession session, IMessage packet)
    {
        S_GetDropItemMotion getDropItemMotionPacket = new S_GetDropItemMotion();
    }
    public static void S_EquipItemListHandler(PacketSession session, IMessage packet)
    {
        S_EquipItemList equipItemListPacket = new S_EquipItemList();
    }
}
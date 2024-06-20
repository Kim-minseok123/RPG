using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

class PacketHandler
{
    public static void C_PongHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandlePong();
    }
    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;
        clientSession.HandleLogin(loginPacket);
    }
    public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        C_CreatePlayer createPlayerPacket = (C_CreatePlayer)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleCreatePlayer(createPlayerPacket);
    }
    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterGamePacket = (C_EnterGame)packet;
        ClientSession clientSession = (ClientSession)session;
        clientSession.HandleEnterGame(enterGamePacket);
    }
    public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleMove, player, movePacket);
	}
    public static void C_StopMoveHandler(PacketSession session, IMessage packet)
    {
        C_StopMove stopMovePacket = (C_StopMove)packet;
        ClientSession clientSession = (ClientSession)session;
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;
        if (stopMovePacket.IsMonster == false)
        {
            room.Push(room.HandleStopMove, player, stopMovePacket);
        }
        else if(stopMovePacket.IsMonster == true) 
        {
            room.Push(room.HandleStopMoveMonster, stopMovePacket);
        }
    }
    public static void C_CheckPosHandler(PacketSession session, IMessage packet)
    {
        C_CheckPos checkPosPacket = (C_CheckPos)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleCheckPos, player, checkPosPacket);
    }
    public static void C_SkillMotionHandler(PacketSession session, IMessage packet)
    {
        C_SkillMotion skillMotionPacket = (C_SkillMotion)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleSkillMotion, player, skillMotionPacket);
    }
    public static void C_MeleeAttackHandler(PacketSession session, IMessage packet)
    {
        C_MeleeAttack meleeAttackPacket = (C_MeleeAttack)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleMeleeAttack, player, meleeAttackPacket);
    }
    public static void C_SkillHandler(PacketSession session, IMessage packet)
	{
		
	}

	public static void C_EquipItemHandler(PacketSession session, IMessage packet)
	{
		C_EquipItem equipPacket = (C_EquipItem)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleEquipItem, player, equipPacket);
	}
    public static void C_GetDropItemHandler(PacketSession session, IMessage packet)
    {
        C_GetDropItem dropItemPacket = (C_GetDropItem)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.GetDropItem, player, dropItemPacket);
    }
    public static void C_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        C_ChangeStat changeStatPacket = (C_ChangeStat)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.StatChange, player, changeStatPacket);
    }
    public static void C_UseItemHandler(PacketSession session, IMessage packet)
    {
        C_UseItem useItemPacket = (C_UseItem)packet;
        ClientSession clientSession= (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if(room == null) return;

        room.Push(room.HandleUseItem, player, useItemPacket);
    }
    public static void C_SkillLevelUpHandler(PacketSession session, IMessage packet)
    {
        C_SkillLevelUp skillLevelUpPacket = (C_SkillLevelUp)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null) return;

        room.Push(room.HandleSkillLevelUp, player, skillLevelUpPacket);
    }
    public static void C_SaveQuickSlotHandler(PacketSession session, IMessage packet)
    {
        C_SaveQuickSlot quickSlotPacket = (C_SaveQuickSlot)packet;
        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null) return;

        room.Push(room.HandleSaveQuickSlot, player, quickSlotPacket);
    }
}

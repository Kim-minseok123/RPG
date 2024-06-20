using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CPong, MakePacket<C_Pong>);
		_handler.Add((ushort)MsgId.CPong, PacketHandler.C_PongHandler);		
		_onRecv.Add((ushort)MsgId.CLogin, MakePacket<C_Login>);
		_handler.Add((ushort)MsgId.CLogin, PacketHandler.C_LoginHandler);		
		_onRecv.Add((ushort)MsgId.CCreatePlayer, MakePacket<C_CreatePlayer>);
		_handler.Add((ushort)MsgId.CCreatePlayer, PacketHandler.C_CreatePlayerHandler);		
		_onRecv.Add((ushort)MsgId.CEnterGame, MakePacket<C_EnterGame>);
		_handler.Add((ushort)MsgId.CEnterGame, PacketHandler.C_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
		_handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);		
		_onRecv.Add((ushort)MsgId.CStopMove, MakePacket<C_StopMove>);
		_handler.Add((ushort)MsgId.CStopMove, PacketHandler.C_StopMoveHandler);		
		_onRecv.Add((ushort)MsgId.CCheckPos, MakePacket<C_CheckPos>);
		_handler.Add((ushort)MsgId.CCheckPos, PacketHandler.C_CheckPosHandler);		
		_onRecv.Add((ushort)MsgId.CMeleeAttack, MakePacket<C_MeleeAttack>);
		_handler.Add((ushort)MsgId.CMeleeAttack, PacketHandler.C_MeleeAttackHandler);		
		_onRecv.Add((ushort)MsgId.CSkillMotion, MakePacket<C_SkillMotion>);
		_handler.Add((ushort)MsgId.CSkillMotion, PacketHandler.C_SkillMotionHandler);		
		_onRecv.Add((ushort)MsgId.CGetDropItem, MakePacket<C_GetDropItem>);
		_handler.Add((ushort)MsgId.CGetDropItem, PacketHandler.C_GetDropItemHandler);		
		_onRecv.Add((ushort)MsgId.CEquipItem, MakePacket<C_EquipItem>);
		_handler.Add((ushort)MsgId.CEquipItem, PacketHandler.C_EquipItemHandler);		
		_onRecv.Add((ushort)MsgId.CChangeStat, MakePacket<C_ChangeStat>);
		_handler.Add((ushort)MsgId.CChangeStat, PacketHandler.C_ChangeStatHandler);		
		_onRecv.Add((ushort)MsgId.CUseItem, MakePacket<C_UseItem>);
		_handler.Add((ushort)MsgId.CUseItem, PacketHandler.C_UseItemHandler);		
		_onRecv.Add((ushort)MsgId.CSkillLevelUp, MakePacket<C_SkillLevelUp>);
		_handler.Add((ushort)MsgId.CSkillLevelUp, PacketHandler.C_SkillLevelUpHandler);		
		_onRecv.Add((ushort)MsgId.CSaveQuickSlot, MakePacket<C_SaveQuickSlot>);
		_handler.Add((ushort)MsgId.CSaveQuickSlot, PacketHandler.C_SaveQuickSlotHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}
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
		_onRecv.Add((ushort)MsgId.SPing, MakePacket<S_Ping>);
		_handler.Add((ushort)MsgId.SPing, PacketHandler.S_PingHandler);		
		_onRecv.Add((ushort)MsgId.SConnected, MakePacket<S_Connected>);
		_handler.Add((ushort)MsgId.SConnected, PacketHandler.S_ConnectedHandler);		
		_onRecv.Add((ushort)MsgId.SLeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)MsgId.SLeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)MsgId.SLogin, MakePacket<S_Login>);
		_handler.Add((ushort)MsgId.SLogin, PacketHandler.S_LoginHandler);		
		_onRecv.Add((ushort)MsgId.SBanish, MakePacket<S_Banish>);
		_handler.Add((ushort)MsgId.SBanish, PacketHandler.S_BanishHandler);		
		_onRecv.Add((ushort)MsgId.SCreatePlayer, MakePacket<S_CreatePlayer>);
		_handler.Add((ushort)MsgId.SCreatePlayer, PacketHandler.S_CreatePlayerHandler);		
		_onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
		_handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)MsgId.SStopMove, MakePacket<S_StopMove>);
		_handler.Add((ushort)MsgId.SStopMove, PacketHandler.S_StopMoveHandler);		
		_onRecv.Add((ushort)MsgId.SChangeHp, MakePacket<S_ChangeHp>);
		_handler.Add((ushort)MsgId.SChangeHp, PacketHandler.S_ChangeHpHandler);		
		_onRecv.Add((ushort)MsgId.SSkillMotion, MakePacket<S_SkillMotion>);
		_handler.Add((ushort)MsgId.SSkillMotion, PacketHandler.S_SkillMotionHandler);		
		_onRecv.Add((ushort)MsgId.SAddItem, MakePacket<S_AddItem>);
		_handler.Add((ushort)MsgId.SAddItem, PacketHandler.S_AddItemHandler);		
		_onRecv.Add((ushort)MsgId.SDie, MakePacket<S_Die>);
		_handler.Add((ushort)MsgId.SDie, PacketHandler.S_DieHandler);		
		_onRecv.Add((ushort)MsgId.SItemList, MakePacket<S_ItemList>);
		_handler.Add((ushort)MsgId.SItemList, PacketHandler.S_ItemListHandler);		
		_onRecv.Add((ushort)MsgId.SMotionOrEffect, MakePacket<S_MotionOrEffect>);
		_handler.Add((ushort)MsgId.SMotionOrEffect, PacketHandler.S_MotionOrEffectHandler);		
		_onRecv.Add((ushort)MsgId.SEquipItemList, MakePacket<S_EquipItemList>);
		_handler.Add((ushort)MsgId.SEquipItemList, PacketHandler.S_EquipItemListHandler);		
		_onRecv.Add((ushort)MsgId.SEquipItem, MakePacket<S_EquipItem>);
		_handler.Add((ushort)MsgId.SEquipItem, PacketHandler.S_EquipItemHandler);		
		_onRecv.Add((ushort)MsgId.SChangeStat, MakePacket<S_ChangeStat>);
		_handler.Add((ushort)MsgId.SChangeStat, PacketHandler.S_ChangeStatHandler);		
		_onRecv.Add((ushort)MsgId.SChangeConsumableItem, MakePacket<S_ChangeConsumableItem>);
		_handler.Add((ushort)MsgId.SChangeConsumableItem, PacketHandler.S_ChangeConsumableItemHandler);		
		_onRecv.Add((ushort)MsgId.SUseItem, MakePacket<S_UseItem>);
		_handler.Add((ushort)MsgId.SUseItem, PacketHandler.S_UseItemHandler);		
		_onRecv.Add((ushort)MsgId.SSkillList, MakePacket<S_SkillList>);
		_handler.Add((ushort)MsgId.SSkillList, PacketHandler.S_SkillListHandler);		
		_onRecv.Add((ushort)MsgId.SSkillLevelUp, MakePacket<S_SkillLevelUp>);
		_handler.Add((ushort)MsgId.SSkillLevelUp, PacketHandler.S_SkillLevelUpHandler);		
		_onRecv.Add((ushort)MsgId.SQuickSlot, MakePacket<S_QuickSlot>);
		_handler.Add((ushort)MsgId.SQuickSlot, PacketHandler.S_QuickSlotHandler);		
		_onRecv.Add((ushort)MsgId.SChangeItemSlot, MakePacket<S_ChangeItemSlot>);
		_handler.Add((ushort)MsgId.SChangeItemSlot, PacketHandler.S_ChangeItemSlotHandler);		
		_onRecv.Add((ushort)MsgId.SRemoveItem, MakePacket<S_RemoveItem>);
		_handler.Add((ushort)MsgId.SRemoveItem, PacketHandler.S_RemoveItemHandler);		
		_onRecv.Add((ushort)MsgId.SMessage, MakePacket<S_Message>);
		_handler.Add((ushort)MsgId.SMessage, PacketHandler.S_MessageHandler);		
		_onRecv.Add((ushort)MsgId.SChangeMap, MakePacket<S_ChangeMap>);
		_handler.Add((ushort)MsgId.SChangeMap, PacketHandler.S_ChangeMapHandler);		
		_onRecv.Add((ushort)MsgId.SSetMasterClient, MakePacket<S_SetMasterClient>);
		_handler.Add((ushort)MsgId.SSetMasterClient, PacketHandler.S_SetMasterClientHandler);		
		_onRecv.Add((ushort)MsgId.SMakeMeteorObject, MakePacket<S_MakeMeteorObject>);
		_handler.Add((ushort)MsgId.SMakeMeteorObject, PacketHandler.S_MakeMeteorObjectHandler);		
		_onRecv.Add((ushort)MsgId.SBossItemCutScene, MakePacket<S_BossItemCutScene>);
		_handler.Add((ushort)MsgId.SBossItemCutScene, PacketHandler.S_BossItemCutSceneHandler);		
		_onRecv.Add((ushort)MsgId.SEndBossItemCutScene, MakePacket<S_EndBossItemCutScene>);
		_handler.Add((ushort)MsgId.SEndBossItemCutScene, PacketHandler.S_EndBossItemCutSceneHandler);		
		_onRecv.Add((ushort)MsgId.SAddQuest, MakePacket<S_AddQuest>);
		_handler.Add((ushort)MsgId.SAddQuest, PacketHandler.S_AddQuestHandler);		
		_onRecv.Add((ushort)MsgId.SClearQuest, MakePacket<S_ClearQuest>);
		_handler.Add((ushort)MsgId.SClearQuest, PacketHandler.S_ClearQuestHandler);		
		_onRecv.Add((ushort)MsgId.SQuestChangeValue, MakePacket<S_QuestChangeValue>);
		_handler.Add((ushort)MsgId.SQuestChangeValue, PacketHandler.S_QuestChangeValueHandler);		
		_onRecv.Add((ushort)MsgId.SAllQuestList, MakePacket<S_AllQuestList>);
		_handler.Add((ushort)MsgId.SAllQuestList, PacketHandler.S_AllQuestListHandler);		
		_onRecv.Add((ushort)MsgId.SChatting, MakePacket<S_Chatting>);
		_handler.Add((ushort)MsgId.SChatting, PacketHandler.S_ChattingHandler);
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
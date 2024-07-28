using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class NetworkManager
{
	public int AccountId { get; set; }
	public int Token { get; set; }
	public List<LobbyPlayer> LobbyPlayerInfos { get; set; } = new List<LobbyPlayer>();
	public ServerSession _session { get; private set; } = new ServerSession();
	public ServerInfo ServInfo { get; private set; }
	public bool Master { get; set; }
	public void Send(IMessage packet)
	{
		_session.Send(packet);
	}

	public void ConnectToGame(ServerInfo info)
	{
        ServInfo = info;
		IPAddress ipAddr = IPAddress.Parse(info.IpAddress);
		IPEndPoint endPoint = new IPEndPoint(ipAddr, info.Port);

		Connector connector = new Connector();

		connector.Connect(endPoint,
			() => { return _session; },
			1);
	}

	public void Update()
	{
		List<PacketMessage> list = PacketQueue.Instance.PopAll();
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}	
	}
	public void Clear()
	{
		_session = new ServerSession();
        LobbyPlayerInfos = new List<LobbyPlayer>();
        ServInfo = null;
    }
}

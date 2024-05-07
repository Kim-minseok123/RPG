using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CreateAccountPacketReq
{
    public string AccountName;
    public string Passwrod;
}

public class CreateAccountPacketRes
{
    public bool CreateOk;
}

public class LoginAccountPacketReq
{
    public string AccountName;
    public string Passwrod;
}
public class ServerInfo
{
    public string Name;
    public string IpAddress;
    public int Port;
    public int BusyScore;
}
public class LoginAccountPacketRes
{
    public bool LoginOk;
    public int AccountId;
    public int Token;
    public List<ServerInfo> ServerList  = new List<ServerInfo>();
}
public class WebPacket
{
    public static void SendCreateAccount(string account, string password)
    {
        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Passwrod = password
        };
        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, res =>
        {
            Debug.Log(res.CreateOk);
        });
    }
    public static void SendLoginAccount(string account, string password)
    {
        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Passwrod = password
        };
        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, res =>
        {
            Debug.Log(res.LoginOk);
        });
    }
}
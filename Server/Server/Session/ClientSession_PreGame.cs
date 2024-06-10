using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public int AccountDbId { get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

		public void HandleLogin(C_Login loginPacket)
		{
			// 현재 플레이어가 로그인 상태에서만 로그인하는지 확인
			if (ServerState != PlayerServerState.ServerStateLogin)
			{
				S_Banish banPacket = new S_Banish();
				Send(banPacket);
				return;
			}
			// 로그인 토큰 확인
			using (SharedDbContext db = new SharedDbContext())
			{
				TokenDb findToken = db.Tokens.Where(a => a.AccountDbId == loginPacket.AccountId).FirstOrDefault();
				if(findToken == null)
				{
                    S_Banish banPacket = new S_Banish();
                    Send(banPacket);
                    return;
                }
				else
				{
					if(DateTime.Compare(DateTime.UtcNow, findToken.Expired) > 0)
					{
                        S_Banish banPacket = new S_Banish();
                        Send(banPacket);
                        return;
                    }
				}
			}

			LobbyPlayers.Clear();

			using (AppDbContext db = new AppDbContext())
			{
				// 이 서버에 계정이 있는지 확인
				AccountDb findAccount = db.Accounts
					.Include(a => a.Players)
					.Where(a => a.AccountLoginId == loginPacket.AccountId).FirstOrDefault();
				// 계정이 있다.
				if (findAccount != null)
				{
					// AccountDbId 메모리에 기억
					AccountDbId = findAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					foreach (PlayerDb playerDb in findAccount.Players)
					{
						LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
						{
							PlayerDbId = playerDb.PlayerDbId,
							Name = playerDb.PlayerName,
							Level = playerDb.Level,
							ClassType = (int)playerDb.PlayerClass,
						};

						// 메모리에도 들고 있다
						LobbyPlayers.Add(lobbyPlayer);

						// 패킷에 넣어준다
						loginOk.Players.Add(lobbyPlayer);
					}

					Send(loginOk);
					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
				else
				{
					AccountDb newAccount = new AccountDb() { AccountLoginId = loginPacket.AccountId };
					db.Accounts.Add(newAccount);
					bool success = db.SaveChangesEx();
					if (success == false)
						return;

					// AccountDbId 메모리에 기억
					AccountDbId = newAccount.AccountDbId;

					S_Login loginOk = new S_Login() { LoginOk = 1 };
					Send(loginOk);
					// 로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
			}
		}

		public void HandleEnterGame(C_EnterGame enterGamePacket)
		{
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;
			if(enterGamePacket.Name == "Master")
			{
				Master = true;
            }
			
            LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if (playerInfo == null)
                return;
			if(playerInfo.ClassType == (int)ClassTypes.Beginner)
			{
                MyPlayer = ObjectManager.Instance.Add<Beginner>();
				MyPlayer.classType = ClassTypes.Beginner;
            }
			else if (playerInfo.ClassType == (int)ClassTypes.Warrior)
			{
                MyPlayer = ObjectManager.Instance.Add<Warrior>();
				MyPlayer.classType = ClassTypes.Warrior;
            }

            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;

                MyPlayer.Session = this;

                //S_ItemList itemListPacket = new S_ItemList();

                // 아이템 목록을 갖고 온다
                using (AppDbContext db = new AppDbContext())
                {
                    PlayerDb findPlayerDb = db.Players.Where(p => p.PlayerName == playerInfo.Name).FirstOrDefault();
                    MyPlayer.Info.PosInfo.Rotate.RotateY = findPlayerDb.rotateY;
                    MyPlayer.Info.PosInfo.Pos.PosX = findPlayerDb.posX;
                    MyPlayer.Info.PosInfo.Pos.PosY = findPlayerDb.posY;
                    MyPlayer.Info.PosInfo.Pos.PosZ = findPlayerDb.posZ;
					MyPlayer.Inven.Money = findPlayerDb.Money;
                    {
                        MyPlayer.Stat.Level = findPlayerDb.Level;
                        MyPlayer.Stat.Hp = findPlayerDb.Hp;
                        MyPlayer.Stat.MaxHp = findPlayerDb.MaxHp;
                        MyPlayer.Stat.Mp = findPlayerDb.Mp;
                        MyPlayer.Stat.MaxMp = findPlayerDb.MaxMp;
                        MyPlayer.Stat.Defense = findPlayerDb.Defense;
                        MyPlayer.Stat.Speed = findPlayerDb.Speed;
                        MyPlayer.Stat.Str = findPlayerDb.Str;
                        MyPlayer.Stat.Dex = findPlayerDb.Dex;
                        MyPlayer.Stat.Luk = findPlayerDb.Luk;
                        MyPlayer.Stat.Int = findPlayerDb.Int;
                        MyPlayer.Stat.Exp = findPlayerDb.Exp;
                        MyPlayer.Stat.StatPoint = findPlayerDb.StatPoint;
                    }

					List<ItemDb> items = db.Items
						.Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
						.ToList();
					S_ItemList itemListPacket = new S_ItemList();

                    foreach (ItemDb itemDb in items)
					{
						Item item = Item.MakeItem(itemDb);
						if (item != null)
						{
							if(item.Equipped == true)
							{
								MyPlayer.Inven.EquipAdd(item.Slot, item);
							}
							else
							{
                                MyPlayer.Inven.Add(item);
                            }
                            ItemInfo info = new ItemInfo();
							info.MergeFrom(item.Info);
							itemListPacket.Items.Add(info);
						}
					}
					itemListPacket.Money = findPlayerDb.Money;
                    Send(itemListPacket);
                }
            }

			MyPlayer.RefreshAdditionalStat();

            ServerState = PlayerServerState.ServerStateGame;

			GameLogic.Instance.Push(() =>
			{
				GameRoom room = GameLogic.Instance.Find(1);
				room.Push(room.EnterGame, MyPlayer);
			});
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
		{
			// TODO : 이런 저런 보안 체크
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			using (AppDbContext db = new AppDbContext())
			{
				PlayerDb findPlayer = db.Players
					.Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

				if (findPlayer != null)
				{
					// 이름이 겹친다
					Send(new S_CreatePlayer());
				}
				else
				{
					// DB에 플레이어 만들어줘야 함
					PlayerDb newPlayerDb = new PlayerDb()
					{
						PlayerName = createPacket.Name,
						IsMale = createPacket.IsMale,
						PlayerClass = ClassTypes.Beginner,
						Level = 1,
						Hp = 50,
						MaxHp = 50,
						Mp = 5,
						MaxMp = 5,
						Defense = 0,
						Str = 4,
						Dex = 4,
						Int = 4,
						Luk = 4,
						Speed = 1,
						Exp = 0,
						StatPoint = 12,
						AccountDbId = AccountDbId,
						posX = 340f,
						posY = 7.5f,
						posZ = 340f,
						rotateY = 0f,
						Money = 0,
					};
					db.Players.Add(newPlayerDb);
					bool success = db.SaveChangesEx();
					if (success == false)
						return;
					ItemDb itemDb = new ItemDb()
					{
						TemplateId = 1,
						Count = 1,
						Slot = 6,
						Equipped = true,
						OwnerDbId = newPlayerDb.PlayerDbId,
					};
					db.Items.Add(itemDb);
					success = db.SaveChangesEx();

					if(success == false)
						return;

					// 메모리에 추가
					LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
					{
						PlayerDbId = newPlayerDb.PlayerDbId,
						Name = createPacket.Name,
						Level = newPlayerDb.Level,
						ClassType = (int)newPlayerDb.PlayerClass,
					};

					// 메모리에도 들고 있다
					LobbyPlayers.Add(lobbyPlayer);

					// 클라에 전송
					S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
					newPlayer.Player.MergeFrom(lobbyPlayer);

					Send(newPlayer);
				}
			}
		}
	}
}

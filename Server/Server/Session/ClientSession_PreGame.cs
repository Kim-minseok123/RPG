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
		public Dictionary<string, List<LobbyPlayerItemInfo>> LobbyPlayerItem = new Dictionary<string, List<LobbyPlayerItemInfo>>();
		public void HandleLogin(C_Login loginPacket)
		{
			// 현재 플레이어가 로그인 상태에서만 로그인하는지 확인
			if (ServerState != PlayerServerState.ServerStateLogin)
			{
				S_Banish banPacket = new S_Banish();
				Send(banPacket);
				return;
			}
            //더미 클라이언트 테스트용
            {
                if (loginPacket.AccountId >= 1000)
                {
                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    Send(loginOk);
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                    return;
                }
            }
            //일반 유저용
            {
                // 로그인 토큰 확인
                using (SharedDbContext db = new SharedDbContext())
                {
                    TokenDb findToken = db.Tokens.Where(a => a.AccountDbId == loginPacket.AccountId).FirstOrDefault();
                    if (findToken == null)
                    {
                        S_Banish banPacket = new S_Banish();
                        Send(banPacket);
                        return;
                    }
                    else
                    {
                        if (DateTime.Compare(DateTime.UtcNow, findToken.Expired) > 0)
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
                        .ThenInclude(p => p.Items)
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
                            LobbyPlayer LP = new LobbyPlayer();
                            LP.Player = lobbyPlayer;
                            foreach (var item in playerDb.Items)
                            {
                                if (item.Equipped == false) continue;
                                LobbyPlayerItemInfo itemInfo = new LobbyPlayerItemInfo();
                                itemInfo.TemplateId = item.TemplateId;
                                itemInfo.Slot = item.Slot;
                                LP.Item.Add(itemInfo);
                            }
                            // 메모리에도 들고 있다
                            LobbyPlayers.Add(lobbyPlayer);
                            LobbyPlayerItem.Add(LP.Player.Name, LP.Item.ToList());
                            // 패킷에 넣어준다
                            loginOk.Players.Add(LP);
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
			
		}

		public void HandleEnterGame(C_EnterGame enterGamePacket)
		{
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;
			if(enterGamePacket.Name == "Master")
			{
				Master = true;
            }
            //더미 클라이언트 테스트용
            {
                if (enterGamePacket.Name.Contains("Dummy_"))
                {
                    MyPlayer = ObjectManager.Instance.Add<Beginner>();
                    MyPlayer.classType = ClassTypes.Beginner;
                    MyPlayer.Info.Name = enterGamePacket.Name;
                    MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                    MyPlayer.Session = this;
                    MyPlayer.Info.PosInfo.Rotate.RotateY = 0;
                    Random rand = new Random();
                    float posX = rand.Next(-70, 71);
                    float posZ = rand.Next(-55, 56);
                    MyPlayer.Info.PosInfo.Pos.PosX = 375 + posX;
                    MyPlayer.Info.PosInfo.Pos.PosY = 7.5f;
                    MyPlayer.Info.PosInfo.Pos.PosZ = 330 + posZ;

                    ServerState = PlayerServerState.ServerStateGame;

                    GameLogic.Instance.Push(() =>
                    {
                        GameRoom room = GameLogic.Instance.Find(1);
                        room.Push(room.EnterGame, MyPlayer);
                    });
                    return;
                }
            }
            //일반 유저용
			{
                LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
                if (playerInfo == null)
                    return;
                if (playerInfo.ClassType == (int)ClassTypes.Beginner)
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
                            MyPlayer.Stat.SkillPoint = findPlayerDb.SkillPoint;
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
                                if (item.Equipped == true)
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
                        // 스킬
                        List<SkillDb> skills = db.Skills
                            .Where(s => s.PlayerDbId == playerInfo.PlayerDbId)
                            .ToList();
                        foreach (SkillDb skillDb in skills)
                        {
                            MyPlayer.HaveSkillData.Add(skillDb.TemplateId, skillDb.SkillLevel);
                        }
                        // 퀵슬롯
                        List<QuickSlotDb> quickSlots = db.QuickSlots
                            .Where(q => q.PlayerDbId == playerInfo.PlayerDbId).ToList();
                        foreach (QuickSlotDb slot in quickSlots)
                        {
                            MyPlayer.QuickSlot.Add(slot.Slot, slot.TemplateId);
                        }
                        // 퀘스트
                        List<QuestDb> quests = db.Quests
                            .Where(q => q.PlayerDbId == playerInfo.PlayerDbId)
                            .Include(q => q.Goals)
                            .ToList();
                        foreach (QuestDb questDB in quests)
                        {
                            Quest quest = Quest.MakeQuest(questDB.TemplateId);
                            if (quest == null) return;
                            switch (quest.QuestType)
                            {
                                case QuestType.Battle:
                                    BattleQuest battleQuest = (BattleQuest)quest;
                                    foreach (var goal in questDB.Goals)
                                    {
                                        battleQuest.Update(new BattleQuestGoals() { enemyId = goal.TemplateId, count = goal.Count });
                                    }
                                    battleQuest.IsFinish = questDB.IsFinish;
                                    break;
                                case QuestType.Collection:
                                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                                    foreach (var goal in questDB.Goals)
                                    {
                                        collectionQuest.Update(new CollectionQuestGoals() { collectionId = goal.TemplateId, count = goal.Count });
                                    }
                                    collectionQuest.IsFinish = questDB.IsFinish;
                                    break;
                                case QuestType.Enter:
                                    EnterQuest enterQuest = (EnterQuest)quest;
                                    foreach (var goal in questDB.Goals)
                                        enterQuest.Update(goal.TemplateId);
                                    quest.IsFinish = questDB.IsFinish;
                                    break;
                            }
                            if(questDB.IsCleard == false)
                                MyPlayer.QuestInven.AddQuest(quest);
                            else
                                MyPlayer.QuestInven.FinishQuest(quest);
                        }
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
		}

		public void HandleCreatePlayer(C_CreatePlayer createPacket)
		{
			// TODO : 이런 저런 보안 체크
			if (ServerState != PlayerServerState.ServerStateLobby || LobbyPlayers.Count >= 3)
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
						SkillPoint = 0,
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
					S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayer() };
					LobbyPlayerItemInfo lobbyPlayerItemInfo = new LobbyPlayerItemInfo();
					LobbyPlayerInfo lobbyPlayerInfo = new LobbyPlayerInfo();
					lobbyPlayerInfo.MergeFrom(lobbyPlayer);
					lobbyPlayerItemInfo.TemplateId = 1;
					lobbyPlayerItemInfo.Slot = 6;
					newPlayer.Player.Item.Add(lobbyPlayerItemInfo);
					newPlayer.Player.Player = lobbyPlayerInfo;
					Send(newPlayer);
					//메모리에도 들고있다.
					LobbyPlayerItem.Add(lobbyPlayer.Name, newPlayer.Player.Item.ToList());
				}
			}
		}
	}
}

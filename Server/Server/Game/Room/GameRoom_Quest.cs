using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleAddQuest(Player player, C_AddQuest addQuestPacket)
        {
            if (player == null) return;
            if (_npc.TryGetValue(addQuestPacket.NpcId, out Npc npc) == false) return;
            QuestNpc questNpc = npc as QuestNpc;

            if (questNpc == null || questNpc.QuestList == null) return;
            if(questNpc.QuestList.Contains(addQuestPacket.QuestId) == false)
            {
                Console.WriteLine($"잘못된 npc 접근입니다. 접근자 : {player.Info.Name}");
                return;
            }
            Quest quest = Quest.MakeQuest(addQuestPacket.QuestId);
            if(quest == null)
            {
                Console.WriteLine("존재하지 않는 퀘스트 입니다.");
                return;
            }
            if(quest.DemandQuest != 0 && player.QuestInven.CheckIsFinishQuest(quest.DemandQuest) == false)
            {
                Console.WriteLine("선행 퀘스트를 클리어하지 않았습니다.");
                return;
            }
            if (quest.IsRepeated == false  && player.QuestInven.CheckIsFinishQuest(quest.TemplateId) == true)
            {
                Console.WriteLine("반복퀘스트가 아닌 일반퀘스트를 이미 수행하셨습니다.");
                return;
            }
            if (player.QuestInven.GetQuest(quest.TemplateId, quest.QuestType) != null)
            {
                Console.WriteLine("이미 퀘스트를 수행중입니다.");
                return;
            }
            player.QuestInven.AddQuest(quest);
            // DB에도 저장해야함.
            
            // 클라 통보
            S_AddQuest addQuestOk = new S_AddQuest();
            addQuestOk.QuestId = addQuestPacket.QuestId;
            player.Session.Send(addQuestOk);

            quest = player.QuestInven.GetQuest(quest.TemplateId, quest.QuestType); 
            // 만약 아이템 획득 퀘스트엿을 경우 이미 그 아이템을 보유했는지도 확인해야함.
            if(quest.QuestType == QuestType.Collection)
            {
                CollectionQuest collectionQuest = (CollectionQuest)quest;
                foreach (var goal in collectionQuest.goals)
                {
                    List<Item> items = player.Inven.FindAll(i => i.TemplateId == goal.collectionId);
                    foreach (var item in items)
                    {
                        player.QuestInven.UpdateQuestProgress(QuestType.Collection, new CollectionQuestGoals() { collectionId = item.TemplateId, count = item.Count });
                    }
                }
            }
        }
        public void HandleClearQuest(Player player, C_ClearQuest clearQuestPacket)
        {
            if (player == null) return;
            if (_npc.TryGetValue(clearQuestPacket.NpcId, out Npc npc) == false) return;
            QuestNpc questNpc = npc as QuestNpc;

            if (questNpc == null || questNpc.QuestList == null) return;
            if (questNpc.QuestList.Contains(clearQuestPacket.QuestId) == false)
            {
                Console.WriteLine($"잘못된 npc 접근입니다. 접근자 : {player.Info.Name}");
            }
            Quest quest = player.QuestInven.GetQuest(clearQuestPacket.QuestId, clearQuestPacket.QuestType);
            if (quest == null)
            {
                Console.WriteLine("존재하지 않는 퀘스트 입니다.");
                return;
            }
            if (player.QuestInven.CheckQuestClear(quest.TemplateId, quest.QuestType) == true)
            {
                player.QuestInven.RemoveQuest(quest);
                player.QuestInven.FinishQuest(quest);
                if(quest.QuestType == QuestType.Collection)
                {
                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                    foreach (var goal in collectionQuest.goals)
                    {
                        int remainingCount = goal.count;

                        while (remainingCount > 0)
                        {
                            Item item = player.Inven.Find(i => i.TemplateId == goal.collectionId);

                            if (item == null)
                            {
                                Item equipItem = player.Inven.EquipFind(i => i.TemplateId == goal.collectionId);
                                if (equipItem == null) 
                                {
                                    Console.WriteLine("아이템을 소유하고 있지 않음.");
                                    return;
                                }
                                int? slot = player.Inven.GetEmptySlot();
                                if (slot == null) 
                                {
                                    Console.WriteLine("빈 슬롯 없음.");
                                    return;
                                }
                                int tempSlot = equipItem.Slot;
                                player.Inven.EquipRemove(equipItem.Slot);
                                equipItem.Equipped = false;
                                equipItem.Slot = (int)slot;
                                player.Inven.Add(equipItem);

                                DbTransaction.EquipItemNoti(player, equipItem);

                                S_EquipItem equipOkItem = new S_EquipItem();
                                equipOkItem.ItemDbId = equipItem.ItemDbId;
                                equipOkItem.Equipped = equipItem.Equipped;
                                equipOkItem.Slot = tempSlot;
                                equipOkItem.ObjectId = player.Id;
                                equipOkItem.TemplateId = equipItem.TemplateId;
                                equipOkItem.NextSlot = equipItem.Slot;
                                Broadcast(player.Pos, equipOkItem);
                                item = equipItem;
                            }

                            int removeCount = Math.Min(remainingCount, item.Count);
                            RemoveItem(player, item, removeCount);
                            remainingCount -= removeCount;
                        }
                    }
                }
                // 보상 지급
                QuestReward questReward = quest.Reward;
                if (questReward != null)
                {
                    if(questReward.exp > 0)
                        player.RewardExp(questReward.exp);
                    if(questReward.money > 0)
                    {
                        C_AddItem addItem = new C_AddItem();
                        addItem.TemplateId = 1000;
                        addItem.Count = questReward.money;
                        addItem.IsBuy = false;
                        HandleAddItem(player, addItem);
                    }
                    if(questReward.itemId != -1)
                    {
                        C_AddItem addItem = new C_AddItem();
                        addItem.TemplateId = questReward.itemId;
                        addItem.Count = 1;
                        addItem.IsBuy = false;
                        HandleAddItem(player, addItem);
                    }
                }
                S_ClearQuest clearQuestOk = new S_ClearQuest();
                clearQuestOk.QuestId = quest.TemplateId;
                clearQuestOk.QuestType = quest.QuestType;
                player.Session.Send(clearQuestOk);
            }
            else
            {
                Console.WriteLine("퀘스트가 완료되지 않았습니다");
            }
        }
    }
}

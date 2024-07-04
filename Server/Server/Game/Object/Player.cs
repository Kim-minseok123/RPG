using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Server.Data;
using Server.DB;
using Server.Game.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Server.Game
{
	public class Player : GameObject
	{
		public int PlayerDbId { get; set; }
		public ClientSession Session { get; set; }
        public VisionCube Vision { get; private set; }

        public Inventory Inven { get; private set; } = new Inventory();
		public ClassTypes classType { get; set; }
		public int BuffDamage { get; set; }
		public int WeaponDamage { get; private set; }
		public int ArmorDefence { get; private set; }
		public Dictionary<int,int> HaveSkillData = new Dictionary<int,int>();
		public Dictionary<string, int> QuickSlot = new Dictionary<string, int>();
		public Player()
		{
			ObjectType = GameObjectType.Player;
            Vision = new VisionCube(this);
        }

        public override void OnDamaged(GameObject attacker, int damage)
		{
            if (Room == null)
                return;

            damage = Math.Max(damage - (ArmorDefence + Stat.Defense), 0);
            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            changePacket.ChangeHp = damage;
            changePacket.IsHeal = false;
            Room.Broadcast(Pos,changePacket);
            //Room.Broadcast(CellPos, changePacket);
            Console.WriteLine(attacker + "에 의한 HP 감소");

            if (Stat.Hp <= 0)
            {
                State = CreatureState.Dead;
                OnDead(attacker);
            }
        }
		public override void OnDead(GameObject attacker)
		{
            if (Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(Pos, diePacket);

            Room.PushAfter(1500, DieEvent);
        }
        public override void DieEvent()
        {
			PosInfo.State = CreatureState.Dead;
			if (Room == null)
				return;

        }
        public void OnLeaveGame()
		{
			// TODO
			// DB 연동?
			// -- 피가 깎일 때마다 DB 접근할 필요가 있을까?
			// 1) 서버 다운되면 아직 저장되지 않은 정보 날아감
			// 2) 코드 흐름을 다 막아버린다 !!!!
			// - 비동기(Async) 방법 사용?
			// - 다른 쓰레드로 DB 일감을 던져버리면 되지 않을까?
			// -- 결과를 받아서 이어서 처리를 해야 하는 경우가 많음.
			// -- 아이템 생성

			DbTransaction.SavePlayerStat(this, Room);

			foreach (var job in buffJob.Values)
			{
				job.Cancel = true;
			}
		}
		public void HandleEquipItem(C_EquipItem equipPacket)
		{
			// 착용 요청이라면, 겹치는 부위 해제
			if (equipPacket.Equipped)
			{
                Item item = Inven.Get(equipPacket.ItemDbId);
                if (item == null)
                    return;

                if (item.ItemType == ItemType.Consumable)
                    return;

				// 플레이어의 클래스나 레벨에 알맞는 아이템만 착용 가능
				ClassTypes? itemRequiredClassType = Item.GetItemRequiredClassType(item);

				if (itemRequiredClassType == null)
					return;

                if(classType != (ClassTypes)itemRequiredClassType)
				{
                    Console.WriteLine($"현재 직업에는 착용할 수 없는 장비 입니다. 요청한 플레이어 : {Info.Name}");
					return;
                }
				if (Stat.Level < item.RequirementLevel)
				{
                    Console.WriteLine($"현재 레벨에는 착용할 수 없는 장비 입니다. 요청한 플레이어 : {Info.Name}");
                    return;
                }

                Item unequipItem = null;

				if (item.ItemType == ItemType.Weapon)
				{
					
                    WeaponType weaponType = ((Weapon)item).WeaponType;
					unequipItem = Inven.EquipFind(
						i => i.Equipped && i.ItemType == ItemType.Weapon
						&& ((weaponType == WeaponType.Assistance && ((Weapon)i).WeaponType == weaponType)
						|| (weaponType != WeaponType.Assistance)));
				}
				else if (item.ItemType == ItemType.Armor)
				{
					ArmorType armorType = ((Armor)item).ArmorType;
					unequipItem = Inven.EquipFind(
						i => i.Equipped && i.ItemType == ItemType.Armor
							&& ((Armor)i).ArmorType == armorType);
				}

				if (unequipItem != null)
				{
					// 메모리 선적용
					int tempSlot = unequipItem.Slot;
					Inven.EquipRemove(unequipItem.Slot);
					unequipItem.Equipped = false;
					unequipItem.Slot = item.Slot;
					Inven.Add(unequipItem);
					// DB에 Noti
					DbTransaction.EquipItemNoti(this, unequipItem);

					// 클라에 통보
					S_EquipItem equipOkItem = new S_EquipItem();
					equipOkItem.ItemDbId = unequipItem.ItemDbId;
					equipOkItem.Equipped = unequipItem.Equipped;
                    equipOkItem.Slot = tempSlot;
                    equipOkItem.ObjectId = Id;
                    equipOkItem.TemplateId = item.TemplateId;
					equipOkItem.NextSlot = unequipItem.Slot;
                    Room.Broadcast(Pos, equipOkItem);
                }
                {
                    int equipSlot = -1;
                    if (item.ItemType == ItemType.Weapon)
                    {
                        WeaponType weaponType = ((Weapon)item).WeaponType;
                        if (weaponType == WeaponType.Assistance)
                        {
                            equipSlot = 8;
                        }
                        else
                        {
                            equipSlot = 6;
                        }
                    }
                    else if (item.ItemType == ItemType.Armor)
                    {
                        ArmorType armor = ((Armor)item).ArmorType;
                        switch (armor)
                        {
                            case ArmorType.Helmet:
                                equipSlot = 1;
                                break;
                            case ArmorType.Armor:
                                equipSlot = 2;
                                break;
                            case ArmorType.Boots:
                                equipSlot = 4;

                                break;
                            case ArmorType.Cape:
                                equipSlot = 5;

                                break;
                            case ArmorType.Gloves:
                                equipSlot = 7;
                                break;
                        }
                    }
                    // 메모리 선적용
                    Inven.Remove(item);
                    item.Slot = equipSlot;
                    item.Equipped = equipPacket.Equipped;
                    Inven.EquipAdd(equipSlot, item);

                    // DB에 Noti
                    DbTransaction.EquipItemNoti(this, item);

                    // 클라에 통보
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = equipPacket.ItemDbId;
                    equipOkItem.Equipped = equipPacket.Equipped;
					equipOkItem.Slot = item.Slot;
                    equipOkItem.ObjectId = Id;
					equipOkItem.TemplateId = item.TemplateId;
					equipOkItem.NextSlot = item.Slot;
					Room.Broadcast(Pos, equipOkItem);
                }
            }
			else
			{
				Item item = Inven.EquipFind(i => i.ItemDbId == equipPacket.ItemDbId);
				if (item == null) return;

				int? slot = Inven.GetEmptySlot();
                if (slot == null)
                    return;
				int tempSlot = item.Slot;
                Inven.EquipRemove(item.Slot);
				item.Equipped = false;
				item.Slot = (int)slot;
				Inven.Add(item);

                DbTransaction.EquipItemNoti(this, item);

                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
				equipOkItem.Slot = tempSlot;
				equipOkItem.ObjectId = Id;
                equipOkItem.TemplateId = item.TemplateId;
				equipOkItem.NextSlot = item.Slot;
                Room.Broadcast(Pos, equipOkItem);
            }

            RefreshAdditionalStat();
		}
		public void RefreshAdditionalStat()
		{
			WeaponDamage = 0;
			ArmorDefence = 0;
			Item[] items = Inven.EquipItems;
			for(int i = 0; i < items.Length; i++)
			{
				if (items[i] == null) continue;
				if (items[i].Equipped == false) continue;

				switch (items[i].ItemType)
				{
					case ItemType.Weapon:
						WeaponDamage += ((Weapon)items[i]).Damage;
						break;
					case ItemType.Armor:
						ArmorDefence += ((Armor)items[i]).Defence;
						break;
				}
			}
			CalAttackValue();

        }
        public virtual void CalAttackValue()
		{

		}
		public void HandleUseItem(C_UseItem useItemPacket)
		{
            Item item = Inven.Get(useItemPacket.ItemDbId);
            if (item == null)
                return;

            ConsumableItemAbility itemAbility = ConsumableItemAbilityFactory.CreateAbility(item.TemplateId);
            if (itemAbility == null) return;
            itemAbility.UseItem(this, (Consumable)item);
			item.Count -= useItemPacket.Count;
            if (item.Count <= 0)
            {
                Inven.Remove(item);
				DbTransaction.RemoveItemNoti(this, item);
            }
			else
			{
				DbTransaction.UseItemNoti(this, item);
            }

			S_UseItem useItemOk = new S_UseItem();
			useItemOk.ItemDbId = item.ItemDbId;
			useItemOk.Count = item.Count;
			useItemOk.StatInfo = new StatInfo();
			useItemOk.StatInfo.MergeFrom(Stat);
			Session.Send(useItemOk);
        }
		public override void RewardExp(int exp)
		{
			ExpData expData = null;
			if (DataManager.ExpDict.TryGetValue(Stat.Level, out expData) == false) return;

			Stat.Exp += exp;
			if(Stat.Exp >= expData.requiredExp)
			{
				Stat.Exp -= expData.requiredExp;
				Stat.Level++;
				Stat.MaxHp += 20;
				Stat.MaxMp += 6;
				Stat.StatPoint += 5;
				Stat.SkillPoint += 3;
                DbTransaction.ExpNoti(this, levelUp: true);

				S_MotionOrEffect levelUpEffect = new S_MotionOrEffect();
				levelUpEffect.ObjectId = Id;
				levelUpEffect.ActionName = "LevelUp";
				Room.Broadcast(Pos, levelUpEffect);
            }
            else
			{
                DbTransaction.ExpNoti(this, levelUp: false);
            }
			S_ChangeStat changeStatPacket = new S_ChangeStat();
			changeStatPacket.StatInfo = new StatInfo();
			changeStatPacket.StatInfo.MergeFrom(Stat);
			Session.Send(changeStatPacket);
        }

		public void HandleSkillLevelUp(C_SkillLevelUp skillLevelUp)
		{
			if (Stat.SkillPoint <= 0) return;
			int id = skillLevelUp.Skill.SkillId;
			int level;
			bool isNew = false;
			if(HaveSkillData.TryGetValue(skillLevelUp.Skill.SkillId, out level) == true)
			{
				HaveSkillData[skillLevelUp.Skill.SkillId]++;
				isNew = false;
				level = HaveSkillData[skillLevelUp.Skill.SkillId];
            }
			else
			{
				HaveSkillData.Add(skillLevelUp.Skill.SkillId, 1);
				level = 1;
				isNew = true;
			}
			Stat.SkillPoint--;
			SkillInfo skillInfo = new SkillInfo() { SkillId = id, Level = level };
			DbTransaction.SkillLevelNoti(this, skillInfo);

			S_SkillLevelUp skillLevelUpOkPacket = new S_SkillLevelUp() { Skill = new SkillInfo() };
			skillLevelUpOkPacket.Skill.MergeFrom(skillInfo);
			skillLevelUpOkPacket.IsNew = isNew;
			Session.Send(skillLevelUpOkPacket);
		}
		public void HandleChangeItemSlot(C_ChangeItemSlot itemSlotPacket)
		{
			Item curItem = Inven.Get(itemSlotPacket.ItemDbId);
			if (curItem == null)
				return;
			Item pointItem = Inven.Find(i=> i.Slot == itemSlotPacket.ChangeItemSlot);
			if(pointItem == null)
			{
				curItem.Slot = itemSlotPacket.ChangeItemSlot;
				pointItem = new Item(ItemType.None) { ItemDbId = -1, Slot = -1, Count = -1 };
				MakeChangeItemSlotPacket(curItem, pointItem);
			}
			else
			{
				if (curItem.Slot == pointItem.Slot) return;
				if(curItem.TemplateId == pointItem.TemplateId && curItem.ItemType == ItemType.Consumable && pointItem.ItemType == ItemType.Consumable)
				{
					
                    if (DataManager.ItemDict.TryGetValue(pointItem.TemplateId, out ItemData itemData) == false) return;
					if (((Consumable)curItem).Stackable == false) return;
                    int maxCount = ((ConsumableData)itemData).maxCount;
                    if (pointItem.Count == maxCount) return;

                    int addCount = curItem.Count + pointItem.Count;
                    if (addCount > maxCount)
                    {
						curItem.Count -= (maxCount - pointItem.Count);
                        pointItem.Count = maxCount;
                        MakeChangeItemSlotPacket(curItem, pointItem);
                    }
                    else
                    {
                        pointItem.Count = addCount;
                        Inven.Remove(curItem);
						curItem.Slot = -1;
						curItem.Count = -1;
                        MakeChangeItemSlotPacket(curItem, pointItem);
                    }
                }
				else
				{
                    (pointItem.Slot, curItem.Slot) = (curItem.Slot, pointItem.Slot);
                    MakeChangeItemSlotPacket(curItem, pointItem);
                }
            }
            DbTransaction.ChangeItemSlotNoti(this, curItem, pointItem);
		}
        public void MakeChangeItemSlotPacket(Item curItem, Item pointItem)
        {
            S_ChangeItemSlot changeItemSlotOk = new S_ChangeItemSlot()
            {
                ItemDbIdOne = curItem.ItemDbId,
                CountOne = curItem.Count,
                SlotOne = curItem.Slot,
                ItemDbIdTwo = pointItem.ItemDbId,
                SlotTwo = pointItem.Slot,
                CountTwo = pointItem.Count
            };
            Session.Send(changeItemSlotOk);
        }
		Dictionary<int, IJob> buffJob = new Dictionary<int, IJob>();
        public void HandleSkillBuff(BuffSkill buff, BuffSkillAbility ability)
        {
            if (State != CreatureState.Idle) return;
            State = CreatureState.Wait;
            Room.PushAfter((int)(buff.cooldown * 1000), ChangeStateAfterTime, CreatureState.Idle);
            if (HaveSkillData.TryGetValue(buff.id, out int skillLevel) == false) return;
            if (buffJob.TryGetValue(buff.id, out IJob job))
            {
                job.Cancel = true;
                buffJob.Remove(buff.id);
            }

            ability.ApplyAbility(this, buff, skillLevel);
            job = Room.PushAfter(buff.duration * skillLevel * 1000, ResetAbility, buff, ability);
            buffJob[buff.id] = job;

            Console.WriteLine(Mp);
            S_ChangeStat changeStat = new S_ChangeStat();
            changeStat.StatInfo = Stat;
            Session.Send(changeStat);
        }

        public void ResetAbility(BuffSkill buff, BuffSkillAbility ability)
        {
            ability.ReSetAbility(this, buff);
            buffJob.Remove(buff.id);
            Console.WriteLine(buff.id + " 버프 제거 됨");
        }
        public void ChangeStateAfterTime(CreatureState state)
        {
            State = state;
        }
    }
}

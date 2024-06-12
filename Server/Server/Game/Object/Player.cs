using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game.Room;
using System;
using System.Collections.Generic;
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
		public int WeaponDamage { get; private set; }
		public int ArmorDefence { get; private set; }

		public Player()
		{
			ObjectType = GameObjectType.Player;
			Vision = new VisionCube(this);
        }

		public override void OnDamaged(GameObject attacker, int damage)
		{
			base.OnDamaged(attacker, damage);
		}
		public override void OnDead(GameObject attacker)
		{
            if (Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(diePacket);

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
                    Room.Broadcast(equipOkItem);
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
					Room.Broadcast(equipOkItem);
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
                Room.Broadcast(equipOkItem);
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

    }
}

using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Equip : UI_Base
{
    enum Images
    {
        HelmetIcon,
        ArmorIcon,
        ShieldIcon,
        BootsIcon,
        CapeIcon,
        WeaponIcon,
        GlovesIcon,
        AssistanceWeaponIcon,
    }
    enum Buttons
    {
        ExitButton,
    }
    GameObject helmetDes;
    GameObject armorDes;
    GameObject shieldDes;
    GameObject bootsDes;
    GameObject capeDes;
    GameObject weaponDes;
    GameObject glovesDes;
    GameObject awDes;

    ItemData helmetData;
    ItemData armorData;
    ItemData shieldData;
    ItemData bootsData;
    ItemData capesData;
    ItemData weaponsData;
    ItemData glovesData;
    ItemData awData;

    public override void Init()
    {
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => { var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("Equip"); });
        {
            GetImage((int)Images.AssistanceWeaponIcon).gameObject.BindEvent((e) =>
            {
                if (awData == null) return;
                awDes = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                awDes.GetComponent<UI_ItemInfoCanvas>().Setting(awData);
            }, Define.UIEvent.PointerEnter);
            GetImage((int)Images.AssistanceWeaponIcon).gameObject.BindEvent((e) =>
            {
                if (awData == null) return;
                Managers.Resource.Destroy(awDes);
            }, Define.UIEvent.PointerExit);
        }
        {
            GetImage((int)Images.WeaponIcon).gameObject.BindEvent((e) =>
            {
                if (weaponsData == null) return;
                weaponDes = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                weaponDes.GetComponent<UI_ItemInfoCanvas>().Setting(weaponsData);
            }, Define.UIEvent.PointerEnter);
            GetImage((int)Images.WeaponIcon).gameObject.BindEvent((e) =>
            {
                if (weaponsData == null) return;
                Managers.Resource.Destroy(weaponDes);
            }, Define.UIEvent.PointerExit);
        }
        {
            GetImage((int)Images.HelmetIcon).gameObject.BindEvent((e) =>
            {
                if (helmetData == null) return;
                helmetDes = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                helmetDes.GetComponent<UI_ItemInfoCanvas>().Setting(helmetData);
            }, Define.UIEvent.PointerEnter);
            GetImage((int)Images.HelmetIcon).gameObject.BindEvent((e) =>
            {
                if (helmetData == null) return;
                Managers.Resource.Destroy(helmetDes);
            }, Define.UIEvent.PointerExit);
        }
        {
            GetImage((int)Images.ArmorIcon).gameObject.BindEvent((e) =>
            {
                if (armorData == null) return;
                armorDes = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                armorDes.GetComponent<UI_ItemInfoCanvas>().Setting(armorData);
            }, Define.UIEvent.PointerEnter);
            GetImage((int)Images.ArmorIcon).gameObject.BindEvent((e) =>
            {
                if (armorData == null) return;
                Managers.Resource.Destroy(armorDes);
            }, Define.UIEvent.PointerExit);
        }
        {
            GetImage((int)Images.BootsIcon).gameObject.BindEvent((e) =>
            {
                if (bootsData == null) return;
                bootsDes = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                bootsDes.GetComponent<UI_ItemInfoCanvas>().Setting(bootsData);
            }, Define.UIEvent.PointerEnter);
            GetImage((int)Images.BootsIcon).gameObject.BindEvent((e) =>
            {
                if (bootsData == null) return;
                Managers.Resource.Destroy(bootsDes);
            }, Define.UIEvent.PointerExit);
        }
        {
            GetImage((int)Images.CapeIcon).gameObject.BindEvent((e) =>
            {
                if (capesData == null) return;
                capeDes = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                capeDes.GetComponent<UI_ItemInfoCanvas>().Setting(capesData);
            }, Define.UIEvent.PointerEnter);
            GetImage((int)Images.CapeIcon).gameObject.BindEvent((e) =>
            {
                if (capesData == null) return;
                Managers.Resource.Destroy(capeDes);
            }, Define.UIEvent.PointerExit);
        }
        {
            GetImage((int)Images.GlovesIcon).gameObject.BindEvent((e) =>
            {
                if (glovesData == null) return;
                glovesDes = Managers.Resource.Instantiate("UI/SubItem/UI_ItemInfoCanvas");
                glovesDes.GetComponent<UI_ItemInfoCanvas>().Setting(glovesData);
            }, Define.UIEvent.PointerEnter);
            GetImage((int)Images.GlovesIcon).gameObject.BindEvent((e) =>
            {
                if (glovesData == null) return;
                Managers.Resource.Destroy(glovesDes);
            }, Define.UIEvent.PointerExit);
        }
        GetImage((int)Images.HelmetIcon).gameObject.BindEvent((e) => { NonEquipItem(0); });
        GetImage((int)Images.ArmorIcon).gameObject.BindEvent((e) => { NonEquipItem(1); });
        GetImage((int)Images.ShieldIcon).gameObject.BindEvent((e) => { NonEquipItem(2); });
        GetImage((int)Images.BootsIcon).gameObject.BindEvent((e) => { NonEquipItem(3); });
        GetImage((int)Images.CapeIcon).gameObject.BindEvent((e) => { NonEquipItem(4); });
        GetImage((int)Images.WeaponIcon).gameObject.BindEvent((e) => { NonEquipItem(5); });
        GetImage((int)Images.GlovesIcon).gameObject.BindEvent((e) => { NonEquipItem(6); });
        GetImage((int)Images.AssistanceWeaponIcon).gameObject.BindEvent((e) => { NonEquipItem(7); });
        RefreshUI();
    }
    public void NonEquipItem(int index)
    {
        Item item = Managers.Inven.EquipItems[index];
        if(item == null) return;

        C_EquipItem equipItem = new C_EquipItem();
        equipItem.ItemDbId = item.ItemDbId;
        equipItem.Equipped = false;
        equipItem.ObjectId = Managers.Object.MyPlayer.Id;
        Managers.Network.Send(equipItem);
    }
    public void RefreshUI()
    {
        for (int i = 0; i < 8; i++)
        {
            GetImage(i).enabled = false;
        }
        Item[] items = Managers.Inven.EquipItems;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                ItemData itemData = null;
                if (Managers.Data.ItemDict.TryGetValue(items[i].TemplateId, out itemData) == false)
                    return;
                Sprite icon = Managers.Resource.Load<Sprite>(itemData.iconPath);

                if (items[i].ItemType == ItemType.Weapon)
                {
                    Weapon weapon = (Weapon)items[i];
                    if (weapon.WeaponType == WeaponType.Assistance)
                    {
                        GetImage((int)Images.AssistanceWeaponIcon).enabled = true;
                        GetImage((int)Images.AssistanceWeaponIcon).sprite = icon;
                        awData = itemData;
                    }
                    else
                    {
                        GetImage((int)Images.WeaponIcon).enabled = true;
                        GetImage((int)Images.WeaponIcon).sprite = icon;
                        weaponsData = itemData;
                    }
                }
                else if (items[i].ItemType == ItemType.Armor)
                {
                    Armor armor = (Armor)items[i];
                    switch (armor.ArmorType)
                    {
                        case ArmorType.Helmet:
                            GetImage((int)Images.HelmetIcon).enabled = true;
                            GetImage((int)Images.HelmetIcon).sprite = icon;
                            helmetData = itemData;
                            break;
                        case ArmorType.Armor:
                            GetImage((int)Images.ArmorIcon).enabled = true;
                            GetImage((int)Images.ArmorIcon).sprite = icon;
                            armorData = itemData;
                            break;
                        case ArmorType.Boots:
                            GetImage((int)Images.BootsIcon).enabled = true;
                            GetImage((int)Images.BootsIcon).sprite = icon;
                            bootsData = itemData;
                            break;
                        case ArmorType.Cape:
                            GetImage((int)Images.CapeIcon).enabled = true;
                            GetImage((int)Images.CapeIcon).sprite = icon;
                            capesData = itemData;
                            break;
                        case ArmorType.Gloves:
                            GetImage((int)Images.GlovesIcon).enabled = true;
                            GetImage((int)Images.GlovesIcon).sprite = icon;
                            glovesData = itemData;
                            break;
                    }
                }

            }
        }
    }
}

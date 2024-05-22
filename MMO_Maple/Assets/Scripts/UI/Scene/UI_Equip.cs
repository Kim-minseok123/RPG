using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
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

    public override void Init()
    {
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => { var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("Stat"); });

        RefreshUI();
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
                    }
                    else
                    {
                        GetImage((int)Images.WeaponIcon).enabled = true;
                        GetImage((int)Images.WeaponIcon).sprite = icon;
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
                            break;
                        case ArmorType.Armor:
                            GetImage((int)Images.ArmorIcon).enabled = true;
                            GetImage((int)Images.ArmorIcon).sprite = icon;
                            break;
                        case ArmorType.Boots:
                            GetImage((int)Images.BootsIcon).enabled = true;
                            GetImage((int)Images.BootsIcon).sprite = icon;
                            break;
                        case ArmorType.Cape:
                            GetImage((int)Images.CapeIcon).enabled = true;
                            GetImage((int)Images.CapeIcon).sprite = icon;
                            break;
                        case ArmorType.Gloves:
                            GetImage((int)Images.GlovesIcon).enabled = true;
                            GetImage((int)Images.GlovesIcon).sprite = icon;
                            break;
                    }
                }

            }
        }
    }
}

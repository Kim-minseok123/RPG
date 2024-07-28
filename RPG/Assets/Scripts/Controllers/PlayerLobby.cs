using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLobby : MonoBehaviour
{
    public GameObject RightHand;
    public GameObject LeftHand;
    public GameObject Head;
    public void Setting(List<LobbyPlayerItemInfo> items)
    {
        foreach (LobbyPlayerItemInfo item in items)
        {
            ItemData data = null;
            if (Managers.Data.ItemDict.TryGetValue(item.TemplateId, out data) == false) return;

            switch (data.itemType)
            {
                case ItemType.Weapon:
                    WeaponData weapon = (WeaponData)data;
                    if (weapon.weaponType == WeaponType.Assistance)
                    {
                        Managers.Resource.Instantiate($"Item/{data.name}", LeftHand.transform);
                    }
                    else
                    {
                        GameObject go = Managers.Resource.Instantiate($"Item/{data.name}", RightHand.transform);
                        if (data.name == "³°Àº °Ë")
                        {
                            go.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
                        }
                    }
                    break;
                case ItemType.Armor:
                    ArmorData armor = (ArmorData)data;
                    if (armor.armorType == ArmorType.Helmet)
                    {
                        Managers.Resource.Instantiate($"Item/{data.name}", Head.transform);
                    }
                    else
                    {

                    }
                    break;
            }
        }
        
    }
}

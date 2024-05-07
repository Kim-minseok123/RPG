using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerSelect_Popup : UI_Popup
{
    List<UI_ServerList_Item> Items = new List<UI_ServerList_Item>();

    enum GameObjects
    {
        BaseObj,
    }
    public override void Init()
    {
        base.Init();

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.BaseObj).transform.DOLocalMoveY(0f, 1f).SetEase(Ease.OutQuad);
    }

    public void ServerSetting(List<ServerInfo> servers)
    {
        Items.Clear();

        GameObject grid = GetComponentInChildren<GridLayoutGroup>().gameObject;
        foreach (Transform child in grid.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < servers.Count; i++)
        {
            var item = Managers.UI.MakeSubItem<UI_ServerList_Item>(grid.transform);
            item.Setting(servers[i]);
            Items.Add(item);
        }
    }
}

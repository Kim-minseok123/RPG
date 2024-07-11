using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    public string[] loading;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        List<GameObject> list = new List<GameObject>();
        foreach (string name in loading)
        {
            list.Add(Managers.Resource.Instantiate($"{name}"));
        }
        foreach (var item in list)
        {
            Managers.Resource.Destroy(item);
        }

    }

    public override void Clear()
    {
        
    }
}

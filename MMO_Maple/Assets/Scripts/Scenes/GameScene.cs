using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    public string[] loadingEffects;
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        List<GameObject> list = new List<GameObject>();
        foreach (string name in loadingEffects)
        {
            list.Add(Managers.Resource.Instantiate($"Effect/{name}"));
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        //Screen.SetResolution(640, 480, false);
    }

    public override void Clear()
    {
        
    }
}

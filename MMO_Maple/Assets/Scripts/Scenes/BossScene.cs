using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BossScene : BaseScene
{
    public PlayableDirector PD;
    protected override void Init()
    {
        SceneType = Define.Scene.Boss;
        PD.Play();
    }
    public override void Clear()
    {

    }
}

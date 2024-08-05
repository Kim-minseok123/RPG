using EasyTransition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScene : BaseScene
{
    WaitForSeconds delay = new WaitForSeconds(1f);
    public override void Clear()
    {
        
    }
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Loading;
        
    }
    public void Start()
    {
        StartCoroutine(NextScene());
    }
    IEnumerator NextScene()
    {
        yield return delay; 
        if (Managers.NextScene == Define.Scene.Unknown) Debug.LogError("잘못된 씬으로 이동 요청");
        TransitionSettings ts = Managers.Resource.Load<TransitionSettings>("Trans/LinearWipe");
        TransitionManager.Instance().Transition(Managers.NextScene, ts, 0, Managers.NextAction);
    }
}

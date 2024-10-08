using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BossScene : BaseScene
{
    public string[] loading;

    public PlayableDirector PD;
    public GameObject room2;
    public GameObject cut;
    protected override void Init()
    {
        SceneType = Define.Scene.Boss;
        PD.Play();
        Managers.Sound.Play("BossBgm", Define.Sound.Bgm);
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
    public void DieRedDragon()
    {
        room2.SetActive(true);
        cut.SetActive(true);
    }
}

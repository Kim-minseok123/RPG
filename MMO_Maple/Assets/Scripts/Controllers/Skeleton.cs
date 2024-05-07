using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonsterController
{
    protected override void Init()
    {
        base.Init();
        Monster monster = null;
        if (Managers.Data.MonsterDict.TryGetValue(1, out monster) == false) return;
        MaxHp = monster.stat.Hp;
        Hp = MaxHp;
    }
}

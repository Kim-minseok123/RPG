using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : BaseController
{
    protected int MaxHp { get { return Stat.MaxHp; }  set { Stat.MaxHp = value; } }
    protected int Hp { get { return Stat.Hp; } set { Stat.Hp = value; } }
    protected StatInfo Stat = new StatInfo();
    protected CreatureState State { get; set; } = CreatureState.Idle;
    public override void SetStat(StatInfo stat)
    {
        Stat = stat;
    }
}

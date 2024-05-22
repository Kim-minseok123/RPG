using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : BaseController
{
    public virtual int MaxHp { get { return Stat.MaxHp; }  protected set { Stat.MaxHp = value; } }
    public virtual int MaxMp { get { return Stat.MaxMp; }  protected set { Stat.MaxMp = value; } }
    public virtual int Hp { get { return Stat.Hp; } protected set { Stat.Hp = value; } }
    public virtual int Mp { get { return Stat.Mp; } protected set { Stat.Mp = value; } }
    public virtual int WeaponDamage { get; protected set; }
    public virtual GameObject FinalAttacker { get; protected set; }
    public ObjectInfo objectInfo { get; protected set; } = new ObjectInfo();
    public StatInfo Stat 
    {
        get { return objectInfo.StatInfo; }
        protected set { objectInfo.StatInfo = value; }
    }
    protected CreatureState State { get; set; } = CreatureState.Idle;
    public virtual void SetInfo(ObjectInfo info)
    {
        objectInfo = info;
    }
}

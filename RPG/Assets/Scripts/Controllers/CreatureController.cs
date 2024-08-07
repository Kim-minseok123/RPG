using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : BaseController
{
    public virtual int MaxHp { get { if (Stat == null) return 0; return Stat.MaxHp; }  set { Stat.MaxHp = value; } }
    public virtual int MaxMp { get { return Stat.MaxMp; }  set { Stat.MaxMp = value; } }
    public virtual int Hp { get { if (Stat == null) return 0; return Stat.Hp; } set { Stat.Hp = value; } }
    public virtual int Mp { get { return Stat.Mp; } set { Stat.Mp = value; } }
    public virtual int WeaponDamage { get; protected set; }
    public virtual int ArmorDefence { get; protected set; }
    public virtual GameObject FinalAttacker { get; protected set; }
    public ObjectInfo objectInfo { get; protected set; } = new ObjectInfo();
    public StatInfo Stat 
    {
        get { return objectInfo.StatInfo; }
        protected set { objectInfo.StatInfo = value; }
    }
    public CreatureState State { get; set; } = CreatureState.Idle;
    public virtual void SetInfo(ObjectInfo info)
    {
        objectInfo = info;
    }
    public virtual void SetStat(StatInfo info)
    {
        Stat = info;
    }
    public virtual void EffectInst(string name, float time, Vector3 pos, Vector3 scale)
    {
        GameObject effect = Managers.Resource.Instantiate($"Effect/{name}", transform);
        effect.transform.position = pos;
        effect.transform.localScale = scale;
        StartCoroutine(CoWaitForSecondsDestroy(effect, time));
    }
    IEnumerator CoWaitForSecondsDestroy(GameObject effect, float time)
    {
        yield return new WaitForSeconds(time);
        Managers.Resource.Destroy(effect);
    }

}

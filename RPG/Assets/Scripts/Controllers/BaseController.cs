﻿using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using static Define;

public class BaseController : MonoBehaviour
{
	void Start()
	{
		Init();
	}
    private void OnEnable()
    {
		Init();
    }
    public int Id { get; set; }
	public Vector3 PrevPos { get; set; }

    protected virtual void Init()
	{
		
	}

	protected virtual void UpdateController()
	{
		
	}

	protected virtual void UpdateIdle()
	{
	}

	// 스르륵 이동하는 것을 처리
	protected virtual void UpdateMoving()
	{
		
	}

	protected virtual void MoveToNextPos()
	{

	}

	protected virtual void UpdateSkill()
	{

	}

	protected virtual void UpdateDead()
	{

	}
	public void SetPos(Positions pos = null, RotateInfo rotate = null)
	{
		if (pos == null && rotate != null)
		{
			transform.rotation = Quaternion.Euler(rotate.RotateX, rotate.RotateY, rotate.RotateZ);
            return;
		}

		if(rotate != null && pos != null) 
			transform.SetPositionAndRotation(new Vector3(pos.PosX, pos.PosY, pos.PosZ), Quaternion.Euler(rotate.RotateX, rotate.RotateY, rotate.RotateZ));
		else
		{
			transform.position = Util.PositionsToVector3(pos);
		}
        if (gameObject.GetComponent<NavMeshAgent>() != null)
            gameObject.GetComponent<NavMeshAgent>().enabled = true;
    }
    public virtual void MoveTarget(Vector3 target, GameObject targetObj = null)
    {

    }
    public virtual void StopMove(Vector3 receivedEuler, Vector3 receivePos)
    {

    }
    public virtual IEnumerator CheckPosInfo()
    {
		yield return null;
    }
	public virtual void OnAttack(SkillInfo info) { }
	public virtual void ChangeHp(int hp, bool isHeal, int damage)
	{

	}
	public virtual void OnDead(GameObject attacker)
	{

	}
}

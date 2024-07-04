using Google.Protobuf.Protocol;
using Server.Game.Room;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}
		protected Random random = new Random();
		public GameRoom Room { get; set; }
		public Positions DestPos { get; set; }
		public ObjectInfo Info { get; set; } = new ObjectInfo();
		public PositionInfo PosInfo { get; private set; } = new PositionInfo();
		public Positions myPos { get; private set; } = new Positions();
		public RotateInfo myRotate { get; private set; } = new RotateInfo();
		public StatInfo Stat { get; private set; } = new StatInfo();
		public virtual int TotalAttack { get { return Attack; } }
		public virtual int TotalDefence { get { return 0; } }
		public virtual int Attack { get { return random.Next(1, Stat.Str + 1); } }
		public Zone curZone { get; set; }
		public float Speed
		{
			get { return Stat.Speed; }
			set { Stat.Speed = value; }
		}
        public bool isMoving = false;

        public int Hp
		{
			get { return Stat.Hp; }
			set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
		}
        public int Mp
        {
            get { return Stat.Mp; }
            set { Stat.Mp = Math.Clamp(value, 0, Stat.MaxMp); }
        }
        public Positions Pos
		{
            get { return PosInfo.Pos; }
            set { PosInfo.Pos = value; }
        }
		public RotateInfo Ratate
		{
			get { return PosInfo.Rotate; }
			set { PosInfo.Rotate = value; }
		}

		public CreatureState State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}
		public Vector3 Forward
		{
			get
			{
                Quaternion vQuat = new Quaternion(0, 0, 1, 0);
				Quaternion q = Utils.RotationToQuaternion(Ratate);
                // 쿼터니언 곱셈: q * v * q^(-1)
                Quaternion qConjugate = Quaternion.Conjugate(q);
                Quaternion qv = q * vQuat;
                Quaternion qvq = qv * qConjugate;
                return new Vector3(qvq.X, qvq.Y, qvq.Z);
            }
		}

		public GameObject()
		{
			Info.PosInfo = PosInfo;
			PosInfo.Pos = myPos;
			PosInfo.Rotate = myRotate;
			Info.StatInfo = Stat;
			//zone 설정
		}

		public virtual void Update()
		{

		}

		public virtual void OnDamaged(GameObject attacker, int damage)
		{
			if (Room == null)
				return;

			damage = Math.Max(damage - Stat.Defense, 0);
			Stat.Hp = Math.Max(Stat.Hp - damage, 0);

			S_ChangeHp changePacket = new S_ChangeHp();
			changePacket.ObjectId = Id;
			changePacket.Hp = Stat.Hp;
			changePacket.ChangeHp = damage;
			changePacket.IsHeal = false;
			Room.Broadcast(Pos, changePacket);
            //Room.Broadcast(CellPos, changePacket);
            Console.WriteLine(attacker +"에 의한 HP 감소");

            if (Stat.Hp <= 0)
			{
				State = CreatureState.Dead;
                OnDead(attacker);
            }
		}

		public virtual void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.AttackerId = attacker.Id;
			Room.Broadcast(Pos, diePacket);

			Room.PushAfter(1000, DieEvent);
		}
		public virtual void DieEvent()
		{
            if (Room == null)
                return;
            GameRoom room = Room;
            room.LeaveGame(Id);
        }

		public virtual GameObject GetOwner()
		{
			return this;
		}
        public virtual void RewardExp(int exp)
        {

        }
    }
}

using Google.Protobuf.Protocol;
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

		public float Speed
		{
			get { return Stat.Speed; }
			set { Stat.Speed = value; }
		}

		public int Hp
		{
			get { return Stat.Hp; }
			set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
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
		}

		public virtual void Update()
		{

		}

		public Vector2Int CellPos
		{
			get
			{
				return new Vector2Int((int)PosInfo.Pos.PosX, (int)PosInfo.Pos.PosY);
			}

			set
			{
				PosInfo.Pos.PosX = value.x;
				PosInfo.Pos.PosY = value.y;
			}
		}

		public Vector2Int GetFrontCellPos()
		{
			return new Vector2Int();
		}

		public Vector2Int GetFrontCellPos(MoveDir dir)
		{
			Vector2Int cellPos = CellPos;

			switch (dir)
			{
				case MoveDir.Up:
					cellPos += Vector2Int.up;
					break;
				case MoveDir.Down:
					cellPos += Vector2Int.down;
					break;
				case MoveDir.Left:
					cellPos += Vector2Int.left;
					break;
				case MoveDir.Right:
					cellPos += Vector2Int.right;
					break;
			}

			return cellPos;
		}

		public static MoveDir GetDirFromVec(Vector2Int dir)
		{
			if (dir.x > 0)
				return MoveDir.Right;
			else if (dir.x < 0)
				return MoveDir.Left;
			else if (dir.y > 0)
				return MoveDir.Up;
			else
				return MoveDir.Down;
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
			Room.Broadcast(changePacket);
            //Room.Broadcast(CellPos, changePacket);
            Console.WriteLine(attacker +"에 의한 HP 감소");

            if (Stat.Hp <= 0)
			{
                //OnDead(attacker);
            }
		}

		public virtual void OnDead(GameObject attacker)
		{
			if (Room == null)
				return;

			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			diePacket.AttackerId = attacker.Id;
			Room.Broadcast(CellPos, diePacket);

			GameRoom room = Room;
			room.LeaveGame(Id);

			Stat.Hp = Stat.MaxHp;
			PosInfo.State = CreatureState.Idle;
			//PosInfo.MoveDir = MoveDir.Down;

			room.EnterGame(this, randomPos: true);
		}

		public virtual GameObject GetOwner()
		{
			return this;
		}
	}
}

using Google.Protobuf.Protocol;
using Server.Game;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class Utils
    {
        public static Vector3 PositionsToVector3(Positions positions)
        {
            return new Vector3(positions.PosX, positions.PosY, positions.PosZ);
        }
        public static Positions Vector3ToPositions(Vector3 vector)
        {
            return new Positions() { PosX = (float)vector.x, PosY = (float)vector.y, PosZ = (float)vector.z };
        }
        public static Quaternion RotationToQuaternion(RotateInfo rotation)
        {
            Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(rotation.RotateX, rotation.RotateY, rotation.RotateZ);
            return quaternion;
        }
    }
    public class Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public float magnitude
        {
            get { return (float)Math.Sqrt(x * x + y * y + z * z); }
        }

        public Vector3 normalized
        {
            get
            {
                float mag = magnitude;
                return new Vector3(x / mag, y / mag, z / mag);
            }
        }
        public static Vector3 operator *(Vector3 a, float scalar)
        {
            return new Vector3(a.x * scalar, a.y * scalar, a.z * scalar);
        }

        // 스칼라 곱셈의 반대 순서를 처리 (double * Vector3)
        public static Vector3 operator *(float scalar, Vector3 a)
        {
            return a * scalar; // 위에서 정의한 연산자를 재활용
        }
        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public Vector3 Cross(Vector3 b)
        {
            return new Vector3(y * b.z - z * b.y, z * b.x - x * b.z, x * b.y - y * b.x);
        }
    }
}

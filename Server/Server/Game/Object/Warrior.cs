using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Warrior : Player
    {
        public override int Attack { get { return random.Next(MinAttack, MaxAttack + 1); } }
        public int MaxAttack { get; set; }
        public int MinAttack { get; set; }
        public Warrior() : base()
        {
            CalAttackValue();
        }
        public override void CalAttackValue()
        {
            MaxAttack = (Stat.Str * 4 + Stat.Dex) * (WeaponDamage + BuffDamage) / 100;
            MinAttack = (int)((Stat.Str * 4 * 0.9 * 0.1 + Stat.Dex) * (WeaponDamage + BuffDamage) / 100);
            if (MinAttack < 1) MinAttack = 1;
            if (MaxAttack < 3) MaxAttack = 3;
        }
    }
}

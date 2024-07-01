using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public static class SkillAbilityFactory
    {
        public static BuffSkillAbility CreateAbility(int skillId)
        {
            switch (skillId)
            {
                case 5:
                    return new AngerBuff();
            }
            return null;
        }
    }
    public interface BuffSkillAbility
    {
        public void ApplyAbility(Player player, BuffSkill buffSkill, int skillLevel);
        public void ReSetAbility(Player player, BuffSkill buffSkill);
    }
    public class AngerBuff : BuffSkillAbility
    {
        public void ApplyAbility(Player player, BuffSkill buffSkill, int skillLevel)
        {
            if (player.Mp < buffSkill.mpConsume) return;
            player.Mp = Math.Max(0, player.Mp - buffSkill.mpConsume);
            player.BuffDamage = 0;
            player.BuffDamage = Math.Max((buffSkill.skillLevelInc + skillLevel) / 2, 1);
            player.RefreshAdditionalStat();
        }

        public void ReSetAbility(Player player, BuffSkill buffSkill)
        {
            player.BuffDamage = 0;
            player.RefreshAdditionalStat();

        }
    }
}

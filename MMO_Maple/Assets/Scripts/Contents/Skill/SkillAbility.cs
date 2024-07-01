using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public void ApplyAbility(MyPlayerController player, BuffSkill buffSkill, int skillLevel);
    public void ReSetAbility(MyPlayerController player, BuffSkill buffSkill);
}
public class AngerBuff : BuffSkillAbility
{
    public void ApplyAbility(MyPlayerController player, BuffSkill buffSkill, int skillLevel)
    {
        if (player.Mp < buffSkill.mpConsume) return;
        player.BuffDamage = 0;
        player.BuffDamage = Math.Max((buffSkill.skillLevelInc + skillLevel) / 2, 1);
        player.RefreshAdditionalStat();
    }

    public void ReSetAbility(MyPlayerController player, BuffSkill buffSkill)
    {
        player.BuffDamage = 0;
        player.RefreshAdditionalStat();

    }
}
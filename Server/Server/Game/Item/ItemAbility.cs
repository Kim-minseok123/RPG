using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public static class ConsumableItemAbilityFactory
    {
        public static ConsumableItemAbility CreateAbility(int templateId)
        {
            switch (templateId)
            {
                case 200:
                    return new RedPotion();
                case 201:
                    return new BluePotion();
            }
            return null;
        }
    }
    public interface ConsumableItemAbility
    {
        public void UseItem(Player player, Consumable item);
    }
    public class RedPotion : ConsumableItemAbility
    {
        public void UseItem(Player player, Consumable item)
        {
            player.Hp += item.HealVal;
        }
    }
    public class BluePotion : ConsumableItemAbility
    {
        public void UseItem(Player player, Consumable item)
        {
            player.Mp += item.HealVal;
        }
    }
    
}

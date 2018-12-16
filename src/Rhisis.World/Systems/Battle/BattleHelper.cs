using Rhisis.Core.Data;
using Rhisis.Core.Helpers;
using Rhisis.World.Game.Common;
using Rhisis.World.Game.Entities;
using Rhisis.World.Game.Structures;
using Rhisis.World.Systems.Inventory;

namespace Rhisis.World.Systems.Battle
{
    /// <summary>
    /// This class provides methods to calculate everything related to the <see cref="BattleSystem"/>.
    /// </summary>
    public static class BattleHelper
    {
        /// <summary>
        /// Gets the melee damages inflicted by an attacker to a defender.
        /// </summary>
        /// <param name="attacker">Attacker entity</param>
        /// <param name="defender">Defender entity</param>
        /// <returns>Damages</returns>
        public static AttackResult GetMeeleAttackDamages(ILivingEntity attacker, ILivingEntity defender)
        {
            var damages = 0;

            if (attacker is IPlayerEntity player)
            {
                Item rightWeapon = player.Inventory.GetItem(x => x.Slot == InventorySystem.RightWeaponSlot);

                if (rightWeapon == null)
                    rightWeapon = InventorySystem.Hand;

                // TODO: GetDamagePropertyFactor()
                int weaponAttack = GetWeaponAttackDamages(rightWeapon.Data.WeaponType, player);
                int weaponMinAbility = rightWeapon.Data.AbilityMin * 2 + weaponAttack;
                int weaponMaxAbility = rightWeapon.Data.AbilityMax * 2 + weaponAttack;

                damages = RandomHelper.Random(weaponMinAbility, weaponMaxAbility);
            }
            else if (attacker is IMonsterEntity monster)
            {
                damages = RandomHelper.Random(monster.Data.AttackMin, monster.Data.AttackMax);
            }

            damages -= GetDefense(attacker, defender);

            return new AttackResult
            {
                Damages = damages > 0 ? damages : 0,
                Flags = AttackFlags.AF_GENERIC
            };
        }

        public static int GetWeaponAttackDamages(WeaponType weaponType, IPlayerEntity player)
        {
            float attribute = 0f;
            float levelFactor = 0f;
            float jobFactor = 1f;

            switch (weaponType)
            {
                case WeaponType.MELEE_SWD:
                    attribute = player.Statistics.Strength - 12;
                    levelFactor = player.Object.Level * 1.1f;
                    jobFactor = player.PlayerData.JobData.MeleeSword;
                    break;
                case WeaponType.MELEE_AXE:
                    attribute = player.Statistics.Strength - 12;
                    levelFactor = player.Object.Level * 1.2f;
                    jobFactor = player.PlayerData.JobData.MeleeAxe;
                    break;
                case WeaponType.MELEE_STAFF:
                    attribute = player.Statistics.Strength - 10;
                    levelFactor = player.Object.Level * 1.1f;
                    jobFactor = player.PlayerData.JobData.MeleeStaff;
                    break;
                case WeaponType.MELEE_STICK:
                    attribute = player.Statistics.Strength - 10;
                    levelFactor = player.Object.Level * 1.3f;
                    jobFactor = player.PlayerData.JobData.MeleeStick;
                    break;
                case WeaponType.MELEE_KNUCKLE:
                    attribute = player.Statistics.Strength - 10;
                    levelFactor = player.Object.Level * 1.2f;
                    jobFactor = player.PlayerData.JobData.MeleeKnucle;
                    break;
                case WeaponType.MAGIC_WAND:
                    attribute = player.Statistics.Intelligence - 10;
                    levelFactor = player.Object.Level * 1.2f;
                    jobFactor = player.PlayerData.JobData.MagicWand;
                    break;
                case WeaponType.MELEE_YOYO:
                    attribute = player.Statistics.Strength - 10;
                    levelFactor = player.Object.Level * 1.1f;
                    jobFactor = player.PlayerData.JobData.MeleeYoyo;
                    break;
                case WeaponType.RANGE_BOW:
                    attribute = (player.Statistics.Dexterity - 14) * 4f;
                    levelFactor = player.Object.Level * 1.3f;
                    jobFactor = (player.Statistics.Strength * 0.2f) * 0.7f;
                    break;
            }

            return (int)(attribute * jobFactor + levelFactor);
        }

        public static int GetDefense(ILivingEntity attacker, ILivingEntity defender)
        {
            return 0;
        }

        // TODO: move this to utility
        private static int MulDiv(int number, int numerator, int denominator)
        {
            return (int)(((long)number * numerator) / denominator);
        }
    }
}

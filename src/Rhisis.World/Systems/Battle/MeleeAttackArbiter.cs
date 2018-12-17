using Rhisis.Core.Helpers;
using Rhisis.World.Game.Common;
using Rhisis.World.Game.Entities;
using Rhisis.World.Game.Structures;
using Rhisis.World.Systems.Inventory;

namespace Rhisis.World.Systems.Battle
{
    /// <summary>
    /// Provides a mechanism to calculate a melee attack result based on the attacker and defender statistics.
    /// </summary>
    public class MeleeAttackArbiter
    {
        private readonly ILivingEntity _attacker;
        private readonly ILivingEntity _defender;

        /// <summary>
        /// Creates a new <see cref="MeleeAttackArbiter"/> instance.
        /// </summary>
        /// <param name="attacker">Attacker entity</param>
        /// <param name="defender">Defender entity</param>
        public MeleeAttackArbiter(ILivingEntity attacker, ILivingEntity defender)
        {
            this._attacker = attacker;
            this._defender = defender;
        }

        /// <summary>
        /// Gets the melee damages inflicted by an attacker to a defender.
        /// </summary>
        /// <returns><see cref="AttackResult"/></returns>
        public AttackResult OnDamage()
        {
            var attackResult = new AttackResult();

            if (this._attacker is IPlayerEntity player)
            {
                Item rightWeapon = player.Inventory.GetItem(x => x.Slot == InventorySystem.RightWeaponSlot);

                if (rightWeapon == null)
                    rightWeapon = InventorySystem.Hand;

                // TODO: GetDamagePropertyFactor()
                int weaponAttack = BattleHelper.GetWeaponAttackDamages(rightWeapon.Data.WeaponType, player);
                int weaponMinAbility = rightWeapon.Data.AbilityMin * 2 + weaponAttack;
                int weaponMaxAbility = rightWeapon.Data.AbilityMax * 2 + weaponAttack;

                attackResult.Damages = RandomHelper.Random(weaponMinAbility, weaponMaxAbility);
                attackResult.Flags = this.GetAttackFlags();
            }
            else if (this._attacker is IMonsterEntity monster)
            {
                attackResult.Damages = RandomHelper.Random(monster.Data.AttackMin, monster.Data.AttackMax);
            }

            if (attackResult.Damages < 0)
                attackResult.Damages = 0;

            return attackResult;
        }

        private AttackFlags GetAttackFlags()
        {
            return AttackFlags.AF_GENERIC;
        }
    }
}

using GlobalEnums;
using Modding;
using UnityEngine;

namespace NotSpikesmas
{
    public class NotSpikesmasModule : ItemChanger.Modules.Module
    {
        private static readonly Modding.ILogger _log = new SimpleLogger("NotSpikesmas.Module");

        public bool hasNotSpikesmas { get; set; } = false;

        public const int NotSpikesmasDamage = 26959;

        public override void Initialize()
        {
            // Set the damage for spikes to a constant so we can zero it in the hook
            // We need to do it like this as 0 damage means the knight won't even be treated as taking damage
            On.DamageHero.OnEnable += OnEnableDamageHero;
            ModHooks.AfterTakeDamageHook += ChangeDamageAmount;
            ModHooks.GetPlayerBoolHook += SkillBoolGetOverride;
            ModHooks.SetPlayerBoolHook += SkillBoolSetOverride;
        }

        public override void Unload()
        {
            On.DamageHero.OnEnable -= OnEnableDamageHero;
            ModHooks.GetPlayerBoolHook -= SkillBoolGetOverride;
            ModHooks.SetPlayerBoolHook -= SkillBoolSetOverride;
            ModHooks.AfterTakeDamageHook -= ChangeDamageAmount;
        }

        private void OnEnableDamageHero(On.DamageHero.orig_OnEnable orig, DamageHero self)
        {
            orig(self);

            GameObject go = self.gameObject;
            
            // Ignore object if it's not pogoable
            NonBouncer nb = go.GetComponent<NonBouncer>();
            if (nb != null && nb.active)
            {
                return;
            }

            // Spikes should have spikes in the name
            if (!go.name.ToLower().Contains("spikes"))
            {
                return;
            }
            
            // Check hazard type
            if (self.hazardType != (int)HazardType.SPIKES + 1)
            {
                return;
            }
            
            self.damageDealt = NotSpikesmasDamage;
        }

        private int ChangeDamageAmount(int hazardType, int damageAmount)
        {
            if (damageAmount != NotSpikesmasDamage)
            {
                return damageAmount;
            }
            // Add 1 because TC have 1-indexed their hazard types
            if (hazardType != (int)HazardType.SPIKES + 1)
            {
                _log.LogWarn($"Received spikes damage from non-spikes source, scene {GameManager.instance.sceneName}");
            }

            if (hasNotSpikesmas)
            {
                return 0;
            }
            return 1;
        }

        private bool SkillBoolGetOverride(string boolName, bool value) => boolName switch
        {
            nameof(hasNotSpikesmas) => hasNotSpikesmas,
            _ => value,
        };

        private bool SkillBoolSetOverride(string boolName, bool value)
        {
            switch (boolName)
            {
                case nameof(hasNotSpikesmas):
                    hasNotSpikesmas = value;
                    break;
            }
            return value;
        }
    }
}

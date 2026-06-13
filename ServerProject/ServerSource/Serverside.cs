namespace Neurotrauma
{

    public partial class NeurotraumaInit
    {
        // Server-specific code

        private void DefineAllAfflictions()
        {

        }

        

        public void InitializeServer()
        {
            LuaCsLogger.Log("Running InitializeServer");
            DefineAllAfflictions();
            NTItemMethods.DefineAllItems();
        }

        public void OnLoadCompletedServer()
        {
            LuaCsLogger.Log("Running OnLoadCompletedServers");
            harmony = new Harmony("neurotrauma.server");

            var originalApplyDamage = AccessTools.Method(typeof(CharacterHealth), "ApplyDamage", [typeof(Limb), typeof(AttackResult), typeof(bool), typeof(bool)]);
            var originalDamageLimb = AccessTools.Method(typeof(Character), "DamageLimb", [typeof(Vector2), typeof(Limb), typeof(IEnumerable<Affliction>), typeof(float), typeof(bool), typeof(Vector2),
                typeof(Character), typeof(float), typeof(bool), typeof(float), typeof(bool), typeof(bool), typeof(bool)]);
            var originalUse = AccessTools.Method(typeof(Item), "Use", [typeof(float), typeof(Character), typeof(Limb), typeof(Entity), typeof(Character)]);
            var originalApplyTreatment = AccessTools.Method(typeof(Item), "ApplyTreatment", [typeof(Character), typeof(Character), typeof(Limb)]);

            harmony.Patch(originalApplyDamage, prefix: new HarmonyMethod(typeof(OnDamaged), nameof(OnDamaged.Override_ApplyDamage)));
            harmony.Patch(originalDamageLimb, prefix: new HarmonyMethod(typeof(OnDamaged), nameof(OnDamaged.Override_DamageLimb)));
            harmony.Patch(originalUse, prefix: new HarmonyMethod(typeof(NTItemMethods), nameof(NTItemMethods.Override_Use)));
            harmony.Patch(originalApplyTreatment, prefix: new HarmonyMethod(typeof(NTItemMethods), nameof(NTItemMethods.Override_ApplyTreatment)));

            // Character Patches
            var characterCreation = AccessTools.Method(typeof(Character), "Create", 
                [typeof(CharacterPrefab),typeof(Vector2),typeof(string),typeof(CharacterInfo),typeof(ushort),typeof(bool),typeof(bool),typeof(bool),typeof(RagdollParams),typeof(bool)]);
            harmony.Patch(characterCreation, postfix: new HarmonyMethod(typeof(HumanUpdate), nameof(HumanUpdate.AddCharacterToUpdate))); // The Character Created hook.

            var characterDeath = AccessTools.Method(typeof(Character), "RecordKill",
                [typeof(Character)]);
            harmony.Patch(characterDeath, prefix: new HarmonyMethod(typeof(HumanUpdate), nameof(HumanUpdate.RemoveCharacterFromUpdate))); // The Character died hook.
        }

        public void DisposeServer()
        {
            harmony.UnpatchSelf();
        }
    }
}

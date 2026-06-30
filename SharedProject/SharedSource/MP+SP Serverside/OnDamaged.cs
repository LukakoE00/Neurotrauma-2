using Barotrauma.Items.Components;

namespace Neurotrauma;

public class OnDamaged
{
    public static readonly Dictionary<string, Action<Character, float, LimbType>> OnDamagedMethods = new();

    public static readonly List<Func<CharacterHealth, List<Affliction>, Limb, List<Affliction>>> ModifyingOnDamagedHooks = new();

    public static readonly List<Action<CharacterHealth, AttackResult, Limb>> OnDamagedHooks = new();
    
    /// <summary>
    /// Reduces Concussion amount based on worn armor.
    /// </summary>
    /// <param name="Armor">Item ID of worn armor.</param>
    /// <param name="Strength">Amount of strength of the Concussion Affliction.</param>
    /// <returns></returns>
    public static float GetCalculatedConcussionReduction(Item Armor, float Strength)
    {
        if (Armor == null)
        {
            return 0f;
        }

        if (!Armor.HasTag("deepdiving") &&
            !Armor.HasTag("deepdivinglarge") &&
            !Armor.HasTag("smallitem"))
        {
            return 0f;
        }

        var wearable = Armor.GetComponent<Wearable>();
        if (wearable == null)
        {
            return 0f;
        }

        foreach (var modifier in wearable.DamageModifiers)
        {
            if (modifier.AfflictionIdentifiers.Contains("concussion"))
            {
                return Strength - (Strength * modifier.DamageMultiplier);
            }
        }

        return 0f;
    }

    public static void Override_DamageLimb(
    Character Character,
    Vector2 WorldPosition,
    Limb HitLimb,
    IEnumerable<Affliction> Afflictions,
    float StunAmount,
    bool PlaySound,
    Vector2 AttackImpulse,
    Character Attacker = null,
    float DamageMultiplier = 1f,
    bool AllowStacking = true,
    float Penetration = 0f,
    bool ShouldImplode = false,
    bool IgnoreDamageOverlay = false,
    bool RecalculateVitality = true)
    {
        // Confirm the attack data is valid.
        if (Character == null || Character.IsDead || !(Character.IsHuman) ||
            Afflictions == null ||
            HitLimb == null || HitLimb.IsSevered ||
            Attacker == null ||
            !(NTConfig.Get<bool>("NT_Calculations", true)))
        {
            return;
        }

        // Pull the Evil Falldamage abusing creatures from config.
        var CreatureCategory = NTConfig.Get<IEnumerable<string>>("NT_creatureNoFallDamage", Enumerable.Empty<string>());

        // If one of these critters caused the attack, counteract the additional damage.
        foreach (string Species in CreatureCategory)
        {
            if (Attacker.SpeciesName == Species)
            {
                HF.AddAffliction(Character, "stopcreatureabuse", 2f);
                break;
            }
        }
    }

    public static void Override_ApplyDamage(
    CharacterHealth CharacterHealth,
    Limb HitLimb,
    AttackResult AttackResult,
    bool AllowStacking = true,
    bool RecalculateVitality = true)
    {
        // Confirm the attack data is valid.
        if (CharacterHealth == null || CharacterHealth.Character == null || CharacterHealth.Character.IsDead || !(CharacterHealth.Character.IsHuman) ||
            AttackResult.Afflictions == null || !(AttackResult.Afflictions.Any()) ||
            HitLimb == null || HitLimb.IsSevered ||
            !NTConfig.Get<bool>("NT_Calculations", true))
        {
            return;
        }

        // Check for Luabotomy.
        if (!HF.HasAffliction(CharacterHealth.Character, "luabotomy"))
        {
            HF.SetAffliction(CharacterHealth.Character, "luabotomy", 1f);
        }

        List<Affliction> Afflictions = AttackResult.Afflictions;

        // NT Compatibility Modifying OnDamaged Hooks
        foreach (var hook in OnDamaged.ModifyingOnDamagedHooks)
        {
            Afflictions = hook(CharacterHealth, Afflictions, HitLimb);
        }

        // Run the method corresponding to the identifier (if it exists)
        foreach (Affliction affliction in Afflictions)
        {
            string Identifier = affliction.Prefab.Identifier.Value;

            if (OnDamaged.OnDamagedMethods.TryGetValue(Identifier, out var method))
            {
                float Resistance = HF.GetResistance(CharacterHealth.Character, Identifier, HitLimb.type);
                float Strength = affliction.Strength * (1f - Resistance);
                method(CharacterHealth.Character, Strength, HitLimb.type);
            }
        }

        // NT Compatibility OnDamaged Hooks
        foreach (var hook in OnDamaged.OnDamagedHooks)
        {
            hook(CharacterHealth, AttackResult, HitLimb);
        }
    }
}
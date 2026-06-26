
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using FarseerPhysics.Common;
using Voronoi2;
using static Neurotrauma.HF;
using static Neurotrauma.NTC;
using static Sdl.Joystick;

namespace Neurotrauma
{
    public static class NTFallDamage
    {

        public static bool HasLungs(Character C)
        {
            return HasAffliction(C, "lungremoved");
        }

        public static double GetReduction(double Strength, Item Armor, string Type = "blunttrauma")
        {
            Wearable Wearable = (Wearable)Armor.GetComponentString("Wearable");
            IEnumerable<DamageModifier> Modifiers = Wearable.DamageModifiers;
            foreach (DamageModifier Modifier in Modifiers)
            {
                if (Modifier.AfflictionIdentifiers.Contains(Type))
                {
                    return Strength - Strength * Modifier.DamageMultiplier;
                }
            }
            return 0;
        }

        public static double GetCalculatedReductionSuit(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("deepdivinglarge") || Armor.HasTag("deepdiving"))
            {
                Reduction = GetReduction(Strength, Armor);
            }
            else if (Armor.HasTag("clothing") || Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor);
            }

            return Reduction;
        }

        public static double GetCalculatedReductionClothes(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("deepdivinglarge") || Armor.HasTag("deepdiving"))
            {
                Reduction = GetReduction(Strength, Armor);
            }
            else if (Armor.HasTag("clothing") || Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor);
            }

            return Reduction;
        }

        public static double GetCalculatedReductionHelmet(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor);
            }

            return Reduction;
        }

        public static double GetCalculatedConcussionReduction(Item Armor, double Strength, LimbType Limb)
        {
            if (Armor == null) { return 0; }
            double Reduction = 0;

            if (Armor.HasTag("deepdivinglarge") || Armor.HasTag("deepdiving"))
            {
                Reduction = GetReduction(Strength, Armor, "concussion");
            }
            else if (Armor.HasTag("clothing") || Armor.HasTag("smallitem"))
            {
                Reduction = GetReduction(Strength, Armor, "concussion");
            }

            return Reduction;
        }

        public static void OnChangeFallDamage(float impactDamage, Character character, Vector2 impactPos, Vector2 velocity)
        {
            Print("Fall Damage");

            if (!NTConfig.Get("NT_Calculations", true)) return;
            
            if (!character.IsHuman) return;

            if (character.InWater) return;

            if (character.SelectedBy != null) return;

            if (HasAffliction(character, "cpr_fracturebuff") || HasAffliction(character, "stopcreatureabuse")) return;

            if (!HasAffliction(character, "luabotomy")) SetAffliction(character, "luabotomy", 1,character,0);

            double VelocityMagnitude = Math.MaxMagnitude(velocity.X, velocity.Y);
            VelocityMagnitude = Math.Pow(VelocityMagnitude,1.3);

            // apply fall damage to all limbs based on fall direction
            Vector2 MainLimbPos = character.AnimController.MainLimb.WorldPosition;

            Dictionary<LimbType,double> LimbDotResults = new();
            double MinDotRes = 1000;

            foreach (Limb Limb in character.AnimController.Limbs)
            {
                foreach (LimbType Type in LimbsToCheck)
                {
                    if (Limb.type != Type ) continue;
                    // Fetch the direction of each limb relative to the torso.
                    Vector2 LimbPosition = Limb.WorldPosition;
                    Vector2 PosDif = LimbPosition - MainLimbPos;
                    PosDif.X /= 100;
                    PosDif.Y /= 100;
                    double PosDifMagnitude = Math.MaxMagnitude(PosDif.X, PosDif.Y);
                    if (PosDifMagnitude > 1) PosDif.Normalize();

                    Vector2 NormalizedVelocity = new(velocity.X, velocity.Y);
                    NormalizedVelocity.Normalize();

                    // Compare those directions to the direction we're moving.
                    // This will later be used to hurt the limbs facing impact more than the others
                    double LimbDot = Vector2.Dot(PosDif, NormalizedVelocity);
                    LimbDotResults[Type] = LimbDot;
                    if (MinDotRes > LimbDot) MinDotRes = LimbDot;
                    break;
                }
            }

            // shift all weights out of the negatives
            // increase the weight of all limbs if speed is high
            // the effect of this is that, at higher speeds, all limbs take damage instead of mainly the ones facing the impact site
            foreach (KeyValuePair<LimbType,double> Pair in LimbDotResults)
            {
                LimbType Type = Pair.Key;
                double DotResult = Pair.Value;
                LimbDotResults[Type] = DotResult - MinDotRes + Math.Max(0, (VelocityMagnitude - 30) / 10);
            }

            double WeightSum = 0;
            foreach (KeyValuePair<LimbType, double> Pair in LimbDotResults)
            {
                WeightSum += Pair.Value;
            }

            foreach (KeyValuePair<LimbType, double> Pair in LimbDotResults)
            {
                double RelativeWeight = Pair.Value / WeightSum;

                // lets limit the numbers to the max value of blunttrauma so that resistances make sense
                double DamageInflictedToThisLimb = Math.Min(
                    RelativeWeight * Math.Max(0, Math.Pow(VelocityMagnitude - 10,1.5) * NTConfig.Get("NT_falldamage",1) * .5),
                    NTConfig.Get("NT_falldamageCeiling",1) * 60);

                CauseFallDamage(character,Pair.Key,DamageInflictedToThisLimb);
            }
        }

        public static void CauseFallDamage(Character character, LimbType limbtype, double strength)
        {

        }

    }
}

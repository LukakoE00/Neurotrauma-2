namespace Neurotrauma
{
    public static class NTC
    {
        public static List<Identifier> AfflictionsAffectingVitality = ["bleeding","bleedingnonstop","burn","acidburn","opiateaddiction",
                                                                "lacerations","gunshotwound","bitewounds","explosiondamage",
                                                                "blunttrauma","internaldamage","organdamage","neurotrauma",
                                                                "gangrene","th_amputation","sh_amputation","alcoholaddiction"];

        // Symptom magic
        public static void SetSymptomTrue(HumanUpdate.NTHuman Human, string SymptomIdentifier, int Duration = 2)
        {
            Dictionary<string, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanSymptomData> Afflictions = Human.LocalAfflictions.UpdatingSymptoms;
            HumanUpdate.NTHuman.CharacterAfflictions.NTHumanSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateTime = Duration;
        }

        public static void SetSymptomFalse(HumanUpdate.NTHuman Human, string SymptomIdentifier, int Duration = 2)
        {
            Dictionary<string, HumanUpdate.NTHuman.CharacterAfflictions.NTHumanSymptomData> Afflictions = Human.LocalAfflictions.UpdatingSymptoms;
            HumanUpdate.NTHuman.CharacterAfflictions.NTHumanSymptomData Sym = Afflictions[SymptomIdentifier];
            Sym.HumanUpdateStoptime = Duration;
        }

        public static void DebugPrintAllData() // UNFINISHED
        {
            string Res = "Neurotrauma Compatibility Data:\n";
            foreach (KeyValuePair<Character,HumanUpdate.NTHuman> Pair in NeurotraumaInit.HU.GetUpdatingCharacters())
            {
                Character Char = Pair.Key;
                HumanUpdate.NTHuman NTHum = Pair.Value;
                Res += "\n" + Char.Name;

            }

            HF.Print(Res); // Not 1:1 with OG NT
        }

        public static List<Action<HumanUpdate.NTHuman>> PreHumanUpdateHooks = new(); // Store our functions to call in here.

        public static void AddPreHumanUpdateHook(Action<HumanUpdate.NTHuman> Hook)
        {
            PreHumanUpdateHooks.Add(Hook);
        }

        public static List<Action<HumanUpdate.NTHuman>> PostHumanUpdateHooks = new(); // Store our functions to call in here.

        public static void AddPostHumanUpdateHook(Action<HumanUpdate.NTHuman> Hook)
        {
            PostHumanUpdateHooks.Add(Hook);
        }

        public static List<Action<CharacterHealth,AttackResult,Limb>> OnDamagedHooks = new();

        public static void AddOnDamagedHook(Action<CharacterHealth, AttackResult, Limb> Hook)
        {
            OnDamagedHooks.Add(Hook);
        }

        public static List<Func<CharacterHealth, AttackResult, Limb, Affliction>> ModifyingOnDamagedHooks = new(); // I Dont't think this one works.

        public static void AddModifyingOnDamagedHook(Func<CharacterHealth, AttackResult, Limb, Affliction> Hook) // I Dont't think this one works.
        {
            ModifyingOnDamagedHooks.Add(Hook);
        }

        public static void AddHematologyAffliction(string Identifier) 
        {

        }

        public class AffApplication
        {
            double XPGain = 1;
            string Case = "";
            Action<Item,HumanUpdate.NTHuman, HumanUpdate.NTHuman,Limb> ActionUpdate = null;
        }


    }

}
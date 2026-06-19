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
    }

}
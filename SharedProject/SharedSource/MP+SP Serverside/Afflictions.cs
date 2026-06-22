using MoonSharp.Interpreter;
using static Barotrauma.LuaCs.NetworkingService;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma
{

    // The priority defines at wich frequency the affliction gets updated. 
    public enum AfflictionPriority : int
    {
        LOW = 6 * 60,  // Every 6s
        MEDIUM = 4 * 60, // Every 4s
        HIGH = 2 * 60 // Every 2s
    }

    // Contains the list of every affliction defined by Neurotrauma. Addons should add their afflictions there.
    // We should provide functions to Lua to add Afflictions here. 
    public static class NTAfflictions
    {
        public static double DeltaTime = 2;

        public static Dictionary<string, NTAffliction> Afflictions { get; } = new Dictionary<string, NTAffliction>(); // Stores all of our registered afflictions (regardless of categeory)

        private static List<string> AfflictionsLOW = [];
        private static List<string> AfflictionsMEDIUM = [];
        private static List<string> AfflictionsHIGH = [];

        public static void DefineAllAfflictions()
        {
            NTAfflictionsToAdd AffsToAdd = new();
        }

        public static void RegisterAffliction(string id, NTAffliction affliction)
        {
            if (!Afflictions.ContainsKey(id))
            {
                Afflictions.Add(id, affliction);
                switch (affliction.Priority)
                {
                    case AfflictionPriority.LOW:
                        AfflictionsLOW.Add(id);
                        break;
                    case AfflictionPriority.MEDIUM:
                        AfflictionsMEDIUM.Add(id);
                        break;
                    case AfflictionPriority.HIGH:
                        AfflictionsHIGH.Add(id);
                        break;
                }
            } else
            {
                LuaCsLogger.LogError($"Affliction with id {id} already exists! Multiple addons might be trying to register the same affliction.\n" +
                    $"If you're trying to override an affliction update function, use OverrideAffliction instead.");
            }
            
        }

        // If an addon needs to change how an affliction works. Could create some incompatibility if multiple addons override the same shit but that's on them, not my problem
        public static void OverrideAffliction(string id, NTAffliction affliction)
        {
            if (Afflictions.ContainsKey(id))
            {
                Afflictions[id] = affliction;
            }
            else
            {
                LuaCsLogger.LogError($"Affliction with id {id} does not exist! Can't override it.");
            }
        }


        // I recommend running this function only once OnLoadCompleted as it could be perf inducing.
        public static Dictionary<string, NTAffliction> GetAfflictionsByPriority(AfflictionPriority priority)
        {
            Dictionary<string, NTAffliction> output = [];

            switch (priority)
            {
                case AfflictionPriority.LOW:
                    AfflictionsLOW.ForEach(affID =>
                    {
                        output.Add(affID, Afflictions[affID]);
                    });
                    break;
                case AfflictionPriority.MEDIUM:
                    AfflictionsMEDIUM.ForEach(affID =>
                    {
                        output.Add(affID, Afflictions[affID]);
                    });
                    break;
                case AfflictionPriority.HIGH:
                    AfflictionsHIGH.ForEach(affID =>
                    {
                        output.Add(affID, Afflictions[affID]);
                    });
                    break;
            }

            return output;
        }

        public static bool HasAffliction(string id)
        {
            return Afflictions.ContainsKey(id);
        }

        public static NTAffliction IDToNTAff(string id)
        {
            if (Afflictions.ContainsKey(id))
            {
                return Afflictions[id];
            }
            return null;
        }

    }

    /// <summary>
    /// The abstract template of our Afflictions. This class and any of it's descendants are never instantiated for a player class. Rather, we use the outline of this affliction class
    /// determine the results of the affliction. The strength is stored seperately in the NTHuman character class!
    /// </summary>
    public abstract class NTAffliction // Added to NTHuman updatingAfflictions
    {
        /// <summary>
        /// Should this affliction always be running? If on, regardless of current affliction strength, this will update.
        /// </summary>
        public bool Const = false;
        /// <summary>
        /// The minimum strength the affliction can have.
        /// </summary>
        public double MinStrength { get; set; }
        /// <summary>
        /// The maximum strength the affliction can have.
        /// </summary>
        public double MaxStrength { get; set; }

        /// <summary>
        /// The strength of the affliction on creation.
        /// </summary>
        public double DefaultStrength { get; set; }
        /// <summary>
        /// The priority of our affliction, higher intervals meaner more updates.
        /// </summary>
        public AfflictionPriority Priority { get; set; }
        /// <summary>
        /// If false, doesnt update on stasis.
        /// </summary>
        public bool IgnoreStasis { get; set; } = true;
        /// <summary>
        /// The ID of the affliction.
        /// </summary>
        public string ID = "";
        /// <summary>
        /// The main update function of our affliction.
        /// </summary>
        public Action<HumanUpdate.NTHuman,string,LimbType> UpdateAction = 
            (HumanUpdate.NTHuman C, string ID, LimbType Limb) => 
            { 
                // Insert your Affliction Update in here.
            };
        public NTAffliction(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0,
                                        AfflictionPriority NewPriority = AfflictionPriority.HIGH)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength,NewMinStrength,NewMaxStrength);
            Priority = NewPriority;
        }
    }

    public class NTNonLimbAffliction : NTAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanNonLimbAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };
        
        public NTNonLimbAffliction(string NewID, double NewMinStrength =0, double NewMaxStrength= 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH) : 
                                        base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);
            Priority = NewPriority;
        }
    }

    public class NTLimbAffliction : NTAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanLimbAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTLimbAffliction(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);
            Priority = NewPriority;
            IgnoreStasis = false;
        }

        public List<LimbType> AllowedLimbs { get; set; } = HF.LimbsToCheck; // I'll add this one later.
    }

    public class NTBloodAffliction : NTAffliction
    {
        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanBloodAffData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanBloodAffData AffData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTBloodAffliction(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);   
            Priority = NewPriority;
        }
    }

    public class NTSymptom : NTNonLimbAffliction
    {

        public Action<HumanUpdate.NTHuman, string, LimbType, HumanUpdate.NTHumanSymptomData> UpdateAction =
            (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanSymptomData SymData) =>
            {
                // Insert your Affliction Update in here.
            };

        public NTSymptom(string NewID, double NewMinStrength = 0, double NewMaxStrength = 100, double NewDefaultStrength = 0, AfflictionPriority NewPriority = AfflictionPriority.HIGH) 
                            : base(NewID, NewMinStrength, NewMaxStrength, NewDefaultStrength, NewPriority)
        {
            ID = NewID;
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DefaultStrength = Math.Clamp(NewDefaultStrength, NewMinStrength, NewMaxStrength);
            Priority = NewPriority;
        }
    }


    public abstract class AfflictionsPackage 
    {
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> AfflictionsToAdd =
                        new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> LimbAfflictionsToAdd =
                                new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();
        Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>> BloodAfflictionsToAdd =
                                new Dictionary<string, Action<HumanUpdate.NTHuman, string, LimbType>>();

        private void AddAfflictions() // Create your afflictions in here.
        {
            throw new NotImplementedException();
        }

        private void AddLimbAfflictions() // Create your afflictions in here.
        {
            throw new NotImplementedException();
        }

        private void AddBloodAfflictions() // Create your afflictions in here.
        {
            throw new NotImplementedException();
        }

        private void AffSymptoms()
        {
            throw new NotImplementedException();
        }
    }


    public class  NTAfflictionsToAdd : AfflictionsPackage
    {

        // Human Updates update functons have 
        // Param 1: NTHuman (The character we updating) [C]
        // Param 2: String (The affliction Identifier) [I]
        // Param 3: LimbType (The limb the aff is on) [L]
        // Param 4: Type???? Idk I forgot what this one is.

        Dictionary<string, NTNonLimbAffliction> AfflictionsToAdd =
                                new Dictionary<string, NTNonLimbAffliction>();
        Dictionary<string, NTLimbAffliction> LimbAfflictionsToAdd =
                                new Dictionary<string, NTLimbAffliction>();
        Dictionary<string, NTBloodAffliction> BloodAfflictionsToAdd =
                                new Dictionary<string, NTBloodAffliction>();
        Dictionary<string, NTSymptom> SymptomsToAdd =
                                new Dictionary<string, NTSymptom>();

        public NTAfflictionsToAdd() // Initalize the afflictions.
        {
            AddAfflictions();
            AddLimbAfflictions();
            AddBloodAfflictions();
            AddSymptoms();
        }

        private void AddAfflictions() // Create your afflictions in here.
        {

            // EXAMPLE AFFLICTION

            AfflictionsToAdd["respiratoryarrest"] = new("respiratoryarrest", 0,100,0,AfflictionPriority.HIGH);
            AfflictionsToAdd["respiratoryarrest"].Const = true; // This affliction should always run
            AfflictionsToAdd["respiratoryarrest"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanNonLimbAffData AffData) =>
                {
                    AffData.Strength -= (0.05 + HF.BoolToNum(C.GetSymptomAffData("unconsciousness").Strength < .1, .45f)) * NTAfflictions.DeltaTime;

                    if
                        (!NTC.HasSymptomFalse(C, "respiratoryarrest")
                        && (
                        C.GetBoolStatStrength("stasis")
                        || C.GetAffData("lungremoved").Strength > 0
                        || C.GetAffData("brainremoved").Strength > 0
                        || C.GetAffData("opiateoverdose").Strength > 50
                        || C.GetAffData("lungdamage").Strength > 99 && HF.Chance(.8f)
                        || C.GetAffData("traumaticshock").Strength > 30 && HF.Chance(.2f)
                        || (
                            (C.GetAffData("neurotrauma").Strength > 100 || C.GetAffData("neurotrauma").Strength > 70
                            && HF.Chance(.05f))
                        )
                      )
                      )
                    {
                        AffData.Strength += 10;
                    }
                };

            AfflictionsToAdd["fracturedribs"] = new("fracturedribs");
            AfflictionsToAdd["fracturedneck"] = new("fracturedneck");
            AfflictionsToAdd["fracturedskull"] = new("fracturedskull");
            AfflictionsToAdd["stroke"] = new("stroke");
            AfflictionsToAdd["neurotrauma"] = new("neurotrauma");
            AfflictionsToAdd["seizure"] = new("seizure");
            AfflictionsToAdd["coma"] = new("coma");
            AfflictionsToAdd["traumaticshock"] = new("traumaticshock");
            // Drugs//
            AfflictionsToAdd["opiateoverdose"] = new("opiateoverdose");
            // Organs //
            AfflictionsToAdd["lungdamage"] = new("lungdamage");
            AfflictionsToAdd["lungremoved"] = new("lungremoved");
            AfflictionsToAdd["brainremoved"] = new("brainremoved");
            AfflictionsToAdd["brainswap"] = new("brainswap");
            // Limbs //
            AfflictionsToAdd["tra_amputation"] = new("tra_amputation");
            AfflictionsToAdd["tla_amputation"] = new("tla_amputation");
            AfflictionsToAdd["trl_amputation"] = new("trl_amputation");
            AfflictionsToAdd["tll_amputation"] = new("tll_amputation");
            AfflictionsToAdd["th_amputation"] = new("th_amputation");
            AfflictionsToAdd["sra_amputation"] = new("sra_amputation");
            AfflictionsToAdd["sla_amputation"] = new("sla_amputation");
            AfflictionsToAdd["srl_amputation"] = new("srl_amputation");
            AfflictionsToAdd["sll_amputation"] = new("sll_amputation");
            AfflictionsToAdd["sh_amputation"] = new("sh_amputation");

            // Now add these afflictions.
            foreach (KeyValuePair<string,NTNonLimbAffliction> Pair in AfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddLimbAfflictions()
        {
            

            LimbAfflictionsToAdd["bleeding"] = new("bleeding");
            LimbAfflictionsToAdd["bleeding"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb, HumanUpdate.NTHumanLimbAffData AffData) =>
                {
                };
            LimbAfflictionsToAdd["bonedamage"] = new("bonedamage");
            LimbAfflictionsToAdd["stimulatedbonegrowth"] = new("stimulatedbonegrowth");
            LimbAfflictionsToAdd["fracturedextremity"] = new("fracturedextremity");
            LimbAfflictionsToAdd["dislocation"] = new("dislocation");
            LimbAfflictionsToAdd["tourniqueted"] = new("tourniqueted");
            LimbAfflictionsToAdd["plastercast"] = new("plastercast");
            LimbAfflictionsToAdd["plastercast"] = new("plastercast");
            LimbAfflictionsToAdd["arterialcut"] = new("arterialcut");
            LimbAfflictionsToAdd["gangrene"] = new("gangrene");

            foreach (KeyValuePair<string, NTLimbAffliction> Pair in LimbAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddBloodAfflictions()
        {
            BloodAfflictionsToAdd["bloodpressure"] = new("bloodpressure");
            BloodAfflictionsToAdd["hypoxemia"] = new("hypoxemia");
            BloodAfflictionsToAdd["alkalosis"] = new("alkalosis");
            BloodAfflictionsToAdd["acidosis"] = new("acidosis");
            BloodAfflictionsToAdd["hemotransfusionshock"] = new("hemotransfusionshock");
            BloodAfflictionsToAdd["sepsis"] = new("sepsis");
            BloodAfflictionsToAdd["immunity"] = new("immunity");

            foreach (KeyValuePair<string, NTBloodAffliction> Pair in BloodAfflictionsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }

        private void AddSymptoms()
        {
            SymptomsToAdd["cough"] = new("cough");
            SymptomsToAdd["paleskin"] = new("paleskin");
            SymptomsToAdd["lightheadedness"] = new("lightheadedness");
            SymptomsToAdd["blurredvision"] = new("blurredvision");
            SymptomsToAdd["confusion"] = new("confusion");
            SymptomsToAdd["headache"] = new("headache");
            SymptomsToAdd["legswelling"] = new("legswelling");
            SymptomsToAdd["weakness"] = new("weakness");
            SymptomsToAdd["wheezing"] = new("wheezing");
            SymptomsToAdd["vomiting"] = new("vomiting");
            SymptomsToAdd["vomitingblood"] = new("vomitingblood");
            SymptomsToAdd["fever"] = new("fever");
            SymptomsToAdd["abdominaldiscomfort"] = new("abdominaldiscomfort");
            SymptomsToAdd["bloating"] = new("bloating");
            SymptomsToAdd["jaundice"] = new("jaundice");
            SymptomsToAdd["sweating"] = new("sweating");
            SymptomsToAdd["palpitations"] = new("palpitations");
            SymptomsToAdd["unconsciousness"] = new("unconsciousness");
            SymptomsToAdd["inflammation"] = new("inflammation");
            SymptomsToAdd["spasm"] = new("spasm");
            SymptomsToAdd["craving"] = new("craving");
            SymptomsToAdd["nausea"] = new("nausea");
            SymptomsToAdd["chestpain"] = new("chestpain");
            SymptomsToAdd["abdominalpain"] = new("abdominalpain");
            SymptomsToAdd["intensepain"] = new("intensepain");

            foreach (KeyValuePair<string, NTSymptom> Pair in SymptomsToAdd)
            {
                NTAfflictions.RegisterAffliction(Pair.Key, Pair.Value);
            }
        }
    }
}


// wtf am i doing with my life
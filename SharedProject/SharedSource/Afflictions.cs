using MoonSharp.Interpreter;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma
{

    // The priority defines at wich frequency the affliction gets updated. 
    public enum AfflictionPriority : int
    {
        LOW = 8 * 60,  // Every 8s
        MEDIUM = 4 * 60, // Every 4s
        HIGH = 2 * 60 // Every 2s
    }

    // Contains the list of every affliction defined by Neurotrauma. Addons should add their afflictions there.
    // We should provide functions to Lua to add Afflictions here. 

    //TODO ADD A WAY TO IMPLEMENT LIMB SPECIFIC AFFLICTIONS, do we really need a special way ?
    public static class NTAfflictions
    {

        public static Dictionary<string, NTAffliction> Afflictions { get; } = new Dictionary<string, NTAffliction>();

        private static List<string> AfflictionsLOW = [];
        private static List<string> AfflictionsMEDIUM = [];
        private static List<string> AfflictionsHIGH = [];

        public static void RegisterAffliciton(string id, NTAffliction affliction)
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
                LuaCsLogger.LogError($"Affliction with id {id} already exists! Multiple addons might be trying to register the same affliciton.\n" +
                    $"If you're trying to override an affliction update function, use OverrideAffliciton instead.");
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

    }

    public abstract class NTAffliction // Added to NTHuman updatingAfflictions
    {
        public double Strength { get; set; } = 0;
        public double MinStrength { get; set; }
        public double MaxStrength { get; set; }
        public string Identifier { get; set; } = "";
        public List<string> DependentAfflictions { get; set; } = [];
        public AfflictionPriority Priority { get; set; }
        public string Name = "";
        public Action<HumanUpdate.NTHuman,string,LimbType> UpdateAction = 
            (HumanUpdate.NTHuman C, string ID, LimbType Limb) => 
            { 
                // Insert your Affliction Update in here.
            };
        public NTAffliction(double NewMinStrength, double NewMaxStrength,
                                        List<string> NewDependentAfflictions, AfflictionPriority NewPriority = AfflictionPriority.HIGH)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DependentAfflictions = NewDependentAfflictions;
            Priority = NewPriority;
        }
    }

    public class NTNonLimbAffliction : NTAffliction
    {
        public NTNonLimbAffliction(double NewMinStrength, double NewMaxStrength, List<string> NewDependentAfflictions, AfflictionPriority NewPriority = AfflictionPriority.HIGH) : 
                                        base(NewMinStrength, NewMaxStrength, NewDependentAfflictions, NewPriority)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DependentAfflictions = NewDependentAfflictions ;
            Priority = NewPriority;
        }
    }

    public class NTLimbAffliction : NTAffliction
    {
        public NTLimbAffliction(double NewMinStrength, double NewMaxStrength,
                                         List<string> NewDependentAfflictions, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewMinStrength, NewMaxStrength, NewDependentAfflictions, NewPriority)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DependentAfflictions = NewDependentAfflictions;
            Priority = NewPriority;
        }

        public List<LimbType> AllowedLimbs { get; set; } = HF.LimbsToCheck; // I'll add this one later.
    }

    public class NTBloodAffliction : NTAffliction
    {
        public NTBloodAffliction(double NewMinStrength, double NewMaxStrength,
                                        List<string> NewDependentAfflictions, AfflictionPriority NewPriority = AfflictionPriority.HIGH) :
                                        base(NewMinStrength, NewMaxStrength, NewDependentAfflictions, NewPriority)
        {
            MinStrength = NewMinStrength;
            MaxStrength = NewMaxStrength;
            DependentAfflictions = NewDependentAfflictions;
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

        public void Initialize() // Initalize the afflictions.
        {
            AddAfflictions();
            AddLimbAfflictions();
            AddBloodAfflictions();
        }

        private void AddAfflictions() // Create your afflictions in here.
        {
            AfflictionsToAdd["Example1"] = new(0, 100, []);
            AfflictionsToAdd["Example1"].UpdateAction = 
                (HumanUpdate.NTHuman C, string ID, LimbType Limb) =>
            {

            };

        }

        private void AddLimbAfflictions()
        {
            AfflictionsToAdd["Example1Limb"] = new(0, 100, []);
            AfflictionsToAdd["Example1Limb"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb) =>
            {

            };
        }

        private void AddBloodAfflictions()
        {
            AfflictionsToAdd["Example1Blood"] = new(0, 100, []);
            AfflictionsToAdd["Example1Blood"].UpdateAction =
                (HumanUpdate.NTHuman C, string ID, LimbType Limb) =>
            {

            };
        }
    }
}


// wtf am i doing with my life
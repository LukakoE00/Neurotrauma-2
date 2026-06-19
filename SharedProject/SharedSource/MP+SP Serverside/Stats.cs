namespace Neurotrauma
{
    public static class NTStats
    {
        public static Dictionary<string, NTStat> Stats = new Dictionary<string, NTStat>();

        public static void DefineAllStats()
        {
            // This isnt done, just a basic template.
            Stats["healingrate"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 

            });
            Stats["specificOrganDamageHealMultiplier"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["neworgandamage"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["clottingrate"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            {

            });
            Stats["bloodamount"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["stasis"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["sedated"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["withdrawal"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["availableoxygen"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["speedmultiplier"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["lockleftarm"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["lockrightarm"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["lockleftleg"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["lockrightleg"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["wheelchaired"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["bonegrowthCount"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
            Stats["burndamage"] = new NTStat(0, 100, 1, (HumanUpdate.NTHuman C) => 
            { 
            
            });
        }

        public static void RegisterStat(string id, NTStat NewStat) // Register a new stat to the NTStat Dictionary.
        {
            if (!Stats.ContainsKey(id))
            {
                Stats.Add(id, NewStat);
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} already exists! Multiple addons might be trying to register the same stat.\n" +
                    $"If you want to recalculate a stat, use CharacterStats.RecalculateSingle instead of registering it again.");
            }
        }

        public static void OverrideStat(string id, NTStat NewStat) // Override a stat in NTStat Dictionary.
        {
            if (Stats.ContainsKey(id))
            {
                Stats[id] = NewStat;
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} does not exist! You can't override a stat that doesn't exist.\n" +
                    $"If you want to register a new stat, use RegisterStat instead of trying to override it.");
            }
        }

        public static void RemoveStat(string id) // Remove a stat to the NTStat Dictionary.
        {
            if (!Stats.ContainsKey(id))
            {
                Stats.Remove(id);
            }
            else
            {
                LuaCsLogger.LogError($"Stat with id {id} does not exist! You can't remove a stat that doesn't exist.");
            }
        }

    }

    public class NTStat(double MinStrength = 0, double MaxStrength = 1, double DefaultStrength = 1, Action<HumanUpdate.NTHuman> Update = null)
    {
        private double Strength = Math.Clamp(DefaultStrength, MinStrength, MaxStrength);
        private Action<HumanUpdate.NTHuman> StatUpdate = Update;

        public void AddStrength(double AddingAmount)
        {
            Strength += AddingAmount;
        }

        public void RemoveStrength(double RemovingAmount) // This function is kinda stupid.
        {
            AddStrength(-RemovingAmount);
        }

        public void SetStrength(double SetingAmount)
        {
            Strength = SetingAmount;
        }

        public void Recalculate(HumanUpdate.NTHuman Character)
        {
            if (StatUpdate != null)
            {
                StatUpdate(Character);
            }
        }
    }
}
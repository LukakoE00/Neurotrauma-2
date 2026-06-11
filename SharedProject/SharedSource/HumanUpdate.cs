using Barotrauma.LuaCs.Compatibility;
using Barotrauma.LuaCs.Events;
using MonoMod.RuntimeDetour;
using static Barotrauma.Networking.MessageFragment;

namespace Neurotrauma;

class HumanUpdate
{
    private static int UpdateCooldown = 0;
    private static readonly int UpdateIntervalHigh = (int)AfflictionPriority.HIGH; // 120 = 2s
    private static readonly int UpdateIntervalMedium = (int)AfflictionPriority.MEDIUM; // 240 = 4s
    private static readonly int UpdateIntervalLow = (int)AfflictionPriority.LOW; // 480 = 8s
    private List<NTHuman> UpdatingHumans = new List<NTHuman>();
    private List<NTMonster> UpdatingMonsters = new List<NTMonster>();

    // ---------------------------------------- NT Human Update Classes -------------------------------------------------- \\

    public class NTHuman(Character Human)
    {

        public Character Human = Human; // Our Human Ref
        public CharacterStats LocalStats = new CharacterStats();
        public CharacterAfflictions LocalAfflictions = new CharacterAfflictions(Human);

        public CharacterStats? GetStats()
        {
            return LocalStats;
        }

        public void Update(List<AfflictionPriority> Priorities)
        {
            LuaCsLogger.Log(Human.Prefab.Identifier.ToString());
        }

        public class CharacterAfflictions(Character Human)
        {
            public Character Human = Human; // Our Human Ref
            private static Dictionary<string,NTNonLimbAffliction> UpdatingAfflictions = new Dictionary<string, NTNonLimbAffliction>(); // Stores the ID's of our updating afflictions.
            private static Dictionary<string, NTLimbAffliction> UpdatingLimbAfflictions = new Dictionary<string, NTLimbAffliction>(); // Stores the ID's of our updating (Limb) afflictions.

            public NTAffliction RegisterGetAffliction(string ID,double MinStrength, double MaxStrength,
                                                List<string> DependentAfflictions, AfflictionPriority Priority = AfflictionPriority.HIGH, bool LimbSpecific = false) // Call this at the start of each affliction.
            {

                if (NTAfflictions.HasAffliction(ID) && (UpdatingAfflictions.ContainsKey(ID) || UpdatingLimbAfflictions.ContainsKey(ID)))
                {
                    if (!LimbSpecific)
                    { 
                        NTNonLimbAffliction NewAffliction = (NTNonLimbAffliction) CreateAffliction(ID,MinStrength, MaxStrength, DependentAfflictions, Priority);
                        UpdatingAfflictions[ID] = NewAffliction;
                        NewAffliction.Name = ID;
                    }
                    else
                    {
                        NTLimbAffliction NewAffliction = (NTLimbAffliction) CreateAffliction(ID,MinStrength, MaxStrength, DependentAfflictions, Priority, true);
                        UpdatingLimbAfflictions[ID] = NewAffliction;
                        NewAffliction.Name = ID;
                    }
                }

                if (!LimbSpecific)
                {
                    return UpdatingAfflictions[ID];
                }
                else
                {
                    return UpdatingLimbAfflictions[ID];
                }
            }

            public void RemoveAffliction(string ID)
            {
                if (NTAfflictions.HasAffliction(ID))
                {
                    UpdatingAfflictions.Remove(ID);
                }
            }

            public List<string> GetUpdatingAfflictons()
            {
                return UpdatingAfflictions.Keys.ToList();
            }

            public NTAffliction GetAff(string ID)
            {
                if (UpdatingAfflictions.ContainsKey(ID))
                {
                    return UpdatingAfflictions[ID];
                }
                return UpdatingLimbAfflictions[ID];
            }

            public NTAffliction CreateAffliction(string ID, double MinStrength, double MaxStrength,
                                                List<string> DependentAfflictions, AfflictionPriority Priority, bool LimbSpecific = false)
            {
                if (LimbSpecific)
                {
                    NTNonLimbAffliction NewNonLimbAffliction = new(MinStrength,MaxStrength,ID,DependentAfflictions,Priority);
                    return NewNonLimbAffliction;
                }
                NTLimbAffliction NewLimbAffliction = new(MinStrength, MaxStrength, ID, DependentAfflictions, Priority);
                return NewLimbAffliction;
            }
        }

        public class CharacterStats
        {
            private Dictionary<string, int> Stats { get; }

            public CharacterStats()
            {
                Stats = new Dictionary<string, int>();
            }

            // If you want to recalculate a single stat
            public void RecalculateSingle(string id, NTUpdateFunctionInfos character)
            {

                if (NTStats.Stats[id] != null)
                {
                    NTStats.Stats[id].Recalculate(character);
                }
            }

            // If we need to recalculate every stats for a character we can call this
            public void RecalculateAll(NTUpdateFunctionInfos character)
            {
                foreach (var stat in NTStats.Stats)
                {
                    stat.Value.Recalculate(character);
                }
            }
        }

    }

    class NTMonster(Character Monster) // To Do
    {
        public Character Monster = Monster; // Our Monster Ref

    }


    // ---------------------------------------- The Human Update -------------------------------------------------- \\

    public void AddEntityToUpdate(Entity AddingEntity)
    {
        if (AddingEntity is Character)
        {
            Character NewCharacter = (Character)AddingEntity;
            if (NewCharacter.IsHuman)
            {
                AddHumanToUpdate(NewCharacter);
            }
            else
            {
                AddMonsterToUpdate(NewCharacter);
            }
        }
    }

    public void RemoveEntityFromUpdate(Entity RemovingEntity)
    {
        if (RemovingEntity is Character)
        {
            Character NewCharacter = (Character)RemovingEntity;
            if (NewCharacter.IsHuman)
            {
                RemoveHumanFromUpdate(NewCharacter);
            }
            else
            {
                RemoveMonsterFromUpdate(NewCharacter);
            }
        }
    }

    public void AddHumanToUpdate(Character AddedCharacter)
    {
        NTHuman NewNTHuman = new NTHuman(AddedCharacter); // Hopefully this wont create a memory leak.
        if (!UpdatingHumans.Contains(NewNTHuman))
        {
            UpdatingHumans.Add(NewNTHuman);
        }
    }

    public void RemoveHumanFromUpdate(Character RemovingCharacter) // Probably a better way to do this.
    {
        NTHuman HumanToRemove = null; // We store the index of what to remove so we don't remove while iterating.
        foreach (NTHuman Human in UpdatingHumans)
        {
            if (Human.Human == RemovingCharacter)
            {
                HumanToRemove = Human;
                break;
            }
        }
        if (HumanToRemove != null)
        {
            UpdatingHumans.Remove(HumanToRemove);
        }
    }

    public void AddMonsterToUpdate(Character AddedMonster)
    {
        if (!AddedMonster.IsHuman)
        {
            NTMonster NewNTMonster = new NTMonster(AddedMonster);
            if (!UpdatingMonsters.Contains(NewNTMonster))
            {
                UpdatingMonsters.Add(NewNTMonster);
            }
        }
    }

    public void RemoveMonsterFromUpdate(Character RemovingMonster) // Probably a better way to do this.
    {
        NTMonster MonsterToRemove = null; // We store the index of what to remove so we don't remove while iterating.
        foreach (NTMonster Monster in UpdatingMonsters)
        {
            if (Monster.Monster == RemovingMonster)
            {
                MonsterToRemove = Monster;
                break;
            }
        }
        if (MonsterToRemove != null)
        {
            UpdatingMonsters.Remove(MonsterToRemove);
        }
    }

    // Returns a list 
    private static List<AfflictionPriority> GetLowestPriority(int cd)
    {
        List<AfflictionPriority> output = [];

        if (cd % UpdateIntervalLow == 0)
        {
            output.Add(AfflictionPriority.LOW);
            output.Add(AfflictionPriority.MEDIUM);
            output.Add(AfflictionPriority.HIGH);
            UpdateCooldown = 0;

        } else if (cd % UpdateIntervalMedium == 0)
        {
            output.Add(AfflictionPriority.MEDIUM);
            output.Add(AfflictionPriority.HIGH);

        } else if (cd % UpdateIntervalHigh == 0)
        {
            output.Add(AfflictionPriority.HIGH);
        }

        return output;
    }

    private int Interval = 120;
    private int Tick = 0;
    private double NTDeltaTime = UpdateIntervalHigh / 120;
    // Gets called 60 times a second
    public void ThinkUpdate(double fixedDeltaTime)
    {
        // If game paused we just skip
        if (HF.GameIsPaused()) return;

        Tick--; // Decrement our tick.
        if (!(Tick < 0)) { return; }
        else { Tick = Interval; HF.Print("Human Update Tick"); }

        // We check if timer is up
        List<AfflictionPriority> checkedPriorities = GetLowestPriority(UpdateCooldown);
        if (checkedPriorities.Count == 0) return;

        Update(checkedPriorities);

        UpdateCooldown++;
    }

    private void Update(List<AfflictionPriority> priorities)
    {
        List<Character> CHList = Character.CharacterList;

        //foreach (Character c in CHList) // This is the old fetching Character for Update system. We're now using a hook method instead. Leaving this here so we can go back incase it breaks.
        //{
            //if (c.isDead) continue; // Skip to next iteration

            //if (c.IsHuman && c.Enabled)
            //{
                //AddHumanToUpdate(c);
            //}
            //else
            //{
                //AddMonsterToUpdate(c);
            //}
        //}

        UpdateHumans(priorities);

        UpdateMonsters(priorities);
    }

    private void UpdateHumans(List<AfflictionPriority> priorities)
    {
        foreach (NTHuman Human in UpdatingHumans)
        {

            Human.Update(priorities);

        }
    }

    private void UpdateMonsters(List<AfflictionPriority> priorities)
    {

    }

    private static void UpdateMonster(Character character)
    {

    }
}
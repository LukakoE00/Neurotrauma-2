namespace Neurotrauma;

public class NTItemMethods
{

    /// <summary>
    /// Contains all the data necessary to add an Affliction to DrainageAfflictions.
    /// </summary>
    public class ItemsAfflictionInfos { 
    
        /// <summary>
        /// The ID defined in the XML. The affliction CANNOT BE Limb-Specific.
        /// </summary>
        public string AfflictionID { get; }

        /// <summary>
        /// The amount of XP given to the surgery or medical skill when the item is applied successfully.
        /// </summary>
        public int XPGain { get; }

        /**<summary>This function will be run to know if the affliction can be cured by the drainage.</summary>
           <example>
           <code>
           bool conditionFunction(ItemUpdateFunctionInfos infos)
           {
               return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
           }
           </code>
           </example>
        */
        public Func<ItemUpdateFunctionInfos, bool> Conditions { get; }

        public ItemsAfflictionInfos(string affID, int xpGain, Func<ItemUpdateFunctionInfos, bool> conditions) 
        { 
            //TODO check if affID is limb specific and throw error if so

            this.AfflictionID = affID;
            this.XPGain = xpGain;
            this.Conditions = conditions;
        }
    
    }

    /// <summary>
    /// Contains the list of every afflictions cured by drainage.
    /// </summary>
    public static List<ItemsAfflictionInfos> DrainageAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of every afflictions removable by traumashears and knives.
    /// </summary>
    public static List<string> CuttableAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of every afflictions removable by traumashears only.
    /// </summary>
    public static List<string> TraumaShearsAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of every afflictions healable by sutures.
    /// </summary>
    public static List<ItemsAfflictionInfos> SutureAfflictions { get; } = [];

    /// <summary>
    /// Contains the list of detectable afflictions when using the Hematology Analyzer
    /// </summary>
    public static List<string> HematologyDetectable { get; } = [];

    /// <summary>
    /// Contains all the data necessary for an item use function.
    /// </summary>
    public class ItemUpdateFunctionInfos
    {
        public Barotrauma.Item item { get; }
        public Character user {  get; }
        public Character target { get; }
        public Limb targetLimb { get; }

        public ItemUpdateFunctionInfos(Barotrauma.Item item, Character user, Character target, Limb targetLimb)
        {
            this.item = item;
            this.user = user;
            this.target = target;
            this.targetLimb = targetLimb;
        }
    }

    public static Dictionary<string, Action<ItemUpdateFunctionInfos>> NTItemsRegistry { get; } = new Dictionary<string, Action<ItemUpdateFunctionInfos>> { };

    // Used by Diagnostic Tools to format chat output
    public static string FormatLine(string content, Color color)
    {
        if (string.IsNullOrEmpty(content)) { return ""; }
        return $"‖color:{color.R},{color.G},{color.B}‖{content}‖color:end‖";
    }

    public static void DefineAllItems()
    {
        // Azathioprine
        RegisterItemUseFunction("immunosuppressant", infos =>
        {
            bool success = HF.GetSkillRequirmentMet(infos.user, "Medical", 10);
            HF.AddAffliction(infos.target, "afimmunosuppressant", success ? 5 : 3, infos.user);
        });

        // Antibiotic Ointment
        RegisterItemUseFunction("ointment", (infos) =>
        {

            bool success = HF.GetSkillRequirmentMet(infos.user, "medical", 10);

            HF.AddAfflictionLimb(infos.target, "ointmented", infos.targetLimb.type, success ? 120 : 60, infos.user);
            HF.AddAfflictionLimb(infos.target, "infectedwound", infos.targetLimb.type, success ? -72 : -24, infos.user);

            // Check for third degree burn might not be working correctly
            if (HF.GetAfflictionStrengthLimb(infos.target, infos.targetLimb.type, "burn", 0) < 50)
            {
                HF.AddAfflictionLimb(infos.target, "burn", infos.targetLimb.type, success ? -12 : (float)-7.2, infos.user);
            }

            HF.GiveItem(infos.target, "ntsfx_ointment");

        });

        // Scalpel
        RegisterItemUseFunction("advscalpel", infos =>
        {
            // Not in stasis
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target) || HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1)) { return; }

            bool success = HF.GetSurgerySkillRequirmentMet(infos.user, 30);

            if (success)
            {
                HF.AddAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user); // TODO change this to using surgery instead of medical

                HF.SetAfflictionLimb(infos.target, "suturedi", infos.targetLimb.type, 0, infos.user, 0);
                HF.SetAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, 0, infos.user, 0);
                HF.SetAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 0, infos.user, 0);

            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
            }

            HF.GiveItem(infos.target, "ntsfx_slash");
        });

        DrainageAfflictions.Add(new ItemsAfflictionInfos("pneumothorax", 3, infos =>
        {
            return HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);
        }));

        DrainageAfflictions.Add(new ItemsAfflictionInfos("tamponade", 3, infos =>
        {
            bool retractedSkin = HF.HasAfflictionLimb(infos.target, "retractedskin", LimbType.Torso, 95);

            // TODO check for Config thingy

            return retractedSkin;
        }));

        // From 48 lines to 12 my point stands, why tf was the lua function so girthy?
        RegisterItemUseFunction("drainage", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            foreach (var affInfos in DrainageAfflictions)
            {
                if (!affInfos.Conditions(infos)) continue;

                HF.SetAffliction(infos.target, affInfos.AfflictionID, 0, infos.user, 0);
                HF.GiveSurgerySkill(infos.user, affInfos.XPGain);
            }
        });

        // Hemostat
        RegisterItemUseFunction("advhemostat", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target)) { return; }

            if (!HF.HasAffliction(infos.target, "surgeryincision", 99) || HF.HasAffliction(infos.target, "clampedbleeders", 1)) { return; }

            HF.AddAfflictionLimb(infos.target, "clampedbleeders", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user);

        });

        // Skin Retractors
        RegisterItemUseFunction("advretractors", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            if (!HF.CanPerformSurgeryOn(infos.target)) { return; }

            if (!HF.HasAffliction(infos.target, "clampedbleeders", 99) || HF.HasAffliction(infos.target, "retractedskin", 1)) { return; }

            HF.AddAfflictionLimb(infos.target, "retractedskin", infos.targetLimb.type, 1 + HF.GetSurgerySkill(infos.user) / 2, infos.user);
        });

        CuttableAfflictions.Add("bandaged");
        CuttableAfflictions.Add("bandageddirty");
        CuttableAfflictions.Add("tourniqueted");

        TraumaShearsAfflictions.Add("gypsumcast");

        // Trauma Shears
        RegisterItemUseFunction("traumashears", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;
            cuttables = [.. cuttables, .. TraumaShearsAfflictions];

            if (HF.GetSkillRequirmentMet(infos.user, "medical", 10))
            {
                foreach (var affID in cuttables)
                {
                    HF.SetAfflictionLimb(infos.target, affID, infos.targetLimb.type, 0, infos.user, 0);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
            }

        });

        // Diving Knife 
        RegisterItemUseFunction("divingknife", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            List<string> cuttables = CuttableAfflictions;

            if (HF.GetSkillRequirmentMet(infos.user, "medical", 30))
            {
                foreach (var affID in cuttables)
                {
                    HF.SetAfflictionLimb(infos.target, affID, infos.targetLimb.type, 0, infos.user, 0);
                }
            }
            else
            {
                HF.AddAfflictionLimb(infos.target, "bleeding", infos.targetLimb.type, 15, infos.user);
                HF.AddAfflictionLimb(infos.target, "lacerations", infos.targetLimb.type, 10, infos.user);
            }

        });

        // Gypsum
        RegisterItemUseFunction("gypsum", infos =>
        {
            if (HF.HasAffliction(infos.target, "stasis", (float)0.1)) { return; }

            // Needs to be bandaged, not already in a cast, not during a surgery, and the limb needs to be extremity.
            if (!HF.HasAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, (float)0.1) ||
            HF.HasAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, (float)0.1) ||
            HF.HasAfflictionLimb(infos.target, "surgeryincision", infos.targetLimb.type, (float)0.1) ||
            !HF.LimbIsExtremity(infos.targetLimb.type)) { return; }

            if (HF.GetSkillRequirmentMet(infos.user, "medical", (float)40))
            {
                HF.SetAfflictionLimb(infos.target, "bandaged", infos.targetLimb.type, 0, infos.user, 0);
                HF.SetAfflictionLimb(infos.target, "gypsumcast", infos.targetLimb.type, 100, infos.user, 0);
                HF.BreakLimb(infos.target, infos.targetLimb.type, -20);
                HF.GiveSkillScaled(infos.user, "medical", 6000);

            }

            HF.RemoveItem(infos.item);


        });

        // Health Scanner
        RegisterItemUseFunction("healthscanner", infos =>
        {
            LimbType limbType = HF.NormalizeLimbType(infos.targetLimb.type);

            var Battery = infos.item.OwnInventory.GetItemAt(0);
            if (Battery == null) return;

            bool HasVoltage = Battery.Condition > 0;
            if (!HasVoltage) return;

            bool useColoredScanner = NTConfig.Get("NTSCAN_enablecoloredscanner", true);

            Color baseColor = useColoredScanner ? HF.GetColor("NTSCAN_basecolor") : new Color(127, 255, 255);
            Color nameColor = useColoredScanner ? HF.GetColor("NTSCAN_namecolor") : new Color(127, 255, 255);
            Color lowColor = useColoredScanner ? HF.GetColor("NTSCAN_lowcolor") : new Color(127, 255, 255);
            Color medColor = useColoredScanner ? HF.GetColor("NTSCAN_medcolor") : new Color(127, 255, 255);
            Color highColor = useColoredScanner ? HF.GetColor("NTSCAN_highcolor") : new Color(127, 255, 255);
            Color vitalColor = useColoredScanner ? HF.GetColor("NTSCAN_vitalcolor") : new Color(127, 255, 255);
            Color removalColor = useColoredScanner ? HF.GetColor("NTSCAN_removalcolor") : new Color(127, 255, 255);
            Color customColor = useColoredScanner ? HF.GetColor("NTSCAN_customcolor") : new Color(127, 255, 255);

            // Floats
            float lowMedThreshold = NTConfig.Get<float>("NTSCAN_lowmedThreshold", 1);
            float medHighThreshold = NTConfig.Get<float>("NT_medhighThreshold", 1);

            // Strings
            List<string> vitalCategory = NTConfig.Get<List<string>>("NTSCAN_VitalCategory", new List<string>());
            List<string> removalCategory = NTConfig.Get<List<string>>("NTSCAN_RemovalCategory", new List<string>());
            List<string> customCategory = NTConfig.Get<List<string>>("NTSCAN_CustomCategory", new List<string>());
            List<string> ignoredCategory = NTConfig.Get<List<string>>("NTSCAN_IgnoredCategory", new List<string>());

            // Not changeable
            List<string> pressureCategory = new() { "bloodpressure" };

            // Readout strings
            string lowPressureReadout = "";
            string highPressureReadout = "";
            string lowStrengthReadout = "";
            string mediumStrengthReadout = "";
            string highStrengthReadout = "";
            string vitalReadout = "";
            string removalReadout = "";
            string customReadout = "";

            // Character effects
            HF.GiveItem(infos.target, "ntsfx_selfscan");
            Battery.Condition -= 5;

            HF.AddAffliction(infos.target, "radiationsickness", 1, infos.user);
            HF.AddAffliction(infos.user, "radiationsickness", (float)0.6, infos.user);

            // Print readout of afflictions
            string startReadout =
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                "Affliction readout for " +
                "‖color:end‖" +
                $"‖color:{nameColor.R},{nameColor.G},{nameColor.B}‖" +
                infos.target.Name +
                "‖color:end‖" +
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                " on limb " +
                HF.LimbToString(limbType) +
                ":\n" +
                "‖color:end‖";

            var afflictionList = infos.target.CharacterHealth.GetAllAfflictions();
            int afflictionsDisplayed = 0;

            foreach (var value in afflictionList)
            {
                float strength = MathF.Round(value.Strength);
                var prefab = value.Prefab;
                var afflictionLimb = infos.target.CharacterHealth.GetAfflictionLimb(value);

                LimbType afflimbtype = LimbType.Torso;
                if (!prefab.LimbSpecific)
                {
                    afflimbtype = prefab.IndicatorLimb;
                }
                else if (afflictionLimb != null)
                {
                    afflimbtype = afflictionLimb.type;
                }

                afflimbtype = HF.NormalizeLimbType(afflimbtype);

                if (strength >= prefab.ShowInHealthScannerThreshold && afflimbtype == limbType)
                {
                    string id = value.Identifier.Value;
                    bool isIgnored = ignoredCategory.Contains(id);

                    if (!isIgnored)
                    {
                        string name = prefab.Name.Value;
                        string entry = $"\n{name}: {strength}%";

                        // Check which category the affliction should be in
                        bool isVital = vitalCategory.Contains(id);
                        bool isRemoval = removalCategory.Contains(id);
                        bool isCustom = customCategory.Contains(id);
                        bool isPressure = pressureCategory.Contains(id);

                        // Add it to the respective readout
                        if (isVital)
                        {
                            vitalReadout += entry;
                        }
                        else if (isRemoval)
                        {
                            removalReadout += entry;
                        }
                        else if (isCustom)
                        {
                            customReadout += entry;
                        }
                        else if (isPressure)
                        {
                            if (strength > 130 || strength < 70)
                            {
                                highPressureReadout += entry;
                            }
                            else
                            {
                                lowPressureReadout += entry;
                            }
                        }

                        // If not in an already mentioned category, just apply normal colour logic
                        else
                        {
                            if (strength < lowMedThreshold)
                            {
                                lowStrengthReadout += entry;
                            }
                            else if (strength < medHighThreshold)
                            {
                                mediumStrengthReadout += entry;
                            }
                            else
                            {
                                highStrengthReadout += entry;
                            }
                        }

                        afflictionsDisplayed++;
                    }
                }
            }
            // Add a message in case there is nothing to display
            if (afflictionsDisplayed <= 0)
            {
                lowStrengthReadout += "\nNo afflictions! Good work!";
            }

            LuaCsSetup.Instance.Timer.Wait((params object[] _) =>
            {
                HF.DMClient(
                    HF.CharacterToClient(infos.user),
                    startReadout
                        + FormatLine(lowPressureReadout, lowColor)
                        + FormatLine(highPressureReadout, highColor)
                        + FormatLine(lowStrengthReadout, lowColor)
                        + FormatLine(mediumStrengthReadout, medColor)
                        + FormatLine(highStrengthReadout, highColor)
                        + FormatLine(vitalReadout, vitalColor)
                        + FormatLine(removalReadout, removalColor)
                        + FormatLine(customReadout, customColor),
                    null
                );
            }, 2000);
        });

        // Add the Hematology Detectable afflictions in here
        HematologyDetectable.AddRange(
        [
            "sepsis",
            "immunity",
            "acidosis",
            "alkalosis",
            "bloodloss",
            "bloodpressure",
            "afimmunosuppressant",
            "afthiamine",
            "afadrenaline",
            "afstreptokinase",
            "afantibiotics",
            "afsaline",
            "afringerssolution",
            "afpressuredrug",
            "afopioid",
            "afanaesthetic"
        ]);

        // Hematology Analyzer
        RegisterItemUseFunction("bloodanalyzer", infos =>
        {
            // Only work if not on cooldown
            if (infos.item.Condition < 50) return;

            bool success = HF.GetSkillRequirmentMet(infos.user, "medical", 30);

            float BloodLossInduced = 3f;

            if (success)
            {
                BloodLossInduced = 1f;
            }

            HF.AddAffliction(infos.target, "bloodloss", BloodLossInduced, infos.user);

            // Spawn donor card
            var ContainedItem = infos.item.OwnInventory.GetItemAt(0);

            bool HasCartridge = ContainedItem != null &&
                (
                    ContainedItem.Prefab.Identifier.Value == "bloodcollector" ||
                    ContainedItem.HasTag("donorCard")
                );

            if (HasCartridge)
            {
                HF.RemoveItem(ContainedItem);

                string BloodType = NTBloodTypes.GetBloodType(infos.target);

                var TargetIDCard = infos.target.Inventory.GetItemAt(0);

                if (TargetIDCard != null && TargetIDCard.OwnInventory != null && TargetIDCard.OwnInventory.GetItemAt(0) == null)
                {
                    HF.PutItemInContainer(TargetIDCard, BloodType + "card");
                }
                else
                {
                    HF.PutItemInContainer(infos.item, BloodType + "card");
                }
            }

            bool useColoredScanner = NTConfig.Get("NTSCAN_enablecoloredscanner", true);

            Color baseColor = useColoredScanner ? HF.GetColor("NTSCAN_basecolor") : new Color(127, 255, 255);
            Color nameColor = useColoredScanner ? HF.GetColor("NTSCAN_namecolor") : new Color(127, 255, 255);
            Color lowColor = useColoredScanner ? HF.GetColor("NTSCAN_lowcolor") : new Color(127, 255, 255);
            Color medColor = useColoredScanner ? HF.GetColor("NTSCAN_medcolor") : new Color(127, 255, 255);
            Color highColor = useColoredScanner ? HF.GetColor("NTSCAN_highcolor") : new Color(127, 255, 255);
            Color vitalColor = useColoredScanner ? HF.GetColor("NTSCAN_vitalcolor") : new Color(127, 255, 255);
            Color removalColor = useColoredScanner ? HF.GetColor("NTSCAN_removalcolor") : new Color(127, 255, 255);
            Color customColor = useColoredScanner ? HF.GetColor("NTSCAN_customcolor") : new Color(127, 255, 255);

            // Floats
            float lowMedThreshold = NTConfig.Get<float>("NTSCAN_lowmedThreshold", 1);
            float medHighThreshold = NTConfig.Get<float>("NT_medhighThreshold", 1);
            
            // Strings
            List<string> vitalCategory = NTConfig.Get<List<string>>("NTSCAN_VitalCategory", []);
            List<string> removalCategory = NTConfig.Get<List<string>>("NTSCAN_RemovalCategory", []);
            List<string> customCategory = NTConfig.Get<List<string>>("NTSCAN_CustomCategory", []);
            List<string> ignoredCategory = NTConfig.Get<List<string>>("NTSCAN_IgnoredCategory", []);

            // Not changeable
            List<string> pressureCategory = ["bloodpressure"];

            // Readout Strings
            string lowPressureReadout = "";
            string highPressureReadout = "";
            string lowStrengthReadout = "";
            string mediumStrengthReadout = "";
            string highStrengthReadout = "";
            string vitalReadout = "";
            string removalReadout = "";
            string customReadout = "";

            string BloodTypeName = AfflictionPrefab.Prefabs[NTBloodTypes.GetBloodType(infos.target)].Name.Value;

            string startReadout =
                $"‖color:{nameColor.R},{nameColor.G},{nameColor.B}‖" +
                $"Bloodtype: {BloodTypeName}" +
                "‖color:end‖\n" +
                $"‖color:{baseColor.R},{baseColor.G},{baseColor.B}‖" +
                $"Affliction readout for {infos.target.Name}:" +
                "‖color:end‖\n";

            int afflictionsDisplayed = 0;

            HashSet<string> checkedAfflictions = [];

            foreach (var value in infos.target.CharacterHealth.GetAllAfflictions())
            {
                float strength = MathF.Round(value.Strength);
                var prefab = value.Prefab;

                string id = value.Identifier.Value;

                if (strength <= 2) continue;
                if (!HematologyDetectable.Contains(prefab.Identifier.Value)) continue;

                if (!checkedAfflictions.Add(id))
                {
                    continue;
                }

                if (ignoredCategory.Contains(id))
                {
                    continue;
                }

                string entry = $"\n{prefab.Name.Value}: {strength}%";

                bool isVital = vitalCategory.Contains(id);
                bool isRemoval = removalCategory.Contains(id);
                bool isCustom = customCategory.Contains(id);
                bool isPressure = pressureCategory.Contains(id);

                if (isVital)
                {
                    vitalReadout += entry;
                }
                else if (isRemoval)
                {
                    removalReadout += entry;
                }
                else if (isCustom)
                {
                    customReadout += entry;
                }
                else if (isPressure)
                {
                    if (strength > 130 || strength < 70)
                    {
                        highPressureReadout += entry;
                    }
                    else
                    {
                        lowPressureReadout += entry;
                    }
                }
                else
                {
                    if (strength < lowMedThreshold)
                    {
                        lowStrengthReadout += entry;
                    }
                    else if (strength < medHighThreshold)
                    {
                        mediumStrengthReadout += entry;
                    }
                    else
                    {
                        highStrengthReadout += entry;
                    }
                }

                afflictionsDisplayed++;
            }

            if (afflictionsDisplayed <= 0)
            {
                lowStrengthReadout += "\nNo blood pressure detected...";
            }

            HF.DMClient(
                HF.CharacterToClient(infos.user),
                startReadout
                    + FormatLine(lowPressureReadout, lowColor)
                    + FormatLine(highPressureReadout, highColor)
                    + FormatLine(lowStrengthReadout, lowColor)
                    + FormatLine(mediumStrengthReadout, medColor)
                    + FormatLine(highStrengthReadout, highColor)
                    + FormatLine(vitalReadout, vitalColor)
                    + FormatLine(removalReadout, removalColor)
                    + FormatLine(customReadout, customColor),
                null
            );
        });
    }


    /**<summary>
     * Register a new identifier => function to the NTItemRegistry.
     * The function will trigger when the item matching the identifier is used.
     * </summary>
     * <param name="itemID">The identifier described in the XML.</param>
     * <param name="function">The item update function.</param>
     * <returns>Returns true if the item was defined correctly (if it was not already defined). Returns false otherwise.</returns>
     */
    public static bool RegisterItemUseFunction(string itemID, Action<ItemUpdateFunctionInfos> function)
    {
        if (!NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }

    /**<summary>
     * Overrides an already defined function matching the itemID.
     * Does nothing if the itemID isn't defined in the registry.
     * </summary>
     * <param name="itemID">The identifier described in the XML.</param>
     * <param name="function">The item update function.</param>
     * <returns>Returns true if the override was succesful, false otherwise.</returns>
     */
    public static bool UpdateItemUseFunction(string itemID, Action<ItemUpdateFunctionInfos> function)
    {
        if (NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry.Add(itemID, function);
            return true;
        }

        return false;
    }


    /**
     * <summary>
     * The function patching the base game Item.ApplyTreatment
     * </summary>
     */
    public static void Override_ApplyTreatment(Barotrauma.Item __instance, Character user, Character character, Limb targetLimb)
    {

        string itemID = __instance.Prefab.Identifier.ToString();
        if (NTItemsRegistry.ContainsKey(itemID))
        {
            NTItemsRegistry[itemID].Invoke(new ItemUpdateFunctionInfos(__instance, user, character, targetLimb));
        }
    }

    /**
     * <summary>
     * The function patching the base game Item.Use
     * </summary>
     */
    public static void Override_Use(Barotrauma.Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
       // LuaCsLogger.Log("use");
    }
}



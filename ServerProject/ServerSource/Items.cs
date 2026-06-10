
namespace Neurotrauma;



public class NTItemMethods
{

    /**
     * <summary>
     * Contains all the data necessary to add an Affliction to DrainageAfflictions.
     * 
     * </summary>
     * 
     */
    public class DrainageAfflictionInfos { 
    
        ///<summary>The ID defined in the XML. The affliction MUST be non limb-specific.</summary>
        public string AfflictionID { get; }

        ///<summary>The amount of XP given to the surgery or medical skill when the item is applied successfully.</summary>
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

        public DrainageAfflictionInfos(string affID, int xpGain, Func<ItemUpdateFunctionInfos, bool> conditions) 
        { 
            //TODO check if affID is limb specific and throw error if so

            this.AfflictionID = affID;
            this.XPGain = xpGain;
            this.Conditions = conditions;
        }
    
    }

    /**
     * <summary>
     * Contains the list of every afflictions cured by drainage. Addons can add to this list.
     * 
     * </summary>
     */
    public static List<DrainageAfflictionInfos> DrainageAfflictions { get; } = [];

    /**
     * <summary>
     * Contains all the data necessary for an item use function.
     * 
     * </summary>
     */
    public class ItemUpdateFunctionInfos
    {
        public Item item { get; }
        public Character user {  get; }
        public Character target { get; }
        public Limb targetLimb { get; }

        public ItemUpdateFunctionInfos(Item item, Character user, Character target, Limb targetLimb)
        {
            this.item = item;
            this.user = user;
            this.target = target;
            this.targetLimb = targetLimb;
        }
    }

    public static Dictionary<string, Action<ItemUpdateFunctionInfos>> NTItemsRegistry { get; } = new Dictionary<string, Action<ItemUpdateFunctionInfos>> { };


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
    public static void Override_ApplyTreatment(Item __instance, Character user, Character character, Limb targetLimb)
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
    public static void Override_Use(Item __instance, float deltaTime, Character user = null, Limb targetLimb = null, Entity useTarget = null, Character userForOnUsedEvent = null)
    {
       // LuaCsLogger.Log("use");
    }
}



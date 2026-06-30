
using Barotrauma;
using Barotrauma.LuaCs.Events;
using FarseerPhysics.Dynamics;
using MoonSharp.Interpreter;
using static Neurotrauma.HF;
using static Neurotrauma.NTC;

namespace Neurotrauma
{
    /// <summary>
    /// Our post Human Update Lua syncing system.
    /// Needed since most addons are written in Lua and require the old Lua Human Update.
    /// </summary>
    public static class HumanUpdateLuaSync
    {

        static bool ScriptsHaveBeenFetched = false;

        static Script NTCompatScript = new Script();
        static Script DummyHumanUpdate = new Script();
        static Script HelperFunctions = new Script();


        static public void FetchLuaScripts()
        {
        }

        static public void Update()
        {

        }

        /// <summary>
        /// Syncs our C# human update with our Lua human update, syncs our C# characters with our lua characters
        /// </summary>
        static public void PreSync(List<HumanUpdate.NTHuman> CharacterList)
        {
            SyncLuaAfflictions(CharacterList);
            SyncLuaCharacters(CharacterList);
        }

        /// <summary>
        /// Syncs our C# human update with our Lua human update, syncs our C# characters with our lua characters
        /// </summary>
        static public void PostSync()
        {

        }

        static public void SyncLuaAfflictions(List<HumanUpdate.NTHuman> CharacterList)
        {
        }

        static public void SyncLuaCharacters(List<HumanUpdate.NTHuman> CharacterList)
        {
        }

    }
}
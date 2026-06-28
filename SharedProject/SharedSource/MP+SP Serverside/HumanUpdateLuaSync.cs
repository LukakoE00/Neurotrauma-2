
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
        static public void PreSync()
        {
            SyncLuaAfflictions();
            SyncLuaCharacters();
        }

        /// <summary>
        /// Syncs our C# human update with our Lua human update, syncs our C# characters with our lua characters
        /// </summary>
        static public void PostSync()
        {

        }

        static public void SyncLuaAfflictions()
        {
        }

        static public void SyncLuaCharacters()
        {
        }

    }
}

// Add our custom Lua Hooks
namespace Barotrauma.LuaCs.Events
{
    internal interface IEventSyncLuaCharacters : IEvent<IEventSyncLuaCharacters>
    {
        void SyncLuaCharacters();

        static IEventSyncLuaCharacters IEvent<IEventSyncLuaCharacters>.GetLuaRunner(
            IDictionary<string, LuaCsFunc> luaFunc) => new LuaWrapper(luaFunc);

        public sealed class LuaWrapper : LuaWrapperBase, IEventSyncLuaCharacters
        {
            public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
            {
            }

            public void SyncLuaCharacters()
            {
                LuaFuncs[nameof(SyncLuaCharacters)]();
            }
        }
    }

    internal interface IEventSyncLuaHumanUpdate : IEvent<IEventSyncLuaHumanUpdate>
    {
        void SyncLuaHumanUpdate();

        static IEventSyncLuaHumanUpdate IEvent<IEventSyncLuaHumanUpdate>.GetLuaRunner(
            IDictionary<string, LuaCsFunc> luaFunc) => new LuaWrapper(luaFunc);

        public sealed class LuaWrapper : LuaWrapperBase, IEventSyncLuaHumanUpdate
        {
            public LuaWrapper(IDictionary<string, LuaCsFunc> luaFuncs) : base(luaFuncs)
            {
            }

            public void SyncLuaHumanUpdate()
            {
                LuaFuncs[nameof(SyncLuaHumanUpdate)]();
            }
        }
    }
}
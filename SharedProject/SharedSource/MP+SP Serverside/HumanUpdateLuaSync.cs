
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
    public class HumanUpdateLuaSync
    {

        bool ScriptsHaveBeenFetched = false;

        Script NTCompatScript = new Script();
        Script DummyHumanUpdate = new Script();
        Script HelperFunctions = new Script();


        public void FetchLuaScripts()
        {
        }

        public void Update()
        {

        }

        /// <summary>
        /// Syncs our C# human update with our Lua human update, syncs our C# characters with our lua characters
        /// </summary>
        public void PreSync()
        {

        }

        /// <summary>
        /// Syncs our C# human update with our Lua human update, syncs our C# characters with our lua characters
        /// </summary>
        public void PostSync()
        {

        }

        public void SyncLuaAfflictions()
        {
            LuaCsSetup.Instance.EventService.PublishEvent<IEventSyncLuaCharacters>(x => x.SyncLuaCharacters());
        }

        public void SyncLuaCharacters()
        {
            LuaCsSetup.Instance.EventService.PublishEvent<IEventSyncLuaHumanUpdate>(x => x.SyncLuaHumanUpdate());
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
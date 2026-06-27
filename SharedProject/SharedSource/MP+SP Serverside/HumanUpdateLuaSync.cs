
using MoonSharp.Interpreter;
using static Neurotrauma.HF;
using static Neurotrauma.NTC;

namespace Neurotrauma;

/// <summary>
/// Our post Human Update Lua syncing system.
/// Needed since most addons are written in Lua and require the old Lua Human Update.
/// </summary>
public class HumanUpdateLuaSync
{

    static Script TestScript = new();
    Table TestTable = new(TestScript);

    public void SyncLuaAfflictions()
    {

    }

    public void ConvertAffTableToClass()
    {

    }

}
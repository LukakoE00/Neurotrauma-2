using Barotrauma.LuaCs.Data;
using MoonSharp.Interpreter;

namespace Neurotrauma;

// This takes the place of the init.lua information and can be used during Initialization (see NeurotraumaInit)
// Addons should add their own file like this which then gets used to do the old print-in-console thing
public static class NTInfo
{
    public const string Name = "Neurotrauma C#";
    public const string Version = "A2.0.0";
    public const int VersionNum = 02000000;

    // Make a new list (like a table! but not!) that only holds NTAddon objects.
    // 'get' means we can read the list, but not replace it and 'new' means it get's created on loading.
    public static List<NTAddon> RegisteredAddons { get; } = new();
    public static List<Table> LuaRegisteredAddons = new();

    // This is the NTC.RegisterExpansion function from NTCompat.
    // NTAddon (defined below!) is an object, or like a 'blueprint' from which other Addon objects are made!
    // Each one has some things it needs to function, which will get passed along.
    // Object Oriented Coding!!!!!!

    public static void RegisterAddon(NTAddon addon)
    {
        RegisteredAddons.Add(addon);
    }
    public static void RegisterAddon(Table addon)
    {
        LuaRegisteredAddons.Add(addon);
    }

    public static void PrintNTInitInfo(ImmutableArray<ILuaScriptResourceInfo> executionOrder, bool enableSandbox)
    {
        LuaCsSetup.Instance.Timer.Wait((params object[] _) => {
            // New string with the first line of the init print; the $ allows the string to interpolate
            string consolePrint = $"\n\n/// Running Neurotrauma V {NTInfo.Version} ///\n";
            // Repeat the dash until the line is just as long as the line above and add 4 more to make it stand out
            consolePrint += new string('-', consolePrint.Length + 4);

            // Now check for addons and react accordingly
            bool hasCSAddons = RegisteredAddons.Count > 0;

            bool hasLuaAddons = LuaRegisteredAddons.Count() > 0;

            consolePrint += "\n";

            if (hasCSAddons) consolePrint += "- C# Addons:\n";
            else consolePrint += "- Not running any C# Addons\n";
            foreach (NTAddon addon in RegisteredAddons)
            {
                consolePrint += $"\n+ {addon.Name} V {addon.Version}";

                if (VersionNum < addon.MinNTVersionNum)
                {
                    consolePrint += $"\n-- WARNING! Neurotrauma version {addon.MinNTVersion} or higher required!";
                }
            }

            if (hasLuaAddons) consolePrint += "- Lua Addons:\n";
            else consolePrint += "- Not running any Lua Addons\n";
            foreach (Table addon in LuaRegisteredAddons)
            {
                consolePrint += $"\n+ {addon.Get("Name").String} V {addon.Get("Version").String}";

                if (VersionNum < addon.Get("MinNTVersionNum").Number)
                {
                    consolePrint += $"\n-- WARNING! Neurotrauma version {addon.Get("MinNTVersion").Number} or higher required!";
                }
            }

            HF.Print(consolePrint);
        }, 1000);
    }
}

// This is the same information currently present in NTCompat.lua; it can be added in the Addon's OnLoadCompleted block:
// Yes, it's addon now. No-one calls it expansion like, ever.

// NTInfo.RegisterAddon(new NTAddon
//    {
//      Name = "My Addon",
//      Version = "1.0.0",
//      VersionNum = 01000000
//      MinNTVersion = "A1.17.4",
//      MinNTVersionNum = 1170400
//    });

public class NTAddon
{
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required int VersionNum { get; set; }
    public required string MinNTVersion { get; set; }
    public required int MinNTVersionNum { get; set; }
}

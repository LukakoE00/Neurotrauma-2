print("[NT2] This is the SharedExample init print!")

NT = {}
NT.Name = "Neurotrauma"
NT.Version = "1.0.0h0"
NT.VersionNum = 000000001
NT.Path = table.pack(...)[1]
NT.SymsForNPC = {}
NT.BLOODTYPE = {}
NT.ContainerFills = {}

LuaUserData.RegisterType("Neurotrauma.NTConfig")
LuaUserData.RegisterType("Neurotrauma.ConfigExpansion")
LuaUserData.RegisterType("Neurotrauma.ConfigEntry")
LuaUserData.RegisterType("Neurotrauma.ConfigEntryType")
LuaUserData.RegisterType("Neurotrauma.NTConfigData")

dofile(NT.Path .. "/Lua/Scripts/Shared/ConfigData.lua") 		
dofile(NT.Path .. "/Lua/Scripts/Shared/HelperFunctions.lua") 		
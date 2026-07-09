print("[NT2] This is the SharedExample init print!")

NT = {}
NT.Name = "Neurotrauma"
NT.Version = "1.0.0h0"
NT.VersionNum = 000000001
NT.Path = table.pack(...)[1]
NT.SymsForNPC = {}
NT.DrainageAfflictions = {}
NT.ItemMethods = {}
NT.BLOODTYPE = {}
NT.HematologyDetectable = {}
NT.SutureAfflictions = {}
NT.ContainerFills = {}

LuaUserData.RegisterType("Neurotrauma.NTConfig")

dofile(NT.Path .. "/Lua/Scripts/Shared/ConfigData.lua") 		
dofile(NT.Path .. "/Lua/Scripts/Shared/HelperFunctions.lua") 		
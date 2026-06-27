print("[NT2] This is the ClientExample init print!")

Hook.Add("SyncLuaCharacters", "NTCS.SyncCharacters", function()
   print("Sync Characters!")
end)

Hook.Add("SyncLuaHumanUpdate", "NTCS.SyncHumanUpdate", function()
   print("Sync!")
end)
NTConfig = { Entries = {}, Expansions = {} } -- contains all config options, their default, type, valid ranges, difficulty influence

local configDirectoryPath = Game.SaveFolder .. "/ModConfigs"
local configFilePath = configDirectoryPath .. "/Neurotrauma.json"

-- This is the function that gets used in other mods to add their own settings to the config
function NTConfig.AddConfigOptions(expansion)

end

function NTConfig.Get(key, default)
	if NTConfig.Entries[key] then return NTConfig.Entries[key].value end
	return default
end

function NTConfig.Set(key, value)
	if NTConfig.Entries[key] then NTConfig.Entries[key].value = value end
end


NT.ConfigData = {

}

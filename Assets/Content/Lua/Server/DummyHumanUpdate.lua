-- Basically we have to trick our Lua NT Addons into using this human update.

local NT = {}

-- Neurotrauma human update functions
-- Hooks Lua event "think" to update and use for applying NT specific character data (its called 'c') with
-- values/functions defined here in NT.UpdateHuman, NT.LimbAfflictions and NT.Afflictions
NT.UpdateCooldown = 0
NT.UpdateInterval = 120
NT.Deltatime = NT.UpdateInterval / 60 -- Time in seconds that transpires between updates

NT.organDamageCalc = function(c, damagevalue, nomaxstrength)
	if damagevalue >= 99 and (nomaxstrength == nil or nomaxstrength == false) then return 100 end
	return damagevalue - 0.01 * c.stats.healingrate * c.stats.specificOrganDamageHealMultiplier * NT.Deltatime
end

-- define all the afflictions and their update functions
NT.Afflictions = {
	-- Unconsciousness
}
-- define all the limb specific afflictions and their update functions
NT.LimbAfflictions = {

}

NT.CharStats = {
	
}
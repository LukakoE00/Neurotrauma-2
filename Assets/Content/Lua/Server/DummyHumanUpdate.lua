-- Basically we have to trick our Lua NT Addons into using this human update.


LuaUserData.MakeMethodAccessible(Descriptors["Neurotrauma.HumanUpdateLuaSync"], "PostSync")

Hook.Patch("Neurotrauma.HumanUpdateLuaSync","SyncLuaAfflictions", function (GameSession, ptable)

end, Hook.HookMethodType.After)


Hook.Patch("Neurotrauma.HumanUpdateLuaSync","SyncLuaCharacters", function (GameSession, ptable)

end, Hook.HookMethodType.After)


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
NT.LegacyAfflictions = {
	-- Unconsciousness
	-- stylua: ignore start
	sym_unconsciousness = {
	},
	-- stylua: ignore end

	-- Arterial cuts
	t_arterialcut = {},
	-- Fractures and amputations
	t_fracture = {
	},
	h_fracture = {
	},
	la_fracture = {
	},
	ra_fracture = {
	},
	ll_fracture = {
	},
	rl_fracture = {
	},
	n_fracture = {
	},
	tla_amputation = {},
	tra_amputation = {},
	tll_amputation = {},
	trl_amputation = {},
	sla_amputation = {},
	sra_amputation = {},
	sll_amputation = {},
	srl_amputation = {},
	t_paralysis = {},
	alv = {}, -- artificial ventilation
	needlec = {
	},
	forceprone = {
	},
	onwheelchair = {
	},

	-- Organ conditions
	cardiacarrest = {
	},
	respiratoryarrest = {
	},
	pneumothorax = {
	},
	tamponade = {
	},
	infectedcavity = {
	},
	heartattack = {
	},
	-- Organs removed
	brainremoved = {
	},
	heartremoved = {
	},
	lungremoved = {
	},
	liverremoved = {
	},
	kidneyremoved = {
	},
	-- Organ damage
	cerebralhypoxia = {
		max = 200,
	},
	heartdamage = {
	},
	lungdamage = {
	},
	liverdamage = {
	},
	kidneydamage = {
	},
	bonedamage = {
	},
	organdamage = {
		max = 200,
	},
	-- Blood
	sepsis = {
	},
	immunity = {
		default = -1,
		min = 5,
	},
	bloodloss = { max = 200 },
	bloodpressure = {
		min = 5,
		max = 200,
		default = 100,
	},
	hypoxemia = {
	},
	hemotransfusionshock = {},
	tshocktimeout = {},
	-- Other
	oxygenlow = {
		max = 200,
	},
	radiationsickness = {
		max = 200,
	},
	stasis = {},
	table = {},
	internalbleeding = {
	},
	acidosis = {
	},
	alkalosis = {
	},
	seizure = {
	},
	stroke = {
	},
	coma = {
	},
	stun = {
	},
	slowdown = {
	},
	givein = {
		max = 1,
	},
	lockedhands = {
	},
	traumaticshock = {
	},
	alcoholwithdrawal = {},
	opiatewithdrawal = {},
	chemwithdrawal = {},
	opiateoverdose = {},
	-- Drugs
	analgesia = { max = 200 },

	-- propofol (i hate it)
	anesthesia = {
	},
	drunk = { max = 200 },
	afadrenaline = {},
	afantibiotics = {},
	afthiamine = {},
	afsaline = {},
	afringerssolution = {},
	afstreptokinase = {},
	afmannitol = {},
	afanaesthetic = {},
	afopioid = {},
	afpressuredrug = {
	},
	combatstimulant = {},
	concussion = {
	},

	-- /// Symptoms ///
	--==============================================================================
	tachycardia = {
	},
	fibrillation = {
	},
	hyperventilation = {
	},
	hypoventilation = {
	},
	dyspnea = {
	},
	sym_cough = {
	},
	sym_paleskin = {
	},
	sym_lightheadedness = {
	},
	sym_blurredvision = {
	},
	sym_confusion = {
	},
	sym_headache = {
	},
	sym_legswelling = {
	},
	sym_weakness = {
	},
	sym_wheezing = {
	},
	sym_vomiting = {
	},
	sym_nausea = {
	},
	sym_hematemesis = {
	},
	fever = {
	},
	sym_abdomdiscomfort = {
	},
	sym_bloating = {
	},
	sym_jaundice = {
	},
	sym_sweating = {
	},
	sym_palpitations = {
	},
	sym_craving = {
	},
	pain_abdominal = {
	},
	pain_chest = {
	},
	luabotomy = {
	},
	modconflict = {
	},
}

NT.Afflictions = {
}

-- define all the limb specific afflictions and their update functions
NT.LegacyLimbAfflictions = {
	bandaged = {
	},
	dirtybandage = {}, -- for bandage dirtifaction logic see above
	iced = {
	},
	skinointmented = {
	},
	gypsumcast = {
	},
	ointmented = {},
	bonegrowth = {
	},
	arteriesclamp = {},
	-- damage
	bleeding = {
	},
	burn = {
		max = 200,
	},
	acidburn = {
		max = 200,
	},
	lacerations = {
		max = 200,
	},
	gunshotwound = {
		max = 200,
	},
	bitewounds = {
		max = 200,
	},
	explosiondamage = {
		max = 200,
	},
	blunttrauma = {
		max = 200,
	},
	internaldamage = {
		max = 200,
	},
	-- other
	infectedwound = {
	},
	foreignbody = {
	},
	gangrene = {
	},
	pain_extremity = {
		max = 10,
	},
	-- limb symptoms
	inflammation = {
	},
	burn_deg1 = {
	},
	burn_deg2 = {
	},
	burn_deg3 = {
	},
}

NT.LimbAfflictions = {
}

NT.LegacyCharStats = {
	healingrate = {
	},
	specificOrganDamageHealMultiplier = {
	},
	neworgandamage = {
	},
	clottingrate = {
	},

	bloodamount = {
	},
	stasis = {
	},
	sedated = {
	},
	withdrawal = {
	},
	availableoxygen = {
	},
	speedmultiplier = {
	},

	lockleftarm = {
	},
	lockrightarm = {
	},
	lockleftleg = {
	},
	lockrightleg = {
	},

	wheelchaired = {
	},

	bonegrowthCount = {
	},
	burndamage = {
	},
}

NT.CharStats = {
}
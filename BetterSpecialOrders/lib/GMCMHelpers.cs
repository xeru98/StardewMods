using BetterSpecialOrders.Menus;
using GenericModConfigMenu;
using StardewModdingAPI;
using StardewValley.Menus;

namespace BetterSpecialOrders;

public static class GMCMHelpers
{
    private struct GMCM_PageConfig
    {
        public string name;
        public string configId;
        public List<string> requiredModIds = new List<string>();
        public bool createSchedule;

        public GMCM_PageConfig(string name, string configId, bool createSchedule, List<string> requiredModIds = null)
        {
            this.name = name;
            this.configId = configId;
            this.createSchedule = createSchedule;
            if (requiredModIds != null)
            {
                this.requiredModIds = requiredModIds;
            }
        }

        public bool allRequiredIdsFound()
        {
            foreach (string requiredModId in requiredModIds)
            {
                if (ModEntry.GHelper.ModRegistry.GetApi(requiredModId) == null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    private static List<GMCM_PageConfig> pageConfigs = new List<GMCM_PageConfig>()
    {
        new GMCM_PageConfig("Stardew Valley", "sv", true),
        new GMCM_PageConfig("Qi's Quest", "qi", true),
        new GMCM_PageConfig("Desert Festival", "de", false),
        new GMCM_PageConfig("Ridgeside Village Town", "rsv_town", true, new List<string>(){"Rafseazz.RidgesideVillage"}),
        new GMCM_PageConfig("Ridgeside Village Ninja", "rsv_ninja", true, new List<string>(){"Rafseazz.RidgesideVillage"}),
        new GMCM_PageConfig("Mt. Vapius", "mt_vapius", true, new List<string>(){"lumisteria.visitmountvapius.code"}),
        new GMCM_PageConfig("CUSTOM BOARDS", "custom", true) // extra for unknown types
    };
    
    private static ModConfig config
    {
        get { return RerollManager.Get().config; }
        set { RerollManager.Get().config = value;  }
    }

    private static void RegisterPage(IGenericModConfigMenuApi GMCM, GMCM_PageConfig pageConfig, ModConfig modConfig)
    {
        //if we don't have the required key the add it to the config
        if (!modConfig.BoardConfigs.ContainsKey(pageConfig.configId))
        {
            ModEntry.GMonitor.Log($"Could not find key {pageConfig.configId} in the config.json... adding", LogLevel.Warn);
            modConfig.BoardConfigs.Add(pageConfig.configId, new BoardConfig());
        }
        
        GMCM.AddPage(
            mod: ModEntry.GManifest,
            pageId: pageConfig.configId,
            pageTitle: () => pageConfig.name
        );

        GMCM.AddBoolOption(
            mod: ModEntry.GManifest,
            name: () => "Allow Rerolls",
            tooltip: () => "When checked, allows this board to be rerolled",
            fieldId: $"{pageConfig.configId}_allowReroll",
            getValue: () => modConfig.BoardConfigs[pageConfig.configId].AllowReroll,
            setValue: value => modConfig.BoardConfigs[pageConfig.configId].AllowReroll = value
        );
        
        GMCM.AddBoolOption(
            mod: ModEntry.GManifest,
            name: () => "Infinite Rerolls",
            tooltip: () => "When checked, allows this board to be rerolled infinitely",
            fieldId: $"{pageConfig.configId}_infiniteReroll",
            getValue: () => modConfig.BoardConfigs[pageConfig.configId].InfiniteRerolls,
            setValue: value => modConfig.BoardConfigs[pageConfig.configId].InfiniteRerolls = value
        );

        GMCM.AddNumberOption(
            mod: ModEntry.GManifest,
            name: () => "Max Daily Rerolls",
            tooltip: () => "The number of daily rerolls the team has shared across them per day for this board",
            fieldId: $"{pageConfig.configId}_maxDailyRerolls",
            getValue: () => modConfig.BoardConfigs[pageConfig.configId].MaxRerolls,
            setValue: value => modConfig.BoardConfigs[pageConfig.configId].MaxRerolls = value,
            min: 1,
            max: 10
        );

        if (pageConfig.createSchedule)
        {
            RerollScheduleOption opt = new RerollScheduleOption(
                getValue: () => modConfig.BoardConfigs[pageConfig.configId].RefreshSchedule,
                setValue: value => modConfig.BoardConfigs[pageConfig.configId].RefreshSchedule = value
            );
            
            GMCM.AddComplexOption(
                mod: ModEntry.GManifest,
                name: () => "Schedule",
                tooltip: () => "List of days of the week where this board will automatically reroll",
                fieldId: $"{pageConfig.configId}_refreshSchedule",
                draw: opt.Draw,
                height: () => opt.height,
                beforeMenuOpened: opt.Reset,
                beforeSave: opt.SaveChanges,
                afterReset: opt.Reset
            );
        }
    }

    // sets up the GMCM
    public static void SetupGMCM()
    {
        IGenericModConfigMenuApi? GMCM = ModEntry.GHelper.ModRegistry.GetApi<GenericModConfigMenu.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (GMCM == null)
        {
            ModEntry.GMonitor.Log("Generic Mod Config Menu not found. Skipping mod menu setup", LogLevel.Info);
            return;
        }
        
        GMCM.Register(mod: ModEntry.GManifest, reset: () => config = new ModConfig(), save: () => ModEntry.GHelper.WriteConfig(config));
        GMCM.OnFieldChanged(
            mod: ModEntry.GManifest,
            onChange: ModEntry.OnGMCMFieldChanged
        );
        
        
        // GENERAL
        GMCM.AddSectionTitle(
            mod: ModEntry.GManifest,
            text: () => "General Settings"
        );
        
        GMCM.AddParagraph(
            mod: ModEntry.GManifest,
            text: () => "All of these settings only need to be set by the host. All joining farmers will use the host's settings"
        );
        
        GMCM.AddBoolOption(
            mod: ModEntry.GManifest,
            name: () => "Use unseeded random generator",
            tooltip: () => "When unchecked, randomizer will use seeded pseudorandom generator. When checked will use unseeded random generator.",
            fieldId: "general_useTrueRandom",
            getValue: () => config.useTrueRandom,
            setValue: value => config.useTrueRandom = value
        );
        
        GMCM.AddKeybindList(
            mod: ModEntry.GManifest,
            name: () => "Reroll Reset Keybind",
            tooltip: () => "Allows the host to reset the number of available rerolls back to their max amount",
            fieldId: "general_resetKeybind",
            getValue: () => config.resetRerollsKeybind,
            setValue: value => config.resetRerollsKeybind = value
        );
        
        GMCM.AddBoolOption(
            mod: ModEntry.GManifest,
            name: () => "Force Unique Orders (when possible)",
            tooltip: () => "When possible will guarantee that as none of the new options are the previous options.",
            fieldId: "general_useTrueRandom",
            getValue: () => config.forceUniqueIfPossible,
            setValue: value => config.forceUniqueIfPossible = value
        );
        
        GMCM.AddSectionTitle(
            mod: ModEntry.GManifest,
            text: () => "Board Configs"
        );

        foreach (GMCM_PageConfig pageConfig in pageConfigs)
        {
            if (pageConfig.allRequiredIdsFound())
            {
                GMCM.AddPageLink(
                    mod: ModEntry.GManifest,
                    pageId: pageConfig.configId,
                    text: () => pageConfig.name
                );
            }
        }
        
        foreach (GMCM_PageConfig pageConfig in pageConfigs)
        {
            if (pageConfig.allRequiredIdsFound())
            {
                RegisterPage(GMCM, pageConfig, RerollManager.Get().config);
            }
        }
        
    }
}
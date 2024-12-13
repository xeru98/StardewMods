using BetterSpecialOrders.Menus;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace BetterSpecialOrders;

public static class GMCMHelpers
{
    private static readonly Dictionary<Func<string>, string> TRANSLATIONS = new Dictionary<Func<string>, string>()
    {
        {I18n.Settings_Credits_Translations_English, "Xeru98"},
        {I18n.Settings_Credits_Translations_Mandarin, "Ctwn17 (Kitto)"},
        {I18n.Settings_Credits_Translations_Portuguese, "Maatsuki"}
    };
    
    private struct GMCM_PageConfig
    {
        public string name;
        public string configId;
        public List<string> requiredModIds = new List<string>();
        public bool createSchedule;

        public GMCM_PageConfig(string name, string configId, bool createSchedule, List<string>? requiredModIds = null)
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
                if (!ModEntry.GHelper!.ModRegistry.IsLoaded(requiredModId))
                {
                    ModEntry.GMonitor!.Log($"Required ModID: {requiredModId} not loaded");
                    return false;
                }
            }

            return true;
        }
    }

    private static List<GMCM_PageConfig> PageConfigs = new List<GMCM_PageConfig>()
    {
        new GMCM_PageConfig("Stardew Valley", Constants.ConfigKeys.SV, true),
        new GMCM_PageConfig("Qi's Quest", Constants.ConfigKeys.Qi, true),
        new GMCM_PageConfig("Desert Festival", Constants.ConfigKeys.DesertFestival, false),
        new GMCM_PageConfig("Ridgeside Village Town", Constants.ConfigKeys.RSVTown, true, new List<string>(){"Rafseazz.RidgesideVillage"}),
        new GMCM_PageConfig("Ridgeside Village Ninja", Constants.ConfigKeys.RSVNinja, true, new List<string>(){"Rafseazz.RidgesideVillage"}),
        new GMCM_PageConfig("Mt. Vapius", Constants.ConfigKeys.MtVapius, true, new List<string>(){"lumisteria.visitmountvapius.code"}),
        new GMCM_PageConfig("CUSTOM BOARDS", Constants.ConfigKeys.Custom, true) // extra for unknown types
    };
    
    private static ModConfig config
    {
        get { return RerollManager.Get().LocalConfig; }
        set { RerollManager.Get().LocalConfig = value;  }
    }

    private static void RegisterPage(IGenericModConfigMenuApi GMCM, GMCM_PageConfig pageConfig, ModConfig modConfig)
    {
        //if we don't have the required key the add it to the config
        if (!modConfig.BoardConfigs.ContainsKey(pageConfig.configId))
        {
            ModEntry.GMonitor!.Log($"Could not find key {pageConfig.configId} in the config.json... adding", LogLevel.Warn);
            modConfig.BoardConfigs.Add(pageConfig.configId, new BoardConfig());
        }
        
        GMCM.AddPage(
            mod: ModEntry.GManifest!,
            pageId: pageConfig.configId,
            pageTitle: () => pageConfig.name
        );

        GMCM.AddBoolOption(
            mod: ModEntry.GManifest!,
            name: I18n.Settings_Board_AllowReroll_Label,
            tooltip: I18n.Settings_Board_AllowReroll_Tooltip,
            fieldId: $"{pageConfig.configId}_allowReroll",
            getValue: () => modConfig.BoardConfigs[pageConfig.configId].AllowReroll,
            setValue: value => modConfig.BoardConfigs[pageConfig.configId].AllowReroll = value
        );
        
        GMCM.AddBoolOption(
            mod: ModEntry.GManifest!,
            name: I18n.Settings_Board_InfiniteRerolls_Label,
            tooltip: I18n.Settings_Board_AllowReroll_Tooltip,
            fieldId: $"{pageConfig.configId}_infiniteReroll",
            getValue: () => modConfig.BoardConfigs[pageConfig.configId].InfiniteRerolls,
            setValue: value => modConfig.BoardConfigs[pageConfig.configId].InfiniteRerolls = value
        );

        GMCM.AddNumberOption(
            mod: ModEntry.GManifest!,
            name: I18n.Settings_Board_MaxRerolls_Label,
            tooltip: I18n.Settings_Board_MaxRerolls_Tooltip,
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
                mod: ModEntry.GManifest!,
                name: I18n.Settings_Board_Schedule_Label,
                tooltip: I18n.Settings_Board_Schedule_Tooltip,
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
        IGenericModConfigMenuApi? GMCM = ModEntry.GHelper!.ModRegistry.GetApi<GenericModConfigMenu.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (GMCM == null)
        {
            ModEntry.GMonitor!.Log("Generic Mod Config Menu not found. Skipping mod menu setup", LogLevel.Info);
            return;
        }
        
        GMCM.Register(mod: ModEntry.GManifest!, reset: () => config = new ModConfig(), save: () => ModEntry.GHelper!.WriteConfig(config));
        GMCM.OnFieldChanged(
            mod: ModEntry.GManifest!,
            onChange: ModEntry.OnGMCMFieldChanged
        );
        
        
        // GENERAL
        GMCM.AddSectionTitle(
            mod: ModEntry.GManifest!,
            text: I18n.Settings_General_Header
        );
        
        GMCM.AddParagraph(
            mod: ModEntry.GManifest!,
            text: I18n.Settings_General_Description
        );
        
        GMCM.AddBoolOption(
            mod: ModEntry.GManifest!,
            name: I18n.Settings_General_TrueRandom_Label,
            tooltip: I18n.Settings_General_TrueRandom_Tooltip,
            fieldId: "general_useTrueRandom",
            getValue: () => config.useTrueRandom,
            setValue: value => config.useTrueRandom = value
        );
        
        GMCM.AddKeybindList(
            mod: ModEntry.GManifest!,
            name: I18n.Settings_General_ResetKeybind_Label,
            tooltip: I18n.Settings_General_ResetKeybind_Tooltip,
            fieldId: "general_resetKeybind",
            getValue: () => config.resetRerollsKeybind,
            setValue: value => config.resetRerollsKeybind = value
        );
        
        GMCM.AddBoolOption(
            mod: ModEntry.GManifest!,
            name: I18n.Settings_General_ForceUnique_Label,
            tooltip: I18n.Settings_General_ForceUnique_Tooltip,
            fieldId: "general_useTrueRandom",
            getValue: () => config.forceUniqueIfPossible,
            setValue: value => config.forceUniqueIfPossible = value
        );
        
        GMCM.AddSectionTitle(
            mod: ModEntry.GManifest!,
            text: I18n.Settings_Board_Header
        );
        
        GMCM.AddParagraph(
            mod: ModEntry.GManifest!,
            text: I18n.Settings_Board_Description1
        );
        GMCM.AddParagraph(
            mod: ModEntry.GManifest!,
            text: I18n.Settings_Board_Description2
        );

        foreach (GMCM_PageConfig pageConfig in PageConfigs)
        {
            if (pageConfig.allRequiredIdsFound())
            {
                GMCM.AddPageLink(
                    mod: ModEntry.GManifest!,
                    pageId: pageConfig.configId,
                    text: () => pageConfig.name
                );
            }
        }
        
        GMCM.AddPageLink(
            mod: ModEntry.GManifest!,
            pageId: "credits",
            text: I18n.Settings_Credits_Header
            );
        
        foreach (GMCM_PageConfig pageConfig in PageConfigs)
        {
            if (pageConfig.allRequiredIdsFound())
            {
                RegisterPage(GMCM, pageConfig, RerollManager.Get().LocalConfig);
            }
        }
        
        // Begin Credits
        GMCM.AddPage(
            mod: ModEntry.GManifest!,
            pageId: "credits",
            pageTitle: I18n.Settings_Credits_Header
        );
        
        GMCM.AddSectionTitle(
            mod: ModEntry.GManifest!,
            text: I18n.Settings_Credits_Programming
        );
        GMCM.AddComplexOption(
            mod: ModEntry.GManifest!,
            name: () => "Xeru98",
            tooltip: null,
            fieldId: null,
            draw: (SpriteBatch beforeSave, Vector2 v) => { },
            height: null,
            beforeMenuOpened: null,
            beforeSave: null,
            afterReset: null
        );
        GMCM.AddSectionTitle(
            mod: ModEntry.GManifest!,
            text: I18n.Settings_Credits_Translations_Header
        );
        foreach(KeyValuePair<Func<string>, string> translator in TRANSLATIONS)
        {
            GMCM.AddComplexOption(
                mod: ModEntry.GManifest!,
                name: translator.Key,
                tooltip: null,
                fieldId: null,
                draw: (SpriteBatch b, Vector2 v) =>
                {
                    Utility.drawTextWithShadow(b, translator.Value, Game1.dialogueFont, v, Color.Black);
                },
                height: null,
                beforeMenuOpened: null,
                beforeSave: null,
                afterReset: null
            );
        }
    }
}
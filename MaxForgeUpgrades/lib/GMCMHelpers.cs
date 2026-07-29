using GenericModConfigMenu;
using StardewModdingAPI;

namespace MaxForgeUpgrades.lib;

public class GMCMHelpers
{
    
    public static void SetupGMCM()
    {
        IGenericModConfigMenuApi? GMCM = ModEntry.GHelper!.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (GMCM is null)
        {
            ModEntry.GMonitor!.Log("Generic Mod Config Menu not found. Skipping mod menu setup", LogLevel.Info);
            return;
        }

        GMCM.Register(mod: ModEntry.GManifest!, reset: () => ModEntry.Config = new ModConfig(), save: () => ModEntry.GHelper!.WriteConfig(ModEntry.Config));
        
        GMCM.AddNumberOption(ModEntry.GManifest!,
            name: I18n.Settings_MaxForgeUpgrades_Label,
            tooltip: I18n.Settings_MaxForgeUpgrades_Tooltip,
            fieldId: "MaxForgeUpgrades_MaxForgeUpgrades",
            getValue: () => ModEntry.Config.MaxForgeUpgrades,
            setValue: (i) => ModEntry.Config.MaxForgeUpgrades = i,
            min: 0
        );
        
        GMCM.AddNumberOption(ModEntry.GManifest!,
            name: I18n.Settings_MaxForgePerGem_Label,
            tooltip: I18n.Settings_MaxForgePerGem_Tooltip,
            fieldId: "MaxForgeUpgrades_MaxForgePerGem",
            getValue: () => ModEntry.Config.MaxForgePerGem,
            setValue: (i) => ModEntry.Config.MaxForgePerGem = i,
            min: 0
        );
        
        GMCM.AddNumberOption(ModEntry.GManifest!,
            name: I18n.Settings_CinderShards_Base_Label,
            tooltip: I18n.Settings_CinderShards_Base_Tooltip,
            fieldId: "MaxForgeUpgrades_CinderShards_Base",
            getValue: () => ModEntry.Config.CinderShardBaseValue,
            setValue: (i) => ModEntry.Config.CinderShardBaseValue = i,
            min: 0
        );
        
        GMCM.AddNumberOption(ModEntry.GManifest!,
            name: I18n.Settings_CinderShards_Scaling_Label,
            tooltip: I18n.Settings_CinderShards_Scaling_Tooltip,
            fieldId: "MaxForgeUpgrades_CinderShards_Scaling",
            getValue: () => ModEntry.Config.CinderShardScaling,
            setValue: (i) => ModEntry.Config.CinderShardScaling = i,
            min: 0
        );
    }
}
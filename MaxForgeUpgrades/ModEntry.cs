using HarmonyLib;
using MaxForgeUpgrades.lib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Menus;
using StardewValley.Tools;

namespace MaxForgeUpgrades;

public class ModEntry : Mod
{
    public static IModHelper? GHelper;
    public static IMonitor? GMonitor;
    public static IManifest? GManifest;
    public static ModConfig Config;

    public override void Entry(IModHelper helper)
    {
        GHelper = helper;
        GMonitor = Monitor;
        GManifest = ModManifest;
        Config = GHelper.ReadConfig<ModConfig>();
        I18n.Init(helper.Translation);

        helper.Events.GameLoop.GameLaunched += Lifecycle_OnGameLaunch;
        
        // All of the patches are in the ModEntry class in the Patches region
        Harmony harmony = new(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.GetMaxForges)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(GetMaxForges_Postfix))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(Tool), nameof(Tool.CanForge)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Tool_CanForge_Postfix))
        );

        harmony.Patch(
            original: AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.GetForgeCostAtLevel)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(ForgeMenu_GetForgeCostAtLevel_Postfix))
        );
    }
    
    #region Game Loop
    // Called when the game is launched and sets up the GMCM integration if found
    private void Lifecycle_OnGameLaunch(object? sender, GameLaunchedEventArgs args)
    {
        GMCMHelpers.SetupGMCM();
    }
    #endregion
    
    #region Patches
    
    internal static void GetMaxForges_Postfix(ref int __result)
    {
        __result = Config.MaxForgeUpgrades;
    }

    internal static void Tool_CanForge_Postfix(Item item, ref Tool __instance, ref bool __result)
    {
        // We only care about melee weapons so we can just skip if it's not.
        if (__instance is not MeleeWeapon)
        {
            return;
        }

        BaseEnchantment? target_enchantment = BaseEnchantment.GetEnchantmentFromItem(__instance, item);
        // We can't find the enchantment
        if (target_enchantment is null)
        {
            return;
        }

        // We have to do this because we are comparing enchantment class types. This should work for any future enchantments as well
        foreach (BaseEnchantment equipped_enchantment in __instance.enchantments)
        {
            if (equipped_enchantment.GetType() == target_enchantment.GetType())
            {
                int currentLevel = equipped_enchantment.Level;
                __result = (currentLevel < Config.MaxForgePerGem); //set to true if we are less than the max forges per gem
                return;
            }
        }
    }

    internal static void ForgeMenu_GetForgeCostAtLevel_Postfix(int level, ref int __result)
    {
        __result = Config.CinderShardBaseValue + (level * Config.CinderShardScaling);
    }
    
    #endregion
}

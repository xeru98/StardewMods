using StardewModdingAPI.Utilities;

namespace BetterSpecialOrders;

public class ModConfig
{
    /** General Settings **/
    public KeybindList resetRerollsKeybind = new KeybindList();

    public bool useTrueRandom = false;

    public bool forceUniqueIfPossible = true;

    // Allows each client ot use their own settings
    public bool allowLocalControl = false;

    
    // Lookup table that uses the configID (not the ordertype in case they conflict) as the key
    public Dictionary<string, BoardConfig> BoardConfigs = new Dictionary<string, BoardConfig>()
    {
        // vanilla
        {Constants.ConfigKeys.SV, new BoardConfig(Constants.OrderTypes.SV)},
        {Constants.ConfigKeys.Qi, new BoardConfig(Constants.OrderTypes.Qi)},
        {Constants.ConfigKeys.DesertFestival, new BoardConfig(Constants.OrderTypes.DesertFestival, new bool[]{true, true, true, true, true, true, true})},
        
        // MODS
        // Ridgeside Village
        {Constants.ConfigKeys.RSVTown, new BoardConfig(Constants.OrderTypes.RSVTown)},
        {Constants.ConfigKeys.RSVNinja, new BoardConfig(Constants.OrderTypes.RSVNinja)},
        
        // Mt. Vapius
        {Constants.ConfigKeys.MtVapius, new BoardConfig(Constants.OrderTypes.MtVapius)},
        
        // All other custom boards
        {Constants.ConfigKeys.Custom, new BoardConfig(Constants.OrderTypes.Custom)},
    };
    
}
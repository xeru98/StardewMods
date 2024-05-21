using StardewModdingAPI.Utilities;

namespace BetterSpecialOrders;

public class ModConfig
{
    /** General Settings **/
    public KeybindList resetRerollsKeybind = new KeybindList();

    public bool useTrueRandom = false;

    public bool forceUniqueIfPossible = true; //unsupported

    public Dictionary<string, BoardConfig> BoardConfigs = new Dictionary<string, BoardConfig>();
}
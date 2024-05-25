using BetterSpecialOrders.Messages;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace BetterSpecialOrders;

public class RerollManager
{

    private static RerollManager? _instance;
    public ModConfig LocalConfig; // config loaded from the local FS
    private ModConfig? _hostConfig; // requested from the host and used for most of the checking
    
    /*
     * Game State (the active configs)
     * BoardConfigs and RerollsRemaining are replicated to all clients
     * Unlike the ModConfig these use the order type as the key for easy lookup
     */
    private Dictionary<string, BoardConfig> _boardConfigs = new Dictionary<string, BoardConfig>();
    private Dictionary<string, int> _rerollsRemaining = new Dictionary<string, int>();
    private int _rerollsToday = 0;
    
    // cache for overriding the Monday reroll if necessary
    private readonly List<SpecialOrder> _lastAvailableSpecialOrders;

    
    // Singleton Pattern
    public static RerollManager Get()
    {
        if (_instance == null)
        {
            _instance = new RerollManager();
        }

        return _instance;
    }

    private RerollManager()
    {
        // Load local Settings from the Mod Config
        LocalConfig = ModEntry.GHelper!.ReadConfig<ModConfig>();
        _hostConfig = null;
        
        RebuildGameState();
        
        _lastAvailableSpecialOrders = new List<SpecialOrder>(); // create this to avoid null issue
    }
    
    #region GETTERS/SETTERS

    // Helper that gets the active config the reroll manager will use to track
    public ModConfig GetActiveConfig()
    {
        if (_hostConfig == null)
        {
            return LocalConfig;
        }

        return _hostConfig.allowLocalControl ? LocalConfig : _hostConfig;
    }
    
    // Gets the config key for a given orderType
    public string GetOrderTypeKey(string orderType)
    {
        
        if (!_boardConfigs.ContainsKey(orderType))
        {
            ModEntry.GMonitor!.Log($"Could not find entry for {orderType}... using custom board settings. Consider messaging the mod developer to request they add support for this key.", LogLevel.Alert);
            return "custom";
        }
        return orderType;
    }

    private BoardConfig GetBoardConfig(string orderType)
    {
        return _boardConfigs[GetOrderTypeKey(orderType)];
    }

    public bool AllowRerolls(string orderType)
    {
        return GetBoardConfig(orderType).AllowReroll;
    }

    public bool InfiniteRerolls(string orderType)
    {
        return GetBoardConfig(orderType).InfiniteRerolls;
    }

    public int GetRerollsRemaining(string orderType)
    {
        return _rerollsRemaining[GetOrderTypeKey(orderType)];
    }

    public void SetRerollsRemaining(string orderType, int value)
    {
        _rerollsRemaining[GetOrderTypeKey(orderType)] = value;
    }
    
    #endregion

    #region REPLICATION

    public void OnRep_RerollsRemaining(RepRerollsRemaining msg)
    {
        _rerollsRemaining = msg.RerollsRemaining;
    }

    public void OnRep_HostConfig(RepHostConfig msg)
    {
        _hostConfig = msg.HostConfig;
        RebuildGameState();
    }

    public void Sync_HostConfig(long? peerId = null)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }
        // sync can only be initiated by the server
        if (!Context.IsMainPlayer)
        {
            return;
        }
        ModEntry.GMonitor!.Log("Host initiating Config sync");
        ModEntry.GHelper!.Multiplayer.SendMessage(
            new RepHostConfig(GetActiveConfig()), 
            Constants.REP_HOST_CONFIG, 
            modIDs: new string[] {ModEntry.ModID}, 
            playerIDs: peerId == null ? null : new long[]{peerId.Value}
        );
    }

    public void Sync_RerollsRemaining(long? peerId = null)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }
        // sync can only be initiated by the server
        if (!Context.IsMainPlayer)
        {
            return;
        }
        ModEntry.GMonitor!.Log("Host initiating rerolls remaining sync");
        ModEntry.GHelper!.Multiplayer.SendMessage(
            new RepRerollsRemaining(_rerollsRemaining), 
            Constants.REP_REROLLS_REMAINING, 
            modIDs: new string[] {ModEntry.ModID}, 
            playerIDs: peerId == null ? null : new long[]{peerId.Value}
        );
    }
    
    #endregion
    
    #region REROLLS
    
    /*
     * Checks to see whether a reroll is allowed
     *  this means checking to see if one of the
     *  options on the board is currently selected.
     * Additionally, we check to see if there are
     *  rerolls left or if we have infinite rerolls
     *  enabled
     */
    public bool CanReroll(string orderType)
    {
        // if we can't reroll then we can't reroll... duh
        if (!AllowRerolls(orderType))
        {
            return false;
        }
        
        // if there are no rerolls left and we don't have unlimited
        if (GetRerollsRemaining(orderType) <= 0 && !InfiniteRerolls(orderType))
        {
            return false;
        }

        // prevent rerolling if we already have one of the quests
        foreach (SpecialOrder availableOrder in Game1.player.team.availableSpecialOrders.Where(order => order.orderType.Value == orderType))
        {
            foreach (SpecialOrder currentOrder in Game1.player.team.specialOrders.Where(order => order.orderType.Value == orderType))
            {
                if (currentOrder.questKey == availableOrder.questKey)
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    /* Rep-Server
     * This is a modified copy of SpecialOrder.UpdateAvailableSpecialOrders
     *  which adds a counter for the number of times rerolled today and uses
     *  it to generate a seed that actually allows the rerolling to occur.
     * If we are not the host then we send a request to the host to replicate
     */
    public void Reroll(string orderType)
    {
        // if we aren't the main player then send a reroll request to the main player
        if (!Context.IsMainPlayer)
        {
            ModEntry.GHelper!.Multiplayer.SendMessage(
                new RequestReroll(orderType), 
                Constants.REQUEST_REROLL, 
                modIDs: new []{ModEntry.ModID}, 
                playerIDs: new []{Game1.MasterPlayer.UniqueMultiplayerID}
            );
            return;
        }

        string configKey = GetOrderTypeKey(orderType);

        //if we can't reroll then exit
        if (!CanReroll(orderType))
        {
            ModEntry.GMonitor!.Log("Cannot Reroll Order Type");
            return;
        }

        ModEntry.GMonitor!.Log($"Rerolling Board: {configKey} OrderType: {orderType}");
        
        // get the current list of quest keys so we can potentially use it to filter out orders
        List<string> currentOrderKeys = Game1.player.team.availableSpecialOrders.Where(order => order.orderType.Value == orderType).Select(order => order.questKey.Value).ToList();
        _rerollsToday += 1; // do this first to avoid getting the same options on the first reroll of the day
        SpecialOrder.RemoveAllSpecialOrders(orderType);
        
        // create random number generator
        Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 1.3, _rerollsToday);
        // if the user wants true random then convert to true random generator
        if (GetActiveConfig().useTrueRandom)
        {
            r = new Random();
        }
        
        // generate a list of possible orders
        List<string> keyQueue = new List<string>();
        List<string> avoidQueue = new List<string>(); //list of orders we just had
        foreach (KeyValuePair<string, SpecialOrderData> pair in DataLoader.SpecialOrders(Game1.content))
        {
            if (pair.Value.OrderType == orderType && SpecialOrder.CanStartOrderNow(pair.Key, pair.Value))
            {
                if (GetActiveConfig().forceUniqueIfPossible && currentOrderKeys.Contains(pair.Key))
                {
                    avoidQueue.Add(pair.Key);
                }
                else
                {
                    keyQueue.Add(pair.Key);
                }
            }
        }
        // if we are using force and the key queue is <2 then add a random one from the avoid queue
        if (GetActiveConfig().forceUniqueIfPossible && keyQueue.Count < 2)
        {
            keyQueue.Add(r.ChooseFrom(avoidQueue));
        }
        
        // remove completed base orders
        List<string> keysIncludingCompleted = new List<string>(keyQueue);
        if (orderType == "")
        {
            keyQueue.RemoveAll(id => Game1.player.team.completedSpecialOrders.Contains(id));
        }

        
        
        // Create a queue of available special orders
        for (int i = 0; i < 2; i++)
        {
            if (keyQueue.Count == 0)
            {
                if (keysIncludingCompleted.Count == 0)
                {
                    break;
                }

                keyQueue = new List<string>(keysIncludingCompleted);
            }

            string key = r.ChooseFrom(keyQueue);
            
            SpecialOrder order = SpecialOrder.GetSpecialOrder(key, r.Next());
            order.SetHardOrderDuration(); // override the current duration (does not affect accepted order duration)
            Game1.player.team.availableSpecialOrders.Add(order);
            keyQueue.Remove(key);
            keysIncludingCompleted.Remove(key);
        }
        
        // after reroll complete subtract from available rerolls and notify clients of update to available orders
        SetRerollsRemaining(orderType, GetRerollsRemaining(orderType) - 1);
        Sync_RerollsRemaining();
    }
    
    // Resets the rerolls for the specified type back to their max values;
    // HOST ONLY
    public void ResetRerolls(string orderType = Constants.ALL, bool resetDayTotal = false)
    {
        // Main player only
        if (!Context.IsMainPlayer)
        {
            return;
        }
        ModEntry.GMonitor!.Log("Resetting rerolls back to max values");

        if (resetDayTotal)
        {
            _rerollsToday = 0;
        }

        if (orderType == Constants.ALL)
        {
            // support custom board types
            foreach (string orderTypeKey in _rerollsRemaining.Keys)
            {
                SetRerollsRemaining(orderTypeKey, GetBoardConfig(orderTypeKey).MaxRerolls);
            }
        }
        else
        {
            SetRerollsRemaining(orderType, GetBoardConfig(orderType).MaxRerolls);
        }
        Sync_RerollsRemaining();
    }
    
    #endregion

    #region CONFIG MANAGEMENT

    // Rebuild the config settings from the config file
    public void RebuildGameState()
    {
        if (Context.IsMainPlayer)
        {
            _hostConfig = LocalConfig;
        }
        _boardConfigs = LoadBoardConfigs();
        _rerollsRemaining = LoadDefaultRerollsRemaining();
        if (Context.IsMainPlayer)
        {
            Sync_HostConfig();
        }
    }
    
    private Dictionary<string, BoardConfig> LoadBoardConfigs()
    {
        ModEntry.GMonitor!.Log("Loading board configs from active config");
        Dictionary<string, BoardConfig> loadedBoardConfigs = new Dictionary<string, BoardConfig>();
        foreach (BoardConfig boardConfig in GetActiveConfig().BoardConfigs.Values)
        {
            if (!loadedBoardConfigs.ContainsKey(boardConfig.OrderType))
            {
                loadedBoardConfigs.Add(boardConfig.OrderType, boardConfig);
            }
            else
            {
                loadedBoardConfigs[boardConfig.OrderType] = boardConfig;
            }
            
        }

        return loadedBoardConfigs;
    }

    private Dictionary<string, int> LoadDefaultRerollsRemaining()
    {
        ModEntry.GMonitor!.Log("Loading default rerolls remaining from active config");
        Dictionary<string, int> loadedRerollsRemaining = new Dictionary<string, int>();
        foreach (BoardConfig boardConfig in GetActiveConfig().BoardConfigs.Values)
        {
            if (!loadedRerollsRemaining.ContainsKey(boardConfig.OrderType))
            {
                loadedRerollsRemaining.Add(boardConfig.OrderType, boardConfig.MaxRerolls);
            }
            else
            {
                loadedRerollsRemaining[boardConfig.OrderType] = boardConfig.MaxRerolls;
            }
            
        }

        return loadedRerollsRemaining;
    }
    
    #endregion

    #region CACHING

    // Clears the current cache and adds back the requested order type from the current available orders
    public void CacheCurrentAvailableSpecialOrders(string orderType = Constants.ALL)
    {
        ModEntry.GMonitor!.Log($"Caching Current Available Special Orders... Type: {orderType}");
        // Clears only the requested type from the cache
        if (orderType == Constants.ALL)
        {
            _lastAvailableSpecialOrders.Clear();
        }
        else
        {
            _lastAvailableSpecialOrders.RemoveAll(order => order.orderType.Get() == orderType);
        }
        
        // Add the selected order type from the current list to the cache
        foreach (SpecialOrder order in Game1.player.team.availableSpecialOrders)
        {
            if (orderType == Constants.ALL || orderType == order.orderType.Get())
            {
                _lastAvailableSpecialOrders.Add(order);
            }
        }
    }

    // Loads the requested orderType from the cache
    public void ReloadSpecialOrdersFromCache(string orderType = Constants.ALL)
    {
        ModEntry.GMonitor!.Log($"Reloading orders from cache matching type {orderType}");
        // remove the current orders that match the type
        if (orderType == Constants.ALL)
        {
            Game1.player.team.availableSpecialOrders.Clear();
        }
        else
        {
            Game1.player.team.availableSpecialOrders.RemoveWhere(order => order.orderType.Get() == orderType);
        }

        // reload from the cache
        foreach (SpecialOrder order in _lastAvailableSpecialOrders)
        {
            if (orderType == Constants.ALL || orderType == order.orderType.Get())
            {
                order.SetHardOrderDuration();
                Game1.player.team.availableSpecialOrders.Add(order);
            }
        }
    }

    #endregion
}
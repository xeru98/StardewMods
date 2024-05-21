﻿using BetterSpecialOrders.Messages;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Network;
using StardewValley.SpecialOrders;

namespace BetterSpecialOrders;

public class RerollManager
{

    private static RerollManager? _instance;
    public ModConfig config;
    
    // Game State
    public IDictionary<string, BoardConfig> LocalBoardConfigs = new Dictionary<string, BoardConfig>();
    public IDictionary<string, int> LocalRerollsRemaining = new Dictionary<string, int>();
    public Dictionary<string, BoardConfig> BoardConfigs = new Dictionary<string, BoardConfig>();
    private Dictionary<string, int> RerollsRemaining = new Dictionary<string, int>();
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
        // Load Settings from the Mod Config
        config = ModEntry.GHelper!.ReadConfig<ModConfig>();
        //use the config to rebuild the state;
        LocalBoardConfigs = LoadBoardConfigs();
        LocalRerollsRemaining = LoadDefaultRerollsRemaining();
        
        RebuildConfig();
        
        _lastAvailableSpecialOrders = new List<SpecialOrder>(); // create this to avoid null issue
    }

    public void Tick()
    {
        
        //ModEntry.GMonitor.Log($"Board Configs: {BoardConfigs.Dirty} | Rerolls Remaining: {RerollsRemaining.Dirty}");
        
        if (!Context.IsMainPlayer)
        {
            return;
        }
    }
    
    #region REROLLS
    
    /*
     * Checks to see whether a reroll is allowed
     *  this means checking to see if one of the
     *  options on the board is currently selected.
     * Additionally, we check to see if there are
     *  rerolls left or if we have infinite rerolls
     *  enabled
     */
    public bool CanReroll(string boardKey)
    {
        // if we can't reroll then we can't reroll... duh
        if (!BoardConfigs[boardKey].AllowReroll)
        {
            return false;
        }
        
        // if there are no rerolls left and we don't have unlimited
        if (RerollsRemaining[boardKey] <= 0 && !BoardConfigs[boardKey].InfiniteRerolls)
        {
            return false;
        }

        foreach (SpecialOrder availableOrder in Game1.player.team.availableSpecialOrders)
        {
            foreach (SpecialOrder currentOrder in Game1.player.team.specialOrders)
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

        string boardKey = orderType;

        // in the event we can't find the board key we just use the custom settings
        if (!BoardConfigs.ContainsKey(boardKey))
        {
            boardKey = "custom";
        }
        //handle actual reroll logic
        if (!CanReroll(boardKey))
        {
            ModEntry.GMonitor!.Log("Cannot Reroll Order Type");
            return;
        }
        
        ModEntry.GMonitor!.Log($"Rerolling Board: {boardKey} OrderType: {orderType}");
        _rerollsToday += 1; // do this first to avoid getting the same options on the first reroll of the day
        SpecialOrder.RemoveAllSpecialOrders(orderType);
        List<string> keyQueue = new List<string>();
        foreach (KeyValuePair<string, SpecialOrderData> pair in DataLoader.SpecialOrders(Game1.content))
        {
            if (pair.Value.OrderType == orderType && SpecialOrder.CanStartOrderNow(pair.Key, pair.Value))
            {
                keyQueue.Add(pair.Key);
            }
        }
        List<string> keysIncludingCompleted = new List<string>(keyQueue);
        if (orderType == "")
        {
            keyQueue.RemoveAll(id => Game1.player.team.completedSpecialOrders.Contains(id));
        }

        Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed * 1.3, _rerollsToday);
        
        // if the user wants true random then scrub and start over
        if (config.useTrueRandom)
        {
            r = new Random();
        }
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
            order.SetHardOrderDuration(); // override the current duration
            Game1.player.team.availableSpecialOrders.Add(order);
            keyQueue.Remove(key);
            keysIncludingCompleted.Remove(key);
        }
        
        // after reroll complete subtract from available rerolls and notify clients of update to available orders
        RerollsRemaining[boardKey] -= 1;
        
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
        ModEntry.GMonitor.Log("Resetting rerolls back to max values");

        if (resetDayTotal)
        {
            _rerollsToday = 0;
        }

        if (orderType == Constants.ALL)
        {
            // support custom board types
            foreach (BoardConfig board in BoardConfigs.Values)
            {
                RerollsRemaining[board.OrderType] = board.MaxRerolls;
            }
        }
        else
        {
            RerollsRemaining[orderType] = BoardConfigs[orderType].MaxRerolls;
        }
    }
    
    #endregion

    #region CONFIG MANAGEMENT

    // Rebuild the config settings from the config file
    public void RebuildConfig()
    {
        BoardConfigs = LoadBoardConfigs();
        RerollsRemaining = LoadDefaultRerollsRemaining();
    }
    
    private Dictionary<string, BoardConfig> LoadBoardConfigs()
    {
        // when these are loaded from the FS we are using the ordertype as the new key
        Dictionary<string, BoardConfig> loadedBoardConfigs = new Dictionary<string, BoardConfig>();
        foreach (BoardConfig boardConfig in config.BoardConfigs.Values)
        {
            loadedBoardConfigs.Add(boardConfig.OrderType, boardConfig);
        }

        return loadedBoardConfigs;
    }

    private Dictionary<string, int> LoadDefaultRerollsRemaining()
    {
        // when these are loaded from the FS we are using the ordertype as the new key
        Dictionary<string, int> loadedRerollsRemaining = new Dictionary<string, int>();
        foreach (BoardConfig boardConfig in config.BoardConfigs.Values)
        {
            loadedRerollsRemaining.Add(boardConfig.OrderType, boardConfig.MaxRerolls);
        }

        return loadedRerollsRemaining;
    }
    
    #endregion

    #region CACHING

    // Clears the current cache and adds back the requested order type from the current available orders
    public void CacheCurrentAvailableSpecialOrders(string orderType = Constants.ALL)
    {
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
        ModEntry.GMonitor!.Log("Running Reload");
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
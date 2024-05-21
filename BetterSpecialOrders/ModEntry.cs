using BetterSpecialOrders.Messages;
using BetterSpecialOrders.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace BetterSpecialOrders;

public class ModEntry : Mod
{

    public static IModHelper? GHelper;
    public static IMonitor? GMonitor;
    public static IManifest? GManifest;

    public static string ModID = "";

    private ModConfig config
    {
        get { return RerollManager.Get().config; }
        set { RerollManager.Get().config = value;  }
    }
    
    /* #TODO
     * 1) test menu options
     * 2) test reroll
     * 3) add multiplayer replication messages
     * 4) test ridgeside compatibility
     */
    
    public override void Entry(IModHelper helper)
    {
        GHelper = helper;
        GMonitor = Monitor;
        GManifest = ModManifest;
        ModID = ModManifest.UniqueID;
        RerollManager.Get(); // init the game state since the monitor and helper now exist

        // hook up events
        helper.Events.GameLoop.GameLaunched += Lifecycle_OnGameLaunch;
        helper.Events.GameLoop.DayEnding += Lifecycle_OnDayEnd;
        helper.Events.GameLoop.DayStarted += Lifecycle_OnDayStart;
        helper.Events.GameLoop.UpdateTicked += Lifecycle_OnUpdateTicked;
        helper.Events.Input.ButtonsChanged += Input_OnButtonsChanged;
        helper.Events.Display.MenuChanged += Display_OnMenuChanged;
        helper.Events.Multiplayer.ModMessageReceived += Multiplayer_OnMessageRecieved;
        
        Monitor.Log("Mod Loaded");
    }
    
    // Called when the game is launched and sets up the GMCM integration if found
    internal void Lifecycle_OnGameLaunch(object? sender, GameLaunchedEventArgs args)
    {
        GMCMHelpers.SetupGMCM();
    }

    internal void Lifecycle_OnUpdateTicked(object? sender, UpdateTickedEventArgs args)
    {
        RerollManager.Get().Tick();
    }
    
    internal void Lifecycle_OnDayEnd(object? sendex, DayEndingEventArgs args)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }

        RerollManager.Get().CacheCurrentAvailableSpecialOrders();
    }

    internal void Lifecycle_OnDayStart(object? sender, DayStartedEventArgs args)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }
        
        // use this over Game1.Date.DayOfWeek because that returns an enum
        int dayOfTheWeek = Game1.Date.DayOfMonth % 7;

        foreach (BoardConfig boardConfig in config.BoardConfigs.Values)
        {
            if (boardConfig.shouldRefreshToday(dayOfTheWeek))
            {
                Monitor.Log($"Daily Refresh: {boardConfig.OrderType}");
                RerollManager.Get().Reroll(Constants.SVBoardContext);
            }
            else
            {
                Monitor.Log($"Daily Refresh... loading from cache: {boardConfig.OrderType}");
                RerollManager.Get().ReloadSpecialOrdersFromCache(Constants.SVBoardContext);
            }
        }

        
        // Reset the daily rerolls
        RerollManager.Get().ResetRerolls(resetDayTotal: true);
    }

    

    // Triggered when the buttons are changed
    internal void Input_OnButtonsChanged(object? sender, ButtonsChangedEventArgs args)
    {
        // only the host can trigger a reset
        if (!Context.IsMainPlayer)
        {
            return;
        }
        
        if (config.resetRerollsKeybind.IsDown())
        {
            Monitor.Log("Host Resetting Reroll With Keybind");
            RerollManager.Get().ResetRerolls();
        }
    }

    internal void Display_OnMenuChanged(object? sender, MenuChangedEventArgs args)
    {
        if (!Context.IsWorldReady)
        {
            return;
        }

        if (args.NewMenu is BetterSpecialOrdersBoard)
        {
            return;
        }

        if (args.NewMenu is SpecialOrdersBoard)
        {
            Monitor.Log("New Menu is a special orders board... replacing with custom one");
            Game1.activeClickableMenu = new BetterSpecialOrdersBoard(args.NewMenu as SpecialOrdersBoard)
            {
                behaviorBeforeCleanup = args.NewMenu.behaviorBeforeCleanup
            };
        }
    }

    internal void Multiplayer_OnMessageRecieved(object? sender, ModMessageReceivedEventArgs args)
    {
        if (args.FromModID != ModID)
        {
            return;
        }
        
        if (args.Type == Constants.REQUEST_REROLL)
        {
            RequestReroll msg = args.ReadAs<RequestReroll>();
            RerollManager.Get().Reroll(msg.orderType);
        }
    }

    public static void OnGMCMFieldChanged(string fieldId, object newValue)
    {
        if (Context.IsMainPlayer)
        {
            GMonitor.Log("Main Player updated Config. Rebuilding board configs");
            RerollManager.Get().RebuildConfig();
        }
    }
}
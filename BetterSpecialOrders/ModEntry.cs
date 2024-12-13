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
        get { return RerollManager.Get().GetActiveConfig(); }
    }
    
    public override void Entry(IModHelper helper)
    {
        GHelper = helper;
        GMonitor = Monitor;
        GManifest = ModManifest;
        ModID = ModManifest.UniqueID;
        I18n.Init(helper.Translation);
        RerollManager.Get(); // init the game state since the monitor and helper now exist

        // hook up events
        helper.Events.GameLoop.GameLaunched += Lifecycle_OnGameLaunch;
        helper.Events.GameLoop.DayEnding += Lifecycle_OnDayEnd;
        helper.Events.GameLoop.DayStarted += Lifecycle_OnDayStart;
        helper.Events.Input.ButtonsChanged += Input_OnButtonsChanged;
        helper.Events.Display.MenuChanged += Display_OnMenuChanged;
        helper.Events.Multiplayer.PeerConnected += Multiplayer_OnPeerConnected;
        helper.Events.Multiplayer.ModMessageReceived += Multiplayer_OnMessageRecieved;
        
        Monitor.Log("Mod Loaded");
    }
    
    #region Game Loop

    // Called when the game is launched and sets up the GMCM integration if found
    private void Lifecycle_OnGameLaunch(object? sender, GameLaunchedEventArgs args)
    {
        GMCMHelpers.SetupGMCM();
    }
    
    private void Lifecycle_OnDayEnd(object? sender, DayEndingEventArgs args)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }

        RerollManager.Get().CacheCurrentAvailableSpecialOrders();
    }

    private void Lifecycle_OnDayStart(object? sender, DayStartedEventArgs args)
    {
        if (!Context.IsMainPlayer)
        {
            return;
        }
        
        // use this over Game1.Date.DayOfWeek because that returns an enum
        int dayOfTheWeek = Game1.Date.DayOfMonth % 7;

        foreach (BoardConfig boardConfig in config.BoardConfigs.Values)
        {
            if (boardConfig.ShouldRefreshToday(dayOfTheWeek))
            {
                Monitor.Log($"Daily Refresh: {boardConfig.OrderType}");
                RerollManager.Get().Reroll(boardConfig.OrderType);
            }
            else
            {
                Monitor.Log($"Daily Refresh... loading from cache: {boardConfig.OrderType}");
                RerollManager.Get().ReloadSpecialOrdersFromCache(boardConfig.OrderType);
            }
        }

        
        // Reset the daily rerolls
        RerollManager.Get().ResetRerolls(resetDayTotal: true);
    }

    #endregion
    
    #region Input

    // Triggered when the buttons are changed
    private void Input_OnButtonsChanged(object? sender, ButtonsChangedEventArgs args)
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
    
    #endregion

    #region Display

    private void Display_OnMenuChanged(object? sender, MenuChangedEventArgs args)
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
            Game1.activeClickableMenu = new BetterSpecialOrdersBoard((args.NewMenu as SpecialOrdersBoard)!)
            {
                behaviorBeforeCleanup = args.NewMenu.behaviorBeforeCleanup
            };
        }
    }
    
    #endregion

    #region Multiplayer
    
    private void Multiplayer_OnPeerConnected(object? sender, PeerConnectedEventArgs args)
    {
        // peer connection sync only initiated by host
        if (!Context.IsMainPlayer)
        {
            return;
        }
        
        RerollManager.Get().Sync_HostConfig(args.Peer.PlayerID);
        RerollManager.Get().Sync_RerollsRemaining(args.Peer.PlayerID);
    }
    
    private void Multiplayer_OnMessageRecieved(object? sender, ModMessageReceivedEventArgs args)
    {
        if (args.FromModID != ModID)
        {
            return;
        }

        // main player listens for reroll requests
        if (Context.IsMainPlayer)
        {
            Monitor.Log($"Message recieved by host: {args.Type}");
            if (args.Type == Constants.REQUEST_REROLL)
            {
                RequestReroll msg = args.ReadAs<RequestReroll>();
                RerollManager.Get().Reroll(msg.orderType);
            }
        }

        // peers listen for updates
        if (args.FromPlayerID == Game1.MasterPlayer.UniqueMultiplayerID)
        {
            Monitor.Log($"Message recieved from host: {args.Type}");
            if (args.Type == Constants.REP_HOST_CONFIG)
            {
                RerollManager.Get().OnRep_HostConfig(args.ReadAs<RepHostConfig>());
            } else if (args.Type == Constants.REP_REROLLS_REMAINING)
            {
                RerollManager.Get().OnRep_RerollsRemaining(args.ReadAs<RepRerollsRemaining>());
            }
        }
    }
    
    #endregion

    public static void OnGMCMFieldChanged(string fieldId, object newValue)
    {
        if (!Context.IsMainPlayer)
        {
            GMonitor!.Log("Main Player updated Config. Rebuilding board configs");
            RerollManager.Get().RebuildGameState();
        }
    }
}
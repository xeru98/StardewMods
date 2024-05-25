namespace BetterSpecialOrders;

public static class Constants
{
    public const string ALL = "all";
    
    public const string REQUEST_REROLL = "REQUEST_Reroll";
    public const string REP_HOST_CONFIG = "REP_HostConfig";
    public const string REP_REROLLS_REMAINING = "REP_RerollsRemaining";

    public static class OrderTypes
    {
        // vanilla
        public const string SV = "";
        public const string Qi = "Qi";
        public const string DesertFestival = "DesertFestivalMarlon";
    
        // mods
        public const string RSVTown = "RSVTownSO";
        public const string RSVNinja = "RSVNinjaSO";
        public const string MtVapius = "Esca.EMP/MtVapiusBoard";
        public const string Custom = "custom";
    }

    public static class ConfigKeys
    {
        // vanilla
        public const string SV = "sv";
        public const string Qi = "qi";
        public const string DesertFestival = "de";
    
        // mods
        public const string RSVTown = "rsv_town";
        public const string RSVNinja = "rsv_ninja";
        public const string MtVapius = "mt_vapius";
        public const string Custom = "custom";
    }
}
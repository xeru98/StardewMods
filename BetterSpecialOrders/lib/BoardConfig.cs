using Netcode;

namespace BetterSpecialOrders;

public class BoardConfig
{
    public string OrderType = new NetString("");
    public bool  AllowReroll = new NetBool(false);
    public bool InfiniteRerolls = new NetBool(false);
    public int MaxRerolls = new NetInt(0);
    public bool[] RefreshSchedule = new bool[7];

    public NetFields NetFields { get; } = new NetFields("BetterSpecialOrders.BoardConfig");

    public BoardConfig()
    {
        OrderType = "";
        
    }

    public BoardConfig(string ctx, bool allowReroll, bool infiniteRerolls, int maxRerolls, bool[] refreshSchedule)
    {
        OrderType = ctx;
        AllowReroll = allowReroll;
        InfiniteRerolls = infiniteRerolls;
        MaxRerolls = maxRerolls;
        RefreshSchedule = refreshSchedule;
    }

    public bool shouldRefreshToday(int dayOfTheWeek)
    {
        return RefreshSchedule[dayOfTheWeek];
    }
}
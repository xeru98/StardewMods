using Netcode;

namespace BetterSpecialOrders;

public class BoardConfig
{
    public string OrderType = "";
    public bool  AllowReroll = false;
    public bool InfiniteRerolls = false;
    public int MaxRerolls = 0;
    public bool[] RefreshSchedule = new bool[7];

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
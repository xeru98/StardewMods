using Netcode;

namespace BetterSpecialOrders;

public class BoardConfig
{
    public string OrderType = "";
    public bool  AllowReroll = false;
    public bool InfiniteRerolls = false;
    public int MaxRerolls = 1;
    public bool[] RefreshSchedule = new bool[7]{true, false, false, false, false, false, false};

    public BoardConfig()
    {

    }

    public BoardConfig(string orderType, bool[]? refreshSchedule = null)
    {
        OrderType = orderType;
        if (refreshSchedule != null)
        {
            RefreshSchedule = refreshSchedule;
        }
    }

    public BoardConfig(string ctx, bool allowReroll, bool infiniteRerolls, int maxRerolls, bool[] refreshSchedule)
    {
        OrderType = ctx;
        AllowReroll = allowReroll;
        InfiniteRerolls = infiniteRerolls;
        MaxRerolls = maxRerolls;
        RefreshSchedule = refreshSchedule;
    }

    public bool ShouldRefreshToday(int dayOfTheWeek)
    {
        return RefreshSchedule[dayOfTheWeek];
    }
}
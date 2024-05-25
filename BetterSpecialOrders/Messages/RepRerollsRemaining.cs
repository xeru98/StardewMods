namespace BetterSpecialOrders.Messages;

public class RepRerollsRemaining
{
    public Dictionary<string, int> RerollsRemaining;

    public RepRerollsRemaining()
    {
        RerollsRemaining = new Dictionary<string, int>();
    }

    public RepRerollsRemaining(IDictionary<string, int> rerollMap)
    {
        RerollsRemaining = new Dictionary<string, int>(rerollMap);
    }
}
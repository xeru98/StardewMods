namespace BetterSpecialOrders.Messages;

// this is sent whenever the reroll count needs to updated
public class UpdateRerollsRemaining
{
    // The context of the board that was rerolled
    public string orderType;
    
    // The number of rerolls remaining for the context's board
    public int rerollsRemaining;

    public UpdateRerollsRemaining()
    {
        orderType = "";
        rerollsRemaining = 0;
    }

    public UpdateRerollsRemaining(string orderType, int rerollsRemaining)
    {
        this.orderType = orderType;
        this.rerollsRemaining = rerollsRemaining;
    }
}
namespace BetterSpecialOrders.Messages;

public class RequestReroll
{
    public string orderType;

    public RequestReroll()
    {
        orderType = "";
    }

    public RequestReroll(string orderType)
    {
        this.orderType = orderType;
    }
}
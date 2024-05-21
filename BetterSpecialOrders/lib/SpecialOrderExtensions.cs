using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;

namespace BetterSpecialOrders;

public static class SpecialOrderExtensions
{
    /* Modified version of SpecialOrder.SetOrderDuration to
     * avoid orders collected on sunday only having 1 day
     */
    public static void SetHardOrderDuration(this SpecialOrder order, QuestDuration duration)
    {
        WorldDate date = WorldDate.Now();
        switch (duration)
        {
            // Month long are usually season dependent so we don't edit those
            case QuestDuration.Month:
                date = new WorldDate(Game1.year, Game1.season, 0);
                date.TotalDays++;
                date.TotalDays += 28;
                break;
            case QuestDuration.TwoWeeks:
                date.TotalDays += 14;
                break;
            case QuestDuration.Week:
                date.TotalDays += 7;
                break;
            case QuestDuration.ThreeDays:
                date.TotalDays += 3;
                break;
            case QuestDuration.TwoDays:
                date.TotalDays += 2;
                break;
            case QuestDuration.OneDay:
                date.TotalDays++;
                break;
        }

        order.dueDate.Set(date.TotalDays);
    }

    public static void SetHardOrderDuration(this SpecialOrder order)
    {
        order.SetHardOrderDuration(order.questDuration.Get());
    }

    /*
     * This is a modified version of the base FarmerTeam::AddSpecialOrder
     *  that used the custom duration for the mod instead of the default that
     *  comes when normally making the order.
     */
    public static void AddSpecialOrderWithModdedDuration(this FarmerTeam team, string id, int? generationSeed = null,
        bool forceRepeatable = false)
    {
        if (team.specialOrders.Any((SpecialOrder p) => p.questKey == id))
        {
            return;
        }
        SpecialOrder order = SpecialOrder.GetSpecialOrder(id, generationSeed);
        order.SetHardOrderDuration();
        if (order == null)
        {
            ModEntry.GMonitor.Log("Can't add special order with ID '" + id + "' because no such ID was found.", LogLevel.Warn);
            return;
        }
        if (team.completedSpecialOrders.Contains(order.questKey.Value) && !forceRepeatable)
        {
            SpecialOrderData data = order.GetData();
            if (data == null || !data.Repeatable)
            {
                return;
            }
        }
        team.specialOrders.Add(order);
    }
}
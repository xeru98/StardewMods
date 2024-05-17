namespace BetterSpecialOrders.Messages;

// Sent whenever we need to sync the game state.
public class SyncGameState
{
    // This is a dictionary of all the boards we are watching
    public IDictionary<string, BoardConfig> boardConfigs;
    
    // This is a dictionary of the number of rerolls remaining
    public IDictionary<string, int> rerollsRemaining;
}
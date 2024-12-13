namespace BetterSpecialOrders.Messages;

public class RepHostConfig
{
    public ModConfig HostConfig;

    public RepHostConfig()
    {
        HostConfig = new ModConfig();
    }

    public RepHostConfig(ModConfig config)
    {
        HostConfig = config;
    }
}
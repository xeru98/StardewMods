# Better Special Orders
### Version: 1.1.1

This mod presents options and a reroll button to allow player to reroll the special orders boards

## How To Use
When you open SV with this mod installed for the first time, the config will be generated. 
By default, this does not allow for any rerolling meaning that if you just load the game and don't edit the settings, 
nothing will be different than the vanilla game. Check the "Configuration Options" section to see the different options
available.

## Configuration Options
### General Options
- **Use True Random**: If checked this will use an unseeded random for rolls. If unchecked, will use the random seed for your current game.
- **Reset Keybind**: If bound this will let the host reset the rerolls amount for all boards on that given day
- **Force Unique**: If checked, rerolling will first check to see if there are missions you haven't completed and offer you those when possible.
### Board Specific Options
Each board has its own config with the following options
- **Allow Rerolls**: If checked then rerolling will be allowed
- **Infinite Rerolls**: If checked AND Allow Rerolls is checked then will allow for infinite rerolls
- **Max Rerolls**: If checked AND Allow Rerolls is checked AND Infinite Rerolls is not checked then will allow the entire team to use this many rerolls
- **Reroll Schedule**: Will automatically reroll the board at the start of the checked day
### Natively Supported Boards
There are some boards that this mod natively supports rerolling for. Each one has it's own settings and reroll amount pool. All other boards
share the settings in CUSTOM (including rerolls remaining). If you find a board that isn't supported that you would like supported please open
a bug report and I'll investigate adding it.
- Stardew Valley Board
- Mr. Qi's Board
- Desert Festival Board
- SVE Boards
- Ridgeside Village Boards

## FAQ
### **I'm playing solo and I can't see the reroll button**
Check your config to make sure you have Allow Rerolls checked for the board you have opened

### **I'm playing multiplayer and farmhands (non-host farmers) can see the button but can't reroll**
Have the host check their config to ensure that they have it configured correctly

### **When I click the reroll button nothing happens**
It's possible that you have run out of rerolls. To check have the host just edit and close the
GMCM menu and try again.

### **I have another question not answered here**
You have a couple options
- Open a dicussion/bug here
- Open an issue on github: https://github.com/xeru98/StardewMods/issues
- Ping me @Xeru98 on the Stardew Valley discord (please save this as a last resort if I don't respond to the other options within 48 hours)
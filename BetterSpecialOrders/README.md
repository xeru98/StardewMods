# Better Special Orders
### Version: 1.0.2

This mod presents options and a reroll button to allow player to reroll the special orders boards

## How To Use
When you open SV with this mod installed for the first time, the config will be generated. 
By default, this does not allow for any rerolling meaning that if you just load the game and don't edit the settings, 
nothing will be different than the vanilla game. Check the "Configuration Options" section to see the different options
available. In order to enable rerolling the host must:
- Check Allow Rerolls
- Check EITHER Infinite Rerolls or set the Max Rerolls to 1-10 (your team gets this many daily)

Additionally, in the current version, all clients must check Allow Rerolls in order to see the reroll button.
This will be patched soon. Each board's configuration is separate so enabling the rerolls on the SV board will
not enable them for Mr. Qi's quests.

If you have questions check the FAQ at the bottom

## Configuration Options
### General Options
- Host Reroll Reset Keybind: When the Host presses the set keybind, all boards will have their rerolls set to the max amount
- Use True Random: If unchecked then Special Orders will be chosen pseudorandomly. If checked then Special Orders will be chosen completely randomly
### Stardew Valley Board
- Allow Rerolls: If checked then rerolling will be allowed
- Infinite Rerolls: If checked AND Allow Rerolls is checked then will allow for infinite rerolls
- Max Rerolls: If checked AND Allow Rerolls is checked AND Infinite Rerolls is not checked then will allow the entire team to use this many rerolls
- Reroll Schedule: Will automatically reroll the board at the start of the checked day
### Mr. Qi's Board
- Allow Rerolls: If checked then rerolling will be allowed
- Infinite Rerolls: If checked AND Allow Rerolls is checked then will allow for infinite rerolls
- Max Rerolls: If checked AND Allow Rerolls is checked AND Infinite Rerolls is not checked then will allow the entire team to use this many rerolls
- Reroll Schedule: Will automatically reroll the board at the start of the checked day
### Desert Festival Board
- Allow Rerolls: If checked then rerolling will be allowed
- Infinite Rerolls: If checked AND Allow Rerolls is checked then will allow for infinite rerolls
- Max Rerolls: If checked AND Allow Rerolls is checked AND Infinite Rerolls is not checked then will allow the entire team to use this many rerolls
### Custom Boards (Coming Soon)

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
- Ping me @Xeru98 on the Stardew Valley discord (save this as a last resort if I don't respond to the other options within 48 hours)
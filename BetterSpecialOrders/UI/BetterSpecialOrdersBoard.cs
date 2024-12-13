using BetterSpecialOrders.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SpecialOrders;

namespace BetterSpecialOrders.Menus;

public class BetterSpecialOrdersBoard : IClickableMenu
{

    public ClickableComponent rerollButton;

    private SpecialOrdersBoard nativeBoard;
    
    public BetterSpecialOrdersBoard(SpecialOrdersBoard rootBoard)
    {
        // configure native board integration
        nativeBoard = rootBoard;
        nativeBoard.exitFunction = delegate
        {
            exitThisMenu();
        };

        // measure string and register reroll button as component
        Vector2 stringSize = Game1.dialogueFont.MeasureString(RerollManager.Get().InfiniteRerolls(GetOrderType()) ? I18n.Board_Reroll() : I18n.Board_Rerolls(amount: 10));
        Vector2 position = Utility.getTopLeftPositionForCenteringOnScreen((int)stringSize.X + 24, (int)stringSize.Y + 24);
        rerollButton = new ClickableComponent(new Rectangle((int)position.X, (int)nativeBoard.yPositionOnScreen + nativeBoard.height - 128, (int)stringSize.X + 24 , (int)stringSize.Y + 24), "")
            {
                myID = 2,
                leftNeighborID = -99998,
                rightNeighborID = -99998,
                upNeighborID = -99998,
                downNeighborID = -99998,
                visible = RerollManager.Get().CanReroll(GetOrderType())
            };
        
        // Run this just in case we created a new SpecialOrdersBoard and need to update defaults
        nativeBoard.leftOrder.SetHardOrderDuration();
        nativeBoard.rightOrder.SetHardOrderDuration();
        
        UpdateButtons();
    }

    public override void draw(SpriteBatch b)
    {
        nativeBoard.leftOrder = Game1.player.team.GetAvailableSpecialOrder(type: nativeBoard.GetOrderType());
        nativeBoard.rightOrder = Game1.player.team.GetAvailableSpecialOrder(1, GetOrderType());
        
        nativeBoard.draw(b);

        if (rerollButton.visible)
        {
            string text =  RerollManager.Get().InfiniteRerolls(GetOrderType()) ? I18n.Board_Reroll() : I18n.Board_Rerolls(amount: RerollManager.Get().GetRerollsRemaining(GetOrderType()));
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), rerollButton.bounds.X, rerollButton.bounds.Y, rerollButton.bounds.Width, rerollButton.bounds.Height, (double) rerollButton.scale > 1.0 ? GetHoverColor() : Color.White, 4f * rerollButton.scale);
            Utility.drawTextWithShadow(b, text, Game1.dialogueFont, new Vector2(rerollButton.bounds.X + 12, rerollButton.bounds.Y + 12), Game1.textColor);
        }
        
        drawMouse(b);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        List<string> currentSpecialOrderKeys = Game1.player.team.specialOrders.Select(order => order.questKey.Value).ToList();
        nativeBoard.receiveLeftClick(x, y, playSound);
        
        //check to see if we just selected an order and overwrite the time remaining
        foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
        {
            if (!currentSpecialOrderKeys.Contains(specialOrder.questKey.Value))
            {
                specialOrder.SetHardOrderDuration();
            }
        }
        
        // handle reroll button
        if (rerollButton.visible && rerollButton.containsPoint(x, y))
        {
            if (RerollManager.Get().CanReroll(nativeBoard.GetOrderType()))
            {
                Game1.playSound("Ship");
                RerollManager.Get().Reroll(nativeBoard.GetOrderType());
            }
            else
            {
                Game1.playSound("detector");
            }
        }   
    }

    public override void performHoverAction(int x, int y)
    {
        nativeBoard.performHoverAction(x, y);
        float prevScale = rerollButton.scale;
        rerollButton.scale = rerollButton.bounds.Contains(x, y) ? 1.5f : 1f;
        if ((double) rerollButton.scale > (double) prevScale)
            Game1.playSound("Cowboy_gunshot");
    }

    public void UpdateButtons()
    {
        nativeBoard.UpdateButtons();
        if (rerollButton != null)
        {
            rerollButton.visible = RerollManager.Get().CanReroll(GetOrderType());
        }
    }

    private Color GetHoverColor()
    {
        return RerollManager.Get().CanReroll(GetOrderType()) ? Color.LightGreen : Color.Pink;
    }

    private string GetOrderType()
    {
        return nativeBoard.GetOrderType();
    }
}
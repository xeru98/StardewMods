using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace BetterSpecialOrders.Menus;

public class RerollScheduleOption : IClickableMenu
{
    private ClickableComponent[] dayButtons = new ClickableComponent[7];
    
    private readonly Texture2D bg = CreateScheduleBGTexture();
    
    private Rectangle uncheckedSprite = new Rectangle(128, 768, 36, 36);
    private Rectangle checkedSprite = new Rectangle(192, 768, 36, 36);

    private ButtonState lastMouseState;
    
    private string[] dayStrings =
    {
        "Mon",
        "Tue",
        "Wed",
        "Thu",
        "Fri",
        "Sat",
        "Sun"
    };
    
    //Function Handlers
    private readonly Func<bool[]> GetValue;
    private readonly Action<bool[]> SetValue;

    private bool[] currentValues;
    
    public RerollScheduleOption(Func<bool[]> getValue, Action<bool[]> setValue)
    {
        GetValue = getValue;
        SetValue = setValue;

        currentValues = GetValue();
        
        width = 560;
        height = 150;
        for (int i = 0; i < 7; i += 1)
        {
            dayButtons[i] =
                new ClickableComponent(new Rectangle(getPositionToCenterCheckboxInCell(i), new Point(70, 70)), dayStrings[i]);
        }
    }
    
    public void Draw(SpriteBatch b, Vector2 pos)
    {
        xPositionOnScreen = (int)pos.X;
        yPositionOnScreen = (int)pos.Y;
        //DrawBorder(b, pos);
        b.Draw(bg, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), Color.White);
        for (int i = 0; i < 7; i += 1)
        {
            dayButtons[i].bounds.Location = getPositionToCenterCheckboxInCell(i);
            Utility.drawTextWithShadow(b, dayStrings[i] , Game1.dialogueFont, getPositionToCenterStringInCell(dayStrings[i], i), Color.Black);
            b.Draw(Game1.menuTexture, dayButtons[i].bounds, currentValues[i] ? checkedSprite : uncheckedSprite, Color.White);
        }

        ProcessMouseInput();
    }

    private void ProcessMouseInput()
    {
        ButtonState currentState = Mouse.GetState().LeftButton;
        if (currentState == ButtonState.Pressed && lastMouseState == ButtonState.Released)
        {
            for (int i = 0; i < 7; i += 1)
            {
                if (dayButtons[i].containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                {
                    Game1.playSound("drumkit6");
                    currentValues[i] = !currentValues[i];
                }
            }
        }

        lastMouseState = currentState;
    }

    public void SaveChanges()
    {
        SetValue(currentValues);
    }

    public void Reset()
    {
        currentValues = GetValue();
    }

    private static Texture2D CreateScheduleBGTexture()
    {
        Color c0 = Color.SandyBrown;
        Color c1 = Color.LightGray;
        Color[] data = new Color[7];
        for (int i = 0; i < 7; i += 1)
        {
            data[i] = i % 2 == 0 ? c0 : c1;
        }

        Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 7, 1);
        texture.SetData(data);
        return texture;
    }

    private Vector2 getPositionToCenterStringInCell(string val, int dayIndex)
    {
        Vector2 stringSize = Game1.dialogueFont.MeasureString(val);
        int slotWidth = width / 7;
        int slotCenterX = xPositionOnScreen + (slotWidth * dayIndex) + (slotWidth/2);
        int slotCenterY = yPositionOnScreen + (height / 4);
        return new Vector2(slotCenterX - (stringSize.X / 2), slotCenterY - (stringSize.Y / 2));
    }

    private Point getPositionToCenterCheckboxInCell(int dayIndex)
    {
        int slotWidth = width / 7;
        int slotCenterX = xPositionOnScreen + (slotWidth * dayIndex) + (slotWidth/2);
        int slotCenterY = yPositionOnScreen + (3 * height / 4);
        return new Point(slotCenterX - 35, slotCenterY - 35);
    }
}
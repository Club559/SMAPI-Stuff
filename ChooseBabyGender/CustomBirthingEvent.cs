using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChooseBabyGender
{
  public class CustomBirthingEvent : FarmEvent
  {
    private int behavior;
    private int timer;
    private string soundName;
    private string message;
    private bool playedSound;
    private bool showedMessage;
    private bool isMale;
    private bool getBabyName;
    private Vector2 targetLocation;
    private TextBox babyNameBox;
    private ClickableTextureComponent okButton;
    private ClickableTextureComponent maleButton;
    private ClickableTextureComponent femaleButton;

    public CustomBirthingEvent()
    {
      this.babyNameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
      {
        X = Game1.graphics.GraphicsDevice.Viewport.Width / 2 - Game1.tileSize * 3,
        Y = Game1.graphics.GraphicsDevice.Viewport.Height / 2 + Game1.tileSize,
        Width = Game1.tileSize * 2
      };
      this.okButton = new ClickableTextureComponent(new Rectangle(this.babyNameBox.X + this.babyNameBox.Width + Game1.tileSize / 2, this.babyNameBox.Y - Game1.tileSize / 8, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
      this.maleButton = new ClickableTextureComponent("Male", new Rectangle(this.okButton.bounds.X + this.okButton.bounds.Width + Game1.tileSize / 2, this.okButton.bounds.Y, Game1.tileSize, Game1.tileSize), (string)null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), Game1.pixelZoom, false);
      this.femaleButton = new ClickableTextureComponent("Female", new Rectangle(this.maleButton.bounds.X + this.maleButton.bounds.Width + Game1.tileSize / 4, this.maleButton.bounds.Y, Game1.tileSize, Game1.tileSize), (string)null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), Game1.pixelZoom, false);
    }

    public bool setUp()
    {
      Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
      Utility.getHomeOfFarmer(Game1.player);
      NPC characterFromName = Game1.getCharacterFromName(Game1.player.spouse, false);
      Game1.player.CanMove = false;
      this.isMale = Game1.player.getNumberOfChildren() != 0 ? Game1.player.getChildren()[0].gender == 1 : random.NextDouble() < 0.5;
      if (characterFromName.isGaySpouse())
        this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", (object)Lexicon.getGenderedChildTerm(this.isMale));
      else if (characterFromName.gender == 0)
        this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", (object)Lexicon.getGenderedChildTerm(this.isMale));
      else
        this.message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", (object)Lexicon.getGenderedChildTerm(this.isMale), (object)characterFromName.name);
      return false;
    }

    public void afterMessage()
    {
      this.getBabyName = true;
      this.babyNameBox.SelectMe();
    }

    public bool tickUpdate(GameTime time)
    {
      Game1.player.CanMove = false;
      this.timer += time.ElapsedGameTime.Milliseconds;
      Game1.fadeToBlackAlpha = 1f;
      if (this.timer > 1500 && !this.playedSound && !this.getBabyName)
      {
        if (this.soundName != null && !this.soundName.Equals(""))
        {
          Game1.playSound(this.soundName);
          this.playedSound = true;
        }
        if (!this.playedSound && this.message != null && (!Game1.dialogueUp && Game1.activeClickableMenu == null))
        {
          Game1.drawObjectDialogue(this.message);
          Game1.afterDialogues = new Game1.afterFadeFunction(this.afterMessage);
          Game1.globalFadeToClear((Game1.afterFadeFunction)null, 0.02f);
        }
      }
      else if (this.getBabyName)
      {
        int mouseX = Game1.getOldMouseX();
        int mouseY = Game1.getOldMouseY();
        if (Game1.oldMouseState.LeftButton == ButtonState.Pressed)
        {
          if (maleButton.containsPoint(mouseX, mouseY))
          {
            this.isMale = true;
            maleButton.scale -= 0.5f;
            maleButton.scale = Math.Max(3.5f, maleButton.scale);
          }
          else if (femaleButton.containsPoint(mouseX, mouseY))
          {
            this.isMale = false;
            femaleButton.scale -= 0.5f;
            femaleButton.scale = Math.Max(3.5f, femaleButton.scale);
          }
          else if (okButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && this.babyNameBox.Text.Length > 0)
          {
            double num = (Game1.player.spouse.Equals("Maru") ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
            bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < num;
            string text = this.babyNameBox.Text;
            foreach (Character allCharacter in Utility.getAllCharacters())
            {
              if (allCharacter.name.Equals(text))
              {
                text += " ";
                break;
              }
            }
            Utility.getHomeOfFarmer(Game1.player).characters.Add((NPC)new Child(text, this.isMale, isDarkSkinned, Game1.player));
            Game1.playSound("smallSelect");
            Game1.player.getSpouse().daysAfterLastBirth = 5;
            Game1.player.getSpouse().daysUntilBirthing = -1;
            if (Game1.player.getChildren().Count == 2)
            {
              Game1.player.getSpouse().setNewDialogue(Game1.random.NextDouble() < 0.5 ? Game1.content.LoadString("Data\\ExtraDialogue:NewChild_SecondChild1") : Game1.content.LoadString("Data\\ExtraDialogue:NewChild_SecondChild2"), false, false);
              Game1.getSteamAchievement("Achievement_FullHouse");
            }
            else if (Game1.player.getSpouse().isGaySpouse())
              Game1.player.getSpouse().setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:NewChild_Adoption", (object)this.babyNameBox.Text), 0 != 0, 0 != 0);
            else
              Game1.player.getSpouse().setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:NewChild_FirstChild", (object)this.babyNameBox.Text), 0 != 0, 0 != 0);
            if (Game1.keyboardDispatcher != null)
              Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)null;
            Game1.player.position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * (float)Game1.tileSize;
            return true;
          }
        }
        else
        {
          if (maleButton.containsPoint(mouseX, mouseY))
            maleButton.scale = Math.Min(maleButton.scale + 0.02f, maleButton.baseScale + 0.1f);
          else
            maleButton.scale = Math.Max(maleButton.scale - 0.02f, maleButton.baseScale);

          if (femaleButton.containsPoint(mouseX, mouseY))
            femaleButton.scale = Math.Min(femaleButton.scale + 0.02f, femaleButton.baseScale + 0.1f);
          else
            femaleButton.scale = Math.Max(femaleButton.scale - 0.02f, femaleButton.baseScale);
        }
      }
      return false;
    }

    public void draw(SpriteBatch b)
    {
    }

    public void makeChangesToLocation()
    {
    }

    public void drawAboveEverything(SpriteBatch b)
    {
      if (!this.getBabyName)
        return;
      Game1.drawWithBorder(Game1.content.LoadString(this.isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), Color.Black, Color.White, new Vector2((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - Game1.tileSize * 4), (float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2 - Game1.tileSize * 2)));
      Game1.drawDialogueBox(this.babyNameBox.X - Game1.tileSize / 2, this.babyNameBox.Y - Game1.tileSize * 3 / 2, this.babyNameBox.Width + Game1.tileSize, this.babyNameBox.Height + Game1.tileSize * 2, false, true, (string)null, false);
      this.babyNameBox.Draw(b);
      this.okButton.draw(b);
      this.maleButton.draw(b);
      this.femaleButton.draw(b);

      if (this.isMale)
        b.Draw(Game1.mouseCursors, maleButton.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);
      else
        b.Draw(Game1.mouseCursors, femaleButton.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34, -1, -1)), Color.White);

      b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getOldMouseX(), (float)Game1.getOldMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
    }
  }
}

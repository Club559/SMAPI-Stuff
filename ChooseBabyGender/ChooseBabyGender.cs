using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChooseBabyGender
{
  public class ChooseBabyGender : Mod
  {
    public override void Entry(params object[] objects)
    {
      GameEvents.UpdateTick += GameEvents_UpdateTick;
    }

    private void GameEvents_UpdateTick(object sender, EventArgs e)
    {
      if (Game1.farmEvent != null && Game1.farmEvent is BirthingEvent)
      {
        Game1.farmEvent = new CustomBirthingEvent();
        Game1.farmEvent.setUp();
      }
    }
  }
}

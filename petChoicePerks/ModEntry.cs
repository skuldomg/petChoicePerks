using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace petChoicePerks
{
    public class ModEntry : Mod
    {
        Buff fishBuff;
        Buff forageBuff;
        int buffDuration;
        bool buffApplied;

        /*********
        ** Public methods
        *********/
        public override void Entry(IModHelper helper)
        {
            //Game1.player.addBuffAttributes(int[] buffAttributes);                       

            Helper.Events.GameLoop.SaveLoaded += this.SaveLoaded;
            Helper.Events.GameLoop.DayStarted += this.TimeEvents_AfterDayStarted;            
            Helper.Events.GameLoop.UpdateTicked += this.UpdateTicked;
        }

        /*********
        ** Private methods
        *********/
        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Initialize buffs
            fishBuff = new Buff(0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30, "", "");
            fishBuff.description = "Petting your cat has given you a temporary fishing bonus.";
            fishBuff.sheetIndex = 1;

            forageBuff = new Buff(0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 30, "", "");
            forageBuff.description = "Petting your dog has given you a temporary foraging bonus.";
            forageBuff.sheetIndex = 5;
        }

        private void TimeEvents_AfterDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            double luck = Game1.dailyLuck;
            this.Monitor.Log("Player's luck: " + luck);

            // Reset daily buff
            buffApplied = false;

            // Base duration of the buff is six hours
            buffDuration = 6*60;

            // TODO: Add/subtract a couple of hours based on daily luck
            double bonus = 0;

            // No bonus on a neutral day
            if (luck <= 0.02 && luck >= -0.02)
                bonus = 0;
            // No buff on a very bad luck day
            else if (luck < -0.07)
                buffApplied = true;
            else
            {
                // TODO: Think about balanced duration
            }            

            // Convert to game time minutes
            buffDuration = buffDuration / 10 * 7000;
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Check every second
            if (Context.IsWorldReady && !buffApplied && e.IsMultipleOf(60))
            {
                // Find the player's pet
                if (Game1.player.hasPet())
                {
                    StardewValley.Characters.Pet thePet = (StardewValley.Characters.Pet)Game1.getCharacterFromName(Game1.player.getPetName());

                    // Was it pet today (ie. just now)? If so, apply the buff
                    if (this.Helper.Reflection.GetField<bool>(thePet, "wasPetToday").GetValue())
                    {
                        if (Game1.player.catPerson)
                        {
                            fishBuff.millisecondsDuration = buffDuration;
                            Game1.buffsDisplay.addOtherBuff(fishBuff);
                        }
                        else
                        {
                            forageBuff.millisecondsDuration = buffDuration;
                            Game1.buffsDisplay.addOtherBuff(forageBuff);
                        }

                        buffApplied = true;
                    }
                }
            }
        }
    }
}

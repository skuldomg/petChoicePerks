using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using Harmony;
using System.Reflection;

namespace petChoicePerks
{
    public class ModEntry : Mod
    {
        Buff fishBuff;
        Buff forageBuff;
        int buffDuration;
        bool buffApplied;

        public static IMonitor ModMonitor;
        public static int PreviousDate = 0;

        /*********
        ** Public methods
        *********/
        public override void Entry(IModHelper helper)
        {
            ModMonitor = this.Monitor;

            //Game1.player.addBuffAttributes(int[] buffAttributes);                       

            Helper.Events.GameLoop.SaveLoaded += this.SaveLoaded;
            Helper.Events.GameLoop.DayStarted += this.TimeEvents_AfterDayStarted;
            Helper.Events.Input.ButtonReleased += this.ButtonReleased;

            // Create a new Harmony instance for patching source code
            HarmonyInstance harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            // Get the method we want to patch
            MethodInfo targetMethod = AccessTools.Method(typeof(Farm), nameof(Farm.addCrows));

            // Get the patch that was created
            MethodInfo prefix = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.Prefix));

            // Apply the patch
            harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefix));
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

        // Check if the player just pet his ... pet
        private void ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if(Context.IsWorldReady && e.Button.IsActionButton() && !buffApplied)
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
        
        private static bool Prefix(ref Farm __instance)
        {
            // Check if player has a cat when day changes
            bool hasCat = false;

            if(ModEntry.PreviousDate != Game1.dayOfMonth)
            {
                ModEntry.PreviousDate = Game1.dayOfMonth;                
                hasCat = Game1.player.catPerson && Game1.player.hasPet();
            }            

            if (hasCat)
                ModEntry.ModMonitor.Log("Player has a cat. No crows will spawn.");
            else
                ModEntry.ModMonitor.Log("Player doesn't have a cat. Crows will spawn.");

            return !hasCat;
        }
    }
}

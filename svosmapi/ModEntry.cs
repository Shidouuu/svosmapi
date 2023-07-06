using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net.Http.Headers;

namespace svosmapi;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod {
    /// <inheritdoc cref="IModHelper"/>
    public override void Entry(IModHelper helper) {
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.Player.Warped += this.OnPlayerWarped;
        helper.Events.World.NpcListChanged += this.OnNpcListChanged;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private readonly string[] CharArray = {"Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliott", "Emily", "Evelyn", "George", "Gunther", "Gus", "Haley", "Harvey", "Jas", "Jodi", "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam", "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent", "Willy", "Wizard"};

    private IDictionary<string, Dictionary<string, Dictionary<string, string>>> JDict = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(); // First (top) level dict
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
    }
    /// <inheritdoc cref="IContentEvents.AssetRequested"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnAssetRequested(object sender, AssetRequestedEventArgs e) {
        void ImageLoad(string name) {
            Random rnd = new();
            string season = Game1.currentSeason;
            string inside_outside;
            string sun_rain;

            if (Game1.currentLocation.IsOutdoors) { inside_outside = "Outdoor"; }
            else { inside_outside = "Indoor"; }

            if (Game1.isRaining) { sun_rain  = "Rain"; }
            else { sun_rain = "Sun"; }

            List<int> ran_int = new() {1, 2, 3, 4};
            bool done = false;
            while (!done) {
                int r = rnd.Next(0, ran_int.Count - 1);
                this.Monitor.Log("r equals " + r);
                try {
                    e.LoadFromModFile<Texture2D>(
                        Path.Combine(
                            "assets", "Portraits", name, 
                            $"{name}_{season}_{inside_outside}_{sun_rain}_{ran_int[r]}"
                        ), 
                        AssetLoadPriority.Medium
                    );
                    this.Monitor.Log($"Portrait {name}_{season}_{inside_outside}_{sun_rain}_{ran_int[r]} loaded", LogLevel.Trace);
                    done = true;
                    this.Monitor.Log("done loading", LogLevel.Trace);
                }
                catch (ContentLoadException) { 
                    this.Monitor.Log($"Portrait {name}_{season}_{inside_outside}_{sun_rain}_{ran_int[r]} not found, removing from list...", LogLevel.Warn);
                    ran_int.Remove(r); 
                }
            }
        }

        foreach (string i in CharArray) {
            if (e.Name.IsEquivalentTo($"Portraits/{i}")) 
                ImageLoad(i);
        }

        /*if (e.Name.IsEquivalentTo("Portraits/Abigail")) {
            ImageLoad("Abigail");
        }*/
    }
    /// <inheritdoc cref="IPlayerEvents.Warped"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnPlayerWarped(object sender, WarpedEventArgs e) {
        for (int i = 0; i < e.NewLocation.characters.Count; i++) {
            if (Array.Exists(CharArray, element => element == e.NewLocation.characters[i].Name)) 
                Helper.GameContent.InvalidateCache($"Portraits/{e.NewLocation.characters[i].Name}");
        }
    }
    /// <inheritdoc cref="IWorldEvents.NpcListChanged"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnNpcListChanged(object sender, NpcListChangedEventArgs e) {}
}    

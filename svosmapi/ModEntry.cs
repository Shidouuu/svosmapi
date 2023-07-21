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
        //helper.Events.Player.Warped += this.OnPlayerWarped;
        helper.Events.World.NpcListChanged += this.OnNpcListChanged;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private readonly string[] CharArray = {"Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliott", "Emily", "Evelyn", "George", "Gunther", "Gus", "Haley", "Harvey", "Jas", "Jodi", "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam", "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent", "Willy", "Wizard"};
    dynamic PortraitsJson;
    dynamic CharactersJson;
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
        using (StreamReader file = File.OpenText(Path.Combine(this.Helper.DirectoryPath, "assets", "data", "Portraits.json"))) {
            string stream_contents = file.ReadToEnd();
            PortraitsJson = JObject.Parse(stream_contents);
        }        
        using (StreamReader file = File.OpenText(Path.Combine(this.Helper.DirectoryPath, "assets", "data", "Characters.json"))) {
            string stream_contents = file.ReadToEnd();
            CharactersJson = JObject.Parse(stream_contents);
        }        
    }
    private readonly string[] preg_array = {"Abigail, Emily, Haley, Leah, Maru, Penny"};
    /// <inheritdoc cref="IContentEvents.AssetRequested"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnAssetRequested(object sender, AssetRequestedEventArgs e) {
        string season = Game1.currentSeason;
        void standard_load(string name) {
            string indoor_outdoor;
            string sun_rain;
            if (Game1.currentLocation.IsOutdoors) { indoor_outdoor = "Outdoor"; }
            else { indoor_outdoor = "Indoor"; }
            if (Game1.isRaining) { sun_rain  = "Rain"; }
            else { sun_rain = "Sun"; }

            List<string> pound_list = new(); // 'pound' for '#'
            bool pound_available = true;
            foreach (JProperty pproperty in PortraitsJson[name]) {
                try {
                    var stest = PortraitsJson[name][pproperty.Name]["Season"].Value;
                    var ltest = PortraitsJson[name][pproperty.Name]["Location"].Value;
                    var wtest = PortraitsJson[name][pproperty.Name]["Weather"].Value;
                    // Must use string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase) since "==" is case-sensitive
                    bool sbool = string.Equals(PortraitsJson[name][pproperty.Name]["Season"].Value, season, StringComparison.OrdinalIgnoreCase);
                    bool lbool = string.Equals(PortraitsJson[name][pproperty.Name]["Location"].Value, indoor_outdoor, StringComparison.OrdinalIgnoreCase);
                    bool wbool = string.Equals(PortraitsJson[name][pproperty.Name]["Weather"].Value, sun_rain, StringComparison.OrdinalIgnoreCase);
                    if (sbool && lbool && wbool) {
                        if (PortraitsJson[name][pproperty.Name]["#"] != null)
                            pound_list.Add(PortraitsJson[name][pproperty.Name]["#"].Value);
                        else
                            pound_available = false;
                    }
                }
                catch (InvalidOperationException) {
                    this.Monitor.Log($"Error attempting to read json from {name}/{pproperty.Name}!", LogLevel.Warn); // need better error message
                }
            }

            Random rnd = new();
            int r = rnd.Next(0, pound_list.Count - 1);
            this.Monitor.Log("r equals " + r);
            e.LoadFromModFile<Texture2D>(Path.Combine("assets", "Portraits", name, $"{name}_{season}_{indoor_outdoor}_{sun_rain}_{pound_list[r]}"), AssetLoadPriority.Medium);
            this.Monitor.Log($"Portrait {name}_{season}_{indoor_outdoor}_{sun_rain}_{pound_list[r]} loaded", LogLevel.Trace);
            this.Monitor.Log("done loading", LogLevel.Trace);
            /*List<int> ran_int = new() {1, 2, 3, 4};
            bool done = false;
            while (!done) {
                int r = rnd.Next(0, ran_int.Count - 1);
                this.Monitor.Log("r equals " + r);
                try {
                    e.LoadFromModFile<Texture2D>(Path.Combine("assets", "Portraits", name, $"{name}_{season}_{indoor_outdoor}_{sun_rain}_{ran_int[r]}"), AssetLoadPriority.Medium);
                    this.Monitor.Log($"Portrait {name}_{season}_{indoor_outdoor}_{sun_rain}_{ran_int[r]} loaded", LogLevel.Trace);
                    done = true;
                    this.Monitor.Log("done loading", LogLevel.Trace);
                }
                catch (ContentLoadException) { 
                    this.Monitor.Log($"Portrait {name}_{season}_{indoor_outdoor}_{sun_rain}_{ran_int[r]} not found, removing from list...", LogLevel.Warn);
                    ran_int.Remove(r); 
                }
            }*/
        }
        void maternity_load(string name ) {
            e.LoadFromModFile<Texture2D>(Path.Combine("assets", "Portraits", name, $"{name}_Maternity"), AssetLoadPriority.Medium);
        }
        // The purpose of this function is to determine which function to use to load the image (standard, festival, maternity, etc).
        void determine_load_method(string name) {
            var spouse = Game1.player.getSpouse();
            bool sbool;
            try { sbool = preg_array.Contains(spouse.Name); }
            catch (NullReferenceException) { sbool = false; }
            if (sbool) { // No idea if this works!
                var frenDict = Game1.player.friendshipData;
                if (frenDict[name].DaysUntilBirthing > 0) 
                    maternity_load(name);
            }
            else
                standard_load(name);
        }

        /*foreach (string i in CharArray) {
            if (e.Name.IsEquivalentTo($"Portraits/{i}")) 
                determine_load_method(i);
        }*/

        if (e.Name.IsEquivalentTo("Portraits/Abigail")) {
            determine_load_method("Abigail");
        }
    }
    /// <inheritdoc cref="IPlayerEvents.Warped"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    /*private void OnPlayerWarped(object sender, WarpedEventArgs e) {
        for (int i = 0; i < e.NewLocation.characters.Count; i++) {
            if (Array.Exists(CharArray, element => element == e.NewLocation.characters[i].Name)) 
                Helper.GameContent.InvalidateCache($"Portraits/{e.NewLocation.characters[i].Name}");
        }
    }*/
    /// <inheritdoc cref="IWorldEvents.NpcListChanged"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnNpcListChanged(object sender, NpcListChangedEventArgs e) {}
}    

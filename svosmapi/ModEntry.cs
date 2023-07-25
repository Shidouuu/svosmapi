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
using GenericModConfigMenu;
using System.Diagnostics;
using System.Reflection;
using static StardewValley.Minigames.MineCart;

namespace svosmapi;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    /// <summary>Get the string value of a property.</summary>
    private string GetStrVal(string name, ModConfig con)
    {
        return con.GetType().GetProperty(name).GetValue(this.Config).ToString();
    }

    /// <summary>The mod configuration from the player.</summary>
    private ModConfig Config;

    /// <inheritdoc cref="IModHelper"/>
    public override void Entry(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.Player.Warped += this.OnPlayerWarped;
        helper.Events.World.NpcListChanged += this.OnNpcListChanged;
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Config = this.Helper.ReadConfig<ModConfig>();
    }

    dynamic PortraitsJson;
    dynamic CharactersJson;
    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        // Import from data
        using (StreamReader file = File.OpenText(Path.Combine(this.Helper.DirectoryPath, "data", "Portraits.json")))
        {
            string stream_contents = file.ReadToEnd();
            PortraitsJson = JObject.Parse(stream_contents);
        }
        using (StreamReader file = File.OpenText(Path.Combine(this.Helper.DirectoryPath, "data", "Characters.json")))
        {
            string stream_contents = file.ReadToEnd();
            CharactersJson = JObject.Parse(stream_contents);
        }

        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        configMenu.Register(
            mod: this.ModManifest,
            reset: () => this.Config = new ModConfig(),
            save: () => this.Helper.WriteConfig(this.Config)
        );

        PropertyInfo[] properties = typeof(ModConfig).GetProperties();
        string[] exclusive_portraits = { "Jas_Spring_Indoor_Sun", "Jas_Spring_Outdoor_Sun" };
        foreach (PropertyInfo property in properties)
        {
            if (exclusive_portraits.Contains(property.Name))
            {
                configMenu.AddTextOption(
                    mod: this.ModManifest,
                    name: () => property.Name,
                    getValue: () => property.GetValue(this.Config).ToString(),
                    setValue: value => property.SetValue(this.Config, value),
                    allowedValues: new string[] { "portrait", "none" }
                );
            }
            else
            {
                configMenu.AddTextOption(
                    mod: this.ModManifest,
                    name: () => property.Name,
                    getValue: () => property.GetValue(this.Config).ToString(),
                    setValue: value => property.SetValue(this.Config, value),
                    allowedValues: new string[] { "both", "portrait", "sprite", "none" }
                );
            }
        }
    }
    string season;
    string weather;
    /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        season = Game1.currentSeason;
        if (Game1.isRaining) { weather = "Rain"; }
        else if (Game1.isSnowing) { weather = "Snow"; }
        else { weather = "Sun"; }
    }

    private readonly string[] preg_array = { "Abigail, Emily, Haley, Leah, Maru, Penny" };
    IDictionary<string, string> Imgs2Load = new Dictionary<string, string> { };
    List<string> ImagesLoaded = new();
    /// <inheritdoc cref="IContentEvents.AssetRequested"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        Stopwatch sw1 = new Stopwatch();
        if (Imgs2Load.Keys.Contains(e.Name.Name) && !ImagesLoaded.Contains(e.Name.Name)) {
            e.LoadFromModFile<Texture2D>(Imgs2Load[e.Name.Name], AssetLoadPriority.Medium);
            sw1.Start();
            this.Monitor.Log($"Loaded {Imgs2Load[e.Name.Name]} in {sw1.ElapsedMilliseconds}."); sw1.Stop();
            ImagesLoaded.Add(e.Name.Name);
        }
        /*foreach (string i in CharArray) {
            if (e.Name.IsEquivalentTo($"Portraits/{i}")) 
                determine_load_method(i);
        }*/

        /*if (e.Name.IsEquivalentTo("Portraits/Abigail")) {
            determine_load_method("Abigail", "Portraits");
        }
        else if (e.Name.IsEquivalentTo("Characters/Abigail")) {
            determine_load_method("Abigail", "Characters");
        }*/
    }
    private readonly string[] CharArray = { "Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliott", "Emily", "Evelyn", "George", "Gunther", "Gus", "Haley", "Harvey", "Jas", "Jodi", "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam", "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent", "Willy", "Wizard" };
    /// <inheritdoc cref="IPlayerEvents.Warped"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnPlayerWarped(object sender, WarpedEventArgs e)
    {
        Imgs2Load.Clear();
        ImagesLoaded.Clear();

        string indoor_outdoor;
        if (Game1.currentLocation.IsOutdoors) { indoor_outdoor = "Outdoor"; }
        else { indoor_outdoor = "Indoor"; }
        void maternity_load(string name, string option)
        {
            if (string.Equals(option, "both", StringComparison.OrdinalIgnoreCase))
            {
                Imgs2Load.Add($"Portraits/{name}", Path.Combine("assets", "Portraits", name, $"{name}_Maternity"));
                Imgs2Load.Add($"Characters/{name}", Path.Combine("assets", "Characters", name, $"{name}_Maternity"));
            }
            else if (string.Equals(option, "portraits", StringComparison.OrdinalIgnoreCase))
                Imgs2Load.Add($"Portraits/{name}", Path.Combine("assets", "Portraits", name, $"{name}_Maternity"));
            else if (string.Equals(option, "characters", StringComparison.OrdinalIgnoreCase))
                Imgs2Load.Add($"Characters/{name}", Path.Combine("assets", "Characters", name, $"{name}_Maternity"));
        }
        void standard_load(string name)
        {
            // For the top-level standard formats, Portraits is a superset of Characters so we can just use that to loop over everything.
            dynamic standard_portraits = PortraitsJson[name]["Standard"]; // TODO: Find type
            List<string> pound_list = new(); // 'pound' for '#'
            bool pound_available = false;
            IDictionary<string, string> valid_images = new Dictionary<string, string>(); // Ex. "Abigail_Fall_Indoor_Rain_1": "both"
            foreach (JProperty pproperty in standard_portraits)
            {
                try
                {
                    // The difference between pproperty and standard_portraits[pporperty.Name]
                    // pproperty is... I don't know we'll find out at runtime
                    // standard_portraits[pproperty.Name] refers to the actual object
                    //bool aaaa = pproperty == standard_portraits[pproperty.Name];
                    string sval = standard_portraits[pproperty.Name]["Season"].Value;
                    string? lval = standard_portraits[pproperty.Name]["Location"].Value;
                    string? wval = standard_portraits[pproperty.Name]["Weather"].Value;
                    bool sbool = string.Equals(sval, season, StringComparison.OrdinalIgnoreCase);
                    bool lbool = string.Equals(lval, indoor_outdoor, StringComparison.OrdinalIgnoreCase);
                    bool wbool = string.Equals(wval, weather, StringComparison.OrdinalIgnoreCase);
                    if (sbool && (lbool || lval == null) && (wbool || wval == null))
                    {
                        string conval = GetStrVal(pproperty.Name, this.Config);
                        /*// TODO: Create function for this
                        // Since a sprite does not exist for this variation (uses default), this will set the config to "portrait" only.
                        if (string.Equals(name, "jas", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(sval, "spring", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(lval, "indoor", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(wval, "sun", StringComparison.OrdinalIgnoreCase))
                            this.Config.GetType().GetProperty(name).SetValue(this.Config, "portrait");*/
                        if (!string.Equals(conval, "none", StringComparison.OrdinalIgnoreCase))
                            valid_images.Add(pproperty.Name, conval);
                        bool pisnull = standard_portraits[pproperty.Name]["#"] == null;
                        if (!pisnull)
                        {
                            pound_list.Add(standard_portraits[pproperty.Name]["#"].Value);
                            pound_available = true;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    this.Monitor.Log($"Error attempting to read json from Portraits/{name}/{pproperty.Name}!", LogLevel.Warn); // need better error message
                }
            }
            string[] vikeys = valid_images.Keys.ToArray();
            int r;
            if (pound_available)
            {
                Random rnd = new();
                r = rnd.Next(0, pound_list.Count - 1);
            }
            else
                r = 0;
            if (valid_images[vikeys[r]] == "both")
            {
                Imgs2Load.Add($"Portraits/{name}", Path.Combine("assets", "Portraits", name, vikeys[r]));
                Imgs2Load.Add($"Characters/{name}", Path.Combine("assets", "Characters", name, vikeys[r]));
            }
            else if (valid_images[vikeys[r]] == "portrait")
                Imgs2Load.Add($"Portraits/{name}", Path.Combine("assets", "Portraits", name, vikeys[r]));
            else if (valid_images[vikeys[r]] == "sprite")
                Imgs2Load.Add($"Characters/{name}", Path.Combine("assets", "Characters", name, vikeys[r]));
        }
        void determine_load_method(string name)
        {
            NPC spouse = Game1.player.getSpouse();
            bool sbool;
            try { sbool = preg_array.Contains(spouse.Name); }
            catch (NullReferenceException) { sbool = false; }
            if (sbool)
            { // No idea if this works!
                var frenDict = Game1.player.friendshipData;
                if (frenDict[name].DaysUntilBirthing > 0) // Must use this until an IsPregnant bool exists
                {
                    string option = GetStrVal(name, this.Config);
                    if (option != "none" && option != null)
                        maternity_load(name, option);
                }
            }
            else
                standard_load(name);
        }
        foreach (NPC i in e.NewLocation.characters)
        {
            if (Array.Exists(CharArray, element => element == i.Name))
            {
                Helper.GameContent.InvalidateCache($"Portraits/{i.Name}");
                Helper.GameContent.InvalidateCache($"Characters/{i.Name}");
                determine_load_method(i.Name);
            }
        }
       
    }
    /// <inheritdoc cref="IWorldEvents.NpcListChanged"/>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnNpcListChanged(object sender, NpcListChangedEventArgs e) { }
}

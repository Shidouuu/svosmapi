private readonly string[] CharArray = {"Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliott", "Emily", "Evelyn", "George", "Gunther", "Gus", "Haley", "Harvey", "Jas", "Jodi", "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam", "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent", "Willy", "Wizard"};

private IDictionary<string, Dictionary<string, Dictionary<string, string>>> JDict = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(); // First (top) level dict
/// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
/// <param name="sender">The event sender.</param>
/// <param name="e">The event data.</param>
private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
    // Uhh so basically what we're doing here is scanning every single folder and subfolder (well almost) and adding the files (portraits) to a dict that includes the properties of them.
    // This way, I'm able to set conditions for them much easier, and assets may be added or removed dynamically.

    string PortraitsPath = Path.Combine(this.Helper.DirectoryPath, "assets", "Portraits"); // Get path of Portraits directory (assets/Portraits)
    string[] PorSubDir = Directory.GetDirectories(PortraitsPath); // Create array containing subdirectories names of Portraits directories ( ["Abigail", "Alex", etc] )
    
    Dictionary<string, Dictionary<string, string>> SubDict = new(); // Second level dict
    foreach (string SubName in PorSubDir) { // SubName ex. "Abigail"
        string NamePath = Path.Combine(this.Helper.DirectoryPath, "assets", "Portraits", SubName); // Get path of individual portrait folder ex.(assets/Portraits/Abigail)
        string[] AssetArray = Directory.GetFiles(NamePath); // Get array of file names in portrait folder ( ["Abigail_Fall_Indoor_Rain_1.png", "Abigail_Fall_Indoor_Rain_2.png", etc] )

        // For some reason C# doesn't want me to leave these variables uninitialized, so that's why the default values are "null"...
        string season = "null";
        string[] SeasonArray = {"Spring", "Summer", "Winter", "Fall"};
        string location = "null";
        string [] LocationArray = {"Indoor", "Outdoor"};
        string weather = "null";
        string num = "null";
        Dictionary<string, string> AssetDict = new();
        foreach (string Asset in AssetArray) { // Asset ex. "Abigail_Fall_Indoor_Rain_1.png"
            bool name_passed = false;
            bool spassed = false;
            bool lpassed = false;
            bool wpassed = false;
            bool num_passed = false;
            int j = 0;
            for (int i = 0; i < Asset.Length; i++) { // Looping through the individual char of Asset
                if (Asset[i] == '_') {
                    try {
                        // We already have SubName to use as the name so we don't need another variable.
                        if (!name_passed) {
                            j = i + 1;
                            name_passed = true;
                        }
                        else if (!spassed) {
                            // This uses the range operator '..', where the left side is the lower bound and the right side is the upper bound. Very nice for extracting a substring.
                            season = Asset[j..(i - 1)]; 
                            // If the file doesn't have a season it's not in the standard format so breaking the for loop makes it go to the next file.
                            if (!SeasonArray.Contains(season)) 
                                break;
                            j = i + 1;
                            spassed = true;
                        }
                        else if (!lpassed) {
                            location = Asset[j..(i - 1)];
                            if (!LocationArray.Contains(location))
                                location = "null"; // There's not always a location, so sets location to null if there's not.
                            j = i + 1;
                            lpassed = true; 
                        }
                        else if (!wpassed) {
                            weather = Asset[j..(i - 1)]; // I might want to put another array here like the others as a safety net but eh
                            j = i + 1;
                            wpassed = true;
                        }
                        else if (!num_passed) {
                            num = Asset[j..(i - 1)];
                            num_passed = true;
                        }
                    }
                    catch (IndexOutOfRangeException) {
                        if (name_passed && spassed && lpassed && wpassed) {
                            this.Monitor.Log($"Caught expected IndexOutOfRangeException for {Asset}.", LogLevel.Trace);
                            num = "null"; // Num may also not always exist. I could probably check if the loop reached the end of the string but this is a thing for now.
                        }
                        else
                            this.Monitor.Log($"Caught unexpected IndexOutOfRangeException for {Asset}: \nname_passed={name_passed} \nspassed={spassed} \nlpassed={lpassed} \nwpassed={wpassed} \nnum_passed={num_passed}", LogLevel.Warn);
                    }
                }
            }
            AssetDict.Add("Season", season);
            AssetDict.Add("Location", location);
            AssetDict.Add("Weather", weather);
            AssetDict.Add("num", num);
            SubDict.Add(Asset, AssetDict);
        }
        JDict.Add(SubName, SubDict);
    }
}

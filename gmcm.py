import json
import os
from pathlib import Path

with open("svosmapi/data/Config-out.json", "r") as fp:
    config_json = json.load(fp)
    with open("gmcm", "w") as gmcm:
        for i in config_json:
            gmcm.write(f'''configMenu.AddTextOption(
    mod: this.ModManifest,
    name: () => "{i}",
    getValue: () => this.Config.{i},
    setValue: value => this.Config.{i} = value,
    allowedValues: new string[] {{ "both", "portrait", "sprite", "none" }}
);
''')
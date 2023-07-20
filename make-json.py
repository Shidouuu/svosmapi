import json
import os
from pathlib import Path

'''
This script is for generating the Characters.json and Portraits.json in data. 
If you need to update the json's, run this script and (theoretically) it will overwrite the old json's.
'''

log = open("log.txt", "w")
CharList = ["Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliott", "Emily", "Evelyn", "George", "Gunther", "Gus", "Haley", "Harvey", "Jas", "Jodi", "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam", "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent", "Willy", "Wizard"]
char_dict = {}
port_dict = {}
def standard_format(AttributeList, FileName):
    LocationList = ["Indoor", "Outdoor"]
    WeatherList = ["Sun", "Rain", "Snow", "SnowBlue", "SnowDown"] # Latter two are for Leah
    Colors = ["Pink", "Blue", "Red", "Black", "Brown"] # Used for Evelyn, Haley, and Leah
    HaleyDir = ["Down", "Updo"]
    LeahDir = ["Down", "Up"] # For Leah
    n = 1
    try:
        standard_dict = {
            "Season": None,
            "Location": None,
            "Weather": None,
            "#": None,
            "Extra": None
        }
        if AttributeList[-1].isdigit(): # Originally under "# 4"
            standard_dict["#"] = AttributeList[-1]
        elif AttributeList[-1] == "Work":
            standard_dict["Extra"] == "Work"
        standard_dict["Season"] = AttributeList[n] # 1
        n += 1
        if AttributeList[n] in LocationList: # 2
            standard_dict["Location"] = AttributeList[n]
        elif AttributeList[n] in WeatherList:
            standard_dict["Weather"] = AttributeList[n]
        n += 1
        if AttributeList[n] in WeatherList: # 3
            standard_dict["Weather"] = AttributeList[n]
        n += 1
        if AttributeList[n] in Colors or AttributeList[n] in HaleyDir: # 4
            standard_dict["#"] = AttributeList[n]
        elif AttributeList[n] == "PostBus": # For Pam
            standard_dict["#"] = "PostBus"
        n += 1
        if AttributeList[n] in LeahDir: # 5
            standard_dict["Extra"] = AttributeList[n]
        elif AttributeList[n] in Colors:
            standard_dict["Extra"] = AttributeList[n]
    except IndexError:
        if n < 5:
            print(f"Tried accessing AttributeList[{n}] for {FileName}, caught index out of range!", file=log)
    return standard_dict
def swim(AttributeList, FileName):
    Variations = ["Pink", "White", "Blue", "Yellow", "Floaties", "NoFloaties", "Censored", "GrassSkirt"] # For Haley, Penny, George, and Linus
    swim_dict = {
        "Swim": True,
        "Variation": None
    }

    try:
        if AttributeList[1] in Variations:
            swim_dict["Variation"] = AttributeList[1]
    except IndexError:
        print(f"Tried accessing AttributeList[1] for {FileName}, caught index out of range!", file=log)
    return swim_dict
def festivals(AttributeList, FileName):
    Variations = [
        "AltBlue", "AltPink", "AltRed", "Standard", "Alternative", # Emily and Leah
        "Mustache", "Shaved", # Harvey FlowerDance, IceFestival, Luau
        "Military", # Kent FlowerDance
        "MadScientist" # Sam SpiritsEve
        "Scarecrow", # Sam SpiritsEve
        "Ghost", # Vincent SpiritsEve
        "Pumpkin" # Vincent SpiritsEve
        ] 
    Extras = ["Mustache", "Shaved", "Facepaint", "NoFacepaint"] # Harvey EggFestival, Sam SpiritsEve
    festival_dict = {
        "Festival": None,
        "Variation": None,
        "Extra": None,
    }

    try:
        festival_dict["Festival"] = AttributeList[1]
        if AttributeList[2] in Variations or AttributeList[2].isdigit():
            festival_dict["Variation"] = AttributeList[2]
        elif AttributeList[2] == "Star": # For Linus_Winter_Star
            festival_dict["Festival"] = "WinterStar"
        if AttributeList[3] in Extras:
            festival_dict["Extra"] = AttributeList[3]
    except IndexError:
        print(f"Tried accessing AttributeList[1] for {FileName}, caught index out of range!", file=log)
    return festival_dict
def krobus(AttributeList, FileName):
    Variations = ["Blue", "Leaf", "Purple", "Scarf", "Trenchcoat", "GreenScarf", "PinkScarf", "RedScarf", "Yellow"]
    krobus_dict = {
        "Season": None,
        "Weather": None,
        "Variation": None
    }

    try:
        if AttributeList[1] == "Winter":
            krobus_dict["Season"] = "Winter"
            krobus_dict["Weather"] = "Snow"
            krobus_dict["Variation"] = AttributeList[2]
        else:
            krobus_dict["Season"] = ["Spring", "Summer", "Fall"]
            krobus_dict["Weather"] = ["Rain", "Storm"]
            krobus_dict["Variation"] = AttributeList[1]
    except IndexError:
        print(f"Tried accessing AttributeList[1] for {FileName}, caught index out of range!", file=log)
    return krobus_dict
def sebastian_glasses_festivals(AttributeList, FileName):
    glasses_dict = {
        "Season": None,
        "Festival": None,
        "#": None
    }
    try:
        if AttributeList[1] == "Summer":
            glasses_dict["Season"] = "Summer"
        else:
            glasses_dict["Season"] = ["Spring", "Fall", "Winter"]
        if AttributeList[-1].isdigit():
            glasses_dict["#"] = AttributeList[-1]
            glasses_dict["Festival"] = AttributeList[-2]
        else:
            glasses_dict["Festival"] = AttributeList[-1]
    except IndexError:
        print(f"Tried accessing AttributeList[1] for {FileName}, caught index out of range!", file=log)
    return glasses_dict
def sebastian_piercing(AttributeList, FileName):
    piercing_dict = {
        "Festival": None,
        "Weather": None,
        "#": None
    }
    try:
        if AttributeList[-1].isdigit():
            piercing_dict["#"] = AttributeList[-1]
            piercing_dict["Festival"] = AttributeList[-2]
        else:
            piercing_dict["Weather"] = AttributeList[-1]
    except IndexError:
        print(f"Tried accessing AttributeList[1] for {FileName}, caught index out of range!", file=log)
    return piercing_dict

def check_attributes(AttList, AssetName):
    specials = [
        "Maternity", # Bachelorettes
        "Aerobics", # Caroline, Emily, Jodi
        "Clint_Apron", 
        "LabCoat", # Demetrius
        "Photokit", # Haley
        "Doctor", # Harvey
        "RFQ", # RandomFlowerQueen
        "MustacheOverlay", # Harvey
        "Wedding" # Characters/WeddingOutfits
    ]
    harvey_overlay = ["BrownEye_Overlay", "GreenEye_Overlay"]
    SeasonList = ["Spring", "Summer", "Winter", "Fall"]
    FestivalList = ["EggFestival", "FlowerDance", "IceFestival", "Luau", "MoonlightJellies", "SpiritsEve", "SVFair", "WinterStar"]
    WizardHatList = ["Hat", "Shoulder", "Spirit"]

    if "Swim" in AttList[0]:
        return swim(AttList, Path(AssetName).name)
    elif AttList[0] == "Krobus":
        return krobus(AttList, Path(AssetName).name)
    elif AttList[0] in WizardHatList:
        return AttList[2]
    elif AttList[1] == "Glasses" or AttList[1] == "Summer" and AttList[-2] == "SpiritsEve":
        return sebastian_glasses_festivals(AttList, Path(AssetName).name)
    elif AttList[1] in SeasonList:
        return standard_format(AttList, Path(AssetName).name)
    elif AttList[1] in FestivalList:
        return festivals(AttList, Path(AssetName).name)
    elif AttList[1] in specials:
        return AttList[1]
    elif AttList[1] == "Piercings":
        return sebastian_piercing(AttList, Path(AssetName).name)
    elif AttList[1] == "Bandanna":
        return AttList[-1]
    elif AttList[1] in harvey_overlay:
        return AttList[1] + "_Overlay"
    elif AttList[1] == "Hospital": # For Maru_Hospital
        return AttList[2] # The color
    elif AttList[1] == "Motorbike": # For Sebastian
        return "Motorbike_Helmet"
def scan_standard(FolderType):
    with os.scandir(f"svosmapi/assets/{FolderType}") as SubDir:
        for SubName in SubDir:
            try:
                second_dict = {}
                with os.scandir(f"svosmapi/assets/{FolderType}/{Path(SubName).name}") as AssetDir:
                    for AssetName in AssetDir:
                        third = None
                        if os.path.isdir(AssetName):
                            new_path = f"{FolderType}/{Path(SubName).name}/{Path(AssetName).name}"
                            third = {}
                            with os.scandir(f"svosmapi/assets/{new_path}") as AssetSubDir:
                                for SubAsset in AssetSubDir:
                                    fourth = None
                                    if os.path.isdir(SubAsset):
                                        new_path = f"{FolderType}/{Path(SubName).name}/{Path(AssetName).name}/{Path(SubAsset).name}"
                                        fourth = {}
                                        with os.scandir(f"svosmapi/assets/{new_path}") as AssetSub3Dir:
                                            for Sub3Asset in AssetSub3Dir:
                                                fifth = None
                                                AttList = (Path(Sub3Asset).stem).split("_")
                                                print(f"N3: {str(AttList)}", file=log)
                                                fifth = check_attributes(AttList, Sub3Asset)
                                                fourth[Path(Sub3Asset).stem] = fifth
                                    else:
                                        AttList = (Path(SubAsset).stem).split("_")
                                        print(f"N2: {str(AttList)}", file=log)
                                        fourth = check_attributes(AttList, SubAsset)
                                    third[Path(SubAsset).stem] = fourth

                        else:
                            AttList = (Path(AssetName).stem).split("_")
                            print(f"N1: {str(AttList)}", file=log)
                            third = check_attributes(AttList, AssetName)
                        second_dict[Path(AssetName).stem] = third
                    if FolderType == "Portraits":
                        port_dict[Path(SubName).stem] = second_dict
                    else:
                        char_dict[Path(SubName).stem] = second_dict
            except NotADirectoryError:
                continue

scan_standard("Portraits")
scan_standard("Characters")
with open('svosmapi/data/Portraits.json', 'w') as fp:
    fp.write(json.dumps(port_dict, indent=4))
with open('svosmapi/data/Characters.json', 'w') as fp:
    fp.write(json.dumps(char_dict, indent=4))
log.close()








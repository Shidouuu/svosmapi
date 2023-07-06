import json
import os
from pathlib import Path

CharList = ["Abigail", "Alex", "Caroline", "Clint", "Demetrius", "Elliott", "Emily", "Evelyn", "George", "Gunther", "Gus", "Haley", "Harvey", "Jas", "Jodi", "Kent", "Krobus", "Leah", "Lewis", "Linus", "Marnie", "Maru", "Pam", "Penny", "Pierre", "Robin", "Sam", "Sandy", "Sebastian", "Shane", "Vincent", "Willy", "Wizard"]
first_dict = {}

def standard_format(AttributeList, FileName, SubFolder=None):
    def create_standard_dict():
        LocationList = ["Indoor", "Outdoor"]
        WeatherList = ["Sun", "Rain", "Snow", "SnowBlue", "SnowDown"] # Latter two are for Leah
        Colors = ["Pink", "Blue", "Red", "Black", "Brown"] # Used for Evelyn, Haley, and Leah
        Extras = ["Down", "Up"] # For Leah
        n = 1
        try:
            standard_dict = {
                "Season": None,
                "Location": None,
                "Weather": None,
                "#": None,
                "Extra": None
            }
            standard_dict["Season"] = AttributeList[n]
            n += 1
            if AttributeList[n] in LocationList:
                standard_dict["Location"] = AttributeList[n]
            elif AttributeList[n] in WeatherList:
                standard_dict["Weather"] = AttributeList[n]
            n += 1
            if AttributeList[n] in WeatherList:
                standard_dict["Weather"] = AttributeList[n]
            n += 1
            try:
                if isinstance(int(AttributeList[-1]), int) or AttributeList[-1] in Colors:
                    standard_dict["#"] = AttributeList[n]
            except ValueError: # Occurs when int() tries to convert a type that cannot be converted to int. Should probably find a better way.
                pass
            n += 1
            # For Leah
            if AttributeList[n] in Extras:
                standard_dict["Extra"] = AttributeList[n]
        except IndexError:
            if n < 5:
                print(f"Tried accessing AttributeList[{n}] for {FileName}, caught index out of range!")
        return standard_dict
    final_dict = {}
    if SubFolder != None:
        SubPath = SubFolder.split("/")
        if len(SubPath) == 2:
            final_dict[SubPath[1]] = create_standard_dict() 
        elif len(SubPath) == 3:
            final_dict[SubPath[1]][SubPath[2]] = create_standard_dict() 
    else:
        final_dict = create_standard_dict()
    return final_dict
def swim(AttributeList, FileName):
    Colors = ["Pink", "White", "Blue", "Yellow"] # For Haley and Penny
    swim_dict = {
        "Swim": True,
        "Color": None,
        "Floaties": None # For George
    }

    try:
        if AttributeList[1] in Colors:
            swim_dict["Color"] = AttributeList[1]
        elif "NoFloaties" in AttributeList[1]:
            swim_dict["Floaties"] = False
        elif "Floaties" in AttributeList[1]:
            swim_dict["Floaties"] = True
    except IndexError:
        print(f"Tried accessing AttributeList[1] for {FileName}, caught index out of range!")
    return swim_dict
def check_attributes(AttList, AssetName, SubFolder=None):
    specials = [
    "Maternity", # Bachelorettes
    "Aerobics", # Caroline, Emily, Jodi
    "Clint_Apron", 
    "LabCoat", # Demetrius
    "Photokit", # Haley
    "Doctor", # Harvey
    "BrownEye_Overlay", # Harvey
    "Harvey_GreenEye_Overlay", # Harvey
    "Harvey_MustacheOverlay" # Harvey
    "RFQ" # RandomFlowerQueen
    ] 
    SeasonList = ["Spring", "Summer", "Winter", "Fall"]

    if "Swim" in AttList[0]:
        return swim(AttList, Path(AssetName).name)
    elif SubFolder != None:
        return standard_format(AttList, Path(AssetName).name, SubFolder)    
    elif AttList[1] in SeasonList:
        return standard_format(AttList, Path(AssetName).name)
    elif AttList[1] in specials:
        return AttList[1]
    elif AttList[1] == "Hospital": # For Maru_Hospital
        return AttList[2] # The color
    elif AttList[1] == "Motorbike": # For Sebastian
        return "Motorbike_Helmet"
def check_folder(FolderType, SubDirName, Folder):
    if Path(Path(f"svosmapi/assets/{FolderType}/{SubDirName}/{Folder}").resolve().parent).name == "Demetrius": # svosmapi/assets/{FolderType}/Work
        return f"{FolderType}/{SubDirName}/{Folder}"
def scan_standard(FolderType):
    with os.scandir(f"svosmapi/assets/{FolderType}") as SubDir:
        for SubName in SubDir:
            second_dict = {}
            try:
                with os.scandir(f"svosmapi/assets/{FolderType}/{Path(SubName).name}") as AssetDir:
                    for AssetName in AssetDir:
                        third = None
                        if os.path.isdir(AssetName):
                            new_path = check_folder(FolderType, Path(SubName).name, Path(AssetName).name)
                            with os.scandir(f"svosmapi/assets/{new_path}") as AssetSubDir:
                                for SubAsset in AssetSubDir:
                                    AttList = (Path(SubAsset).stem).split("_")
                                    print(AttList)
                                    third = check_attributes(AttList, SubAsset, new_path)
                        else:
                            AttList = (Path(AssetName).stem).split("_")
                            print(AttList)
                            third = check_attributes(AttList, AssetName)
                        second_dict[Path(AssetName).stem] = third
                    first_dict[Path(SubName).stem] = second_dict
            except NotADirectoryError:
                continue

scan_standard("Portraits")
with open('Assets.json', 'w') as fp:
    fp.write(json.dumps(first_dict, indent=4))








# Archive of previous attempt to sort through subfolders

def check_folder(FolderType, SubDir, nest1, nest2="", nest3=""):
    SubDirName = Path(SubDir).name
    if nest3:
        with os.scandir(f"svosmapi/assets/{FolderType}/{SubDirName}/{nest1}/{nest2}/{nest3}") as SubFolder:
            for AssetDir in SubFolder:
                AssetName = Path(AssetDir).stem
                return folder_or_asset(AssetName, FolderType, SubDirName, nest2, nest3)
    elif nest2:
        with os.scandir(f"svosmapi/assets/{FolderType}/{SubDirName}/{nest1}/{nest2}") as SubFolder:
            for AssetDir in SubFolder:
                AssetName = Path(AssetDir).stem
                return folder_or_asset(AssetName, FolderType, SubDirName, nest2, nest3)
    else:
        with os.scandir(f"svosmapi/assets/{FolderType}/{SubDirName}/{Path(nest1).name}") as SubFolder:
            for AssetDir in SubFolder:
                AssetName = Path(AssetDir).stem
                return folder_or_asset(nest1, FolderType, SubDir, nest2)
def folder_or_asset(FolderType, SubDir, AssetName, nest2=1, nest3=1):
    third = None
    def split_check(n1, n2="", n3=""):
        if os.path.isfile(n3):
            Name = n3
        elif os.path.isfile(n2):
            Name = n2
        else:
            Name = n1
        AttList = (Path(Name).name).split("_")
        print(AttList)
        atuple = [check_attributes(AttList, AssetName), [n1, n2, n3]]
        return atuple
    if os.path.isfile(nest3):
        third = split_check(AssetName, nest2, nest3)
    elif os.path.isfile(nest2):
        third = split_check(AssetName, nest2)    
    elif os.path.isdir(nest3):
        third = check_folder(FolderType, SubDir, AssetName, nest2, nest3)
    elif os.path.isdir(nest2):
        third = check_folder(FolderType, SubDir, AssetName, nest2)
    elif os.path.isdir(AssetName):
        third = check_folder(FolderType, SubDir, AssetName)
    else:
        third = split_check(AssetName)
    return third
def scan_standard(FolderType):    
    with os.scandir(f"svosmapi/assets/{FolderType}") as SubDirectories:
        for SubDir in SubDirectories:
            second_dict = {}
            SubDirName = Path(SubDir).name
            try:
                with os.scandir(f"svosmapi/assets/{FolderType}/{SubDirName}") as AssetDirectories:
                    for AssetDir in AssetDirectories:
                        AssetName = Path(AssetDir).stem
                        third_tuple = folder_or_asset(FolderType, SubDir, AssetDir)
                        third = third_tuple[0]
                        third_paths = third_tuple[1]
                        if os.path.isfile(third_paths[2]):
                            second_dict[AssetName][Path(third_paths[1]).name][Path(third_paths[2]).name] = third
                        elif os.path.isfile(third_paths[1]):
                            second_dict[AssetName][Path(third_paths[1]).name] = third
                        else:
                            second_dict[AssetName] = third
                    first_dict[SubDirName] = second_dict
            except NotADirectoryError:
                continue

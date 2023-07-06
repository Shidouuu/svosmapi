import os
from pathlib import Path

print([d for d in os.listdir(os.getcwd()) if os.path.isdir(d)])
#!/usr/bin/python
import os

cpy = open("copyright.template").read()

for dirpath, dnames, fnames in os.walk("./"):
    for f in fnames:
        if (not ".git" in dirpath):
            if (not "Extras" in dirpath):
                if(f.endswith(".cs") and not(f.endswith("Designer.cs"))):
                    path = os.path.join(dirpath, f)
                    print (path)
                    original = open(path).read()
                    if not 'copyright file=' in original:
                        with file(path, 'w') as modified:
                            modified.write(cpy.replace("FILENAME", f) + original)

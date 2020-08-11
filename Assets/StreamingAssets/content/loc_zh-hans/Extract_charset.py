import os
import itertools
from tkinter import Tk

##
## Function pulls all unique characters from the localiseds string in a .PO (Portable Object) file
## and returns them sorted into order. Used for determining what characters are actually used ingame
##
def ExtractCharSet( filename ):
	charset = ''
	with open(filename, encoding="utf8") as fp:
		line = fp.readline()
		cnt = 1
		while line:
			##if line.find("msgstr") == 0:
			for ch in line:
				if ord(ch) > 32:
					if charset.find(ch) == -1:
						charset = charset + ch
			##print("Line {}: {}".format(cnt, line.strip()))
			##print(charset)
			line = fp.readline()
			cnt += 1
		#Bubble sort resulting charset into code order
		charset = ''.join(sorted(charset))
	return charset

gcharset = ''
print("Extracting unique chars...")
print("NOTE: easiest way to extract UI strings is to make a temp copy of content/strings.csv in the target folder, and remove all the unwanted language columns.")
for root, dirs, files in os.walk("."):
	path = root.split(os.sep)
	print((len(path) - 1) * '---', os.path.basename(root))
	for file in files:
		if (file.endswith(".json") or file.endswith(".JSON") or file.endswith(".csv") or file.endswith(".CSV")):
			print(len(path) * '---', file)
			fcharset = ExtractCharSet(os.path.join(root, file))
			gcharset += fcharset;

#Bubble sort into order and remove duplicate characters
gcharset = ''.join(sorted(gcharset))
gcharset = ''.join(ch for ch, _ in itertools.groupby(gcharset))
print("Gathered unique characters to Charset.txt - list below:")
print(gcharset)
with open("Charset.txt", "w", encoding="utf8") as text_file:
    text_file.write(gcharset)
wait = input("PRESS ENTER TO CONTINUE.")

#Tried to get it to copy results into clipboard but no joy :/
#Have to copy them from the cmd line window instead..
from tkinter import Tk
r = Tk()
r.withdraw()
r.clipboard_clear()
r.clipboard_append('i can has clipboardz?')
r.update() # now it stays on the clipboard after the window is closed
r.destroy()


# File Find

## About

* Find files based on search keyword

## Usage

* Enter or browse for a starting directory to search in
* Enter a keyword to search for
* Select various search option and desired file-type 


### Screenshots

![Results](imgs/Screenshot%201.PNG)
![Right Click](imgs/Screenshot%202.PNG)
![File Types](imgs/Screenshot%203.PNG)

### Features

* Ability to search for keyword within various file-types
  - Text Files (.txt)
  - Word Documents (.docx)
  - Excel Files (.xlsx)
  - CSV Files (.csv)
* Open the found file(s) straight from the application
* Open found files directory straight from the application
* Copy slected file directory
* Various search options included
  - Navigate Subdirectories
  - Match the whole searchword
  - Match the case of the search word
* Ability to open File Find from current directory (see below for instructions)

  ### Setup Right-Click to Open File Find

1. Locate File Find .exe location
2. Copy full file-path to clipboard -> (D:\C#\File-Find\File Find\bin\Release\File Find.exe)
3. Open Registry Editor by searching for "regedit" in windows start menu
4. Naviagte to HKEY_CLASSES_ROOT\Directory\Background\shell
5. Right-click shell and click New -> Key
    1. Rename folder to File-Find
6. Right-click newly created File-Find folder and clikc New -> Key
    1. Rename folder to command 
7. Double clikc the (Default) item within the command folder
8. Paste in the copied File-Find file-path in the Value data section copied in step 2. with quote and also at "%V" (within quotes)
    1. Ex: "D:\C#\File-Find\File Find\bin\Release\File Find.exe" "%V"

![Command](imgs/Screenshot%204.PNG)

* That's it! You know can open the File-Find application by right clicking within a explorer window
* The current directory will automically be inserted into the directory textbox for faster searching

9. (Optional) If you wish to add an icon to the right-click menu option follow these steps:
    1. Right-click the File-Find folder we created in step 5 and click New -> String Value
    2. Rename the new item Icon
    3. Double click the Icon item to edit the options
    4. Paste in the same File Find direcotry copied in step 2 within quotes (this time leave off the "%V" we added in step 8)

![Icon](imgs/Screenshot%205.PNG)
  
* Final Results
  
![RightClick](imgs/Screenshot%206.PNG)


 
### Executing

1. Open the solution file and start the application
2. Run the executable within the Release folder for faster execution

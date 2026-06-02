Snipit
Save sets of Grasshopper components once, reuse them anywhere.
Snipit lets you capture any selection of Grasshopper components — with their wiring intact — and drop them back onto any canvas, in any file, whenever you need them. It lives as a button right in the Grasshopper canvas toolbar, so your reusable building blocks are always one click away.
Think of it as a personal library of Grasshopper snippets that travels with you across every definition you work on.

Why Snipit?
Every Grasshopper user rebuilds the same little clusters over and over — a remapping setup, a favourite data-tree wrangle, a lighting rig, a panel-numbering routine. Copy-pasting between files means digging through old definitions to find them. User Objects and clusters help, but they flatten your components into a single black box.
Snipit keeps your components exactly as they are — multiple components, fully wired, ready to drop in and edit. Save once, reuse forever.

Features
Capture a selection
Select any group of components on the canvas, click the Snipit button in the toolbar, choose Capture Selection, give it a name, and it's saved. The internal wiring between the captured components is preserved.
<img width="1843" height="1077" alt="image" src="https://github.com/user-attachments/assets/730195cd-76bf-4c78-9289-5b9c53e175ba" />

save with name
<img width="1635" height="939" alt="image" src="https://github.com/user-attachments/assets/88b5d292-eb30-4395-a829-628f0eccbac1" />

notification of successfull save
<img width="1683" height="929" alt="image" src="https://github.com/user-attachments/assets/706dda15-2f12-4fad-a763-96aab32343ba" />

Deploy anywhere
Click Snipit → Deploy Snipit and pick any saved snipit from your library. It drops onto the canvas right at your cursor, with fresh component IDs so you can deploy the same snipit multiple times without conflicts.

deploy saved scrips across files
<img width="1750" height="1090" alt="image" src="https://github.com/user-attachments/assets/815e1289-8a6e-462c-b458-0da95d7664d2" />

choose from the saved list
<img width="134" height="37" alt="image" src="https://github.com/user-attachments/assets/5d5c6175-8af0-49ac-8293-040f72b5dd4b" />



Delete what you no longer need
Right-click any snipit in the deploy list to remove it from your library, with a confirmation prompt so nothing disappears by accident.

<img width="268" height="196" alt="image" src="https://github.com/user-attachments/assets/9645447c-dbaf-45cb-96a0-6d1d91bf3062" />

Built to never lose your work
Every snipit is stored as its own independent file on disk. Unlike tools that keep everything in a single shared file, a problem with one snipit can never wipe out the rest of your library.

How it works

Working Demo<img width="2552" height="1160" alt="Working Demo" src="https://github.com/user-attachments/assets/48589dea-bc2b-471e-b866-f9735fdaa791" />

Snipit stores each saved snippet as a small file under:
%AppData%\Snipit\
Snipits are grouped into folders (currently a single "General" group; named tabs are coming). Each snipit is a binary file containing the serialized Grasshopper components and their connections. Because each one is a separate file, your library is robust, easy to back up, and simple to move between machines — just copy the Snipit folder.

Installation
Snipit runs on Rhino 8 (Windows), under both the .NET Framework and .NET 7 (Core) runtimes.

Roadmap
Snipit is under active development. Planned features:

Named tabs — organise snipits into custom categories instead of one flat list.
Thumbnail previews — see a small preview image of each snipit before deploying.
Search — filter your library by name as it grows.
Rename — rename snipits and tabs in place.
Import / export — share whole libraries, or individual snipits, with teammates.
Missing-dependency warnings — clear notice when a snipit needs a plugin you don't have installed.

Feedback
Snipit is a young project and feedback is welcome. If you hit a bug or have a feature request, please open an issue on the repository.

Snipit — your reusable Grasshopper building blocks, always one click away.

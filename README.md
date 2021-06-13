## Unity selected objects History Window

[![openupm](https://img.shields.io/npm/v/com.gemserk.selectionhistory?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gemserk.selectionhistory/)

This is a small plugin that keeps a history of the Unity's Editor object selection (it stores in the background) and displays it in a Window to easily access it. 

It is really useful when editing stuff and following a link to an object reference to see some details and then go back to previous selection.

**How it works**

<img src="screenshots/demo.gif" width="400" />
<br/>

### Features

* Stores history of selected objects (custom count), both assets and scene objects (optional)
* Drag objects from the window to Scene, Hierarchy, Project or Inspector ([doesn't work on mac](https://fogbugz.unity3d.com/default.asp?894059_1u4pe70tnj823875))
* Keeps tracks for selection from unloaded scenes (optional)
* Select (left click) or ping/focus objects without losing selection (right click).
* Favorite items to [Favorites window](#favorites-window) (optional)

### Install it from Open UPM

This package can be installed using [![openupm](https://img.shields.io/npm/v/com.gemserk.selectionhistory?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gemserk.selectionhistory/) (<-- click here for more info ;)).

### Install from Git URL

You can also install it by opening Unity Package Manager and selecting add package from git URL and add this `https://github.com/acoppes/unity-history-window.git#1.1.2`, Or add it manually to the `manifest.json`.

### Other examples

**Dragging from window to inspector**

<img src="screenshots/demodrag.gif" width="400" />
<br/>

**Dragging from window to scene**

<img src="screenshots/demodrag2.gif" width="400" />
<br/>

## Favorites Window

This window keeps a list of favorite items, similar to searches but specific objects instead. It works in synergy with selection history window.

<img src="screenshots/favorites01.gif" width="400" />
<br/>

### Features

* Keep your list of most used or important assets at hand.
* Drag assets directly to window to favorite them.
* Stores favorites for project in custom asset (can be ignored in SCM or shared with other devs).
* Drag objects from the window to Scene, Hierarchy, Project or Inspector ([doesn't work on mac](https://fogbugz.unity3d.com/default.asp?894059_1u4pe70tnj823875))

### Roadmap

* Filters by type of asset (textures, text, etc).
* Adding/Remove tags to favorite assets.
* Filters by tags.
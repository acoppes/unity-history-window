## Unity selected objects History Window

[![openupm](https://img.shields.io/npm/v/com.gemserk.selectionhistory?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gemserk.selectionhistory/)

This plugin keeps a history of selected objects in Unity. It comes with a [window](#selection-history-window) to see the list of objects and interact with them with other behaviors like, for example, focus an previously selected object or even drag them to scene, project view, etc. 

<img src="screenshots/demo.gif" width="500" />
<br/>

### Install it from OpenUPM

This package can be installed using OpenUPM, just click here [![openupm](https://img.shields.io/npm/v/com.gemserk.selectionhistory?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gemserk.selectionhistory/) for more details.

### Install from Git URL

You can also install it by opening Unity Package Manager and selecting add package from git URL and add this `https://github.com/acoppes/unity-history-window.git#1.1.2`, Or add it manually to the `manifest.json`. Check the latest released version.

### Features

* Stores history of selected objects (custom count), both assets and scene objects (optional)
* Drag objects from the window to Scene, Hierarchy, Project or Inspector (alt/option key pressed)
* Keeps tracks for selection from unloaded scenes (optional)
* Select (left click) or ping/focus objects without losing selection (right click).
* Quick menu options to customize behavior.
* Favorite items to [Favorites window](#favorites-window) (optional)

### Roadmap

* Add more icons to show visually actions like ping, select and drag.

As extra, it comes with a [another window](#favorites-window) to keep track of a list of favorite assets and easily access them.

**Dragging from window to inspector**

<img src="screenshots/demodrag.gif" width="500" />
<br/>

**Dragging from window to scene**

<img src="screenshots/demodrag2.gif" width="500" />
<br/>

**Quick options to override behavior**

<img src="screenshots/history04.gif" width="500" />
<br/>

## Favorites Window

This window keeps a list of favorite items, similar to searches but specific objects instead. It works in synergy with selection history window.

<img src="screenshots/favorites01.gif" width="500" />
<br/>

### Features

* Keep your list of most used or important assets at hand.
* Drag assets directly to window to favorite them.
* Stores favorites for project in custom asset (can be ignored in SCM or shared with other devs).
* Drag objects from the window to Scene, Hierarchy, Project or Inspector (alt/option key pressed)

### Roadmap

* Filters by type of asset (textures, text, etc).
* Adding/Remove tags to favorite assets.
* Filters by tags.
* Menu options to customize behavior.

### Collaborate

Feel free to fork the project and make modifications for yourself or to share by creating pull requests. Also, create issues for feature requests or bug reports if you want to help improving this plugin, thanks!

### Contact

<a href="https://twitter.com/intent/tweet?screen_name=arielsan&ref_src=twsrc%5Etfw"><img src="screenshots/twitter_logo.png" width="48"/><br/>@arielsan</a>

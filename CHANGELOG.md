# 1.4.1

* Fixed to clone search bar element and not the template, to fix an issue with searchbar hidden from scroll elements.
* Fixed stylesheet to match colors and also added some padding to search toolbar.

# 1.4.1

* Removed direct dependencies to assets from the csharp files to avoid issues when reloading the window, now it just searches the stylesheet and others in OnEnable methods.
* Fixed package.json version

# 1.4.0

* Added search toolbar for both history and favorites windows.

# 1.3.1 

* Reverted to use Drag from history window without having to use Alt modifier (was a fix/workaround for mac users but I suppose windows is more common for Unity devs)

# 1.3.0

* Moved asset type declaration to editor assembly now they use ScriptableSingleton, changed to use ScriptableSingleton for selection history to avoid issues creating the asset the first time. Had to change min unity version to 2020.x in order to use this new API.
* Changed min unity version to 2020.1.1f1.
* Changed default path for SelectionHistory and Favorites assets (so previously stored values will be lost with this update), also remember to update .gitignore (or similar) in order to ignore.

# 1.2.0

* Huge window performance improvements (use with caution) by caching all visual elements instead of regenerating the window all the time (my bad there, should've done this when I migrated to use UIElements the first time)

# 1.1.19

* Don't record selection changes while selection history window is closed (no need for focus). Optional background selection changes recording too.

# 1.1.18

* Now eye open icon and and double click also opens scenes (previous modified dialog check). NOTE: you have to close and open both Favorites and History windows if you want the new icons to appear.

# 1.1.17

* Added button to open prefab in edit mode from favorites.
* Added button to open prefab in edit mode from history.
* Also added double click on prefab opens prefab to edit in both history and favorites window.

# 1.1.16 

* Dont record hierarchy objects while application is playing to avoid issues with references to dynamic objects.

# 1.1.15

* Possible fix for scene corruption bug by disabling reloading root object on scene unloaded when having multiscenes and hierarchy objects turned on.
* Fixed to not serialize references to hierarchy objects.
* Fixed bug modified history size was not being used unless editor or window was closed and open again. 
* Fixed editing preferences was losing focus on each change by reloading the window (disabled that for now, reload manually).

# 1.1.14

* Fixed bug with pressing favorite icon from the history window will add same item multiple times instead of removing it.
* Changed favorite icons to support newer versions of unity. 

# 1.1.13

* Added option in preferences to render first last selected object (could be a button in local menu too).

# 1.1.12

* Fixed bug from last version objects in project view were not detected if hierarchy objects disabled. 

# 1.1.11

* Dont store global id if scene selection is disabled to improve performance. 

# 1.1.10

* Fixed issue with scene references when moving to another scene and then back

# 1.1.9

* Now selection history is stored in file in order to recover session if Unity was closed.
* Fix for stored scene objects were detected as the same and removed on unity reload.
* Always store global id for scene objects even if scene didn't change.

# 1.1.8

* Fixed favorite prefab issue #36.

# 1.1.7

* Removed unused variable to fix issue #35 for strict build pipelines
* Removed background style for .history element for issue #34

# 1.1.6

* Fixed bug when hierarchy object selected while hierarchy objects hidden.

# 1.1.5

* Fixed bug when favorites controller asset wasn't created yet.

# 1.1.4

* Now drag objects work on mac too.
* Moved drag to mouse down event, alt key has to be pressed to perform drag.

# 1.1.3

* Fixed bugs on scroll when selecting with new window version using UIElements.

# 1.1.0

* Separated Favorite assets in a new window and removed from history window.
* There is now a Favorites asset to keep track of user's favorites, can be ignored or not from scm, depends on what the user want. 
* Added icons for both windows title bar. 
* Drag objects to fav window favorites them
* Added some settings to selection history window custom menu. 
* New history window using UIElements
* Now user preferences for selection history can be open from custom menu.

# 1.0.8

* Now scene is pinged when pressing ping for unloaded objects.
* Now all scene references are shown with scene name prefix.

# 1.0.7

* Feature: store selection from unloaded scenes.

# 1.0.4

* Fixed missing .meta was failing on reimporting package in readonly folder.

# 1.0.3

## Fixed

 * On mac, the editor become unresponsive when clicking an entry.

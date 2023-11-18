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
* Fixed editing prefrences was losing focus on each change by reloading the window (disabled that for now, reload manually).

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

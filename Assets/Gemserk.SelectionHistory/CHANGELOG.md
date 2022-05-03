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

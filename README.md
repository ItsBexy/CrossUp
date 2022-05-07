# CrossUp (Test Release)

This plugin adds several customization options to FFXIV's main Gamepad interface, the Cross Hotbar.

![Separate the two halves of the Cross Hotbar](images/example_1.png)
![Choose the highlight colour for the selected bar](images/example_2.png)

## Current Features

* Split up the Left/Right Cross Hotbars, allowing for other UI elements (such as job gauges or the minimap) to be placed in the centre between them.
* Edit the highlight colour for the currently-selected bar, or hide the highlight backdrop entirely
* Edit positions of individual Cross Hotbar elements
* Display the Expanded Hold Controls as their own UI element, fully separate from the main hotbar
	* NOTE: This feature requires you to select two of your mouse/keyboard hotbars for the plugin to "borrow". It will then rearrange the buttons from those bars to appear as part of the Cross Hotbar. The contents of the bars will not be overwritten, but they will be unavailable while the feature is active.

## Potential/Planned Features

* "Hide Unassigned Slots" option, like the one that exists for mouse/kb hotbars
* Bar-specific mappings for WXHB and Expanded Hold Controls
	* For example, displaying bar 3 on the WXHB when the main bar is set to 1, bar 8 when the main bar is set to 2, etc
* More colour customization (ie, the glow frame whenever a button is pressed)
* Greater control over bar highlighting, including setting the alpha
* Allowing the Expanded Hold Controls to be set to a different scale than the main Cross Hotbar

## Known Issues

* "Borrowed" bars do not always immediately arrange into their appropriate position/shape when selected in CrossUp's configuration window.
	* This should fix itself promptly as soon as you press a trigger on your controller
* Visual issues when "Cross Hotbar Display Type" is set to "D-pad + D-pad / Action Buttons + Action Buttons"
	* When this setting is selected in Character Configuration, and Left/Right separation in CrossUp is set greater than 0, the bar highlight backdrop is hidden. This is a stopgap solution to handle the fact that those backdrops cannot be easily moved, and would appear misaligned from the bars if they were left visible.
	* Bar scale tends to "jiggle" when switching from Expanded Hold Controls back to main bars. This issue was already fixed for the default bar layout, but the fix still needs to be tailored for the mixed layout as well.
* Pet hotbars appear to switch to different contents while using Expanded Hold Controls. This is only a visual bug and does not interfere with pet hotbar functionality.
* When "Hide Unassigned Slots" is selected for standard hotbars, the setting is also applied to the Expanded Hold Controls display
	* As noted above, a feature is planned to both fix this issue and give the user full control over this behaviour
* Switching HUD layouts, or opening the HUD layout interface, can cause unprompted movement for bars that have been affected by the plugin
	* Attempted fixes have been implemented for some instances of this, but more testing is needed
	* It may be best to make HUD layout changes with CrossUp settings at their defaults (or with the plugin disabled), and then restore your preferred CrossUp settings afterwards
* Drag-and-drop issues
	* When dragging actions to/from the Expanded Hold bars, the changes are actually saved to the underlying hotbar being used to represent it, NOT saved to the actual mapped cross hotbar.
	* When you wish to reassign your Expanded Hold Controls, you should continue doing so via the main Cross Hotbar's interface, not via the additional elements created by CrossUp.

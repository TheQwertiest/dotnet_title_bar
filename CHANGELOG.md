# Changelog

## [Unreleased]

## [1.0.4a] - 2017-10-18
### Fixed
- Fixed text truncation when DPI scaling is enabled.
- Fixed invalid height of the default text in flourish skin ([link](https://github.com/TheQwertiest/foo_title/wiki/Skin-Gallery#flourish-linkfixed-version)).

## [1.0.4] - 2017-10-17
### Added
- Added in-built DPI scaling, which is enabled by default (see [docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#Advanced)).

### Changed 
- Removed 'scaleable' attribute from text layer.

### Fixed
- Fixed bug: preferences changes were not always reverted, when exiting without applying those changes.

## [1.0.3] - 2017-10-04
### Added
- Added 'persistent' attribute to layer (see [docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#Layer)).
- Added animated .gif support to 'absolute-images' layer.

### Changed 
- Updated Milk Plate skin to make use of new 'persistent' attribute: now the panel's state is persistent across fb2k launches.
- Updated White skin to make use of .gif support.

## [1.0.2] - 2017-09-22
### Added
- Added foo_title to Preferences > Components menu.

### Changed 
- Updated flourish skin ([git directory](https://github.com/TheQwertiest/foo_title/tree/master/foo_title/skins/fixed_user_skins/flourish2)).

### Fixed
- Fixed bug: 'enable when minimized' was not working at all.
- Fixed inconsistent fade-in/fade-out animations (hopefully for the last time).

## [1.0.1] - 2017-09-14
### Added
- Added 'scaleable' attribute to text layer (see [docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#text)).

### Fixed
- Text is not scaled with DPI by default now, thus preserving skins, that are not compatible with scaling, from breaking on high DPI screens.
- Fixed preferences page being cut-off, when resolution scaling is above 100%.

## [1.0.0] - 2017-08-15
### Added
- Added 'speed' property to animation layer (see [docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#animation)).
- Added [>>gallery<<](https://github.com/TheQwertiest/foo_title/wiki/Skin-gallery) showcasing stock and user-created skins.
- Updated various user-created skins for v1.0.0 (see [git directory](https://github.com/TheQwertiest/foo_title/tree/master/foo_title/skins/fixed_user_skins)).

### Changed
- Greatly reduced CPU usage: now UI is redrawn only on request and won't be redrawn if there is nothing to do.
- Renamed "Update Interval" in Preferences to "Maximum refresh rate" to better reflect it's new meaning.
- Skin list loading is now asynchronous.
- Tweaked skin list for cleaner look.
- Updated to .NET 4.5.

### Fixed
- Fixed bug: foo_title might move beyond screen borders, when changing to skin with different anchor position.
- Fixed bug: inconsistent fade-out animation when mouse is leaving foo_title.

## [0.9.5] - 2017-08-10
### Added
- Added 'clip' property to skin element: set to false to disable clipping (see [docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#Layer)).
- Added 'no-content' layer type: this layer has no contents itself and is used solely for positioning other layers, it is also transparent for tool-tip layer detection (see [docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#no-content)).

### Changed
- Updated Milk Plate skin.

### Fixed
- Fixed tool-tip behavior on complex skins.
- Fixed minimal type geometry behavior on complex skins.
- Fixed error that prevented the filling of the skin list, when one of the skins is not parsable.
- Fixed clipping not being applied.

## [0.9.4a] - 2017-08-08
### Fixed
- Fixed crash on exit from foobar2000.

## [0.9.4] - 2017-08-07
### Added
- Added ability to display tooltip ([docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:0.9_Titlebar_%28foo_title%29#Layer)).
- Added 'align' attribute to minimal geometry ([docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:0.9_Titlebar_%28foo_title%29#minimal)).
- Added skin loading time logging to foobar2000 console.

### Changed
- Reduced CPU usage.
- Renamed dotnet_title.dll to foo_title.dll.
- Skin's name for preferences page is taken from skin.xml now, rather than from directory's name.
- Added skin's author's name to the skin list.

### Fixed
- Disabled foo_title window dragging when mouse is over button layer, thus preventing accidental button presses.
- Preferences page now displays only folders containing skin.xml.
- Fixed anchor drawing.

## [0.9.3] - 2017-08-01
### Added
- Added double-click action to button layer ([docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#button)).
- Added 'minimal' geometry type ([docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#minimal)).

### Changed
- **(!!!)** Renamed anchor_type property to anchor.
- **(!!!)** Added culling to layer drawing, meaning that the layer can't draw outside of it's specified size. Might break skins that rely on drawing without culling.
- Value for 'anchor' property is now case insensitive: i.e. 'Right' and 'right' are treated the same.

### Fixed
- Fixed popup being out of screen when changing to skin with different anchor.
- Fixed error being displayed on startup, when there is no foo_title folder found.

## [0.9.2] - 2017-07-25
### Added
- Added ability to configure anchor point position in skin.xml ([docs](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Components/Titlebar_(foo_managed_wrapper)#Header)).
- Added ability to display anchor point (see preferences page).

### Fixed
- Fixed inability to evaluate foobar2000 queries in skin.xml, when there is no song playing.
- Fixed memory leak.

## 0.9.1 - 2017-07-18
### Added
- Added peek action: this action displays foo_title popup for a brief time. Can be assigned to hotkeys. Can only be used when foo_title is enabled.
- Added fade in and fade away animations for popup.

### Changed
- Renamed dll to "foo_managed_wrapper" for consistent component naming.

### Fixed
- Fixed Unicode logging messages handling.

## Prior releases
- See https://github.com/lepkoqq/foo_title/releases

[Unreleased]: https://github.com/theqwertiest/foo_title/compare/v1.0.4a...HEAD
[1.0.4a]: https://github.com/theqwertiest/foo_title/compare/v1.0.4...v1.0.4a
[1.0.4]: https://github.com/theqwertiest/foo_title/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/theqwertiest/foo_title/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/theqwertiest/foo_title/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/theqwertiest/foo_title/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/theqwertiest/foo_title/compare/v0.9.5...v1.0.0
[0.9.5]: https://github.com/theqwertiest/foo_title/compare/v0.9.4a...v0.9.5
[0.9.4a]: https://github.com/theqwertiest/foo_title/compare/v0.9.4...v0.9.4a
[0.9.4]: https://github.com/theqwertiest/foo_title/compare/v0.9.3...v0.9.4
[0.9.3]: https://github.com/theqwertiest/foo_title/compare/v0.9.2...v0.9.3
[0.9.2]: https://github.com/theqwertiest/foo_title/compare/v0.9.1...v0.9.2
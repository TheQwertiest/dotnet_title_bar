# Changelog

## [Unreleased]
### Changed
- Updated to .NET 4.5.
- Skin list loading is now asynchronous.
- Tweaked skin list for cleaner look.
- Changed screen redraw mechanism: now it redraws on request only and won't redraw if there is nothing to do.

## [0.9.5] - 2017-08-10
### Added
- Added clip property to skin element: set to false to disable clipping.
- Added no-content layer type: this layer has no contents itself and is used solely for positioning other layers, it is also transparent for tool-tip layer detection.

### Changed
- Updated Milk Plate skin.

### Fixed
- Fixed tooltip behaviour on complex skins.
- Fixed minimal type geometry behaviour on complex skins.
- Fixed error that prevented the filling of the skin list, when one of the skins is not parsable.
- Fixed clipping not being applied.

## [0.9.4a] - 2017-08-08
### Fixed
- Fixed crash on exit from foobar2000.

## [0.9.4] - 2017-08-07
### Added
- Added ability to display tooltip.
- Added align attribute to minimal geometry.
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
- Added double-click action to button layer.
- Added 'minimal' geometry type.

### Changed
- **(!!!)** Renamed anchor_type property to anchor.
- **(!!!)** Added culling to layer drawing, meaning that the layer can't draw outside of it's specified size. Might break skins that rely on drawing without culling.
- Value for 'anchor' property is now case insensitive: i.e. 'Right' and 'right' are treated the same.

### Fixed
- Fixed popup being out of screen when changing to skin with different anchor.
- Fixed error being displayed on startup, when there is no foo_title folder found.

## [0.9.2] - 2017-07-25
### Added
- Added ability to configure anchor point position in skin.xml.
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

[Unreleased]: https://github.com/theqwertiest/foo_title/compare/v0.9.5...HEAD
[0.9.5]: https://github.com/theqwertiest/foo_title/compare/v0.9.4a...v0.9.5
[0.9.4a]: https://github.com/theqwertiest/foo_title/compare/v0.9.4...v0.9.4a
[0.9.4]: https://github.com/theqwertiest/foo_title/compare/v0.9.3...v0.9.4
[0.9.3]: https://github.com/theqwertiest/foo_title/compare/v0.9.2...v0.9.3
[0.9.2]: https://github.com/theqwertiest/foo_title/compare/v0.9.1...v0.9.2
# Changelog

## [Unreleased]

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

[Unreleased]: https://github.com/theqwertiest/foo_title/compare/v0.9.4...HEAD
[0.9.3]: https://github.com/theqwertiest/foo_title/compare/v0.9.3...v0.9.4
[0.9.3]: https://github.com/theqwertiest/foo_title/compare/v0.9.2...v0.9.3
[0.9.2]: https://github.com/theqwertiest/foo_title/compare/v0.9.1...v0.9.2
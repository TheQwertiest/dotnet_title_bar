---
title: Component settings
nav_order: 6
---

# Component settings
{: .no_toc }

## Table of contents
{: .no_toc .text-delta }

* TOC
{:toc}

---

## Preferences page

### Main
{% include functions/clickable_img.html
    img='/assets/img/screenshots/docs/preferences_main.png'
    title='Screenshot of the `Main` preferences tab'
    alt='Screenshot of the `Main` preferences tab'
  %}

- `Show Popup`: this box only affects behavior of the enabled Title Bar, i.e. when `Main`>`Enabled` is set to `Always` or `When foobar2000 is minimized` with fb2k minimized.
  - `Always` - Title Bar is always displayed. When one of the triggers is called (see `Popup Triggers` below) the opacity of the Title Bar is changed to the value from Advanced > Opacity on trigger.
  - `On trigger` - Title Bar is displayed only when triggered. All the remaining time it's not visible and is not interactable.
- `Popup Triggers`: this box is used to enable or disable triggers, that affect the appearance of the Title Bar. Mouse-over and `Peek Title Bar` keyboard action are also considered a trigger.
- `Popup Peek`: used to set the delay before Title Bar is hidden (see [Keyboard Shortcuts](#keyboard-shortcuts) for more info).

### Advanced
{% include functions/clickable_img.html
    img='/assets/img/screenshots/docs/preferences_advanced.png'
    title='Screenshot of the `Advanced` preferences tab'
    alt='Screenshot of the `Advanced` preferences tab'
  %}
- `Max Refresh Rate`: Title Bar is only refreshed\redrawn when it is needed, but in some complex skins there might be a lot of such calls, resulting in lots of redraws and high CPU usage. This option limits the maximum allowed redraws per second to avoid the issue. The value can be further reduced for reduced CPU usage or further increased for more fluidity.
- `Dpi Scaling`: when enabled, Title Bar will scale the skin according to system DPI settings. All images in the skin will be stretched, if required.
- `Anchor Position`: see [Header Format](skin_format_documentation.md/#header) for more info.

## Keyboard shortcuts

Title Bar contains the following functions that can be mapped to keyboard shortcuts in `Preferences`>`Keyboard Shortcuts`>`Action`:
- `Toggle Title Bar` : calling this one is equal to toggling between `Always` and `Never` in `Preferences`>`Display`>`Title Bar`>`Main`>`Enabled`.
- `Peek Title Bar` : causes Title Bar to show briefly with full opacity (see [`Popup Triggers`](#main)). The time before fade-away can be configured through via `Main`>`Popup Peek`.

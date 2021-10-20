---
title: Skin format documentation
nav_order: 4
---

# Skin format documentation
{: .no_toc }

## Table of contents
{: .no_toc .text-delta }

* TOC
{:toc}

Skin is defined by the main `skin.xml` file, that contains skin's description, and various resources that are used by `skin.xml`.
Skin supports all the image formats that are supported by the .NET framework itself - that is bmp, jpg, png (for transparent images) and perhaps more.

`skin.xml` has the following structure:

```xml
<HEADER/>
<LAYER/>
<!-- more <LAYER> elements ->
```

## Header

Every skin must begin with a header like this one:

```xml
<?xml version="1.0" encoding="utf-8"?>
<skin
   author="[name of the author of the skin]"
   name="[name of the skin]"
   width="[width in pixel]"
   height="[height in pixel]"
   anchor="[anchor type]"/>
```

```
anchor::== left | right | top | bottom | center | ... | comma-separated combinations | ... | [top,left]
```

In some case the size of the skin will adjusted automatically (see [Text Elements](#text-elements) for more info).

When skin size changes, anchor stays on the same place relative to display, meaning that the skin will expand or contract only in non-anchored directions. The position of the anchor can be displayed via `Preferences`>`Display`>`Title Bar`>`Advanced`>`Anchor Position`>`Display anchor`.

{% include functions/clickable_img.html
    img='/assets/img/screenshots/docs/anchor.gif'
    title='Illustration of anchor position displayed'
    alt='Illustration of anchor position displayed'
  %}

## Layer

After the header

A layer is an element, a kind of container. Layers can be nested or placed consecutively.

The structure of the layer is as following:
```xml
<layer name="[layer name]" type="[layer type]" tooltip="[tooltip text]" clip="[clip enabled]" persistent="[persistent]" enabled="[enabled]">
    <geometry type="[geometry type]">
        <!-- geometry data -->
    </geometry>
    <contents>
        <!-- contents data -->
    </contents>
    <!-- more <layer> elements -->
</layer>
<!-- more <layer> elements -->
```

```
<layer> type::= absolute-images | album-art | animation | button | color | fill-images | no-content | scrolling-text | text
enabled::=      false | [true]
clip::=         false | [true]
persistent::=   true | [false]
<geometry> type::= absolute | full | minimal
```

Tooltip will be displayed only when text in tooltip property is not empty and the containing layer is topmost (with the exception of [[#no-content|no-content]] layer).
Tooltip text can also contain foobar2000 queries.

If `clip` property is set to `true` (which is the default value), then all the nested layers will be clipped by the parent layer's boundaries when drawn.

If `persistent` property is set to `true`, then the enabled state of the layer will be saved upon foobar2000 exit and will be restored on the next launch.

## Geometry type

`size`, `position` and `padding` properties of geometry can contain foobar2000 queries. Everything that works in foobar2000 will work here as well, see [Title Formatting Reference](http://wiki.hydrogenaud.io/index.php?title=Foobar2000:Title_Formatting_Reference) for further reference. Be advised, that using queries in `geometry` might degrade your performance if overused, since it parses those queries every time frame update is called (see [Advanced Preferences](settings.md/#advanced)).

### full

```xml
<geometry type="full">
    <padding left="[left padding]" top="[top padding]" right="[right padding]" bottom="[bottom padding]" />
</geometry>
```

The layer occupies as much space as possible - the whole client area of the parent layer. 

`full` type requires a padding element with the following attributes: `left`, `top`, `right`, `bottom`. These attributes adjust the position of the client area of the layer within the parent layer. For example: 
```xml
<geometry type="full">
    <padding left="32" top="8" right="32" bottom="8" />
</geometry>
```

{% include functions/clickable_img.html
    img='/assets/img/screenshots/docs/geometry_full_1.png'
    title='Illustration of the `full` geometry before resize'
    alt='Illustration of the `full` geometry before resize'
  -%} ==> {%- include functions/clickable_img.html
    img='/assets/img/screenshots/docs/geometry_full_2.png'
    title='Illustration of the `full` geometry after resize'
    alt='Illustration of the `full` geometry after resize'
  %}

### absolute

```xml
<geometry type="absolute">
    <size x="[width]" y="[height]" />
    <position x="[horizontal position]" y="[vertical position]" align="[alignment type]" />
</geometry>
```

```
align::== right | [left]
```

The layer has fixed absolute size and position relative to it's parent layer, i.e. resizing the window won't change the placement or the size of the `absolute` element.

The `x` and `y` attributes adjust the position of the layer relative to it's alignment. For example:

```xml
<geometry type="absolute">
    <size x="78" y="78" />
    <position x="13" y="1" align="left" />
</geometry>
```

{% include functions/clickable_img.html
    img='/assets/img/screenshots/docs/geometry_absolute_1.png'
    title='Illustration of the `absolute` geometry with left alignment'
    alt='Illustration of the `absolute` geometry with left alignment'
  %}

```xml
<geometry type="absolute">
    <size x="170" y="78" />
    <position x="13" y="1" align="right" />
</geometry>
```

{% include functions/clickable_img.html
    img='/assets/img/screenshots/docs/geometry_absolute_2.png'
    title='Illustration of the `absolute` geometry with right alignment'
    alt='Illustration of the `absolute` geometry with right alignment'
  %}

### minimal
```xml
<geometry type="minimal">
    <padding left="[left padding]" top="[top padding]" right="[right padding]" bottom="[bottom padding]" />
    <position align="[align position]"/>
</geometry>
```

```
align::== left | right | top | bottom | center | ... | comma-separated combinations | ... | [top,left]
```

This layer occupies only as much space as it is required by nested layers it contains and it's padding.  
Thus, for example, it's possible to wrap `text` layers with other layers, without specifying it's absolute size or filling the whole parent layer.

## Contents
`contents` defined the data contained in the layer.

### no-content
This layer does not contain contents any data. It's sole purpose is positioning of other layers. It is also transparent for tooltip layer detection, meaning that it won't disable tooltip for layers underneath.

### Graphic Elements

#### fill-images
This content type requires 3 images: one for the left border, one for the center, one for the right border.

```xml
<contents>
    <image position="left" src="left_back.png" />
    <image position="center" repeat="true" src="repeat_back.png" />
    <image position="right" src="right_back.png" />
</contents>
```

```
repeat::== true | [false]
```

If `repeat` attribute is set to `true`, then the picture will be repeated, otherwise it will be stretched.

#### absolute-images
Displays images stretched to the full size of the layer, one over another. This layer also supports animated .gif images.

```xml
<contents>
    <image src="[image path]"/> 
    <!-- more <image> elements -->
</contents>
```

#### animation
Cycles images repeatedly. Could be used instead of animated .gif images. Images are stretched to the full size of the layer. 
Cycle frequency is equal to 15 frames per second by default and can be adjusted via `speed` property.

```xml
<contents speed="[frames per second]">
    <frame src="[image_1 path]"/>
    <frame src="[image_2 path]"/>
    <frame src="[image_3 path]"/>
    <frame src="[image_4 path]"/>
    <frame src="[image_5 path]"/>
    <frame src="[image_6 path]"/>
    <frame src="[image_7 path]"/>
    <frame src="[image_8 path]"/>
    <nowiki><!-- more <frame> elements -></nowiki>
</contents>
```

```
speed::== ... | [15]
```

#### album-art
This layer displays the album art. There is one sub-element, `NoAlbumArt` which defines the image to show when there is no art to display.

```xml
<contents>
    <NoAlbumArt>
        <!-- [image path] -->
    </NoAlbumArt>
</contents>
```

#### color
Fills the whole layer with the specified color.

```xml
<contents color="[HEX coded ARGB color]"/>
```

### Text Elements
#### text
This layer displays the text. 

It is the only content type that can resize the skin dynamically. It does so according to the text width. Layer's geometry must be set to `full` or `minimal` for resizing to work.

```xml
<contents spacing="[text spacing]" angle="[angle of the text]" font="[font name]" size="[font size]" bold="[bold]" italic="[italic]" color="[hex coded argb color]">
    <defaultText>
        <!-- default text which is displayed when nothing is playing or when labels are empty -->
    </defaultText>
    <label position="left" font="[font name]" size="[font size]" bold="[bold]" italic="[italic]" color="[hex coded argb color]">
        <!-- text that might contain foobar2000 query -->
    </label>   
    <label position="right" font="[font name]" size="[font size]" bold="[bold]" italic="[italic]" color="[hex coded argb color]">
        <!-- text that might contain foobar2000 query -->
    </label>   
</contents>
```

```
spacing::==   ... | [20]
size::==      ... | [0]
font::==      ... | [Arial]
size::==      ... | [9]
bold::==      true | [false]
italic::==    true | [false]
color::==     ... | [FF000000]
<label> position::== right | [left]
```

`spacing` is the space between the left and right labels. 

By default `angle` attribute is set to zero. It can have an arbitrary value, but sizing works properly only for multiples of 90.

`color` defines the color. It uses AARRGGBB format.

Example:
```xml
<contents spacing="20" font="Verdana" size="8" bold="true" italic="true">
    <defaultText>foobar2000</defaultText>
    <label position="left" color="ff1234f6" bold="false" font="tahoma">
        %artist% '('%album%')' - %title%
    </label>
    <label position="right" color="ff000000">
        %_time_elapsed%/%_length%
    </label>
</contents>
```

#### scrolling-text
This layer is almost the same as `text` layer, except for the following:

- It can only have one label.
- It does not resize the skin.
- It has two more attributes: `speed` which specifies the speed of scrolling in pixels per second (default value is 25) and `pause` which specifies the delay in ms till the scrolling continue when the text reaches either of it's edges (default is 1000).
- Label `position` attribute has a different meaning and possible values: it is used to specify text alignment, when it's shorter than layer size:

```
label position::== center | right | [left]
```

Example:

```xml
<contents spacing="20" font="Verdana" size="8" bold="true" speed="20" pause="2000">
    <defaultText>foobar2000</defaultText>
    <label position="left" color="ff000000">%title%</label>
</contents>
```

### Interactive Elements
#### button
This layer creates a clickable button. It's sub-element `action` defines the action to execute. The action supports multiple types of actions, which selected using the `type` attribute.

The sub-elements `normalImg`, `overImg`, `downImg` define which images will be used for the button in each of the three states.

There can be more than one action element. They are all executed in the order they appear in the xml file.
    
```xml
<contents>
    <normalImg src="[path to the image of the button in normal state]" />
    <overImg src="[path to the image of the button in hover state]" />
    <downImg src="[path to the image of the button in pressed state]" />
    <action type="[action type]" button="[button that triggers action]" scroll="[scroll that triggers action]">
        <!-- action content -->
    </action>
    <!-- more <action> elements -->
</contents>
```

```
type::=   mainmenu | contextmenu | toggle | [legacy]
button::= left | left_doubleclick | right | right_doubleclick | middle | back | forward | none | [all]
scroll::= up | down | [none]
```

`normalImg`, `overImg`, `downImg` are optional and nothing will be drawn if they are not specified.
Only a single attribute from `button` and `scroll` can contain values different from `none` at the same time.

#### mainmenu
Executes a main menu command. The path to the menu item is entered using `/` (slash) as a separator. For example the following will set the playback order to random:

```xml
<action type="menu">
    Playback/Order/Random
</action>
```

It's adviced to enter the full path to the command to avoid mismatches with a command that have a similar name.
E.g. a command with a name `5` can be matched both to a command `Rate/5` and to a command `Layout/5`.

#### legacy

Work exactly the same as the ```mainmenu``` element.

#### contextmenu

Executes a context menu command. The path to the menu item is entered using `/` (slash) as a separator. The target of this command is defined by the `context` attribute.

```xml
<action type="contextmenu" context="[context type]">
    <!-- context menu item path -->
</action>
```

```
context ::= nowplaying | playlist
```

For example the following will show properties of the track that is currently being played:

```xml
<action type="contextmenu" context="nowplaying">
    Properties
</action>
```

#### toggle

Toggles a layer (including its sublayers) on or off. Disabled layer is not drawn, updated, nor does it react to input. The `target` attribute contains the name of the affected layer. 

```xml
<action type="toggle" only="[toggle type]" target="[layer name]"/>
```

```
only ::= enable | disable | [toggle]
```

To enable/disable more layers at once, simply include multiple `action` tags in the button.

## Examples
See skins supplied with component and user-created skins: [Skin Showcase](skin_showcase.md)

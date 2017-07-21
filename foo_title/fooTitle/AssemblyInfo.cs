/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("fooTitle")]
[assembly: AssemblyDescription("The main module of foo_title")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]		

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("0.9.0.*")]
[assembly: AssemblyInformationalVersion("0.9.0")]

/* Changelog:
   0.9.0+
     - See: https://github.com/LepkoQQ/foo_title

   0.8.1
     - compatibility with foobar2000 1.1
     - all files are now in the user profile directory as there is no reliable way to do it otherwise
 
   0.8
     - two new skins ;)
     - buttons can enable/disable parts of the skin
     - buttons can use context menu commands 
     - images are no longer locked - simplifies skin development
     - option to show foo_title when playback is stopped/paused (when not set to show always)
     - fixed show n seconds after song start when changing song
     - re-loading skin just by clicking Apply skin
     - rotated text
     - foo_managedWrapper supports creating mainmenu popups
     - foo_managedWrapper supports enumerating mainmenu commands, mainmenu groups and popups
     - fixed a problem that foo_title would not show when it is set to only show when foobar is minimized
     - on song start showing now takes stream title change into account
     - metadb_handle handling is probably done better this time
     - album art image is resized once and cached, this avoids high CPU usage with large album arts
 
   0.7.1
     - auto restoring on top position every now and then ;)
     - foo_managedWrapper works on Vista again
     - foo_title no longer prevents Windows from shutting down
     - skins are now also loaded from the user profile directory (if enabled in foobar)
  
   0.7
     - fixed disappearing fades
     - added support for choosing always on top/normal/on desktop z-order
     - added support for creating menu commands from .NET code
     - album art image file is no longer locked, it is loaded into memory
     - created a custom configuration system
     - foo_title can now pop up at the beginning and/or the end of a song
     - pressing Alt-F4 on foo_title will disable it instead of crashing
     - sticking to screen borders now permits pushing it behind the edge (this should make it possible to put foo_title on a different monitor)
     - reset button in the pref page - resets everything, including window position
 
   0.6
     - support for bold/italic text, textlayer rewritten 
     - fixed a bug that prevented foo_title from loading when foobar wasn't started from it's directory
       (fooManagedWrapper)
     - added window position storing
     - fixed album art not showing when stored in a directory with unicode characters
     - skins list refreshes when property page is displayed
     - fixed fill-images overlapping it's client rect
     - removed icon from alt-tab
     - option to show/hide/only when foobar is minimized
     - using pure WinAPI to make it stay on top (not sure if it hels though)
     - the text is now updating also on stop and pause
     - support for defaultText - shown when nothing is playing at the moment
     - now it's possible to set the align of scrolling-text when the text is shorter than layer's size
     - support for changing opacity - in normal and mouseover state
     - fixed button not going to normal state when mouse leaves foo_title
     - other bugfixes
 
*/

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]

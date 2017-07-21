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
using System;
using System.Collections.Generic;
using System.Text;

using fooManagedWrapper;
using naid;

namespace fooTitle {

    public class ContextMenuUtils {

        /// <summary>
        /// Tries to find a contextmenu command given by its default path. Use
        /// forward slashes '/' to separate categories. Supports also dynamically
        /// generated menu items.
        /// </summary>
        public static bool FindContextCommandByDefaultPath(
            string path, Context context, out CContextMenuItem commands, out Guid id, out uint index,
            out bool dynamic) {

            foreach (CContextMenuItem cmds in new CContextMenuItemEnumerator()) {
                for (uint i = 0; i < cmds.Count; i++) {
                    
                    string currentPath = cmds.GetName(i);
                    if (!String.IsNullOrEmpty(cmds.GetDefaultPath(i))) {
                        currentPath = cmds.GetDefaultPath(i) + '/' + currentPath;
                    }

                    if (path == currentPath) {
                        id = new Guid();
                        index = i;
                        dynamic = false;
                        commands = cmds;
                        return true;

                    }

                    if (path.StartsWith(currentPath, StringComparison.OrdinalIgnoreCase)) {
                        string rest;
                        if (path.Length > currentPath.Length) {
                            rest = path.Substring(currentPath.Length + 1);
                        } else {
                            rest = "";
                        }

                        Guid? cmdId = cmds.FindDynamicCommand(i, rest, context);
                        if (cmdId.HasValue) {
                            id = cmdId.Value;
                            commands = cmds;
                            index = i;
                            dynamic = true;
                            return true;
                        }
                    }
                }
            }

            commands = null;
            id = new Guid();
            index = 0;
            dynamic = false;
            return false;
        }
    }
}

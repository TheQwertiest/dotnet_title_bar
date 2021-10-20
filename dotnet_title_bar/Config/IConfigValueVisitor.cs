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

namespace fooTitle.Config
{
    /// <summary>
    /// By implementing this interface and calling the ReadVisit and WriteVisit methods
    /// on the ConfValue class users can distinguish between different types even when they
    /// only have a base ConfValue reference.
    /// </summary>
    public interface IConfigValueVisitor
    {
        /// <summary>
        /// Used to read a value from the ConfInt instance into the visitor
        /// </summary>
        void ReadInt(ConfInt val);
        /// <summary>
        /// Used to read a value from the ConfString instance
        /// </summary>
        void ReadString(ConfString val);


        /// <summary>
        /// Used to store an int to an instance of ConfInt
        /// </summary>
        void WriteInt(ConfInt val);

        /// <summary>
        /// Used to store a string to an instance of ConfString
        /// </summary>
        void WriteString(ConfString val);
    }
}
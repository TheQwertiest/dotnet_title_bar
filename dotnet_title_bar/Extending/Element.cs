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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace fooTitle.Extending
{
    public class Element
    {
        public static XElement GetFirstChildByName(XElement where, string name)
        {
            var element = GetFirstChildByNameOrNull(where, name);
            if (element == null)
            {
                throw new XmlException($"Node {name} not found under {where.Name}");
            }

            return element;
        }

        public static XElement GetFirstChildByNameOrNull(XElement where, string name)
        {
            return
                where.Elements().FirstOrDefault(i => i.Name == name);
        }

        public static string GetNodeValue(XElement a, bool trim = true)
        {
            return trim ? a.Value.Trim(' ', '\n', '\r', '\t') : a.Value;
        }

        public static string GetAttributeValue(XElement where, string name, string def)
        {
            if (where.Attribute(name) != null)
            {
                return where.Attribute(name).Value;
            }

            return def;
        }

        public static T GetCastedAttributeValue<T>(XElement where, string name, T def)
        {
            if (where.Attribute(name) == null)
            {
                return def;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(where.Attribute(name).Value);
        }

        /// <summary>
        /// If the attribute contains an expression, it is evaluated and the result is returned.
        /// If the attribute is a number, the number is returned
        /// </summary>
        /// <param name="where">the node</param>
        /// <param name="name">name of the attribute</param>
        /// <param name="def">if the attribute doesn't exist, def is read</param>
        /// <returns>Evaluated expression or number (depending on what's in the attribute)</returns>
        public static float GetNumberFromAttribute(XElement where, string name, float def)
        {
            if (where.Attribute(name) == null)
            {
                return def;
            }

            string val = where.Attribute(name).Value;
            try
            {
                if (IsExpression(val))
                {
                    var tf = Main.Get().Fb2kControls.TitleFormat(val);
                    return float.Parse(tf.Eval(force: true));
                }
                else
                {
                    // just a plain number
                    return float.Parse(val, NumberFormatInfo.InvariantInfo);
                }
            }
            catch (Exception e)
            {
                Console.Get().LogWarning(e.ToString());
                Console.Get().LogWarning(val);
                return def;
            }
        }

        /// <summary>
        /// Reads an expression from XML node attribute. If the node contains no expression, returns null
        /// (to indicate that it's not an expression and no evaluation should be done)
        /// </summary>
        /// <param name="where">The node to extract the attribute from</param>
        /// <param name="name">The name of the attribute</param>
        /// <returns>the expression if it's present or null if there's just a string or a number</returns>
        public static string GetExpressionFromAttribute(XNode where, string name)
        {
            string val = GetAttributeValue((XElement)where, name, "");
            if (val == null || !IsExpression(val))
            {
                return null;
            }

            return val;
        }

        /// <summary>
        /// Calculates the expression using the currently playing song. If the expression is null, returns the default valued
        /// </summary>
        /// <param name="expr">the expression to evaluate</param>
        /// <param name="def">the default value which is evaluated if expr is null</param>
        /// <returns>An integer result</returns>
        public static int GetValueFromExpression(string expr, int def)
        {
            if (expr == null)
            {
                return def;
            }

            try
            {
                var tf = Main.Get().Fb2kControls.TitleFormat(expr);
                return int.Parse(tf.Eval(force: true));
            }
            catch (Exception)
            {
                return def;
            }
        }

        public static int GetScaledValueFromExpression(string expr, int def)
        {
            // TODO: replace this vague function with a more proper one
            if (expr == null)
            {
                return def;
            }

            try
            {
                var tf = Main.Get().Fb2kControls.TitleFormat(expr);
                return DpiHandler.ScaleValueByDpi(int.Parse(tf.Eval(force: true)));
            }
            catch (Exception)
            {
                return def;
            }
        }

        public static string GetStringFromExpression(string expr, string def)
        {
            if (expr == null)
            {
                return def;
            }

            if (!IsExpression(expr))
            {
                return expr;
            }

            try
            {
                var tf = Main.Get().Fb2kControls.TitleFormat(expr);
                return tf.Eval(force: true);
            }
            catch (Exception)
            {
                return def;
            }
        }

        public static bool IsExpression(string expr)
        {
            return (expr.IndexOfAny(new[] { '%', '$' }) != -1);
        }
    }

}

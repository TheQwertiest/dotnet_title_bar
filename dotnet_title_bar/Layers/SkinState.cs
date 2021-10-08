/*
*  This file is part of foo_title.
*  Copyright 2017 TheQwertiest (https://github.com/TheQwertiest/foo_title)
*  
*  This library is free software; you can redistribute it and/or
*  modify it under the terms of the GNU Lesser General Public
*  License as published by the Free Software Foundation; either
*  version 2.1 of the License, or (at your option) any later version.
*  
*  This library is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
*  
*  See the file COPYING included with this distribution for more
*  information.
*/

using fooTitle.Config;
using System.Linq;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    public class SkinState
    {
        private static readonly ConfXml _skinState = Configs.Skin_State;

        public static void ResetState()
        {
            _skinState.Reset();
        }

        public static void LoadState(Skin skin)
        {
            LoadStateInternal(skin, _skinState.Value);
        }

        private static void LoadStateInternal(Layer layer, XElement curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XElement iNode = curNode.Elements("layer").FirstOrDefault(el => el.Attribute("name") != null
                                                                                && el.Attribute("name").Value == i.Name);

                if (i.IsPersistent && iNode?.Attribute("enabled") != null)
                {
                    i.Enabled = bool.Parse(iNode.Attribute("enabled").Value);
                }

                LoadStateInternal(i, iNode);
            }
        }

        public static void SaveState(Skin skin)
        {
            XElement newNode = new XElement("skin");
            SaveStateInternal(skin, newNode);

            _skinState.Reset();
            _skinState.Value = newNode;

            if (!Properties.IsOpen)
            { // Config should not be written when preferences page is open, since user might not apply changes
                ConfValuesManager.GetInstance().SaveTo(Main.Get().Config);
            }
        }

        private static void SaveStateInternal(Layer layer, XElement curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XElement newNode = new XElement("layer");
                newNode.SetAttributeValue("name", i.Name);
                if (i.IsPersistent)
                {
                    newNode.SetAttributeValue("enabled", i.Enabled.ToString());
                }

                SaveStateInternal(i, newNode);

                curNode.Add(newNode);
            }
        }

        public static bool IsStateValid(Skin skin)
        {
            return _skinState.Value != null && IsStateValidInternal(skin, _skinState.Value);
        }

        private static bool IsStateValidInternal(Layer layer, XElement curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XElement iNode = curNode.Elements("layer").FirstOrDefault(el => el.Attribute("name") != null
                                                                                && el.Attribute("name").Value == i.Name);
                if (iNode == null || !IsStateValidInternal(i, iNode))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

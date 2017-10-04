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

using System.Xml;
using fooTitle.Config;

namespace fooTitle.Layers
{
    public class SkinState
    {
        private readonly XmlDocument _doc = new XmlDocument();
        private XmlNode State
        {
            set => _skinState.Value = value;
            get => _skinState.Value;
        }

        private readonly ConfXml _skinState = new ConfXml("skin/state");

        public void ResetState()
        {
            _skinState.Reset();
        }

        public void LoadState(Skin skin)
        {
            LoadStateInternal(skin, State);
        }

        private static void LoadStateInternal(Layer layer, XmlNode curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XmlNode iNode = curNode.SelectSingleNode("//layer[@name='" + i.Name + "']");

                if (i.IsPersistent && iNode?.Attributes?.GetNamedItem("enabled") != null)
                    i.Enabled = bool.Parse(iNode.Attributes.GetNamedItem("enabled").Value);

                LoadStateInternal(i, iNode);
            }
        }

        public void SaveState(Skin skin)
        {
            XmlElement newNode = _doc.CreateElement("skin");
            SaveStateInternal(skin, newNode);

            _skinState.Reset();
            State = newNode;

            ConfValuesManager.GetInstance().SaveTo(Main.GetInstance().Config);
        }

        private void SaveStateInternal(Layer layer, XmlNode curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XmlElement newNode = _doc.CreateElement("layer");
                newNode.SetAttribute("name", i.Name);
                if (i.IsPersistent)
                {
                    newNode.SetAttribute("enabled", i.Enabled.ToString());
                }

                SaveStateInternal(i, newNode);

                curNode.AppendChild(newNode);
            }
        }

        public bool IsStateValid(Skin skin)
        {
            return State != null && IsStateValidInternal(skin, State);
        }

        private static bool IsStateValidInternal(Layer layer, XmlNode curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XmlNode iNode = curNode.SelectSingleNode("//layer[@name='" + i.Name + "']");
                if (iNode == null || !IsStateValidInternal(i, iNode))
                    return false;
            }

            return true;
        }
    }
}
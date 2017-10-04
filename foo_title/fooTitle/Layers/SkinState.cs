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

using System.Linq;
using System.Xml.Linq;
using fooTitle.Config;

namespace fooTitle.Layers
{
    public class SkinState
    {
        private readonly XDocument _doc = new XDocument();
        private XElement State
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

        private static void LoadStateInternal(Layer layer, XElement curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XElement iNode = curNode.Elements("layer").FirstOrDefault(el => el.Attribute("name") != null
                                                                                && el.Attribute("name").Value == i.Name);

                if (i.IsPersistent && iNode?.Attribute("enabled") != null)
                    i.Enabled = bool.Parse(iNode.Attribute("enabled").Value);

                LoadStateInternal(i, iNode);
            }
        }

        public void SaveState(Skin skin)
        {
            XElement newNode = new XElement("skin");
            SaveStateInternal(skin, newNode);

            _skinState.Reset();
            State = newNode;

            ConfValuesManager.GetInstance().SaveTo(Main.GetInstance().Config);
        }

        private void SaveStateInternal(Layer layer, XElement curNode)
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

        public bool IsStateValid(Skin skin)
        {
            return State != null && IsStateValidInternal(skin, State);
        }

        private static bool IsStateValidInternal(Layer layer, XElement curNode)
        {
            foreach (Layer i in layer.SubLayers)
            {
                XElement iNode = curNode.Elements("layer").FirstOrDefault(el => el.Attribute("name") != null 
                                                                                && el.Attribute("name").Value == i.Name);
                if (iNode == null || !IsStateValidInternal(i, iNode))
                    return false;
            }

            return true;
        }
    }
}
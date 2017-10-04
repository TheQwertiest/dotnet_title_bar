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
using System.Xml.Linq;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using fooTitle.Geometries;
using System.Collections.Generic;


namespace fooTitle.Layers {
    public interface IContiniousRedraw
    {
        bool IsRedrawNeeded();
    }

    /// <summary>
    /// Layers exist in a hierarchical structure
    /// </summary>
    [LayerTypeAttribute("empty")]
    public class Layer : fooTitle.Extending.Element {
        public string Name { get; }

        public string Type { get; }

        public bool IsPersistent { get; }

        public Point Position => geometry.GetPosition();

        public Rectangle ClientRect => geometry.ClientRect;
        protected Display Display { get; set; }

        private bool _enabled = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                if (!value)
                {
                    OnLayerDisable();
                }
                else
                {
                    OnLayerEnable();
                }
            }
        }
        private readonly bool _clipEnabled = true;

        public IList<Layer> SubLayers => layers;

        protected Geometry geometry;

        protected ArrayList images = new ArrayList();
        protected List<Layer> layers = new List<Layer>();

        public Layer ParentLayer;

        public bool IsMouseOver { get; private set; } = false;

        /// <summary>
        /// Indicates if layer is used only for positioning
        /// </summary>
        public bool HasContent { get; } = false;

        public virtual bool HasToolTip { get; } = false;
        public string ToolTipText { get; }

        public Layer(Rectangle parentRect, XElement node) {
            // read name and type
            Name = node.Attribute("name").Value;
            Type = node.Attribute("type").Value;
            IsPersistent = GetCastedAttributeValue<bool>(node, "persistent", "false");
            _enabled = GetCastedAttributeValue<bool>(node, "enabled", "true");
            _clipEnabled = GetCastedAttributeValue<bool>(node, "clip", "true");
            HasContent = GetFirstChildByNameOrNull(node, "contents") != null;

            // create the geometry
            XElement geomNode = GetFirstChildByName(node, "geometry");
            string geomType = geomNode.Attribute("type").Value;
            geometry = Main.GetInstance().GeometryFactory.CreateGeometry(geomType, parentRect, geomNode);
            if (geometry.IsDynamic())
                Main.GetInstance().CurrentSkin.DynamicLayers.Add(this);

            UpdateGeometry(parentRect);

            // tooltip
            bool hasDynamicToolTip = false;
            ToolTipText = GetAttributeValue(node, "tooltip", null);
            if (ToolTipText != null)
            {
                HasToolTip = true;
                hasDynamicToolTip = IsExpression(ToolTipText);
            }

            Main.GetInstance().CurrentSkin.OnMouseMove += OnMouseMove;
            Main.GetInstance().CurrentSkin.OnMouseLeave += OnMouseLeave;
            if (HasToolTip && hasDynamicToolTip)
            {
                Main.GetInstance().CurrentSkin.OnPlaybackTimeEvent += OnPlaybackTime;
            }
        }

        protected Layer() { }

        private void OnPlaybackTime(double time)
        {
            if (HasToolTip && IsMouseOver)
            {
                Main.GetInstance().CurrentSkin.ToolTip.UpdateToolTip(this, ToolTipText);
            }
        }

        private void OnMouseLeave(object sender, EventArgs e) {
            IsMouseOver = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            IsMouseOver = ClientRect.Contains(e.X, e.Y);
        }

        protected virtual void OnLayerEnable()
        {
            
        }

        protected virtual void OnLayerDisable()
        {

        }

        protected virtual void LoadLayers(XElement node) {
            foreach (XElement i in node.Elements()) {
                if (i.Name == "layer")
                    AddLayer(i);
            }
        }

        private void AddLayer(XElement node) {
            string type = node.Attribute("type").Value;

            Layer layer = Main.GetInstance().LayerFactory.CreateLayer(type, this.ClientRect, node);

            if (layer != null) {
                layer.Display = this.Display;
                layer.ParentLayer = this;
                layer.LoadLayers(node);
                layers.Add(layer);
            }
        }

        public virtual void UpdateGeometry(Rectangle parentRect) {
            geometry.Update(parentRect);

            // now update all sub-layers
            foreach (Layer i in layers) {
                i.UpdateGeometry(ClientRect);
            }
        }

        public void UpdateThisLayerGeometry(Rectangle parentRect) {
            geometry.Update(parentRect);
        }

        protected void DrawSubLayers() {
            foreach (Layer i in layers) {
                i.Draw();
            }
        }

        /// <summary>
        /// Draws the layer.
        /// </summary>
		public void Draw() {
            if (!Enabled)
                return;

            // Prevent layers from drawing outside their clientRect
            RectangleF prevClipRegion = Display.Canvas.ClipBounds;
            if (_clipEnabled)
            {
                Display.Canvas.SetClip(RectangleF.Intersect(prevClipRegion, ClientRect));
            }

            DrawImpl();
            DrawSubLayers();

            Display.Canvas.SetClip(prevClipRegion);            
        }

        /// <summary>
        /// Subclasses of Layer should override this method to perform any drawing.
        /// </summary>
        protected virtual void DrawImpl() { }

        /// <summary>
        /// This function calculates the minimal width for itself and sublayers to fit into.
        /// </summary>
        /// <returns>Returns the minimal width required to fit the sublayers and self's content</returns>
		public Size GetMinimalSize()
        {
            return Enabled ? GetMinimalSizeImpl() : new Size(0, 0);
        }

        protected virtual Size GetMinimalSizeImpl() {
            return geometry.GetMinimalSize(GetContentSize());
        }

        /// <summary>
        /// This calculates the size of this's content. Does not take geometry into account.
        /// </summary>
        protected Size GetContentSize() {
            Size minSize = new Size(0, 0);
            foreach (Layer i in layers) {
                Size layerSize = i.GetMinimalSize();
                if (i.Position.X + layerSize.Width > minSize.Width)
                    minSize.Width = i.Position.X + layerSize.Width;
                if (i.Position.Y + layerSize.Height > minSize.Height)
                    minSize.Height = i.Position.Y + layerSize.Height;
            }
            return minSize;
        }
    }
}

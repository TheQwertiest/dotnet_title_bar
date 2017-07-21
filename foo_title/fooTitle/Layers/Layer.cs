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
using System.Xml;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using fooTitle.Geometries;
using System.Collections.Generic;


namespace fooTitle.Layers {
    /// <summary>
    /// Layers exist in a hiaerchical structure
    /// </summary>
    [LayerTypeAttribute("empty")]
    public class Layer : fooTitle.Extending.Element {
        private string myName;
        public string Name {
            get {
                return myName;
            }
        }

        private string myType;
        public string Type {
            get {
                return myType;
            }
        }

        public Point Position {
            get {
                return geometry.GetPosition();
            }
        }

        public virtual System.Drawing.Rectangle ClientRect {
            get {
                return geometry.ClientRect;
            }
        }
        protected Display display;
        protected Display Display {
            get {
                return display;
            }
            set {
                display = value;
            }
        }

        private bool enabled = true;
        public bool Enabled {
            get {
                return enabled;
            }
            set {
                enabled = value;
            }
        }

        public ICollection<Layer> SubLayers {
            get {
                return layers;
            }
        }

        protected Geometry geometry;

        protected ArrayList images = new ArrayList();
        protected List<Layer> layers = new List<Layer>();

        public Layer ParentLayer;

        private bool isTopToolTipLayer = false;

        private bool isMouseOver = false;
        public bool IsMouseOver { get { return isMouseOver; } }

        private bool hasToolTip = false;
        public bool HasToolTip { get { return hasToolTip; } }

        Timer toolTipTimer;
        string toolTipText;
        bool isTooltipShowing = false;

        private void delayToolTip() {
            if (toolTipTimer != null) {
                toolTipTimer.Start();
            }
        }

        private void showToolTip() {
            if (toolTipTimer != null) {
                toolTipTimer.Stop();
            }
            Main.GetInstance().ttd.SetText(Extending.Element.GetStringFromExpression(toolTipText, null));
            Main.GetInstance().ttd.SetWindowsPos(Win32.WindowPosition.Topmost);
            Main.GetInstance().ttd.Show();
            isTooltipShowing = true;
        }

        private void updateToolTip() {
            if (isTooltipShowing) {
                Main.GetInstance().ttd.SetText(Extending.Element.GetStringFromExpression(toolTipText, null));
            }
        }

        private void removeToolTip(bool force) {
            if (toolTipTimer != null) {
                toolTipTimer.Stop();
            }
            if (isTooltipShowing) {
                Main.GetInstance().ttd.Hide();
                isTooltipShowing = false;
            }
        }

        private void toolTipTimer_OnTick(object sender, EventArgs e) {
            showToolTip();
        }

        public Layer(Rectangle parentRect, XmlNode node) {
            XmlNode contents = GetFirstChildByName(node, "contents");
            toolTipText = GetAttributeValue(contents, "tooltip", null);
            if (toolTipText != null) {
                hasToolTip = true;
                toolTipTimer = new Timer();
                toolTipTimer.Interval = 500;
                toolTipTimer.Tick += new EventHandler(toolTipTimer_OnTick);
            }

            // read name and type
            myName = node.Attributes.GetNamedItem("name").Value;
            myType = node.Attributes.GetNamedItem("type").Value;
            Enabled = GetAttributeValue(node, "enabled", "true").ToLowerInvariant() == "true";

            // create the geometry
            foreach (XmlNode child in node.ChildNodes) {
                if (child.Name == "geometry") {
                    string geomType = child.Attributes.GetNamedItem("type").Value;
                    geometry = Main.GetInstance().GeometryFactory.CreateGeometry(geomType, parentRect, child);
                    if (geometry.IsDynamic())
                        Main.GetInstance().CurrentSkin.DynamicLayers.Add(this);
                    break;
                }
            }

            UpdateGeometry(parentRect);

            Main.GetInstance().CurrentSkin.OnPlaybackTimeEvent += new OnPlaybackTimeDelegate(OnPlaybackTime);
            Main.GetInstance().CurrentSkin.OnMouseMove += new MouseEventHandler(OnMouseMove);
            Main.GetInstance().CurrentSkin.OnMouseLeave += new EventHandler(OnMouseLeave);
        }

        protected Layer() { }

        private void OnPlaybackTime(double time) {
            if (isTooltipShowing) {
                updateToolTip();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e) {
            isMouseOver = false;
            removeToolTip(true);
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            bool wasMouseOver = isMouseOver;
            isMouseOver = (e.X >= ClientRect.Left) && (e.X <= ClientRect.Right) && (e.Y >= ClientRect.Top) && (e.Y <= ClientRect.Bottom);

            bool wasTopToolTipLayer = isTopToolTipLayer;
            isTopToolTipLayer = Main.GetInstance().CurrentSkin.TopToolTipLayer == this;

            if (!wasTopToolTipLayer && isTopToolTipLayer) {
                delayToolTip();
            }
            if (wasTopToolTipLayer && !isTopToolTipLayer) {
                removeToolTip(false);
            }
        }

        protected virtual void loadLayers(XmlNode node) {
            foreach (XmlNode i in node.ChildNodes) {
                if (i.Name == "layer")
                    addLayer(i);
            }
        }

        private void addLayer(XmlNode node) {
            Layer layer = null;
            string type = node.Attributes.GetNamedItem("type").InnerText;

            layer = Main.GetInstance().LayerFactory.CreateLayer(type, this.ClientRect, node);

            if (layer != null) {
                layer.Display = this.Display;
                layer.ParentLayer = this;
                layer.loadLayers(node);
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

        protected void drawSubLayers() {
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

            drawImpl();
            drawSubLayers();
        }

        /// <summary>
        /// Subclasses of Layer should override this method to perform any drawing.
        /// </summary>
        protected virtual void drawImpl() { }

        /// <summary>
        /// This function calculates the minimal width for itself and sublayers to fit into.
        /// </summary>
        /// <returns>Returns the minimal width required to fit the sublayers and self's content</returns>
		public Size GetMinimalSize() {
            if (!Enabled) {
                return new Size(0, 0);
            } else {
                return getMinimalSizeImpl();

            }
        }

        protected virtual Size getMinimalSizeImpl() {
            return geometry.GetMinimalSize(Display, getContentSize());
        }

        /// <summary>
        /// This calculates the size of this's content. Does not take geometry into account.
        /// </summary>
        protected Size getContentSize() {
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

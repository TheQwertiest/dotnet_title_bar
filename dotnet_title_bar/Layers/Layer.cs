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
using fooTitle.Geometries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace fooTitle.Layers
{
    public interface IContiniousRedraw
    {
        bool IsRedrawNeeded();
    }

    /// <summary>
    /// Layers exist in a hierarchical structure
    /// </summary>
    [LayerType("empty")]
    public class Layer : Extending.Element
    {
        public Layer ParentLayer;

        protected readonly Skin ParentSkin;
        protected Geometry ContainedGeometry;
        protected ArrayList ContainedImages = new();
        protected List<Layer> ContainedLayers = new();

        private bool _enabled = true;

        private readonly bool _clipEnabled;

        public Layer(Rectangle parentRect, XElement node, Skin skin)
        {
            ParentSkin = skin;

            // read name and type
            Name = node.Attribute("name").Value;
            Type = node.Attribute("type").Value;
            IsPersistent = GetCastedAttributeValue(node, "persistent", false);
            _enabled = GetCastedAttributeValue(node, "enabled", true);
            _clipEnabled = GetCastedAttributeValue(node, "clip", true);
            IsTooltipTransparent = GetCastedAttributeValue(node, "tooltip-transparent", false);
            HasContent = GetFirstChildByNameOrNull(node, "contents") != null;

            // create the geometry
            XElement geomNode = GetFirstChildByName(node, "geometry");
            string geomType = geomNode.Attribute("type").Value;
            ContainedGeometry = Main.Get().GeometryFactory.CreateGeometry(geomType, parentRect, geomNode);
            if (ContainedGeometry.IsDynamic())
            {
                ParentSkin.DynamicLayers.Add(this);
            }

            UpdateGeometry(parentRect);

            // tooltip
            bool hasDynamicToolTip = false;
            ToolTipText = GetAttributeValue(node, "tooltip", null);
            if (ToolTipText != null)
            {
                HasToolTip = true;
                hasDynamicToolTip = IsExpression(ToolTipText);
            }

            ParentSkin.MouseMove += OnMouseMove;
            ParentSkin.MouseLeave += OnMouseLeave;
            if (HasToolTip && hasDynamicToolTip)
            {
                ParentSkin.TrackPlaybackPositionChanged += OnPlaybackTime;
            }
        }

        protected Layer()
        {
            ParentSkin = this as Skin;
        }

        public string Name { get; }

        public string Type { get; }

        public bool IsPersistent { get; }

        public Point Position => ContainedGeometry.GetPosition();

        public Rectangle ClientRect => ContainedGeometry.ClientRect;
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

        public IList<Layer> SubLayers => ContainedLayers;

        public bool IsMouseOver { get; private set; } = false;

        public bool HasContent { get; } = false;

        public virtual bool IsTooltipTransparent { get; } = false;

        public virtual bool HasToolTip { get; } = false;
        public string ToolTipText { get; }

        /// <summary>
        /// Draws the layer.
        /// </summary>
        public void Draw(Graphics canvas)
        {
            if (!Enabled)
            {
                return;
            }

            // Prevent layers from drawing outside their clientRect
            RectangleF prevClipRegion = canvas.ClipBounds;
            if (_clipEnabled)
            {
                canvas.SetClip(RectangleF.Intersect(prevClipRegion, ClientRect));
            }

            DrawImpl(canvas);
            DrawSubLayers(canvas);

            canvas.SetClip(prevClipRegion);
        }

        /// <summary>
        /// This function calculates the minimal width for itself and sublayers to fit into.
        /// </summary>
        /// <returns>Returns the minimal width required to fit the sublayers and self's content</returns>
        public Size GetMinimalSize()
        {
            return Enabled ? GetMinimalSizeImpl() : new Size(0, 0);
        }

        public virtual void UpdateGeometry(Rectangle parentRect)
        {
            ContainedGeometry.Update(parentRect);

            // now update all sub-layers
            foreach (Layer i in ContainedLayers)
            {
                i.UpdateGeometry(ClientRect);
            }
        }

        public void UpdateThisLayerGeometry(Rectangle parentRect)
        {
            ContainedGeometry.Update(parentRect);
        }

        protected void DrawSubLayers(Graphics canvas)
        {
            foreach (Layer i in ContainedLayers)
            {
                i.Draw(canvas);
            }
        }

        /// <summary>
        /// Subclasses of Layer should override this method to perform any drawing.
        /// </summary>
        protected virtual void DrawImpl(Graphics canvas)
        {
        }

        /// <summary>
        /// This calculates the size of this's content. Does not take geometry into account.
        /// </summary>
        protected Size GetContentSize()
        {
            Size minSize = new Size(0, 0);
            foreach (Layer i in ContainedLayers)
            {
                Size layerSize = i.GetMinimalSize();
                if (i.Position.X + layerSize.Width > minSize.Width)
                {
                    minSize.Width = i.Position.X + layerSize.Width;
                }

                if (i.Position.Y + layerSize.Height > minSize.Height)
                {
                    minSize.Height = i.Position.Y + layerSize.Height;
                }
            }
            return minSize;
        }
        protected virtual Size GetMinimalSizeImpl()
        {
            return ContainedGeometry.GetMinimalSize(GetContentSize());
        }

        protected virtual void LoadLayers(XElement node)
        {
            foreach (XElement i in node.Elements())
            {
                if (i.Name == "layer")
                {
                    AddLayer(i);
                }
            }
        }

        protected virtual void OnLayerEnable()
        {
        }

        protected virtual void OnLayerDisable()
        {
        }

        private void AddLayer(XElement node)
        {
            string type = node.Attribute("type").Value;

            Layer layer = Main.Get().LayerFactory.CreateLayer(type, this.ClientRect, node, ParentSkin);
            if (layer != null)
            {
                layer.ParentLayer = this;
                layer.LoadLayers(node);
                ContainedLayers.Add(layer);
            }
        }

        private void OnPlaybackTime(double time)
        {
            if (HasToolTip && IsMouseOver)
            {
                ParentSkin.ToolTip.UpdateToolTip(this, ToolTipText);
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            IsMouseOver = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            IsMouseOver = ClientRect.Contains(e.X, e.Y);
        }
    }
}

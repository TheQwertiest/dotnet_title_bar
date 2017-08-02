using System;
using System.Drawing;
using System.Windows.Forms;
using fooTitle.Extending;
using fooTitle.Layers;

namespace fooTitle
{
    public class ToolTip
    {
        private readonly Display _display;
        private readonly Layer _parentLayer;

        private readonly object _tooltipLock = new object();
        private readonly Timer _toolTipTimer;

        private bool _wasShowCalled;
        private Layer _tooltipLayer;
        private string _tooltipText;

        public ToolTip(Display display, Layer parentLayer)
        {
            _parentLayer = parentLayer;
            _display = display;

            _toolTipTimer = new Timer { Interval = 500 };
            _toolTipTimer.Tick += ToolTipTimer_OnTick;
        }

        private Layer _curTopToolTipLayer;
        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            _curTopToolTipLayer = GetTopToolTipLayer(GetTopLayerUnderMouse(_parentLayer));

            if (!Main.GetInstance().ttd.Visible || _tooltipLayer != _curTopToolTipLayer)
            {
                Point mouse = _display.PointToScreen(new Point(e.X, e.Y));
                Main.GetInstance().ttd.Left = mouse.X;
                Main.GetInstance().ttd.Top = mouse.Y + 18;
            }
        }

        private static Layer GetTopToolTipLayer(Layer layer)
        {
            foreach (Layer i in layer.SubLayers)
            {
                if (i.IsMouseOver && i.HasToolTip)
                    return i;
            }

            foreach (Layer i in layer.SubLayers)
            {
                Layer topLayer = GetTopToolTipLayer(i);
                if (topLayer != null)
                    return topLayer;
            }

            return null;
        }

        private static Layer GetTopLayerUnderMouse(Layer parent)
        {
            foreach (Layer layer in parent.SubLayers)
            {
                if (layer.IsMouseOver)
                {
                    return GetTopLayerUnderMouse(layer);
                }
            }
            return parent;
        }

        private void ToolTipTimer_OnTick(object sender, EventArgs e)
        {
            lock (_tooltipLock)
            {
                Main.GetInstance().ttd.SetText(Element.GetStringFromExpression(_tooltipText, null));
                Main.GetInstance().ttd.SetWindowsPos(Win32.WindowPosition.Topmost);
                Main.GetInstance().ttd.Show();
            }

            _toolTipTimer.Stop();
        }

        public void ShowDelayedToolTip(Layer caller, string toolTipText)
        {
            lock (_tooltipLock)
            {
                if (!_wasShowCalled )
                {
                    _wasShowCalled = true;
                    _tooltipText = toolTipText;
                    _tooltipLayer = caller;
                    _toolTipTimer.Start();
                }
                else if (caller != _tooltipLayer && caller == _curTopToolTipLayer)
                {
                    _tooltipLayer = caller;
                    _tooltipText = toolTipText;
                    Main.GetInstance().ttd.SetText(Element.GetStringFromExpression(_tooltipText, null));
                }
            }
        }

        public void UpdateToolTip(Layer caller, string toolTipText)
        {
            if (_tooltipLayer != caller)
                return;

            lock (_tooltipLock)
            {
                if (_tooltipLayer != caller)
                    return;

                _tooltipText = toolTipText;
                Main.GetInstance().ttd.SetText(Element.GetStringFromExpression(_tooltipText, null));
            }
        }

        public void ClearToolTip()
        {
            if (!_wasShowCalled)
                return;

            lock (_tooltipLock)
            {
                if (!_wasShowCalled)
                    return;

                _wasShowCalled = false;
                _toolTipTimer.Stop();
                Main.GetInstance().ttd.Hide();
                _tooltipText = null;
                _tooltipLayer = null;
            }
        }
    }
}

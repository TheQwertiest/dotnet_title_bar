using fooTitle.Extending;
using fooTitle.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace fooTitle
{
    public class ToolTip : IDisposable
    {
        private readonly SkinForm _display;
        private readonly ToolTipForm _tooltipForm;
        private readonly Skin _skin;

        private readonly object _tooltipLock = new();
        private readonly Timer _toolTipTimer;

        private bool _wasShowCalled;
        private Layer _curTopToolTipLayer;
        private Layer _tooltipLayer;
        private string _tooltipText;

        private bool _isMouseDown = false;

        public ToolTip(ToolTipForm tooltipForm, SkinForm display, Skin skin)
        {
            _skin = skin;
            _display = display;
            _tooltipForm = tooltipForm;

            _toolTipTimer = new Timer { Interval = 500 };
            _toolTipTimer.Tick += ToolTipTimer_OnTick;
        }

        public void Dispose()
        {
            ClearToolTip();
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                return;
            }

            _curTopToolTipLayer = GetTopToolTipLayer();
            if (_curTopToolTipLayer != null)
            {
                ShowDelayedToolTip(_curTopToolTipLayer, _curTopToolTipLayer.ToolTipText);
            }
            else
            {
                ClearToolTip();
            }
        }

        public void OnMouseDown(object sender, MouseEventArgs e)
        {
            _isMouseDown = true;
            ClearToolTip();
        }

        public void OnMouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
        }

        public void ShowDelayedToolTip(Layer caller, string toolTipText)
        {
            lock (_tooltipLock)
            {
                if (!_wasShowCalled)
                {
                    if (!_tooltipForm.Visible || _tooltipLayer != _curTopToolTipLayer)
                    {
                        Win32.GetCursorPos(out var cursorPos);
                        _tooltipForm.Left = cursorPos.x;
                        _tooltipForm.Top = cursorPos.y + 18;
                    }

                    _wasShowCalled = true;
                    _tooltipText = toolTipText;
                    _tooltipLayer = caller;
                    _toolTipTimer.Start();
                }
                else if (caller != _tooltipLayer && caller == _curTopToolTipLayer)
                {
                    _tooltipLayer = caller;
                    _tooltipText = toolTipText;
                    _tooltipForm.SetText(Element.GetStringFromExpression(_tooltipText, null));
                }
            }
        }

        public void UpdateToolTip(Layer caller, string toolTipText)
        {
            if (_tooltipLayer != caller)
            {
                return;
            }

            lock (_tooltipLock)
            {
                if (_tooltipLayer != caller)
                {
                    return;
                }

                _tooltipText = toolTipText;
                _tooltipForm.SetText(Element.GetStringFromExpression(_tooltipText, null));
            }
        }

        public void ClearToolTip()
        {
            if (!_wasShowCalled)
            {
                return;
            }

            lock (_tooltipLock)
            {
                if (!_wasShowCalled)
                {
                    return;
                }

                _wasShowCalled = false;
                _toolTipTimer.Stop();
                _tooltipForm.Hide();
                _tooltipText = null;
                _tooltipLayer = null;
            }
        }
        private Layer GetTopToolTipLayer()
        {
            Layer tmp = GetTopLayerUnderMouse(_skin);
            return tmp.HasToolTip ? tmp : null;
        }

        private static Layer GetTopLayerUnderMouse(Layer parent)
        {
            for (int i = parent.SubLayers.Count - 1; i >= 0; --i)
            { // Last layer is the top one
                Layer layer = parent.SubLayers[i];
                if (!layer.IsMouseOver || layer.IsTooltipTransparent)
                {
                    continue;
                }

                Layer tmpLayer = GetTopLayerUnderMouse(layer);
                if (tmpLayer.HasContent)
                {
                    return tmpLayer;
                }
            }
            return parent;
        }

        private void ToolTipTimer_OnTick(object sender, EventArgs e)
        {
            lock (_tooltipLock)
            {
                _tooltipForm.SetText(Element.GetStringFromExpression(_tooltipText, null));
                _tooltipForm.SetWindowsPos(Win32.WindowPosition.Topmost);
                _tooltipForm.Show();
            }

            _toolTipTimer.Stop();
        }
    }
}

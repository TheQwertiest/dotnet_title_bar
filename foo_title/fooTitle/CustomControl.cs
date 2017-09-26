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

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace fooTitle
{
    namespace CustomControl
    {
        /// <summary>
        /// A group box that has ability to auto size in horizontal dimension only.
        /// </summary>
        public class HorizontalFillDockGroupBox : System.Windows.Forms.GroupBox
        {
            private int _tmpH = 0;
            private bool _isFirstResize = true;
            private bool _isScaledResize = false;
            private bool _isResizeInProgress = false;

            protected override void OnResize(EventArgs e)
            {
                if (_isResizeInProgress)
                {
                    base.OnResize(e);
                    return;
                }

                if (_isFirstResize || _isScaledResize)
                {
                    _tmpH = this.Height;
                    if (_isFirstResize)
                    {
                        this.Dock = DockStyle.Fill;
                    }
                }

                base.OnResize(e);

                if (!_isFirstResize && !_isScaledResize)
                {
                    _isResizeInProgress = true;
                    this.Height = _tmpH;
                    _isResizeInProgress = false;
                }
                _isFirstResize = false;
            }

            protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
            {
                if (factor.Width != 1F || factor.Height != 1F)
                {
                    this.Dock = DockStyle.None;
                    _isScaledResize = true;
                }

                base.ScaleControl(factor, specified);
            }
        }

        /// <summary>
        /// A list view that has ability to auto size in horizontal dimension only.
        /// </summary>
        public class HorizontalFillDockListView : System.Windows.Forms.ListView
        {
            private int _tmpH = 0;
            private bool _isFirstResize = true;
            private bool _isScaledResize = false;
            private bool _isResizeInProgress = false;

            protected override void OnResize(EventArgs e)
            {
                if (_isResizeInProgress)
                {
                    base.OnResize(e);
                    return;
                }

                if (_isFirstResize || _isScaledResize)
                {
                    _tmpH = this.Height;
                    if (_isFirstResize)
                    {
                        this.Dock = DockStyle.Fill;
                    }
                }

                base.OnResize(e);

                if (!_isFirstResize && !_isScaledResize)
                {
                    _isResizeInProgress = true;
                    this.Height = _tmpH;
                    _isResizeInProgress = false;
                }
                _isFirstResize = false;
            }

            protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
            {
                if (factor.Width != 1F || factor.Height != 1F)
                {
                    this.Dock = DockStyle.None;
                    _isScaledResize = true;
                }

                base.ScaleControl(factor, specified);
            }
        }

        /// <summary>
        /// A track bar that has ability to auto size in horizontal dimension only.
        /// </summary>
        public class HorizontalFillDockTrackBar : System.Windows.Forms.TrackBar
        {
            private int _tmpH = 0;
            private bool _isFirstResize = true;
            private bool _isScaledResize = false;
            private bool _isResizeInProgress = false;

            protected override void OnResize(EventArgs e)
            {
                if (_isResizeInProgress)
                {
                    base.OnResize(e);
                    return;
                }

                if (_isFirstResize || _isScaledResize)
                {
                    _tmpH = this.Height;
                    if (_isFirstResize)
                    {
                        this.Dock = DockStyle.Fill;
                    }
                }

                base.OnResize(e);

                if (!_isFirstResize && !_isScaledResize)
                {
                    _isResizeInProgress = true;
                    this.Height = _tmpH;
                    _isResizeInProgress = false;
                }
                _isFirstResize = false;
            }

            protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
            {
                if (factor.Width != 1F || factor.Height != 1F)
                {
                    this.Dock = DockStyle.None;
                    _isScaledResize = true;
                }

                base.ScaleControl(factor, specified);
            }
        }

        /// <summary>
        /// A check box that can wrap its text onto multiple lines as needed.
        /// </summary>
        public class WrappingCheckBox : System.Windows.Forms.CheckBox
        {
            private System.Drawing.Size _cachedSizeOfOneLineOfText = System.Drawing.Size.Empty;
            private readonly Dictionary<Size, Size> _preferredSizeHash = new Dictionary<Size, Size>(3); // typically we've got three different constraints.

            public WrappingCheckBox()
            {
                this.AutoSize = true;
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                CacheTextSize();
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                CacheTextSize();
            }

            private void CacheTextSize()
            {
                //When the text has changed, the preferredSizeHash is invalid...
                _preferredSizeHash.Clear();

                if (string.IsNullOrEmpty(this.Text))
                {
                    _cachedSizeOfOneLineOfText = System.Drawing.Size.Empty;
                }
                else
                {
                    _cachedSizeOfOneLineOfText = TextRenderer.MeasureText(this.Text, this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.WordBreak);
                }
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                Size prefSize = base.GetPreferredSize(proposedSize);
                if ((prefSize.Width > proposedSize.Width) && (!string.IsNullOrEmpty(this.Text) && !proposedSize.Width.Equals(int.MaxValue) || !proposedSize.Height.Equals(int.MaxValue)))
                {
                    // we have the possibility of wrapping... back out the single line of text
                    Size bordersAndPadding = prefSize - _cachedSizeOfOneLineOfText;
                    // add back in the text size, subtract baseprefsize.width and 3 from proposed size width so they wrap properly
                    Size newConstraints = proposedSize - bordersAndPadding - new Size(3, 0);
                    if (!_preferredSizeHash.ContainsKey(newConstraints))
                    {
                        prefSize = bordersAndPadding + TextRenderer.MeasureText(this.Text, this.Font, newConstraints, TextFormatFlags.WordBreak);
                        _preferredSizeHash[newConstraints] = prefSize;
                    }
                    else
                    {
                        prefSize = _preferredSizeHash[newConstraints];
                    }
                }
                return prefSize;
            }
        }

        public class WrappingRadioButton : System.Windows.Forms.RadioButton
        {
            System.Drawing.Size _cachedSizeOfOneLineOfText = System.Drawing.Size.Empty;
            readonly Dictionary<Size, Size> _preferredSizeHash = new Dictionary<Size, Size>(3); // typically we've got three different constraints.

            public WrappingRadioButton()
            {
                this.AutoSize = true;
            }

            protected override void OnTextChanged(EventArgs e)
            {
                base.OnTextChanged(e);
                CacheTextSize();
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                CacheTextSize();
            }

            private void CacheTextSize()
            {
                //When the text has changed, the preferredSizeHash is invalid...
                _preferredSizeHash.Clear();

                if (string.IsNullOrEmpty(this.Text))
                {
                    _cachedSizeOfOneLineOfText = System.Drawing.Size.Empty;
                }
                else
                {
                    _cachedSizeOfOneLineOfText = TextRenderer.MeasureText(this.Text, this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.WordBreak);
                }
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                Size prefSize = base.GetPreferredSize(proposedSize);
                if ((prefSize.Width > proposedSize.Width) && (!string.IsNullOrEmpty(this.Text) && !proposedSize.Width.Equals(int.MaxValue) || !proposedSize.Height.Equals(int.MaxValue)))
                {
                    // we have the possibility of wrapping... back out the single line of text
                    Size bordersAndPadding = prefSize - _cachedSizeOfOneLineOfText;
                    // add back in the text size, subtract baseprefsize.width and 3 from proposed size width so they wrap properly
                    Size newConstraints = proposedSize - bordersAndPadding - new Size(3, 0);
                    if (!_preferredSizeHash.ContainsKey(newConstraints))
                    {
                        prefSize = bordersAndPadding + TextRenderer.MeasureText(this.Text, this.Font, newConstraints, TextFormatFlags.WordBreak);
                        _preferredSizeHash[newConstraints] = prefSize;
                    }
                    else
                    {
                        prefSize = _preferredSizeHash[newConstraints];
                    }
                }
                return prefSize;
            }
        }
    }
}
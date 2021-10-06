//
// Copyright 2002 Rui Godinho Lopes <rui@ruilopes.com>
// All rights reserved.
//
// This source file(s) may be redistributed unmodified by any means
// PROVIDING they are not sold for profit without the authors expressed
// written consent, and providing that this notice and the authors name
// and all copyright notices remain intact.
//
// Any use of the software in source or binary forms, with or without
// modification, must include, in the user documentation ("About" box and
// printed documentation) and internal comments to the code, notices to
// the end user as follows:
//
// "Portions Copyright � 2002 Rui Godinho Lopes"
//
// An email letting me know that you are using it would be nice as well.
// That's not much to ask considering the amount of work that went into
// this.
//
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED. USE IT AT YOUT OWN RISK. THE AUTHOR ACCEPTS NO
// LIABILITY FOR ANY DATA DAMAGE/LOSS THAT THIS PRODUCT MAY CAUSE.
//

// Modified by Roman Plasil for foo_managedWrapper and foo_title projects


using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace fooTitle
{
    /// <para>PerPixel forms should derive from this base class</para>
    /// <author><name>Rui Godinho Lopes</name><email>rui@ruilopes.com</email></author>
    public class PerPixelAlphaForm : Form
    {
        public void SetWindowsPos(Win32.WindowPosition newPos)
        {
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOMOVE = 0x0002;
            const uint SWP_NOACTIVATE = 0x0010;
            const int HWND_TOPMOST = -1;
            const int HWND_BOTTOM = 1;
            const int HWND_NOTOPMOST = -2;

            int pos = HWND_TOPMOST;
            if (newPos == Win32.WindowPosition.Bottom)
            {
                pos = HWND_BOTTOM;
            }
            else if (newPos == Win32.WindowPosition.NoTopmost)
            {
                pos = HWND_NOTOPMOST;
            }

            Win32.SetWindowPos(Handle, (IntPtr)pos, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        /// <para>Changes the current bitmap.</para>
        public void SetBitmap(Bitmap bitmap)
        {
            SetBitmap(bitmap, 255);
        }

        /// <para>Changes the current bitmap with a custom opacity level.  Here is where all happens!</para>
        public void SetBitmap(Bitmap bitmap, byte opacity)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");
            }

            if (IsDisposed)
            {
                return;
            }

            // The idea of this is very simple,
            // 1. Create a compatible DC with screen;
            // 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
            // 3. Call the UpdateLayeredWindow.

            var screenDc = Win32.GetDC(IntPtr.Zero);
            var memDc = Win32.CreateCompatibleDC(screenDc);
            var hBitmap = IntPtr.Zero;
            var oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
                oldBitmap = Win32.SelectObject(memDc, hBitmap);

                var size = new Win32.Size(bitmap.Width, bitmap.Height);
                var pointSource = new Win32.Point(0, 0);
                var topPos = new Win32.Point(Left, Top);
                var blend = new Win32.BLENDFUNCTION
                {
                    BlendOp = Win32.AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = opacity,
                    AlphaFormat = Win32.AC_SRC_ALPHA
                };

                Win32.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);
            }
            finally
            {
                _ = Win32.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBitmap);
                    // The documentation says that we have to use the Windows.DeleteObject... but since there is no such method I use the normal DeleteObject from Win32 GDI and it's working fine without any resource leak.
                    Win32.DeleteObject(hBitmap);
                }
                Win32.DeleteDC(memDc);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW, to disable the icon in alt-tab menu
                return cp;
            }
        }
    }

}
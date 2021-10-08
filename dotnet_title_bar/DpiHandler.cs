using fooTitle;
using System;
using System.Drawing;

namespace fooTitle
{
    class DpiHandler
    {
        // Using system DPI only
        private static readonly int _currentDpi = (int)Graphics.FromHwnd(IntPtr.Zero).DpiX;
        private static readonly float _dpiScaleCoefficient = (float)_currentDpi / 96;

        public static int ScaleValueByDpi(int oldValue)
        {
            return Configs.Display_IsDpiScalingEnabled.Value ? (int)(_dpiScaleCoefficient * oldValue) : oldValue;
        }
    }
}

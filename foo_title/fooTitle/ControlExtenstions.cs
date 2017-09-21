using System.Reflection;
using System.Windows.Forms;

namespace fooTitle {
    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            PropertyInfo doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }
    }
}

using System.Windows.Forms;

namespace fooTitle
{
    /// <summary>
    /// This should work around the windows issue described at http://support.microsoft.com/kb/149501
    ///
    /// It can be easily reproduced by clicking `Apply` on the Preferences page that has a Form with TabControl
    /// </summary>
    class SafeTabControl : TabControl
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00010000; // WS_EX_CONTROLPARENT
                return cp;
            }
        }
    }
}

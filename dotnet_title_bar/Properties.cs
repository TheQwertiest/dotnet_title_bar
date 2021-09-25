using fooTitle.Config;
using Qwr.ComponentInterface;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace fooTitle
{

    class Properties : IPreferencesPage
    {
        public IPreferencesPageCallback PreferencesCallback;
        private ContainerControl _impl;

        public static PreferencesPageInfo Info
        {
            get
            {
                PreferencesPageInfo info;
                // TODO: fill properly
                info.Name = Main.ComponentName;
                info.Guid = new Guid(1414, 548, 7868, 98, 46, 78, 12, 35, 14, 47, 68);
                info.ParentGuid = Main.GetInstance().Fb2kUtils.Fb2kGuid(Fb2kGuidId.PrefPage_Display);
                info.HelpUrl = null;

                return info;
            }
        }

        [DllImport("user32")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


        public Properties()
        {
        }

        public void Initialize(IntPtr parentHandle, IPreferencesPageCallback callback)
        {
            PreferencesCallback = callback;

            _impl = new PropertiesForm(this);
            SetParent(_impl.Handle, parentHandle);
            _impl.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right |
                           AnchorStyles.Bottom);
            _impl.Show();
        }

        public void Reset()
        {
            ConfValuesManager.GetInstance().Reset();
        }

        public void Apply()
        {
            ConfValuesManager.GetInstance().SaveTo(Main.GetInstance().Config);
        }

        public PreferencesPageState State()
        {
            PreferencesPageState state = PreferencesPageState.IsResettable;
            if (ConfValuesManager.GetInstance().HasChanged())
            {
                state |= PreferencesPageState.HasChanged;
            }

            return state;
        }

        IntPtr IPreferencesPage.Handle()
        {
            return _impl.Handle;
        }

        static public bool IsOpen => PropertiesForm.IsOpen;
    }
}
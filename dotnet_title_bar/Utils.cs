using System.Collections.Generic;

namespace fooTitle
{
    internal class Utils
    {
        public static void ReportErrorWithPopup(string message)
        {
            Main.Console?.LogError(message);
            Main.GetInstance().Fb2kUtils?.ShowPopup(message, Main.ComponentNameUnderscored);
        }

        public class ExplorerLikeSort : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return Win32.StrCmpLogicalW(x, y);
            }
        }
    }
}

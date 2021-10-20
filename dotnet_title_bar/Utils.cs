using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace fooTitle
{
    public class Utils
    {
        public static void ReportErrorWithPopup(string message)
        {
            Console.Get()?.LogError(message);
            Main.Get().Fb2kUtils?.ShowPopup(message, Constants.ComponentNameUnderscored);
        }

        public static string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
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

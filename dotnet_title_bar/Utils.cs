using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace fooTitle
{
    public class Utils
    {
        public static void ReportErrorWithPopup(string message)
        {
            Console.Get()?.LogError(message);
            Main.Get().Fb2kUtils?.ShowPopup(message, Constants.ComponentNameUnderscored);
        }

        public static void ReportErrorWithPopup(Exception e)
        {
            var generatedMessage = $"{ e.Message }\n\n"
                                   + $"{e}";
            ReportErrorWithPopup(generatedMessage);
        }

        public static void ReportErrorWithPopup(string message, Exception e)
        {
            var generatedMessage = $"{message}\n"
                                   + $"{ e.Message }\n\n"
                                   + $"{e}";
            ReportErrorWithPopup(generatedMessage);
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

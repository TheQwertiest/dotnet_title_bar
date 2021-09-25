using Qwr.ComponentInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fooTitle
{
    public class ConsoleWrapper
    {
        private IConsole _console;
        private string _componentName;

        public ConsoleWrapper(IConsole console)
        {
            _console = console;
            _componentName = Main.ComponentNameUnderscored;
        }

        public void LogInfo(string message)
        {
            _console.Log($"{_componentName}:\n"
                       + $"{message}\n");
        }

        public void LogWarning(string message)
        {
            _console.Log($"{_componentName}:\n"
                       + "Warning:\n"
                       + $"{message}\n");
        }

        public void LogError(string message)
        {
            _console.Log($"{_componentName}:\n"
                       + "Error:\n"
                       + $"{message}\n");
        }
    }
}
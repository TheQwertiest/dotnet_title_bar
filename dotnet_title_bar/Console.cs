using Qwr.ComponentInterface;
using System;

namespace fooTitle
{
    public class Console
    {
        private static Console _instance;
        private readonly IConsole _console;
        private readonly string _componentName;

        private Console(IConsole console)
        {
            _console = console;
            _componentName = Constants.ComponentNameUnderscored;
        }

        public static Console Get()
        {
            if (_instance == null)
            {
                var console = Main.Get().Fb2kControls?.Console();
                if (console != null)
                {
                    _instance = new Console(console);
                }
            }

            return _instance;
        }

        public void LogInfo(string message)
        {
            LogFormattedMessage(message);
        }

        public void LogWarning(string message)
        {
            LogFormattedMessage("Warning", message);
        }

        public void LogWarning(Exception e)
        {
            LogFormattedMessage("Warning", e);
        }

        public void LogWarning(string message, Exception e)
        {
            LogFormattedMessage("Warning", message, e);
        }

        public void LogError(string message)
        {
            LogFormattedMessage("Error", message);
        }

        public void LogError(Exception e)
        {
            LogFormattedMessage("Error", e);
        }
        public void LogError(string message, Exception e)
        {
            LogFormattedMessage("Error", message, e);
        }

        private void LogFormattedMessage(string message)
        {
            if (message.Contains('\n'))
            {
                _console.Log($"{_componentName}:\n"
                         + $"  {message}\n");
            }
            else
            {
                _console.Log($"{_componentName}: {message}");
            }
        }

        private void LogFormattedMessage(string prefix, string message)
        {
            if (message.Contains('\n'))
            {
                _console.Log($"{_componentName}: {prefix}:\n"
                         + $"  {message}\n");
            }
            else
            {
                _console.Log($"{_componentName}: {prefix}: {message}");
            }
        }

        private void LogFormattedMessage(string prefix, Exception e)
        {
            _console.Log($"{_componentName}: {prefix}:\n"
                        + $"  {e.Message}\n\n"
                                        + $"{e}\n");
        }

        private void LogFormattedMessage(string prefix, string message, Exception e)
        {
            _console.Log($"{_componentName}: {prefix}:\n"
                        + $"  {message}\n"
                                         + $"  {e.Message}\n\n"
                                         + $"{e}\n");
        }
    }
}

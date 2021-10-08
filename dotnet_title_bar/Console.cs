using Qwr.ComponentInterface;

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

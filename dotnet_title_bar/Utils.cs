namespace fooTitle
{
    internal class Utils
    {
        public static void ReportErrorWithPopup(string message)
        {
            Main.Console?.LogError(message);
            System.Windows.Forms.MessageBox.Show($"{Main.ComponentNameUnderscored}:\n" + message, Main.ComponentNameUnderscored);
        }
    }
}
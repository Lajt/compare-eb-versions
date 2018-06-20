using System.Windows;
using JetBrains.Annotations;

namespace Default.EXtensions
{
    public static class MessageBoxes
    {
        public static void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [StringFormatMethod("message")]
        public static void Error(string message, params object[] args)
        {
            MessageBox.Show(string.Format(message, args), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        public static void Warning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        [StringFormatMethod("message")]
        public static void Warning(string message, params object[] args)
        {
            MessageBox.Show(string.Format(message, args), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
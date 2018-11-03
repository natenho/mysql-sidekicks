using MySqlSideKicks.Win.Repositories;
using System;
using System.Threading;
using System.Windows.Forms;

namespace MySqlSideKicks.Win
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Application.ThreadException += ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var connectionRepository = new ConnectionRepository();
            var connectionView = new ConnectionForm();
            var connectionPresenter = new ConnectionPresenter(connectionView, connectionRepository);
            
            Application.Run(connectionView);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowError(e.ExceptionObject.ToString());
        }

        private static void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowError(e.Exception.ToString());
        }

        private static void ShowError(string text)
        {
            MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

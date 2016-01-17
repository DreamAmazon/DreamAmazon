using System;
using System.Windows.Forms;
using DreamAmazon.Configs;
using DreamAmazon.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DependencyConfig.Config();

#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
        }

        private static void RunInDebugMode()
        {
            var context = CustomApplicationContext.Create(new SplashForm());
            Application.Run(context);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            var logger = ServiceLocator.Current.GetInstance<ILogger>();
            if (logger != null)
                logger.Error(e.Exception);

            CustomApplicationContext.Current.ThrowOnUI(e.Exception);
        }

        private static void RunInReleaseMode()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

            try
            {
                var context = CustomApplicationContext.Create(new SplashForm());
                Application.Run(context);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void HandleException(Exception ex)
        {
            if (ex == null) return;

            var logger = ServiceLocator.Current.GetInstance<ILogger>();
            if (logger != null)
                logger.Error(ex);

            MessageBox.Show(ex.Message, "exception", MessageBoxButtons.OK, MessageBoxIcon.Error);

            Environment.Exit(1);
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }    
    }
}

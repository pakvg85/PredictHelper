using NLog;
using System;
using System.Windows;

namespace PredictHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public Logger GlobalLogger = LogManager.GetCurrentClassLogger();

        App()
        {
        }

        //private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //    GlobalLogger.Fatal(e.Exception, "An unhandled exception occurred");
        //    //MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    e.Handled = true;
        //}
    }
}
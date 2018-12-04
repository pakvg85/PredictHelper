using NLog;
using System.Windows;

namespace PredictHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Logger GlobalLogger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            InitializeComponent();

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MainViewModel viewModel = (MainViewModel)mainGrid.DataContext;
            viewModel.ProcessException(e.Exception, Common.MessageImportance.Fatal);
            e.Handled = true;
        }
    }
}
using System.Windows;

namespace PredictHelper.MessageBoxDialog
{
    /// <summary>
    /// Interaction logic for MessageBoxDialogWindow.xaml
    /// </summary>
    public partial class MessageBoxDialogWindow : Window
    {
        public MessageBoxDialogWindow()
        {
            InitializeComponent();
            ResponseText = "";
        }
        public string ResponseText
        {
            get => ResponseTextBox.Text;
            set { ResponseTextBox.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
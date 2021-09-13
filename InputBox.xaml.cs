using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YT
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox(string title, string userDisplayMessage)
        {
            InitializeComponent();

            Title = title;
            UserDisplayLabel.Content = userDisplayMessage;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            UserInputTextBox.Focus();
        }

        public string UserInput
        {
            get { return UserInputTextBox.Text; }
        }

        private void OKDialogButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine($"New category input window closed with result UserInput={UserInput}");
            DialogResult = true;
        }
    }
}

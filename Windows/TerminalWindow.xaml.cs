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
using static System.Windows.Forms.LinkLabel;

namespace SemantiCore.Windows
{
    /// <summary>
    /// Логика взаимодействия для TerminalWindow.xaml
    /// </summary>
    public partial class TerminalWindow : Window
    {
        private TextBlock GetLine(string line, bool command = false)
        {
            var textBox = new TextBlock();
            var x = command ? " > " : DateTime.Now.ToString("HH:mm:ss") + ": ";
            textBox.Text = $"{x} {line}";
            textBox.Margin = new Thickness(0, 3, 0, 3);
            textBox.Foreground = Brushes.Black;
            return textBox;
        }


        private void Execute(string text)
        {
            switch(text)
            {
                case "exit":
                    App.Hosting.Stop();
                    break;
            }
        }

        public TerminalWindow()
        {
            InitializeComponent();
        }

        public void WriteLine(string line)
        {
            CommandsStack.Children.Add(GetLine(line));
        }

        private void SendClicked(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void Send()
        {
            var text = InputBox.Text;
            if (string.IsNullOrWhiteSpace(text))
                return;

            CommandsStack.Children.Add(GetLine(text, true));
            Execute(text);
            InputBox.Clear();
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Send();
        }
    }
}

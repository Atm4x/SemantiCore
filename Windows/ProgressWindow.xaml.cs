using Newtonsoft.Json.Linq;
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

namespace SemantiCore.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public delegate void Cancelling();
        public event Cancelling OnCancel;

        public ProgressWindow()
        {
            InitializeComponent();
        }

        public void SetMax(int max)
        {
            Progress.Maximum = max;
            Progress.Minimum = 0;
            Progress.Value = 0;
            ProgressStatus.Text = $"Индексировано: {0} из {Progress.Maximum}";
        }

        public void SetValue(int value, string filename) 
        {
            Progress.Value = value;
            ProgressFilename.Text = filename;
            ProgressStatus.Text = $"Индексировано: {value} из {Progress.Maximum}";
            if(value == Progress.Maximum)
            {
                this.Close();
                MessageBox.Show("Индексирование завершено.");
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke();
            this.Close();
        }
    }
}

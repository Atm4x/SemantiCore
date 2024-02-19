using SemantiCore.Helpers;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace SemantiCore.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            IndexingPath.Text = App.MainConfig.IndexingPath;
        }

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            App.MainConfig.IndexingPath = IndexingPath.Text;
            Config.WriteConfig(App.MainConfig);
            if(!App.ReadAllIndexed() && !string.IsNullOrWhiteSpace(IndexingPath.Text))
            {
                MessageBox.Show("Указанный каталог не найден.", "Ошибка.");
            }
            foreach (var directory in App.IndexedDirectories)
            {
                directory.Save();
            }
            Close();

            MessageBox.Show("Данные сохранены.");
        }

        private void OpenClicked(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                var result = folderBrowser.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                {
                    IndexingPath.Text = folderBrowser.SelectedPath;
                }
            }
        }
    }
}

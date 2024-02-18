﻿using SemantiCore.Helpers;
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
        }
    }
}
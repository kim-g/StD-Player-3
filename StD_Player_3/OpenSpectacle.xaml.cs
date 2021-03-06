﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для OpenSpectacle.xaml
    /// </summary>
    public partial class OpenSpectacle : Window
    {
        private string Result = null;
        List<string> Files;
        Dictionary<string, string> FilesAndNames = new Dictionary<string, string>();

        public OpenSpectacle(Window DialogParentWindow)
        {
            InitializeComponent();
            Owner = DialogParentWindow;
        }

        public static string Open(Window ParentWindow, string FilesDir)
        {
            OpenSpectacle OS = new OpenSpectacle(ParentWindow);
            if (!(Directory.Exists(FilesDir))) Directory.CreateDirectory(FilesDir);
            OS.Files = Directory.EnumerateFiles(FilesDir,"*.sdb").ToList<string>();
            foreach (string FileToAdd in OS.Files)
            {
                SQLite.MusicDB MDB = new SQLite.MusicDB(FileToAdd);
                string Key = MDB.Name;
                string FileName = System.IO.Path.GetFileName(FileToAdd);
                Key = OS.FilesAndNames.ContainsKey(Key) ? Key + $" ({FileName})" : Key;
                OS.FileList.Items.Add(Key);
                OS.FilesAndNames.Add(Key, FileName);
            }

            OS.ShowDialog();
            return OS.Result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSp();
        }

        private void LoadSp()
        {
            if (FileList.SelectedItem == null) return;

            Result = FilesAndNames[(string)FileList.SelectedItem];
            Close();
        }

        private void LoadButton_Click(object sender, MouseButtonEventArgs e)
        {
            LoadSp();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для OpenFile.xaml
    /// </summary>
    public partial class OpenFile : Window
    {
        private string Result = null;
        private string InitialDirectory;
        List<string> Files;
        Dictionary<string, string> FilesAndNames = new Dictionary<string, string>();

        public OpenFile()
        {
            InitializeComponent();
        }

        public static string Open(string FilesDir)
        {
            OpenFile OS = new OpenFile();
            OS.InitialDirectory = FilesDir;
            if (!(Directory.Exists(FilesDir))) Directory.CreateDirectory(FilesDir);
            OS.Files = Directory.EnumerateFiles(FilesDir, "*.sdb").ToList();
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

            Result = (string)FileList.SelectedItem;
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

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OD = new OpenFileDialog()
            {
                Filter = "StD Player List Database file (*.sdb) | *.sdb",
                Multiselect = false,
                Title = "Выберите спектакль",
                InitialDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, InitialDirectory)
            };

            if (OD.ShowDialog() == true)
            {
                Result = OD.FileName;
                Close();
            }
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SD = new SaveFileDialog()
            {
                Filter = "StD Player List Database file (*.sdb) | *.sdb",
                AddExtension = true,
                Title = "Создание спектакля",
                InitialDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, InitialDirectory)
            };

            if (SD.ShowDialog() == true)
            {
                Result = SD.FileName;
                Close();
            }
        }
    }
}

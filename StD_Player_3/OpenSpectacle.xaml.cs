using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для OpenSpectacle.xaml
    /// </summary>
    public partial class OpenSpectacle : Window
    {
        private string Result = null;
        private Drag WindowDrag = null;
        List<string> Files;

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
                OS.FileList.Items.Add(System.IO.Path.GetFileNameWithoutExtension(FileToAdd));
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
    }
}

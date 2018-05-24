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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point New = e.GetPosition(this);
                WindowDrag = new Drag(New);
            }
                
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (WindowDrag != null)
                WindowDrag = null;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (WindowDrag != null)
                {
                    GetCursorPos(out POINT p);
                    Point NewPos = new Point(p.X, p.Y);
                    Left = NewPos.X - WindowDrag.X;
                    Top = NewPos.Y - WindowDrag.Y;
                }
        }
    }
}

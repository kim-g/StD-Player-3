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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public SQLite.SQLiteConfig Config = new SQLite.SQLiteConfig("Config.db");

        public EditorWindow()
        {
            InitializeComponent();
            string FileName = OpenFile.Open(Config.GetConfigValue("MusicDir"));
            if (FileName == null)
                Close();
        }
    }
}

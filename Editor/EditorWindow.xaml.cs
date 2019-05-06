using System;
using System.IO;
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
        SQLite.MusicDB PlayList;

        public EditorWindow()
        {
            InitializeComponent();
            string FileName = OpenFile.Open(Config.GetConfigValue("MusicDir"));
            if (FileName == null)
            {
                Close();
                return;
            }

            PlayList = new SQLite.MusicDB(System.IO.Path.Combine(Config.GetConfigValue("MusicDir"), FileName));
            PlayListName.Text = PlayList.Name;
            PlayListComment.Text = PlayList.Comment;
        }

        private void PlayListName_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlayList.Name = PlayListName.Text;
        }

        private void PlayListComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlayList.Comment = PlayListComment.Text;
        }
    }
}

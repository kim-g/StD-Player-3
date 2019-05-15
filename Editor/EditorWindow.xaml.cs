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
using Un4seen.Bass;

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public SQLite.SQLiteConfig Config = new SQLite.SQLiteConfig("Config.db");
        SQLite.MusicDB PlayList;

        List<SQLite.MusicFile> Files;

        public EditorWindow()
        {
            InitializeComponent();
            SoundChannel.Initiate();
            for (int i = 1; i < Bass.BASS_GetDeviceInfos().Length; i++)
                SoundChannel.Initiate(i);

            string FileName = OpenFile.Open(Config.GetConfigValue("MusicDir"));
            if (FileName == null)
            {
                Close();
                return;
            }

            PlayList = new SQLite.MusicDB(System.IO.Path.Combine(Config.GetConfigValue("MusicDir"), FileName));
            PlayListName.Text = PlayList.Name;
            PlayListComment.Text = PlayList.Comment;

            Files = PlayList.GetFiles();
            foreach (SQLite.MusicFile MF in Files)
            {
                SoundFile SF = new SoundFile() { Music = MF };
                FilesPanel.Children.Add(SF);
            }
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

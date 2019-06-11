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
using Microsoft.Win32;
using System.Data;

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
        StackPanel[] DeskPanels;

        Point ClickPoint;
        object ClickObject;

        public EditorWindow()
        {
            InitializeComponent();
            DeskPanels = new StackPanel[] { Desk1Stack, Desk2Stack, Desk3Stack };

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
                SF.MouseDown += MusicFileMouseDown;
                SF.MouseMove += MusicFileMouseMove;
                SF.InformationChanged += MusicFileChanged;
                FilesPanel.Children.Add(SF);
            }

            
            int Desk = 1;
            foreach (StackPanel DeskStack in DeskPanels)
                foreach (SQLite.Track track in PlayList.GetTracks(Desk++))
                {
                    SoundTrack ST = new SoundTrack() { Track = track, Opened = false, Margin = new Thickness(0,0,0,10) };
                    DeskStack.Children.Add(ST);
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

        private void ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                AddFiles(files);
            }
        }

        /// <summary>
        /// Добавление файлов в БД и на панель
        /// </summary>
        /// <param name="files"></param>
        private void AddFiles(string[] files)
        {
            foreach (string file in files)
            {
                if (System.IO.Path.GetExtension(file) != ".mp3")
                    continue;

                SQLite.MusicFile MF;
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    string NewTitle = System.IO.Path.GetFileNameWithoutExtension(file);
                    MemoryStream ms = new MemoryStream();
                    fs.CopyTo(ms);

                    MF = SQLite.MusicFile.CreateNewRecord(PlayList, NewTitle, "",
                        false, ms);
                }

                SoundFile SF = new SoundFile() { Music = MF };
                SF.MouseDown += MusicFileMouseDown;
                SF.MouseMove += MusicFileMouseMove;
                FilesPanel.Children.Add(SF);

            }
        }

        private void Add_Image_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog OD = new OpenFileDialog()
            {
                Title = "Выберите файлы для добавления",
                Multiselect = true
            };
            if (OD.ShowDialog() == true)
                AddFiles(OD.FileNames);
        }

        private void MusicFileMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ( (new string[2] { "TimeLine", "TimeLinePosition" }).Contains(
                ((FrameworkElement)(e.OriginalSource)).Name))
                return;

            ClickPoint = e.GetPosition((IInputElement)sender);
            ClickObject = sender;
            /*SoundFile SF = (SoundFile)sender;
            DragDrop.DoDragDrop(SF, SF.Music, DragDropEffects.Copy | DragDropEffects.Move);*/
        }

        private void MusicFileMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;
            if (ClickObject == null) return;
            if ((new string[] { "TimeLine", "TimeLinePosition", "TitleBox", "CommentBox" }).Contains(
                ((FrameworkElement)(e.OriginalSource)).Name))
                return;

            Point Pos = e.GetPosition((IInputElement)sender);
            if (Math.Sqrt(Math.Pow(ClickPoint.X - Pos.X, 2) + Math.Pow(ClickPoint.Y - Pos.Y, 2)) > 20)
            {
                SoundFile SF = (SoundFile)ClickObject;
                DragDrop.DoDragDrop(SF, SF.Music, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void MusicFileChanged(object sender, EventMusicFileArgs e)
        {
            foreach (StackPanel DeskPanel in DeskPanels)
            {
                List<SoundTrack> ST_To_Change = DeskPanel.Children.OfType<SoundTrack>().Where(x => x.Track.File == e.FileID).ToList();
                foreach (SoundTrack STrack in ST_To_Change)
                    STrack.Update();
            }
        }

        private void TrackDesk_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(SQLite.MusicFile)))
            {
                ScrollViewer Desk = (ScrollViewer)sender;
                SQLite.MusicFile MF = (SQLite.MusicFile)e.Data.GetData(typeof(SQLite.MusicFile));
                int DeskN = Convert.ToInt32(Desk.Tag);

                int NewNumber = 0;
                while (PlayList.GetCount("desk", $"`number`={++NewNumber}") > 0) ;
                DataTable dt = PlayList.ReadTable($"SELECT `order` FROM `desk` WHERE `desk_n`={DeskN} ORDER BY `order` DESC LIMIT 1");
                int Order = Convert.ToInt32(dt.Rows.Count == 0 ? 1 : dt.Rows[0].ItemArray[0]) + 1;

                SQLite.Track track = SQLite.Track.Create(PlayList, DeskN, NewNumber.ToString(), "",
                    MF.ID, Order);

                SoundTrack ST = new SoundTrack()
                {
                    Track = track,
                    Opened = false,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                DeskPanels[DeskN - 1].Children.Add(ST);
            }
                
        }
    }
}

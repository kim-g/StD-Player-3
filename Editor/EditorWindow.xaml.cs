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
        DataBaseElement _SelectedElement = null;

        string[] ElementsExclude = new string[] { "TimeLine", "TimeLinePosition", "TitleBox", "CommentBox", "NumberBox" };

        public DataBaseElement SelectedElement
        {
            get => _SelectedElement;
            set
            {
                if (_SelectedElement != null)
                    _SelectedElement.Selected = false;
                _SelectedElement = value;
                _SelectedElement.Selected = true;
            }
        }

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
                SF.FileDeleted += SoundFileDeleted;
                FilesPanel.Children.Add(SF);
            }

            
            int Desk = 1;
            foreach (StackPanel DeskStack in DeskPanels)
            {
                foreach (SQLite.Track track in PlayList.GetTracks(Desk++))
                {
                    SoundTrack ST = new SoundTrack() { Track = track, Opened = false, Margin = new Thickness(0, 0, 0, 10) };
                    SoundTrackEventsAdd(ST);
                    DeskStack.Children.Add(ST);
                }
                DeskStack.SizeChanged += (object Sender, SizeChangedEventArgs e) => { SetOrder((StackPanel)Sender); };
            }
        }

        private void SoundTrackEventsAdd(SoundTrack ST)
        {
            ST.MouseDown += MusicFileMouseDown;
            ST.MouseMove += MusicFileMouseMove;
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
                SF.InformationChanged += MusicFileChanged;
                SF.FileDeleted += SoundFileDeleted;
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
            if ( (ElementsExclude).Contains(
                ((FrameworkElement)(e.OriginalSource)).Name))
                return;

            ClickPoint = e.GetPosition((IInputElement)sender);
            ClickObject = sender;
            SelectedElement = (DataBaseElement)sender;
        }

        private void MusicFileMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;
            if (ClickObject == null) return;
            if ((ElementsExclude).Contains(
                ((FrameworkElement)(e.OriginalSource)).Name))
                return;

            Point Pos = e.GetPosition((IInputElement)sender);
            if (Math.Sqrt(Math.Pow(ClickPoint.X - Pos.X, 2) + Math.Pow(ClickPoint.Y - Pos.Y, 2)) > 20)
            {
                object Object = null;
                if (ClickObject.GetType() == typeof(SoundFile))
                {
                    SoundFile SF = (SoundFile)ClickObject;
                    Object = SF.Music;
                }
                if (ClickObject.GetType() == typeof(SoundTrack))
                {
                    SoundTrack SF = (SoundTrack)ClickObject;
                    Object = SF.Track;
                }
                DragDrop.DoDragDrop((DependencyObject)ClickObject, Object, DragDropEffects.Copy | DragDropEffects.Move);
                ClickObject = null;
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
                SoundTrackEventsAdd(ST);
                DeskPanels[DeskN - 1].Children.Add(ST);
            }

            if (e.Data.GetDataPresent(typeof(SQLite.Track)))
            {
                ScrollViewer Desk = (ScrollViewer)sender;
                SQLite.Track Track = (SQLite.Track)e.Data.GetData(typeof(SQLite.Track));
                int DeskN = Convert.ToInt32(Desk.Tag);

                if (Track.Desk == DeskN)
                {
                    
                }
                else
                {
                    Track.Desk = DeskN;

                    SoundTrack STD = null;
                    foreach (StackPanel DeskStack in DeskPanels)
                    {
                        foreach (SoundTrack ST in
                            DeskStack.Children.OfType<SoundTrack>().Where(X => X.Track.ID == Track.ID))
                        {
                            STD = ST;
                        }
                    }
                    ((StackPanel)(STD.Parent)).Children.Remove(STD);

                    SoundTrack NST = new SoundTrack()
                    {
                        Track = Track,
                        Opened = false,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    SoundTrackEventsAdd(NST);

                    int Num = -1;
                    StackPanel DescContent = (StackPanel)Desk.Content;

                    for (int i = 0; i < DescContent.Children.Count; i++)
                    {
                        double ElementY = e.GetPosition(DescContent.Children[i]).Y;
                        if (ElementY > 0 - ((SoundTrack)DescContent.Children[i]).Margin.Bottom &&
                            ElementY < ((SoundTrack)DescContent.Children[i]).ActualHeight)
                        {
                            if (DescContent.Children[i].Equals(STD)) return;
                            Num = i;
                        }
                    }
                    DescContent.Children.Insert(Num == -1 ? DescContent.Children.Count : Num, NST);
                    SetOrder(DescContent);
                }
            }
        }

        private void SetOrder(ScrollViewer Desk)
        {
            int DeskN = Convert.ToInt32(Desk.Tag);
            SetOrder(DeskPanels[DeskN - 1]);
        }

        private void SetOrder(StackPanel Desk)
        {
            for (int i = 0; i < Desk.Children.Count; i++)
            {
                ((SoundTrack)(Desk.Children[i])).Track.Order = i+1;
            }
        }

        private void Desk1_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void Desk1_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void Desk1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(SQLite.Track)))
            {
                ScrollViewer Desk = (ScrollViewer)sender;
                SQLite.Track Track = (SQLite.Track)e.Data.GetData(typeof(SQLite.Track));
                int DeskN = Convert.ToInt32(Desk.Tag);

                if (Track.Desk == DeskN)
                {
                    int Num = -1;
                    StackPanel DescContent = (StackPanel)(Desk.Content);

                    SoundTrack STD = null;
                    foreach (SoundTrack ST in
                        DescContent.Children.OfType<SoundTrack>().Where(X => X.Track.ID == Track.ID))
                    {
                        STD = ST;
                    }

                    for (int i = 0; i < DescContent.Children.Count; i++)
                    {
                        double ElementY = e.GetPosition(DescContent.Children[i]).Y;
                        if (ElementY > 0 - ((SoundTrack)DescContent.Children[i]).Margin.Bottom && 
                            ElementY < ((SoundTrack)DescContent.Children[i]).ActualHeight)
                        {
                            if (DescContent.Children[i].Equals(STD)) return;
                            Num = i;
                        }
                    }

                    DescContent.Children.Remove(STD);
                    DescContent.Children.Insert(Num == -1 ? DescContent.Children.Count : Num, STD);
                    SetOrder(DescContent);
               }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if ((new string[] { "TitleBox", "CommentBox", "NumberBox" }).Contains(
                ((FrameworkElement)(e.OriginalSource)).Name))
                return;

            if (SelectedElement == null) return;
            switch (e.Key)
            {
                case Key.Space:
                    SelectedElement.Play();
                    break;
                case Key.Delete:
                    SelectedElement.Delete();
                    break;
            }
        }

        public void DeleteAllTracks(long FileID)
        {
            List<SoundTrack> ListToDelete = new List<SoundTrack>();
            foreach (StackPanel SP in DeskPanels)
            {
                ListToDelete.AddRange(SP.Children.OfType<SoundTrack>().Where<SoundTrack>(X => 
                    X.Track.File == FileID));
            }
            for (int i = 0; i < ListToDelete.Count(); i++)
                ListToDelete[i].ForceDelete();
        }

        private void SoundFileDeleted(object sender, EventMusicFileArgs e)
        {
            DeleteAllTracks(e.FileID);
        }
    }
}

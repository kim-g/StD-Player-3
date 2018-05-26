using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Un4seen.Bass;

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Desk Channel_1;
        Desk Channel_2;
        DispatcherTimer timer;
        DispatcherTimer TimeTimer;
        bool Loading = false;
        SQLite.SQLiteConfig Config = new SQLite.SQLiteConfig("Config.db");

        public MainWindow()
        {
            InitializeComponent();

            Background = new SolidColorBrush(ProjectColors.Background);

            Left = 0;
            Top = 0;
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
            TitleRect.Fill = new SolidColorBrush(ProjectColors.Gray);

            SoundChannel.Initiate();

            timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal);
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            TimeTimer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal);
            TimeTimer.Tick += new EventHandler(TimeTimerTick);
            TimeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            TimeTimer.Start();

            Channel_1 = new Desk(Desk1, -1, 100, 1);
            Channel_2 = new Desk(Desk2, 1, 100, 2);
            LoadMusic(Config.GetConfigValue("file"));
        }

        private void LoadMusic(string MusicDBFileName)
        {
            Grid LoadGrid = new Grid();
            Rectangle LoadRect = new Rectangle();
            LoadRect.Fill = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
            LoadRect.Margin = new Thickness(0);
            LoadGrid.Children.Add(LoadRect);
            Label LoadLabel = new Label();
            LoadLabel.Margin = new Thickness(0);
            LoadLabel.HorizontalAlignment = HorizontalAlignment.Center;
            LoadLabel.VerticalAlignment = VerticalAlignment.Center;
            LoadLabel.Content = "Загрузка спектакля";
            LoadGrid.Children.Add(LoadLabel);
            Grid.SetRow(LoadGrid, 0);
            Grid.SetRowSpan(LoadGrid, 3);
            Grid.SetColumn(LoadGrid, 0);
            Grid.SetColumnSpan(LoadGrid, 2);
            MainGrid.Children.Add(LoadGrid);

            Loading = true;
            Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(delegate ()
            {
                MusicDB MDB = new MusicDB(System.IO.Path.GetFullPath(System.IO.Path.Combine(
                            Config.GetConfigValue("MusicDir"),
                            System.IO.Path.ChangeExtension(MusicDBFileName, ".sdb"))));
                SpNameLabel.Content = MDB.Name;


                Channel_1.LoadTrackList(MDB.LoadDesk(Channel_1.DeskN));
                Channel_2.LoadTrackList(MDB.LoadDesk(Channel_2.DeskN));

                MainGrid.Children.Remove(LoadGrid);
                Loading = false;
            }));
        }

        private void LoadInThread(object x)
        {
            
        }

        private void timerTick(object sender, EventArgs e)
        {
            if (!Loading) Update();
        }

        private void TimeTimerTick(object sender, EventArgs e)
        {
            if (!Loading)
                TimeLabel.Content = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
        }

        private void Update()
        {
            Channel_1.UpdateVisualElements();
            Channel_2.UpdateVisualElements();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            Channel_1.Play();
            Update();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Channel_1.Pause();
            Update();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Channel_1.Stop();
            Update();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void button_Click_4(object sender, RoutedEventArgs e)
        {
            string LoadSp = OpenSpectacle.Open(this, Config.GetConfigValue("MusicDir"));
            Config.SetConfigValue("file", LoadSp);
            if (File.Exists(System.IO.Path.Combine(Config.GetConfigValue("MusicDir"), LoadSp+".sdb")))
                LoadMusic(LoadSp);

        }
    }

    class ToLoadThread
    {
        public string Name;
        public Grid ParentGrid;
        public Label SpNameLabel;

        public ToLoadThread(string name, Grid parent, Label NameLabel)
        {
            Name = name;
            ParentGrid = parent;
            SpNameLabel = NameLabel;
        }
    }
}

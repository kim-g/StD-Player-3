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
        protected double Scale;

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

            Scale = Height / 984.0; // Масштаб относительно модельного 1024 - панель задач

            TopRow.Height = new GridLength(ScaleTo(120.0));
            BottomRow.Height = new GridLength(ScaleTo(70.0));
            CloseButton.FontSize = ScaleTo(24.0);
            TimeLabel.FontSize = ScaleTo(48.0);
            SpNameLabel.FontSize = ScaleTo(60.0);

            Channel_1 = new Desk(Desk1, -1, 100, 1, Scale);
            Channel_2 = new Desk(Desk2, 1, 100, 2, Scale);
            LoadMusic(Config.GetConfigValue("file"));
        }

        /// <summary>
        /// Масштабирование значения
        /// </summary>
        /// <param name="Value">Значеие модельной позиции для масштабирования</param>
        /// <returns></returns>
        private int ScaleTo(int Value)
        {
            return Convert.ToInt32(Value * Scale);
        }

        /// <summary>
        /// Масштабирование значения
        /// </summary>
        /// <param name="Value">Значеие модельной позиции для масштабирования</param>
        /// <returns></returns>
        private double ScaleTo(double Value)
        {
            if (Value == double.NaN)
                return double.NaN;
            return Value * Scale;
        }

        private void LoadMusic(string MusicDBFileName)
        {
            Grid LoadGrid = new Grid();
            Rectangle LoadRect = new Rectangle();
            LoadRect.Fill = ProjectSolids.LoadingBackground;
            LoadRect.Margin = new Thickness(0);
            LoadGrid.Children.Add(LoadRect);
            Label LoadLabel = new Label();
            LoadLabel.Margin = new Thickness(0);
            LoadLabel.HorizontalAlignment = HorizontalAlignment.Center;
            LoadLabel.VerticalAlignment = VerticalAlignment.Center;
            LoadLabel.Content = "Загрузка спектакля";
            LoadLabel.FontSize = ScaleTo(48.0);
            LoadLabel.FontWeight = FontWeights.Bold;
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
            TimeLabel.Content = DateTime.Now.Hour.ToString("D2") + ":" + DateTime.Now.Minute.ToString("D2");
        }

        private void Update()
        {
            Channel_1.UpdateVisualElements();
            Channel_2.UpdateVisualElements();
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Loading) return;

            switch (e.Key)
            {
                case Key.S: Channel_1.Play(); Channel_1.UpdateVisualElements(); break;
                case Key.D: Channel_1.Pause(); Channel_1.UpdateVisualElements(); break;
                case Key.F: Channel_1.Stop(); Channel_1.UpdateVisualElements(); break;
                case Key.E:
                    Channel_1.Stop(); Channel_1.CurrentTrack--;
                    Channel_1.UpdateVisualElements(); break;
                case Key.X:
                    Channel_1.Stop(); Channel_1.CurrentTrack++;
                    Channel_1.UpdateVisualElements(); break;

                case Key.K: Channel_2.Play(); Channel_2.UpdateVisualElements(); break;
                case Key.L: Channel_2.Pause(); Channel_2.UpdateVisualElements(); break;
                case Key.OemSemicolon: Channel_2.Stop(); Channel_2.UpdateVisualElements(); break;
                case Key.O:
                    Channel_2.Stop(); Channel_2.CurrentTrack--;
                    Channel_2.UpdateVisualElements(); break;
                case Key.OemComma:
                    Channel_2.Stop(); Channel_2.CurrentTrack++;
                    Channel_2.UpdateVisualElements(); break;
            }
        }
    }
}

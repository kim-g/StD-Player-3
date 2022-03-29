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
using Un4seen.BassAsio;
using Extentions;
using SQLite;

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Desk[] Channels;
        DispatcherTimer timer;
        DispatcherTimer LevelsTimer;
        DispatcherTimer TimeTimer;
        bool Loading = true;
        public SQLiteConfig Config = new SQLiteConfig("Config.db");
        protected double Scale;
        public static LinearGradientBrush LevelsO;
        public static LinearGradientBrush LevelsI;
        public static int UpdateTime = 50;

        private BassAsioHandler _asio1;
        Sound.SoundBase Music;

        private byte DeskCount
        {
            get
            {
                byte Desks = 3;
                if (Channels[2].TracksCount == 0)
                {
                    if (Channels[1].TracksCount == 0)
                        Desks = 1;
                    else
                        Desks = 2;
                }

                return Desks;
            }
        }

        public MainWindow()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            if (Config.GetConfigValue("Theme") == "")
                Config.SetConfigValue("Theme", "light");
            Application.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{Config.GetConfigValue("Theme")}.xaml") });

            InitializeComponent();
            // Настройка цветов
            Left = 0;
            Top = 0;
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;

            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, UpdateTime);
            timer.Start();

            LevelsTimer = new DispatcherTimer(DispatcherPriority.Normal);
            LevelsTimer.Tick += new EventHandler(LevelsTimerTick);
            LevelsTimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            LevelsTimer.Start();

            TimeTimer = new DispatcherTimer(DispatcherPriority.Normal);
            TimeTimer.Tick += new EventHandler(TimeTimerTick);
            TimeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            TimeTimer.Start();

            Scale = (Height - 1) / 984.0;// *0.99; // Масштаб относительно модельного 1024 - панель задач



        }

        private void LevelsTimerTick(object sender, EventArgs e)
        {
            foreach (Desk Channel in Channels)
                Channel?.SetLevels();

            /*Channel_1.SetLevels();
            Channel_2.SetLevels();
            Channel_3.SetLevels();*/
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
            Rectangle LoadRect = new Rectangle() { Margin = new Thickness(0) };
            LoadRect.SetResourceReference(Rectangle.FillProperty, "LoadingBackground");
            LoadGrid.Children.Add(LoadRect);
            Label LoadLabel = new Label()
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Content = "Загрузка спектакля",
                FontSize = ScaleTo(48.0),
                FontWeight = FontWeights.Bold,
            };
            LoadLabel.SetResourceReference(Label.ForegroundProperty, "Foreground");

            LoadGrid.Children.Add(LoadLabel);
            Grid.SetRow(LoadGrid, 0);
            Grid.SetRowSpan(LoadGrid, 3);
            Grid.SetColumn(LoadGrid, 0);
            Grid.SetColumnSpan(LoadGrid, 2);
            MainGrid.Children.Add(LoadGrid);
            Thread.SpinWait(100);

            Loading = true;
            Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(delegate ()
            {
                MusicDB MDB = null;
                MDB = new MusicDB(System.IO.Path.GetFullPath(System.IO.Path.Combine(
                            Config.GetConfigValue("MusicDir"),
                            System.IO.Path.ChangeExtension(MusicDBFileName, ".sdb"))));
                SpNameLabel.Content = MDB.Name;

                LoadSDB(MDB, SpNameLabel, MusicDBFileName, Channels);

                MainGrid.Children.Remove(LoadGrid);

                Loading = false;
            }));
        }

        private void LoadSDB(MusicDB MDB, Label NameLabel, string MusicDBFileName, Desk[] Channel)
        {
            Task<List<MusicTrack>>[] LoadDesks = new Task<List<MusicTrack>>[Channels.Length];
            for (int i = 0; i < Channels.Length; i++)
            {
                int j = i;
                LoadDesks[i] = Task.Run(() => { return MDB.LoadDesk(Channel[j].DeskN); });
            }

            Task.WaitAll(LoadDesks);
            for (int i = 0; i < Channels.Length; i++)
                Channel[i].LoadTrackList(LoadDesks[i].Result);

            

            switch (DeskCount)
            {
                case 1:
                    Grid.SetColumnSpan(Desk1, 6);
                    Desk2.Visibility = Visibility.Collapsed;
                    Desk3.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    Grid.SetColumnSpan(Desk1, 3);
                    Grid.SetColumnSpan(Desk2, 3);
                    Grid.SetColumn(Desk2, 3);
                    Desk2.Visibility = Visibility.Visible;
                    Desk3.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    Grid.SetColumnSpan(Desk1, 2);
                    Grid.SetColumnSpan(Desk2, 2);
                    Grid.SetColumnSpan(Desk3, 2);
                    Grid.SetColumn(Desk2, 2);
                    Grid.SetColumn(Desk3, 4);
                    Desk2.Visibility = Visibility.Visible;
                    Desk3.Visibility = Visibility.Visible;
                    break;
            }
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
            if (Loading) return;
            foreach (Desk Channel in Channels)
            {
                Channel.UpdateVisualElements();
                Channel.DrawFriq();
            }
        }

        
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            switch (Config.GetConfigValue("Close"))
            {
                case "close": Bass.BASS_Free(); BassAsio.BASS_ASIO_Free(); Application.Current.Shutdown(); break;
                case "shutdown": System.Diagnostics.Process.Start("shutdown", "/s /f /t 00"); break;
                default: Bass.BASS_Free(); BassAsio.BASS_ASIO_Free(); Application.Current.Shutdown(); break;
            }
        }

        private void button_Click_4(object sender, RoutedEventArgs e)
        {
            string LoadSp = OpenSpectacle.Open(this, Config.GetConfigValue("MusicDir"));
            if (LoadSp == null) return;
            Config.SetConfigValue("file", LoadSp);
            if (File.Exists(System.IO.Path.Combine(Config.GetConfigValue("MusicDir"), LoadSp)))
                LoadMusic(LoadSp);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Loading) return;

            switch (e.Key)
            {
                case Key.S: Channels[0].Sound.Play(); Channels[0].UpdateVisualElements(); break;
                case Key.D: Channels[0].Sound.Pause(); Channels[0].UpdateVisualElements(); break;
                case Key.F: Channels[0].Sound.Stop(); Channels[0].UpdateVisualElements(); break;
                case Key.E:
                    Channels[0].Sound.Stop(); Channels[0].CurrentTrack--;
                    Channels[0].UpdateVisualElements(); break;
                case Key.X:
                    Channels[0].Sound.Stop(); Channels[0].CurrentTrack++;
                    Channels[0].UpdateVisualElements(); break;

                case Key.K: if (DeskCount < 2) return; Channels[1].Sound.Play(); Channels[1].UpdateVisualElements(); break;
                case Key.L: if (DeskCount < 2) return; Channels[1].Sound.Pause(); Channels[1].UpdateVisualElements(); break;
                case Key.OemSemicolon: if (DeskCount < 2) return; Channels[1].Sound.Stop(); Channels[1].UpdateVisualElements(); break;
                case Key.O:
                    if (DeskCount < 2) return;
                    Channels[1].Sound.Stop(); Channels[1].CurrentTrack--;
                    Channels[1].UpdateVisualElements(); break;
                case Key.OemComma:
                    if (DeskCount < 2) return;
                    Channels[1].Sound.Stop(); Channels[1].CurrentTrack++;
                    Channels[1].UpdateVisualElements(); break;
                case Key.Tab:
                    if (Config.GetConfigValue("Theme") == "dark")
                        Config.SetConfigValue("Theme", "light");
                    else Config.SetConfigValue("Theme", "dark");
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(
                        new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{Config.GetConfigValue("Theme")}.xaml") });
                    LevelsO = (LinearGradientBrush)TryFindResource("LevelsOut");
                    LevelsI = (LinearGradientBrush)TryFindResource("LevelsOutLight");
                    LevelsO.EndPoint = new Point(0, ScaleTo(200.0));
                    LevelsI.EndPoint = new Point(0, ScaleTo(200.0));
                    foreach (Desk Channel in Channels)
                        Channel.SetLevels(LevelsI, LevelsO);
                    break;
                case Key.Add:
                    Parameters.Set(this);
                    SetSoundCards(Channels);
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TopRow.Height = new GridLength(ScaleTo(120.0));
            BottomRow.Height = new GridLength(ScaleTo(70.0));
            LoadButton.FontSize = ScaleTo(24.0);
            LoadButton.Height = ScaleTo(46.0);
            loadImage.Width = ScaleTo(32.0);
            loadImage.Height = ScaleTo(32.0);
            CloseButton.FontSize = ScaleTo(24.0);
            CloseButton.Height = ScaleTo(46.0);
            TimeLabel.FontSize = ScaleTo(48.0);
            SpNameLabel.FontSize = ScaleTo(60.0);

            LevelsO = (LinearGradientBrush)TryFindResource("LevelsOut");
            LevelsI = (LinearGradientBrush)TryFindResource("LevelsOutLight");
            LevelsO.EndPoint = new Point(0, ScaleTo(200.0));
            LevelsI.EndPoint = new Point(0, ScaleTo(200.0));

            SoundType ST = SoundType.ASIO;

            switch (ST)
            {
                case SoundType.Standart:
                    for (int i = 1; i < Bass.BASS_GetDeviceInfos().Length; i++)
                        Sound.SoundChannel.Initiate(i);
                    break;
                case SoundType.ASIO:
                    for (int i = 1; i < BassAsio.BASS_ASIO_GetDeviceCount(); i++)
                        Sound.ASIO_Channel.Initiate(i);
                    break;
            }
            Channels = new Desk[3];

            Channels[0] = new Desk(Desk1, GetPan(1), 100, 1, Scale, -1, ST);
            Channels[1] = new Desk(Desk2, GetPan(2), 100, 2, Scale, -1, ST);
            Channels[2] = new Desk(Desk3, GetPan(2), 100, 3, Scale, -1, ST);

            SetSoundCards(Channels);

            LoadMusic(Config.GetConfigValue("file"));
        }

        /// <summary>
        /// Настройка звуковых карт для дек из конфига
        /// </summary>
        /// <param name="SoundCards">Массив звуковых карт</param>
        public void SetSoundCards(Desk[] SoundCards)
        {
            int[] Channels = new int[SoundCards.Length];
            for (int i = 0; i < Channels.Length; i++)
                Channels[i] = -1;
            BASS_DEVICEINFO[] Cards = Bass.BASS_GetDeviceInfos();
            for (int Card = 0; Card < Cards.Length; Card++)
            {
                if (Cards[Card].id == null) continue;
                for (int i = 0; i < SoundCards.Length; i++)
                    if (Cards[Card].id == Config.GetConfigValue($"Desk_{i + 1}_Sound_Card"))
                        Channels[i] = Card;
            }
            for (int i = 0; i < SoundCards.Length; i++)
                SoundCards[i].Sound.SoundCard = Channels[i];
            foreach (Desk D in SoundCards)
            {
                D.SetBalance(GetPan(D.DeskN));
                D.CurrentTrack = D.CurrentTrack;
            }
        }

        private void LoadButtonLabel_Loaded(object sender, RoutedEventArgs e)
        {
           
            
        }

        private int GetPan(byte DeskN)
        {
            switch (Config.GetConfigValue($"Desk_{DeskN}_Pan"))
            {
                case "left":
                    return -1;
                case "center":
                    return 0;
                case "right":
                    return 1;
                default:
                    return 0;
            }
        }

        private void TimeLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

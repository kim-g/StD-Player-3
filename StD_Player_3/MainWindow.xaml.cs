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
using Extentions;

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
        bool Loading = true;
        SQLite.SQLiteConfig Config = new SQLite.SQLiteConfig("Config.db");
        protected double Scale;
        public static LinearGradientBrush LevelsO;
        public static LinearGradientBrush LevelsI;

        public MainWindow()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(
                new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{Config.GetConfigValue("Theme")}.xaml") });

            InitializeComponent();
            // Настройка цветов

            Left = 0;
            Top = 0;
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;

            SoundChannel.Initiate();

            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            TimeTimer = new DispatcherTimer(DispatcherPriority.Normal);
            TimeTimer.Tick += new EventHandler(TimeTimerTick);
            TimeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            TimeTimer.Start();

            Scale = (Height-1) / 984.0*0.99; // Масштаб относительно модельного 1024 - панель задач

            TopRow.Height = new GridLength(ScaleTo(120.0));
            BottomRow.Height = new GridLength(ScaleTo(70.0));
            LoadButton.FontSize = ScaleTo(24.0);
            CloseButton.FontSize = ScaleTo(24.0);
            TimeLabel.FontSize = ScaleTo(48.0);
            SpNameLabel.FontSize = ScaleTo(60.0);

            LevelsO = (LinearGradientBrush)this.TryFindResource("LevelsOut");
            LevelsI = (LinearGradientBrush)this.TryFindResource("LevelsOutLight");
            LevelsO.EndPoint = new Point(0, ScaleTo(200.0));
            LevelsI.EndPoint = new Point(0, ScaleTo(200.0));

            Channel_1 = new Desk(Desk1, -1, 100, 1, Scale);
            Channel_2 = new Desk(Desk2, 1, 100, 2, Scale);
            
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

                LoadSDB(MDB, SpNameLabel, MusicDBFileName, new Desk[2] { Channel_1, Channel_2});

                MainGrid.Children.Remove(LoadGrid);
                Loading = false;
            }));
        }

        private void LoadSDB(MusicDB MDB, Label NameLabel, string MusicDBFileName, Desk[] Channel)
        {
            

            Task<List<MusicTrack>> LoadDesk1 = Task.Run(() => { return MDB.LoadDesk(Channel[0].DeskN); });
            Task<List<MusicTrack>> LoadDesk2 = Task.Run(() => { return MDB.LoadDesk(Channel[1].DeskN); });
            Task.WaitAll(new Task[2]
                {
                        LoadDesk1, LoadDesk2
                });
            Channel[0].LoadTrackList(LoadDesk1.Result);
            Channel[1].LoadTrackList(LoadDesk2.Result);
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
            if (Loading) return;
            Channel_1.UpdateVisualElements();
            Channel_2.UpdateVisualElements();
            Channel_1.DrawFriq();
            Channel_2.DrawFriq();
        }

        
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void button_Click_4(object sender, RoutedEventArgs e)
        {
            string LoadSp = OpenSpectacle.Open(this, Config.GetConfigValue("MusicDir"));
            if (LoadSp == null) return;
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
                case Key.Tab:
                    if (Config.GetConfigValue("Theme") == "dark")
                        Config.SetConfigValue("Theme", "light");
                    else Config.SetConfigValue("Theme", "dark");
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(
                        new ResourceDictionary() { Source = new Uri($"pack://application:,,,/{Config.GetConfigValue("Theme")}.xaml") });
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMusic(Config.GetConfigValue("file"));
        }
    }
}

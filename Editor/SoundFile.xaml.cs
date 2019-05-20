using Extentions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Un4seen.Bass;

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для SoundFile.xaml
    /// </summary>
    public partial class SoundFile : UserControl
    {
        private string _Title;
        private string _Comment;
        private bool _Cycle;
        private SQLite.MusicFile _Music;
        private bool Escape = false;
        private SoundChannel Sound = new SoundChannel();
        private DispatcherTimer timer;
        private bool PositionDrag = false;
        private bool TimeLinePressed = false;

        /// <summary>
        /// Заголовок файла
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set
            {
                SetTitle(value);
                _Music.Title = _Title;
                               
            }
        }

        /// <summary>
        /// Заголовок файла
        /// </summary>
        public string Comment
        {
            get { return _Comment; }
            set
            {
                SetComment(value);
                _Music.Comment = _Comment;
            }
        }

        /// <summary>
        /// Установка значения Title с учётом пустого поля
        /// </summary>
        /// <param name="value"></param>
        private void SetTitle(string value)
        {
            string LastTitle = _Title;
            _Title = value;

            TitleText.Text = _Title == "" ? "Введите название" : _Title;
            if (_Title == "" && TitleLabel.FontStyle == FontStyles.Normal)
            {
                TitleLabel.FontStyle = FontStyles.Italic;
                TitleLabel.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else if (_Title != "" && LastTitle == "")
            {
                TitleLabel.FontStyle = FontStyles.Normal;
                TitleLabel.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        /// <summary>
        /// Установка значения Comment с учётом пустого поля
        /// </summary>
        /// <param name="value"></param>
        private void SetComment(string value)
        {
            string LastComment = _Comment;
            _Comment = value;

            CommentText.Text = _Comment == "" ? "Введите комментарий" : _Comment;
            if (_Comment == "" && CommentLabel.FontStyle == FontStyles.Normal)
            {
                CommentLabel.FontStyle = FontStyles.Italic;
                CommentLabel.Foreground = new SolidColorBrush(Colors.Gray);
            }
            else if (_Comment != "" && LastComment == "")
            {
                CommentLabel.FontStyle = FontStyles.Normal;
                CommentLabel.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        public bool Cycle
        {
            get { return _Cycle; }
            set
            {
                _Cycle = value;
                _Music.Cycle = _Cycle;
                RepeatImage.Source = LoadBitmapFromResources(_Cycle
                    ? "images/Repeat.png"
                    : "images/Repeat_Disabled.png");
            }
        }

        /// <summary>
        /// Запись в БД, связанная с панелью
        /// </summary>
        public SQLite.MusicFile Music
        {
            get { return _Music; }
            set
            {
                _Music = value;
                _Title = _Music.Title;
                _Comment = _Music.Comment;
                _Cycle = _Music.Cycle;
                Sound.Open(_Music.Data);
                Sound.Position = 0;
                TimeText.Text = $"{Sound.PositionTime()} | {Sound.LengthTime()}";
                SetPositionRect(Sound.Position);

                Update();
            }
        }

        public SoundFile()
        {
            InitializeComponent();
            Sound.SoundCard = -1;
            Sound.Stop();
            Sound.AutoStop += new EventHandler((object sender, EventArgs e) => { timer.Stop(); });
            timer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100),
                IsEnabled = false
            };
            timer.Tick += TimerTick;

        }

        /// <summary>
        /// Обновление строки навигации и времени
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerTick(object sender, EventArgs e)
        {
            SetPositionRect(Sound.Position);
            TimeText.Text = $"{Sound.PositionTime()} | {Sound.LengthTime()}";
        }

        /// <summary>
        /// Загрузить данные из БД
        /// </summary>
        public void Update()
        {
            SetTitle(_Title);
            SetComment(_Comment);
            RepeatImage.Source = LoadBitmapFromResources(_Cycle
                    ? "images/Repeat.png"
                    : "images/Repeat_Disabled.png");
        }

        /// <summary>
        /// Загрузить изображения из ресурсов
        /// </summary>
        /// <param name="URI">Адрес</param>
        /// <returns></returns>
        private static BitmapImage LoadBitmapFromResources(string URI)
        {
            Uri Link = new Uri($"pack://application:,,,/{URI}", UriKind.Absolute);
            return new BitmapImage(Link)
            { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            Cycle = !Cycle;
        }

        private void TitleLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Escape = false;
            TitleBox.Text = Title;
            TitleBox.Visibility = Visibility.Visible;
            TitleLabel.Visibility = Visibility.Collapsed;
        }

        private void TitleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Escape)
                return;
            Title = TitleBox.Text;
            TitleBox.Visibility = Visibility.Collapsed;
            TitleLabel.Visibility = Visibility.Visible;
        }

        private void TitleBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    Title = TitleBox.Text;
                    TitleBox.Visibility = Visibility.Collapsed;
                    TitleLabel.Visibility = Visibility.Visible;
                    break;
                case Key.Escape:
                    Escape = true;
                    TitleBox.Visibility = Visibility.Collapsed;
                    TitleLabel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void CommentBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Escape = false;
            CommentBox.Text = Comment;
            CommentBox.Visibility = Visibility.Visible;
            CommentLabel.Visibility = Visibility.Collapsed;
        }

        private void CommentBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Escape)
                return;
            Comment = CommentBox.Text;
            CommentBox.Visibility = Visibility.Collapsed;
            CommentLabel.Visibility = Visibility.Visible;
        }

        private void CommentBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    Comment = CommentBox.Text;
                    CommentBox.Visibility = Visibility.Collapsed;
                    CommentLabel.Visibility = Visibility.Visible;
                    break;
                case Key.Escape:
                    Escape = true;
                    CommentBox.Visibility = Visibility.Collapsed;
                    CommentLabel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void SetPositionRect(int Pos)
        {
            if (PositionDrag) return;

            double Max = TimeLine.ActualWidth - TimeLinePosition.ActualWidth;
            double NewPos = Convert.ToDouble(Pos) / 1000f * Max;
            TimeLinePosition.Margin = new Thickness(NewPos, 0, 0, 0);
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (Sound.State) timer.Stop();
            else timer.Start();
            Sound.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Sound.Stop();
            timer.Stop();
        }

        private void TimeLine_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PositionDrag = true;
            TimeLinePressed = true;

            double NewPos = e.GetPosition((Grid)sender).X - TimeLinePosition.ActualWidth / 2;
            if (NewPos < 0) NewPos = 0;
            if (NewPos > TimeLine.ActualWidth - TimeLinePosition.ActualWidth)
                NewPos = TimeLine.ActualWidth - TimeLinePosition.ActualWidth;
            TimeLinePosition.Margin = new Thickness(NewPos, 0, 0, 0);
        }

        private void TimeLine_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;

            double NewPos = e.GetPosition((Grid)sender).X - TimeLinePosition.ActualWidth / 2;
            if (NewPos < 0) NewPos = 0;
            if (NewPos > TimeLine.ActualWidth - TimeLinePosition.ActualWidth)
                NewPos = TimeLine.ActualWidth - TimeLinePosition.ActualWidth;
            TimeLinePosition.Margin = new Thickness(NewPos, 0, 0, 0);
        }

        private void TimeLine_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Sound.Position = Convert.ToInt32( Math.Round( TimeLinePosition.Margin.Left /
                (TimeLine.ActualWidth - TimeLinePosition.ActualWidth) * 1000));
            PositionDrag = false;
            TimeLinePressed = false;
        }

        private void TimeLine_MouseEnter(object sender, MouseEventArgs e)
        {
            if (TimeLinePressed)
                PositionDrag = true;
        }

        private void TimeLine_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TimeLinePressed)
                PositionDrag = false;
        }

        private void Other_Click(object sender, RoutedEventArgs e)
        {
            if (OtherFunctionsPanel.Height == 0)
                Animator.ChangeHeight(OtherFunctionsPanel, 74, 200);
            else Animator.ChangeHeight(OtherFunctionsPanel, 0, 200);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (OtherFunctionsPanel.Height > 0)
                Animator.ChangeHeight(OtherFunctionsPanel, 0, 200);
        }
    }
}

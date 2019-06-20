using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using Extentions;
using Microsoft.Win32;
using Un4seen.Bass;

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для SoundTrack.xaml
    /// </summary>
    public partial class SoundTrack : DataBaseElement
    {
        private SQLite.Track _Track;
        private bool Escape = false;
        private SoundChannel Sound = new SoundChannel();
        private DispatcherTimer timer;
        private bool PositionDrag = false;
        private bool TimeLinePressed = false;
        private bool _Opened = false;        

        public SQLite.Track Track
        {
            get => _Track;
            set
            {
                _Track = value;
                OpenSound();
                SetPositionRect(Sound.Position);

                Update();
            }
        }

        /// <summary>
        /// Заголовок файла
        /// </summary>
        public string Title
        {
            get => Track.Title == "" ? Track.Sound.Title : Track.Title;
            set
            {
                SetTitle(value);
                Track.Title = value;

            }
        }

        public bool Opened
        {
            get => _Opened;
            set
            {
                if (_Opened == value) return;
                _Opened = value;
                if (_Opened)
                    OpenSpoiler();
                else
                    CloseSpoiler();
            }
        }

        /// <summary>
        /// Установка значения Title с учётом пустого поля
        /// </summary>
        /// <param name="value"></param>
        private void SetTitle(string value)
        {
            string Text = value == "" ? Track.Sound.Title : value;
            SetComment(Track.Sound.Comment);

            TitleText.Text = Text;
        }

        /// <summary>
        /// Установка значения Comment с учётом пустого поля
        /// </summary>
        /// <param name="value"></param>
        private void SetComment(string value)
        {
            string comment = Track.Title == "" ? value : $"{Track.Sound.Title} ({value})";
            comment = comment == "" ? " " : comment;

            CommentText.Text = comment;
        }

        /// <summary>
        /// Зацикленность файла
        /// </summary>
        public bool Cycle
        {
            get => Track.Cycle;
        }

        /// <summary>
        /// Открытие звука из БД
        /// </summary>
        private void OpenSound()
        {
            Sound.Open(Track.Data);
            Sound.Position = 0;
            TimeText.Text = $"{Sound.PositionTime()} | {Sound.LengthTime()}";
        }

        public SoundTrack()
        {
            InitializeComponent();
            SelectedRect = SelectedRectangle;

            Animator.ChangeHeight(this, _Opened ? 170 : 60, 0);
            UpDownImage.Source = LoadBitmapFromResources(_Opened ? "images/Up.png" : "images/Down.png");

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
            Track.Update();
            SetTitle(Track.Title);
            SetComment(Track.Comment);
            RepeatImage.Source = LoadBitmapFromResources(Track.Cycle
                    ? "images/Repeat.png"
                    : "images/Repeat_Disabled.png");
            RepeatImageTop.Source = LoadBitmapFromResources(Track.Cycle
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
            Title = TitleBox.Text == Track.Sound.Title ? "" : TitleBox.Text;
            TitleBox.Visibility = Visibility.Collapsed;
            TitleLabel.Visibility = Visibility.Visible;
        }

        private void TitleBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    Title = TitleBox.Text == Track.Sound.Title ? "" : TitleBox.Text;
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

        private void SetPositionRect(int Pos)
        {
            if (PositionDrag) return;

            double Max = TimeLine.ActualWidth - TimeLinePosition.ActualWidth;
            double NewPos = Convert.ToDouble(Pos) / 1000f * Max;
            TimeLinePosition.Margin = new Thickness(NewPos, 0, 0, 0);
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Sound.Stop();
            timer.Stop();
            SetPositionRect(0);
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
            Sound.Position = Convert.ToInt32(Math.Round(TimeLinePosition.Margin.Left /
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

        /// <summary>
        /// Экспорт музыки в файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            Animator.ChangeHeight(OtherFunctionsPanel, 0, 0);

            SaveFileDialog SD = new SaveFileDialog()
            {
                Title = $"Экспорт дорожки «{Title}»",
                FileName = $"{Title}.mp3",
                AddExtension = true,
                DefaultExt = "mp3",
                Filter = "MPEG Layer 3 Music file (*.mp3)|*.mp3"
            };
            if (SD.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(SD.FileName, FileMode.Create))
                {
                    Track.Data.CopyTo(fs);
                    fs.Close();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        private void UpDown_Click(object sender, RoutedEventArgs e)
        {
            Opened = !Opened;
        }

        /// <summary>
        /// Скрыть подробности о треке
        /// </summary>
        public void CloseSpoiler()
        {
            Animator.ChangeHeight(this, 60, 250);
            UpDownImage.Source = LoadBitmapFromResources("images/Down.png");
        }

        /// <summary>
        /// Показать подробности о треке
        /// </summary>
        public void OpenSpoiler()
        {
            Animator.ChangeHeight(this, 170, 250);
            UpDownImage.Source = LoadBitmapFromResources("images/Up.png");
        }

        public override void Play()
        {
            if (Sound.State) timer.Stop();
            else timer.Start();
            Sound.Pause();
        }

        public override void Delete()
        {
            Animator.ChangeHeight(OtherFunctionsPanel, 0, 0);

            if (MessageBox.Show("Вы уверены, что хотите удалить этот трек?\nОтменить это действие будет невозможно!",
                $"Удаление трека {Track.Number} – {Title}", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                ForceDelete();
        }

        public void ForceDelete()
        {
            Track.Delete();
            ((Panel)Parent).Children.Remove(this);
        }
    }
}

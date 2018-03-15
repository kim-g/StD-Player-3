using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Un4seen.Bass;

namespace StD_Player_3
{
    class Desk : SoundChannel
    {
        // Внутренние свойства
        private int сurrentrack = 0;
        private List<MusicTrack> tracklist = new List<MusicTrack>();
        private int ListElementsToShow = 8; // Сколько элементов в списке показывать до и после текущего

        // Наследуемые внешние свойства
        protected Rectangle PositionRect;
        protected Rectangle BackgroundRect;
        protected Label PositionLabel;
        protected Label LengthLabel;
        protected Label NameLabel;
        protected Button PlayButton;
        protected Button PauseButton;
        protected Button StopButton;
        protected Button NextButton;
        protected Button PreviosButton;
        protected Canvas ProgressCanvas;
        protected Canvas ListCanvas;
        protected Label[] ListLabels;
        protected List<MusicTrack> TrackList
        {
            get { return tracklist; }
            set
            {
                tracklist.RemoveRange(0, tracklist.Count);
                tracklist = value;
                CurrentTrack = 0;
            }
        }

        protected int CurrentTrack
        {
            get
            {
                return сurrentrack;
            }
            set
            {
                сurrentrack = value;
                if (value < 0) сurrentrack = 0;
                if (value >= TrackList.Count) сurrentrack = TrackList.Count - 1;
                Open(TrackList[сurrentrack].Data, TrackList[сurrentrack].Repeat);
                UpdateList();
            }
        }


        void DeskAutoStop(object sender, EventArgs e)
        {
            MessageBox.Show("Стоп");
        }

        public Desk(Grid CurGrid, int balance, int volume)
        {
            Grid ButtonsGrid;
            Grid LabelGrig;
            Grid ProgressGrid;
            Grid ListGrid;
            Grid[] ButtonGrids = new Grid[5];

            ListLabels = new Label[ListElementsToShow * 2 + 1];


            void SetBinding(object BindSource, FrameworkElement Element, string Property, 
                DependencyProperty DP)
            {
                Binding binding = new Binding();
                binding.Source = BindSource;
                binding.Path = new PropertyPath(Property);
                binding.Mode = BindingMode.OneWay;
                Element.SetBinding(DP, binding);
            }

            Button SetButton(Panel Parent, int Left, object Content, RoutedEventHandler OnClick)
            {
                Button NewButton = new Button();
                Parent.Children.Add(NewButton);
                NewButton.Margin = new Thickness(Left, 0, 0, 0);
                NewButton.HorizontalAlignment = HorizontalAlignment.Center;
                NewButton.VerticalAlignment = VerticalAlignment.Center;
                NewButton.Width = 75;
                NewButton.Content = Content;
                NewButton.Click += OnClick;
                return NewButton;
            }

            Label SetLabel(Panel Parent, int Left, int Top)
            {
                Label NewLabel = new Label();
                Parent.Children.Add(NewLabel);
                NewLabel.Margin = new Thickness(Left, Top, 0, 0);
                NewLabel.Content = "";
                NewLabel.VerticalAlignment = VerticalAlignment.Center;
                return NewLabel;
            }

            Canvas SetCanvas(Panel Parent, Thickness Margin, double Height, double Width = double.NaN)
            {
                Canvas NewCanvas = new Canvas();
                Parent.Children.Add(NewCanvas);
                NewCanvas.Margin = Margin;
                NewCanvas.Height = Height;
                NewCanvas.Width = Width;
                return NewCanvas;
            }

            Rectangle SetRectangle(Panel Parent, Color Fill, Color Stroke, bool BindWidth)
            {
                Rectangle NewRect = new Rectangle();
                NewRect.Fill = new SolidColorBrush(Fill);
                NewRect.Stroke = new SolidColorBrush(Stroke);
                SetBinding(Parent, NewRect, "Height", FrameworkElement.HeightProperty);
                if (BindWidth)
                    SetBinding(Parent, NewRect, "ActualWidth", FrameworkElement.WidthProperty);
                return NewRect;
            }

            RowDefinition RowDefenitionHeight(int height = 0)
            {
                RowDefinition RD = new RowDefinition();
                RD.Height = height == 0
                    ? GridLength.Auto
                    : new GridLength(height);
                return RD;
            }

            Canvas SetCanvasList(Grid Parent, Thickness Margin)
            {
                Canvas CurCan = SetCanvas(Parent, Margin, (ListElementsToShow * 2 + 1) * 25, double.NaN);

                Rectangle Rect = SetRectangle(CurCan, Colors.White, Colors.Black, true);
                Rect.StrokeThickness = 1;
                CurCan.Children.Add(Rect);
                
                for (int i = 0; i < ListElementsToShow * 2 + 1; i++)
                {
                    ListLabels[i] = SetLabel(CurCan, 0, i * 25);
                    ListLabels[i].FontFamily = new FontFamily("Courier New");
                    ListLabels[i].FontSize = 20;
                    SetBinding(CurCan, ListLabels[i], "ActualWidth", FrameworkElement.WidthProperty);
                    ListLabels[i].Tag = i - ListElementsToShow;
                    ListLabels[i].MouseLeftButtonUp += ListElement_Click;
                }

                ListLabels[ListElementsToShow].Background = new SolidColorBrush(Color.FromRgb(0x00,0x00, 0x88));
                ListLabels[ListElementsToShow].Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                ListLabels[ListElementsToShow].FontWeight = FontWeights.Bold;

                return CurCan;
            }

            void MakeGridStructure(Grid ParentGrid)
            {
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight(30));
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight(25));
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight(31));
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight());

                ButtonsGrid = new Grid();
                LabelGrig = new Grid();
                ProgressGrid = new Grid();
                ListGrid = new Grid();
                Grid.SetRow(ButtonsGrid, 2);
                Grid.SetRow(LabelGrig, 0);
                Grid.SetRow(ProgressGrid, 1);
                Grid.SetRow(ListGrid, 3);
                ParentGrid.Children.Add(ButtonsGrid);
                ParentGrid.Children.Add(LabelGrig);
                ParentGrid.Children.Add(ProgressGrid);
                ParentGrid.Children.Add(ListGrid);
                for (int i=0; i<5; i++)
                {
                    ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    ButtonGrids[i] = new Grid();
                    Grid.SetColumn(ButtonGrids[i], i);
                    ButtonsGrid.Children.Add(ButtonGrids[i]);
                }
            }

            MakeGridStructure(CurGrid);
            PlayButton = SetButton(ButtonGrids[0], 0, "Play", PlayButton_Click);
            PauseButton = SetButton(ButtonGrids[1], 0, "Pause", PauseButton_Click);
            StopButton = SetButton(ButtonGrids[2], 0, "Stop", StopButton_Click);
            NextButton = SetButton(ButtonGrids[4], 0, "Next", NextButton_Click);
            PreviosButton = SetButton(ButtonGrids[3], 0, "Prev", PreviosButton_Click);
            PositionLabel = SetLabel(LabelGrig, 0, 0);
            LengthLabel = SetLabel(LabelGrig, 45, 0);
            NameLabel = SetLabel(LabelGrig, 90, 0);
            ProgressCanvas = SetCanvas(ProgressGrid, new Thickness(10, 0, 10, 0), 25);
            BackgroundRect = SetRectangle(ProgressCanvas, Color.FromRgb(0, 77, 0), Colors.Black, true);
            PositionRect = SetRectangle(ProgressCanvas, Color.FromRgb(0, 255, 0), Colors.Black, false);
            ListCanvas = SetCanvasList(ListGrid, new Thickness(10, 0, 10, 0));

            //Добавление в Canvas
            ProgressCanvas.Children.Add(BackgroundRect);
            ProgressCanvas.Children.Add(PositionRect);

            Balance = balance;
            Volume = volume;
            UpdateList();
        }

        protected void UpdateList()
        {
            for (int i = 0; i < ListElementsToShow * 2 + 1; i++)
            {
                if (CurrentTrack + i - ListElementsToShow < 0)
                {
                    ListLabels[i].Content = "";
                    continue;
                }
                if (CurrentTrack + i - ListElementsToShow >= TrackList.Count)
                {
                    ListLabels[i].Content = "";
                    continue;
                }
                ListLabels[i].Content = TrackList[CurrentTrack + i - ListElementsToShow].FullName();
            }
        }

        public void UpdateVisualElements()
        {
            // Вычисление текущего времени
            double PosSeconds = Bass.BASS_ChannelBytes2Seconds(Channel, BytePosition());
            int PosMinute = Convert.ToInt32(Math.Floor(PosSeconds / 60f));
            int PosSecond = Convert.ToInt32(Math.Round(PosSeconds - (PosMinute * 60)));
            string Pos = PosMinute.ToString("D2") + ":" + PosSecond.ToString("D2");
            if (Pos != PositionLabel.Content.ToString()) PositionLabel.Content = Pos;

            //Вычисление времени длины
            PosSeconds = Bass.BASS_ChannelBytes2Seconds(Channel, Length);
            PosMinute = Convert.ToInt32(Math.Floor(PosSeconds / 60f));
            PosSecond = Convert.ToInt32(Math.Round(PosSeconds - (PosMinute * 60)));
            Pos = PosMinute.ToString("D2") + ":" + PosSecond.ToString("D2");
            if (Pos != LengthLabel.Content.ToString()) LengthLabel.Content = Pos;

            //Отображение позиции трека
            PositionRect.Width = BackgroundRect.Width * Position() / 1000;

            // Отображение названия трека
            string NewName = TrackList[CurrentTrack].FullName();
            if ((string)NameLabel.Content != NewName) NameLabel.Content = NewName;
        }

        // События кнопок
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Play();
            UpdateVisualElements();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Pause();
            UpdateVisualElements();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            UpdateVisualElements();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            CurrentTrack++;
            UpdateVisualElements();
        }

        private void PreviosButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            CurrentTrack--;
            UpdateVisualElements();
        }

        // События нажатия на элементы списка
        private void ListElement_Click(object sender, RoutedEventArgs e)
        {
            if ((string)((Label)sender).Content == "") return;

            CurrentTrack += (int)((Label)sender).Tag;
        }

        /// <summary>
        /// Добавить файл в список из файла
        /// </summary>
        /// <param name="FileName">Имя файла для открытия</param>
        /// <param name="number">Номер трека</param>
        /// <param name="name">Название трека</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public void AddTrack(string FileName, string Number, string Name, bool Repeat=false)
        {
            TrackList.Add(new MusicTrack(FileName, Number, Name, Repeat));
            if (Channel == 0) Open(TrackList[CurrentTrack].Data, TrackList[CurrentTrack].Repeat);
            UpdateList();
        }

        /// <summary>
        /// Добавить файл в список из потока
        /// </summary>
        /// <param name="FileStream">Поток файла</param>
        /// <param name="number">Номер трека</param>
        /// <param name="name">Название трека</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public void AddTrack(Stream FileStream, string Number, string Name, bool Repeat = false)
        {
            TrackList.Add(new MusicTrack(FileStream, Number, Name, Repeat));
            if (Channel == 0) Open(TrackList[CurrentTrack].Data, TrackList[CurrentTrack].Repeat);
            UpdateList();
        }

        /// <summary>
        /// Добавить MusicTrack в список
        /// </summary>
        /// <param name="Track">MusicTrack</param>
        public void AddTrack(MusicTrack Track)
        {
            TrackList.Add(Track);
            if (Channel == 0) Open(TrackList[CurrentTrack].Data, TrackList[CurrentTrack].Repeat);
            UpdateList();
        }

        /// <summary>
        /// Загрузить новый список треков 
        /// </summary>
        /// <param name="InTrackList">Новый список треков.</param>
        public void LoadTrackList(List<MusicTrack> InTrackList = null)
        {
            TrackList = InTrackList;
        }
    }

    //Класс одного муз. файла
    public class MusicTrack
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public Stream Data { get; set; }
        public bool Repeat { get; set; }

        /// <summary>
        /// Открыть файл из файла
        /// </summary>
        /// <param name="FileName">Имя файла для открытия</param>
        /// <param name="number">Номер трека</param>
        /// <param name="name">Название трека</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public MusicTrack(string FileName, string number, string name, bool repeat)
        {
            Number = number;
            Name = name;
            Repeat = repeat;
            FileStream FS = new FileStream(FileName, FileMode.Open);
            Data = new MemoryStream();
            FS.CopyTo(Data);
            FS.Close();
            FS.Dispose();
        }

        /// <summary>
        /// Открыть файл из файла
        /// </summary>
        /// <param name="FileStream">Имя файла для открытия</param>
        /// <param name="number">Номер трека</param>
        /// <param name="name">Название трека</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public MusicTrack(Stream FileStream, string number, string name, bool repeat)
        {
            Number = number;
            Name = name;
            Repeat = repeat;
            Data = new MemoryStream();
            FileStream.CopyTo(Data);
        }

        /// <summary>
        /// Получить номер трека и его название
        /// </summary>
        public string FullName()
        {
            return Number + " — " + Name;
        }
    }
}

using StD_Player_3.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private int ListHeight = 25; // Высота элемента
        private int ListHeightMargin = 3; // Сдвиг элемента сверху

        // Наследуемые внешние свойства
        protected Rectangle PositionRect;
        protected Rectangle BackgroundRect;
        protected Rectangle CurrentTrackRect;
        protected Rectangle PressedTrackRect;
        protected Rectangle StatusColorRect;
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
        protected PlayState DeskState;
        protected Label[] ListLabels;
        protected ListClick LabelClick;
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
                DeskState.State = PlayState.Stop;
                Open(TrackList[сurrentrack].Data, TrackList[сurrentrack].Repeat);
                UpdateList();
            }
        }

        public byte DeskN { get; protected set; }


        void DeskAutoStop(object sender, EventArgs e)
        {
            MessageBox.Show("Стоп");
        }

        public Desk(Grid CurGrid, int balance, int volume, byte N)
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

            Button SetButton(Panel Parent, int Left, object Content, RoutedEventHandler OnClick, 
                string Image="")
            {
                Button NewButton = new Button();
                Parent.Children.Add(NewButton);
                NewButton.Margin = new Thickness(Left, 0, 0, 0);
                NewButton.HorizontalAlignment = HorizontalAlignment.Center;
                NewButton.VerticalAlignment = VerticalAlignment.Center;
                NewButton.Width = 75;
                StackPanel stackPnl = new StackPanel();
                stackPnl.Orientation = Orientation.Horizontal;
                stackPnl.Margin = new Thickness(0);
                if (Image!="")
                {
                    NewButton.Width = 32;
                    NewButton.Height = 32;
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(Image + ".png", UriKind.Relative))
                        { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
                    stackPnl.Children.Add(img);
                    NewButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    NewButton.Background = new SolidColorBrush(Colors.Transparent);
                }
                if ((string)Content != "")
                {
                    Label ButtonLabel = SetLabel(stackPnl, 0, 0);
                    ButtonLabel.Content = Content;
                }

                NewButton.Content = stackPnl;
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

            Rectangle SetRectangle(Panel Parent, Color Fill, Color Stroke, bool BindWidth, 
                bool BindHeight = true)
            {
                Rectangle NewRect = new Rectangle();
                NewRect.Fill = new SolidColorBrush(Fill);
                NewRect.Stroke = new SolidColorBrush(Stroke);
                if (BindHeight)
                    SetBinding(Parent, NewRect, "Height", FrameworkElement.HeightProperty);
                if (BindWidth)
                    SetBinding(Parent, NewRect, "ActualWidth", FrameworkElement.WidthProperty);
                Parent.Children.Add(NewRect);
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

            ColumnDefinition ColumnDefenitionWidth(int width = 0)
            {
                ColumnDefinition CD = new ColumnDefinition();
                CD.Width = width == 0
                    ? GridLength.Auto
                    : new GridLength(width);
                return CD;
            }

            Canvas SetCanvasList(Grid Parent, Thickness Margin)
            {
                Canvas CurCan = SetCanvas(Parent, Margin, (ListElementsToShow * 2 + 1) * 25, double.NaN);
                CurCan.MouseLeave += List_MouseLeave;

                Rectangle Rect = SetRectangle(CurCan, Colors.White, Colors.Black, true);
                Rect.StrokeThickness = 1;

                CurrentTrackRect = SetRectangle(CurCan, ProjectColors.SelectedElement, 
                    Colors.Black, true, false);
                CurrentTrackRect.Height = ListHeight;
                CurrentTrackRect.Margin = new Thickness(0, (ListElementsToShow) * ListHeight + 
                    ListHeightMargin, 0, 0);

                PressedTrackRect = SetRectangle(CurCan, Colors.Yellow,
                    Colors.Black, true, false);
                PressedTrackRect.Height = ListHeight;
                PressedTrackRect.Margin = new Thickness(0, 0 + ListHeightMargin, 0, 0);
                PressedTrackRect.Visibility = Visibility.Hidden;

                for (int i = 0; i < ListElementsToShow * 2 + 1; i++)
                {
                    ListLabels[i] = SetFont(SetLabel(CurCan, 0, i * ListHeight), "Courier New", 20,
                        ProjectColors.FontColor, FontWeights.Normal);
                    SetBinding(CurCan, ListLabels[i], "ActualWidth", FrameworkElement.WidthProperty);
                    ListLabels[i].Tag = i - ListElementsToShow;
                    ListLabels[i].MouseLeftButtonUp += ListElement_Click;
                    ListLabels[i].MouseLeftButtonDown += ListElement_MouseDown;
                    ListLabels[i].MouseEnter += ListElement_MouseEnter;
                }
                ListLabels[ListElementsToShow].Foreground = 
                    new SolidColorBrush(ProjectColors.SelectedElementFont);
                ListLabels[ListElementsToShow].FontWeight = FontWeights.Bold;

                return CurCan;
            }

            Label SetFont(Label Input, string FontName, int FontSize, Color FontColor, 
                FontWeight Weight)
            {
                Input.FontFamily = new FontFamily(FontName);
                Input.FontSize = FontSize;
                Input.Foreground = new SolidColorBrush(FontColor);
                Input.FontWeight = Weight;
                return Input;
            }

            void SetLabelGrid()
            {
                Label SetTimeLabel(Panel Parent, string Title, Color Background)
                {
                    
                    Canvas MainCanvas = SetCanvas(Parent, new Thickness(0), 80);
                    Thickness Margin = new Thickness(10);
                    MainCanvas.Margin = Margin;
                    SetBinding(Parent, MainCanvas, "ActualWidth", FrameworkElement.WidthProperty);

                    Rectangle Ground = SetRectangle(MainCanvas, Background, Colors.Black, 
                        true, true);
                    //Ground.Margin = Margin;
                    Label TimeLabel = SetLabel(MainCanvas, 0, 0);
                    Label TitleLabel = SetLabel(MainCanvas, 0, 0);
                    SetBinding(MainCanvas, TimeLabel, "Height", FrameworkElement.HeightProperty);
                    SetBinding(MainCanvas, TimeLabel, "Width", FrameworkElement.WidthProperty);
                    SetBinding(MainCanvas, TitleLabel, "Width", FrameworkElement.WidthProperty);
                    //SetBinding(MainCanvas, Ground, "Margin", FrameworkElement.MarginProperty);

                    TimeLabel = SetFont(TimeLabel, "Courier New", 60, ProjectColors.TimeFont, 
                        FontWeights.Bold);
                    TimeLabel.Padding = new Thickness(0, 15, 0, 0);
                    TitleLabel = SetFont(TitleLabel, "Courier New", 12, ProjectColors.TimeFont,
                        FontWeights.Normal);
                    TitleLabel.Padding = new Thickness(0, 5, 0, 0);
                    MainCanvas.HorizontalAlignment = HorizontalAlignment.Center;
                    TimeLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                    TimeLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    TitleLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

                    TitleLabel.Foreground = new SolidColorBrush(Colors.White);
                    TimeLabel.Foreground = new SolidColorBrush(Colors.White);

                    TitleLabel.Content = Title;
                    return TimeLabel;
                }

                LabelGrig = new Grid();
                LabelGrig.RowDefinitions.Add(RowDefenitionHeight(52));
                LabelGrig.RowDefinitions.Add(RowDefenitionHeight(40));
                LabelGrig.RowDefinitions.Add(RowDefenitionHeight(100));
                Grid[] Lines = new Grid[3];
                for (int i = 0; i < Lines.Count(); i++)
                {
                    Lines[i] = new Grid();
                    Grid.SetRow(Lines[i], i);
                    LabelGrig.Children.Add(Lines[i]);
                }
                Lines[0].Margin = new Thickness(10);
                StatusColorRect = SetRectangle(Lines[0], ProjectColors.Red,
                    Colors.Black, true, true);
                Label DeskName = SetFont(SetLabel(Lines[0], 5, 0), "TimesNewRoman", 14, Colors.White, 
                    FontWeights.Normal);
                DeskName.Content = "дека " + DeskN.ToString();

                Lines[1].Margin = new Thickness(10,0,10,0);
                SetRectangle(Lines[1], ProjectColors.TitleRect,
                    ProjectColors.Border, true, true);
                NameLabel = SetFont(SetLabel(Lines[1], 0, 0), "Courier New", 20, ProjectColors.FontColor,
                    FontWeights.Normal);

                Grid[] L2 = new Grid[2];

                Lines[2].ColumnDefinitions.Add(ColumnDefenitionWidth(10));
                for (int i = 0; i < L2.Count(); i++)
                {
                    ColumnDefinition CD = new ColumnDefinition();
                    CD.Width = new GridLength(50, GridUnitType.Star);
                    Lines[2].ColumnDefinitions.Add(CD);
                    Lines[2].ColumnDefinitions.Add(ColumnDefenitionWidth(10));
                }

                for (int i = 0; i < L2.Count(); i++)
                {
                    L2[i] = new Grid();
                    L2[i].Margin = new Thickness(0);
                    Grid.SetColumn(L2[i], i*2+1);
                    Lines[2].Children.Add(L2[i]);
                }
                PositionLabel = SetTimeLabel(L2[0], "Прошло", ProjectColors.Green);
                LengthLabel = SetTimeLabel(L2[1], "Всего", ProjectColors.Red);

            }

            void MakeGridStructure(Grid ParentGrid)
            {
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight());
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight(25));
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight(31));
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight());

                ButtonsGrid = new Grid();
                SetLabelGrid();
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
                for (int i = 0; i < ButtonGrids.Length; i++)
                {
                    ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    ButtonGrids[i] = new Grid();
                    Grid.SetColumn(ButtonGrids[i], i);
                    ButtonsGrid.Children.Add(ButtonGrids[i]);
                }
            }

            DeskN = N;
            MakeGridStructure(CurGrid);
            PlayButton = SetButton(ButtonGrids[0], 0, "", PlayButton_Click, "Play");
            PauseButton = SetButton(ButtonGrids[1], 0, "", PauseButton_Click, "Pause");
            StopButton = SetButton(ButtonGrids[2], 0, "", StopButton_Click, "Stop");
            NextButton = SetButton(ButtonGrids[4], 0, "Next", NextButton_Click);
            PreviosButton = SetButton(ButtonGrids[3], 0, "Prev", PreviosButton_Click);
            ProgressCanvas = SetCanvas(ProgressGrid, new Thickness(10, 0, 10, 0), 25);
            BackgroundRect = SetRectangle(ProgressCanvas, ProjectColors.Green, Colors.Black, true);
            PositionRect = SetRectangle(ProgressCanvas, ProjectColors.GreenLight, Colors.Black, false);
            ListCanvas = SetCanvasList(ListGrid, new Thickness(10, 0, 10, 0));

            DeskState = new PlayState(StatusColorRect);
            Balance = balance;
            Volume = volume;
            LabelClick = new ListClick(PressedTrackRect, ListHeightMargin);
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
            if (!LabelClick.MouseDown) return;
            LabelClick.Free();

            if ((string)((Label)sender).Content == "") return;
            if ((int)((Label)sender).Tag == 0) return;
            CurrentTrack += (int)((Label)sender).Tag;
        }

        private void ListElement_MouseDown(object sender, RoutedEventArgs e)
        {
            if ((string)((Label)sender).Content == "") return;

            LabelClick.Set((Label)sender);
        }

        private void ListElement_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((string)((Label)sender).Content == "") return;
            if (e.LeftButton == MouseButtonState.Pressed)
                LabelClick.Element = (Label)sender;
            else
                LabelClick.Free();
        }

        private void List_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (LabelClick.MouseDown) LabelClick.Bleach();
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

        protected override void DoOnPlay(object InObject)
        {
            DeskState.State = PlayState.Play;
        }

        protected override void DoOnPause(object InObject)
        {
            DeskState.State = PlayState.Pause;
        }

        protected override void DoOnStop(object InObject)
        {
            DeskState.State = PlayState.Stop;
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

    class ListClick
    {
        private Label element = null;
        private bool mousedown = false;
        private Rectangle PressedRect;
        private int ListHeightMargin;

        public ListClick(Rectangle MovableRect, int TopMargin)
        {
            PressedRect = MovableRect;
            ListHeightMargin = TopMargin;
        }
        
        // Свойства
        public bool MouseDown
        {
            get { return mousedown; }
        }

        public Label Element
        {
            get { return element; }
            set
            {
                if (!MouseDown) return;
                if (element != null && (int)element.Tag != 0) Bleach();
                element = value;
                if (element != null && (int)element.Tag != 0) Colorize();
            }
        }

        /// <summary>
        /// Окрашивает выделяемый элемент в определённый цвет
        /// </summary>
        public void Colorize()
        {
            PressedRect.Margin = new Thickness(Element.Margin.Left, Element.Margin.Top + ListHeightMargin,
                Element.Margin.Right, Element.Margin.Bottom);
            PressedRect.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Возвращает выделенному элементу прозрачность.
        /// </summary>
        public void Bleach()
        {
            PressedRect.Visibility = Visibility.Hidden;
        }

        public void Set(Label LabelToSet)
        {
            mousedown = true;
            Element = LabelToSet;
        }

        public void Free()
        {
            Element = null;
            mousedown = false;
        }
    }

    /// <summary>
    /// Показывает состояние плеера (Играет, пауза, остановка) 
    /// и управляет графическими элементами интерфейса
    /// </summary>
    class PlayState
    {
        // Внутренние параметры
        private byte state = 0;
        private Rectangle Rect;
        private Brush StopBrush = new SolidColorBrush(ProjectColors.Red);
        private Brush PlayBrush = new SolidColorBrush(ProjectColors.Green);
        private Brush PauseBrush = new SolidColorBrush(ProjectColors.Yellow);

        // Константы
        public const byte Stop = 0;
        public const byte Play = 1;
        public const byte Pause = 2;

        public PlayState(Rectangle StateRect)
        {
            Rect = StateRect;
        }

        public byte State
        {
            get
            {
                return state;
            }

            set
            {
                if (value < 0) return;
                if (value > 2) return;

                if (value == Stop)
                {
                    Rect.Fill = StopBrush;
                    return;
                }

                if (value == Play)
                {
                    Rect.Fill = PlayBrush;
                    return;
                }

                if (value == Pause)
                {
                    Rect.Fill = PauseBrush;
                    return;
                }
            }
        }
    }
}

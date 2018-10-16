﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Un4seen.Bass;
using Extentions;

namespace StD_Player_3
{
    class Desk : SoundChannel
    {
        // Внутренние свойства
        private int сurrentrack = 0;
        private List<MusicTrack> tracklist = new List<MusicTrack>();
        private int ListElementsToShow = 9; // Сколько элементов в списке показывать до и после текущего
        private int ListHeight = 25; // Высота элемента
        private int ListHeightMargin = 6; // Сдвиг элемента сверху 
        private bool PositionTrackShow = false; // Показывать в треке Label и позицию к мышке
        private string PositionTrackLabel = "";
        private double MouseX;

        // Элементы интерфейса
        protected Rectangle PositionRect;
        protected Rectangle BackgroundRect;
        protected Rectangle MiddleRect;
        protected Rectangle CurrentTrackRect;
        protected Rectangle PressedTrackRect;
        protected Rectangle StatusColorRect;
        protected Rectangle ListRect;
        protected Label PositionLabel;
        protected Label LengthLabel;
        protected Label NameLabel;
        protected Label TrackTimeLabel;
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
        protected Rectangle Freq;
        protected Rectangle FreqBkg;
        protected double Scale = 1.0;
        const byte LevelsThickness = 1;

        // Наследуемые внешние свойства
        protected List<MusicTrack> TrackList
        {
            get { return tracklist; }
            set
            {
                foreach (MusicTrack MT in tracklist)
                {
                    MT.Dispose();
                }

                tracklist.Clear();
                GC.Collect();
                tracklist = value;
                CurrentTrack = 0;
                UpdateList();
            }
        }
        public int CurrentTrack
        {
            get
            {
                return сurrentrack;
            }
            set
            {
                Stop();
                сurrentrack = value;
                if (value < 0) сurrentrack = 0;
                if (value >= TrackList.Count) сurrentrack = TrackList.Count - 1;

                string Fill = CurrentTrack < 0
                    ? "Gray"
                    : "DeepBkgd";
                ListRect.SetResourceReference(Rectangle.FillProperty, Fill);
                CurrentTrackRect.Visibility = CurrentTrack < 0
                    ? Visibility.Hidden
                    : Visibility.Visible;

                if (сurrentrack < 0) return;
                DeskState.State = PlayState.Stop;
                Open(TrackList[сurrentrack].Data, TrackList[сurrentrack].Repeat);
                UpdateList();
            }
        }

        public byte DeskN { get; protected set; }

        /// <summary>
        /// Создание новой деки и её интерфейса
        /// </summary>
        /// <param name="CurGrid">Панель Grid для размещения интерфейса деки</param>
        /// <param name="balance">Баланс звука (левая -1, правая 1)</param>
        /// <param name="volume">Громкость трека (0..100)</param>
        /// <param name="N">Номер деки</param>
        public Desk(Grid CurGrid, int balance, int volume, byte N, double scale)
        {
            // Панели
            Grid ButtonsGrid;
            Grid LabelGrig;
            Grid InfoGrid;
            Grid FreqGrid;
            Grid ProgressGrid;
            Grid ListGrid;
            Grid[] ButtonGrids = new Grid[5];
            StackPanel Buttons;

            // Элементы в списке для выбора 
            ListLabels = new Label[ListElementsToShow * 2 + 1];

            /// <summary>
            /// Привязка свойства объекта к свойству другого объекта OneWay
            /// </summary>
            /// <param name="BindSource">Элемент, с которого копируем свойства</param>
            /// <param name="Element">Элемент, которому устанавливаем привязку</param>
            /// <param name="Property">Свойство, откуда берём значение</param>
            /// <param name="DP">Свойство, которому устанавливается зависимость</param>
            void SetBinding(object BindSource, FrameworkElement Element, string Property, 
                DependencyProperty DP)
            {
                Binding binding = new Binding();
                binding.Source = BindSource;
                binding.Path = new PropertyPath(Property);
                binding.Mode = BindingMode.OneWay;
                Element.SetBinding(DP, binding);
            }

            /// <summary>
            /// Создание кнопки и привязка её к родителю
            /// </summary>
            /// <param name="Parent">Родительский элемент</param>
            /// <param name="Left">Отступ слева</param>
            /// <param name="Content">Надпись</param>
            /// <param name="OnClick">Событие нажатия на кнопку</param>
            /// <param name="Image">Изображение</param>
            Button SetButton(Panel Parent, double Left, object Content, RoutedEventHandler OnClick, 
                string Image="")
            {
                Button NewButton = new Button();
                Parent.Children.Add(NewButton);
                NewButton.Margin = new Thickness(ScaleTo(Left), 0, 0, 0);
                NewButton.HorizontalAlignment = HorizontalAlignment.Center;
                NewButton.VerticalAlignment = VerticalAlignment.Center;
                NewButton.Width = ScaleTo(75.0);
                StackPanel stackPnl = new StackPanel();
                stackPnl.Orientation = Orientation.Horizontal;
                stackPnl.Margin = new Thickness(0);
                if (Image!="")
                {
                    NewButton.Width = ScaleTo(32.0);
                    NewButton.Height = ScaleTo(32.0);
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

            /// <summary>
            /// Создание надписи и привязка её к родителю
            /// </summary>
            /// <param name="Parent">Родительский элемент</param>
            /// <param name="Left">Отступ слева</param>
            /// <param name="Top">Отступ сверху</param>
            Label SetLabel(Panel Parent, double Left, double Top)
            {
                Label NewLabel = new Label();
                Parent.Children.Add(NewLabel);
                NewLabel.Margin = new Thickness(ScaleTo(Left), ScaleTo(Top), 0, 0);
                NewLabel.Content = "";
                NewLabel.VerticalAlignment = VerticalAlignment.Center;
                return NewLabel;
            }

            /// <summary>
            /// Создание холста и привязка его к родителю
            /// </summary>
            /// <param name="Parent">Родительский элемент</param>
            /// <param name="Margin">Отступы</param>
            /// <param name="Height">Высота</param>
            /// <param name="Width">Ширина</param>
            Canvas SetCanvas(Panel Parent, Thickness Margin, double Height, double Width = double.NaN)
            {
                Canvas NewCanvas = new Canvas();
                Parent.Children.Add(NewCanvas);
                NewCanvas.Margin = Margin;
                NewCanvas.Height = ScaleTo(Height);
                NewCanvas.Width = ScaleTo(Width);
                return NewCanvas;
            }

            /// <summary>
            /// Создание прямоугольника и привязка его к родителю
            /// </summary>
            /// <param name="Parent">Родительский элемент</param>
            /// <param name="Fill">Цвет заливки</param>
            /// <param name="Stroke">Цвет обводки</param>
            /// <param name="BindWidth">Привязывать ли по ширине к родителю</param>
            /// <param name="BindHeight">Привязывать ли по высоте к родителю</param>
            Rectangle SetRectangle(Panel Parent, Color Fill, Color Stroke, bool BindWidth, 
                bool BindHeight = true)
            {
                Rectangle NewRect = new Rectangle();
                NewRect.SetResourceReference(Rectangle.FillProperty, Fill);
                NewRect.Fill = new SolidColorBrush(Fill);
                NewRect.Stroke = new SolidColorBrush(Stroke);
                if (BindHeight)
                    SetBinding(Parent, NewRect, "Height", FrameworkElement.HeightProperty);
                if (BindWidth)
                    SetBinding(Parent, NewRect, "ActualWidth", FrameworkElement.WidthProperty);
                Parent.Children.Add(NewRect);
                return NewRect;
            }
            
            /// <summary>
            /// Создание прямоугольника и привязка его к родителю
            /// </summary>
            /// <param name="Parent">Родительский элемент</param>
            /// <param name="Fill">Цвет заливки</param>
            /// <param name="Stroke">Цвет обводки</param>
            /// <param name="BindWidth">Привязывать ли по ширине к родителю</param>
            /// <param name="BindHeight">Привязывать ли по высоте к родителю</param>
            Rectangle SetRectangleRes(Panel Parent, string Fill, string Stroke, bool BindWidth,
                bool BindHeight = true)
            {
                Rectangle NewRect = new Rectangle();
                NewRect.SetResourceReference(Rectangle.FillProperty, Fill);
                NewRect.SetResourceReference(Rectangle.StrokeProperty, Stroke);
                if (BindHeight)
                    SetBinding(Parent, NewRect, "Height", FrameworkElement.HeightProperty);
                if (BindWidth)
                    SetBinding(Parent, NewRect, "ActualWidth", FrameworkElement.WidthProperty);
                Parent.Children.Add(NewRect);
                return NewRect;
            }
            /// <summary>
            /// Создание разметки строки
            /// </summary>
            /// <param name="height">Высота строки</param>
            RowDefinition RowDefenitionHeight(double height = 0)
            {
                RowDefinition RD = new RowDefinition();
                RD.Height = height == 0
                    ? GridLength.Auto
                    : new GridLength(ScaleTo(height));
                return RD;
            }

            /// <summary>
            /// Создание разметки столбца
            /// </summary>
            /// <param name="width">Ширина столбца</param>
            ColumnDefinition ColumnDefenitionWidth(double width = 0)
            {
                ColumnDefinition CD = new ColumnDefinition();
                CD.Width = width == 0
                    ? GridLength.Auto
                    : new GridLength(width);
                return CD;
            }

            /// <summary>
            /// Создание разметки столбца
            /// </summary>
            /// <param name="width">Ширина столбца</param>
            ColumnDefinition ColumnDefenitionWidthStar(double width=0)
            {
                ColumnDefinition CD = new ColumnDefinition();
                CD.Width = width == 0
                    ? GridLength.Auto
                    : new GridLength(width, GridUnitType.Star);
                return CD;
            }

            /// <summary>
            /// Создание списка треков на основе Canvas
            /// </summary>
            /// <param name="Parent">Родительский элемент</param>
            /// <param name="Margin">Отступы</param>
            Canvas SetCanvasList(Grid Parent, Thickness Margin)
            {
                Canvas CurCan = SetCanvas(Parent, Margin, (ListElementsToShow * 2 + 1) * 25 + 5, 
                    double.NaN);
                CurCan.MouseLeave += List_MouseLeave;

                ListRect = CurrentTrack < 0 
                    ? SetRectangleRes(CurCan, "Gray", "Border", true)
                    : SetRectangleRes(CurCan, "DeepBkgd", "Foreground", true);
                ListRect.StrokeThickness = 1;

                CurrentTrackRect = CurrentTrack < 0
                    ? SetRectangleRes(CurCan, "Gray", "Gray", true)
                    : SetRectangleRes(CurCan, "SelectedElement", 
                    "Foreground", true, false);
                CurrentTrackRect.Height = ScaleTo(ListHeight);
                CurrentTrackRect.Margin = new Thickness(0, ScaleTo((ListElementsToShow) * ListHeight + 
                    ListHeightMargin), 0, 0);

                PressedTrackRect = SetRectangleRes(CurCan, "PressedElement",
                    "Foreground", true, false);
                PressedTrackRect.Height = ScaleTo(ListHeight);
                PressedTrackRect.Margin = new Thickness(0, ScaleTo(0 + ListHeightMargin), 0, 0);
                PressedTrackRect.Visibility = Visibility.Hidden;

                for (int i = 0; i < ListElementsToShow * 2 + 1; i++)
                {
                    ListLabels[i] = SetFontRes(SetLabel(CurCan, 0, i * ListHeight), "Courier New", 24,
                        "FontColor", FontWeights.Normal);
                    ListLabels[i].SetResourceReference(Label.ForegroundProperty, "FontColor");
                    SetBinding(CurCan, ListLabels[i], "ActualWidth", FrameworkElement.WidthProperty);
                    ListLabels[i].Tag = i - ListElementsToShow;
                    ListLabels[i].MouseLeftButtonUp += ListElement_Click;
                    ListLabels[i].MouseLeftButtonDown += ListElement_MouseDown;
                    ListLabels[i].MouseEnter += ListElement_MouseEnter;
                }
                ListLabels[ListElementsToShow].SetResourceReference(Label.ForegroundProperty, "SelectedElementFont"); ;
                ListLabels[ListElementsToShow].FontWeight = FontWeights.Bold;

                return CurCan;
            }

            /// <summary>
            /// Настройка шрифта надписи и вывод в Label
            /// </summary>
            /// <param name="Input">Входящий Label</param>
            /// <param name="FontName">Имя шрифта</param>
            /// <param name="FontSize">Размер шрифта</param>
            /// <param name="FontColor">Цвет шрифта</param>
            /// <param name="Weight">Вес шрифта</param>
            Label SetFont(Label Input, string FontName, double FontSize, Color FontColor, 
                FontWeight Weight)
            {
                Input.FontFamily = new FontFamily(FontName);
                Input.FontSize = ScaleTo(FontSize);
                Input.Foreground = new SolidColorBrush(FontColor);
                Input.FontWeight = Weight;
                return Input;
            }

            /// <summary>
            /// Настройка шрифта надписи и вывод в Label
            /// </summary>
            /// <param name="Input">Входящий Label</param>
            /// <param name="FontName">Имя шрифта</param>
            /// <param name="FontSize">Размер шрифта</param>
            /// <param name="FontColor">Цвет шрифта</param>
            /// <param name="Weight">Вес шрифта</param>
            Label SetFontRes(Label Input, string FontName, double FontSize, string FontColor,
                FontWeight Weight)
            {
                Input.FontFamily = new FontFamily(FontName);
                Input.FontSize = ScaleTo(FontSize);
                Input.SetResourceReference(Label.ForegroundProperty, FontColor);
                Input.FontWeight = Weight;
                return Input;
            }


            /// <summary>
            /// Настройка блока вывода информации о проигрываемом треке
            /// </summary>
            void SetLabelGrid()
            {

                /// <summary>
                /// Создание блока вывода времени
                /// </summary>
                /// <param name="Parent">Родительский элемент</param>
                /// <param name="Title">Текст заголовка сверху</param>
                /// <param name="Background">Цвет фона</param>
                Label SetTimeLabel(Panel Parent, string Title, string Background)
                {
                    
                    Canvas MainCanvas = SetCanvas(Parent, new Thickness(0), ScaleTo(110.0));
                    Thickness Margin = new Thickness(ScaleTo(10));
                    MainCanvas.Margin = Margin;
                    SetBinding(Parent, MainCanvas, "ActualWidth", FrameworkElement.WidthProperty);

                    Rectangle Ground = SetRectangleRes(MainCanvas, Background, "Foreground", 
                        true, true);
                    //Ground.Margin = Margin;
                    Label TimeLabel = SetLabel(MainCanvas, 0, 0);
                    Label TitleLabel = SetLabel(MainCanvas, 0, 0);
                    SetBinding(MainCanvas, TimeLabel, "Height", FrameworkElement.HeightProperty);
                    SetBinding(MainCanvas, TimeLabel, "Width", FrameworkElement.WidthProperty);
                    SetBinding(MainCanvas, TitleLabel, "Width", FrameworkElement.WidthProperty);
                    //SetBinding(MainCanvas, Ground, "Margin", FrameworkElement.MarginProperty);

                    TimeLabel = SetFontRes(TimeLabel, "Courier New", 70, "TimeFont", 
                        FontWeights.Bold);
                    TimeLabel.Padding = new Thickness(0, ScaleTo(15.0), 0, 0);
                    TitleLabel = SetFontRes(TitleLabel, "Cambria", 12, "TimeFont",
                        FontWeights.Normal);
                    TitleLabel.Padding = new Thickness(0, ScaleTo(5.0), 0, 0);
                    MainCanvas.HorizontalAlignment = HorizontalAlignment.Center;
                    TimeLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                    TimeLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    TitleLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

                    TitleLabel.SetResourceReference(Label.ForegroundProperty, "DeepBkgd");
                    TimeLabel.SetResourceReference(Label.ForegroundProperty, "DeepBkgd");

                    TitleLabel.Content = Title;
                    TimeLabel.Content = "--:--";
                    return TimeLabel;
                }

                InfoGrid = new Grid();
                InfoGrid.Name = "InfoGrid";
                InfoGrid.ColumnDefinitions.Add(ColumnDefenitionWidthStar(50));
                InfoGrid.ColumnDefinitions.Add(ColumnDefenitionWidth(ScaleTo(35.0)));


                LabelGrig = new Grid();
                LabelGrig.Name = "LabelGrig";
                LabelGrig.RowDefinitions.Add(RowDefenitionHeight(ScaleTo(60.0)));
                LabelGrig.RowDefinitions.Add(RowDefenitionHeight(ScaleTo(40.0)));
                LabelGrig.RowDefinitions.Add(RowDefenitionHeight(ScaleTo(130.0)));
                Grid[] Lines = new Grid[3];
                for (int i = 0; i < Lines.Count(); i++)
                {
                    Lines[i] = new Grid();
                    Grid.SetRow(Lines[i], i);
                    LabelGrig.Children.Add(Lines[i]);
                }
                Lines[0].Name = "StatusGrid";
                Lines[0].Margin = new Thickness(10, ScaleTo(10.0), 10, ScaleTo(10.0));
                StatusColorRect = SetRectangleRes(Lines[0], "Red",
                    "Foreground", true, true);
                Label DeskName = SetFontRes(SetLabel(Lines[0], 5, 0), "TimesNewRoman", 14.0,
                    "DeepBkgd", FontWeights.Normal);
                DeskName.Content = "дека " + DeskN.ToString();

                Lines[1].Name = "NameGrid";
                Lines[1].Margin = new Thickness(10, 0, 10, 0);
                SetRectangleRes(Lines[1], "TitleRect", "BorderSolid", true, true);
                NameLabel = SetFontRes(SetLabel(Lines[1], 0, 0), "Courier New", 20.0, 
                    "FontColor", FontWeights.Normal);

                Grid[] L2 = new Grid[2];

                Lines[2].Name = "TimeGrid";
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
                PositionLabel = SetTimeLabel(L2[0], "Прошло", "Green");
                LengthLabel = SetTimeLabel(L2[1], "Всего", "Red");

                FreqGrid = new Grid();
                FreqGrid.Name = "FreqGrid";
                FreqGrid.Margin = new Thickness(0, ScaleTo(10.0), ScaleTo(10.0), ScaleTo(10.0));
                FreqBkg = new Rectangle()
                {
                    Name = "FreqBkg",
                    Fill = MainWindow.LevelsI,
                    StrokeThickness = LevelsThickness
                };
                FreqBkg.SetResourceReference(Rectangle.StrokeProperty, "BorderSolid");

                Freq = new Rectangle()
                {
                    Margin = new Thickness(LevelsThickness),
                    Fill = MainWindow.LevelsO,
                    Name = "Freq"
                };

                FreqGrid.Children.Add(FreqBkg);
                FreqGrid.Children.Add(Freq);

                Grid.SetColumn(LabelGrig, 0);
                Grid.SetColumn(FreqGrid, 1);

                InfoGrid.Children.Add(LabelGrig);
                InfoGrid.Children.Add(FreqGrid);
            }

            /// <summary>
            /// Создание разметки деки
            /// </summary>
            /// <param name="ParentGrid">Родительский элемент</param>
            void MakeGridStructure(Grid ParentGrid)
            {
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight());
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight(ScaleTo(25.0)));
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight(ScaleTo(50.0)));
                ParentGrid.RowDefinitions.Add(RowDefenitionHeight());

                ButtonsGrid = new Grid()
                {
                    Name = "ButtonsGrid",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                SetLabelGrid();
                ProgressGrid = new Grid();
                ProgressGrid.Name = "ProgressGrid";
                ListGrid = new Grid();
                ListGrid.Name = "ListGrid";
                Grid.SetRow(InfoGrid, 0);
                Grid.SetRow(ProgressGrid, 1);
                Grid.SetRow(ButtonsGrid, 2);
                Grid.SetRow(ListGrid, 3);
                ParentGrid.Children.Add(InfoGrid);
                ParentGrid.Children.Add(ProgressGrid);
                ParentGrid.Children.Add(ButtonsGrid);
                ParentGrid.Children.Add(ListGrid);
                Buttons = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                ButtonsGrid.Children.Add(Buttons);
            }

            DeskN = N;
            Scale = scale;
            MakeGridStructure(CurGrid);
            PlayButton = SetButton(Buttons, 0, "", PlayButton_Click, "Play");
            PauseButton = SetButton(Buttons, 20, "", PauseButton_Click, "Pause");
            StopButton = SetButton(Buttons, 20, "", StopButton_Click, "Stop");
            ProgressCanvas = SetCanvas(ProgressGrid, new Thickness(10, 0, 10, 0), 
                25);
            BackgroundRect = SetRectangleRes(ProgressCanvas, "Green", "Border", true);
            PositionRect = SetRectangleRes(ProgressCanvas, "GreenLight", "Border", false);
            MiddleRect = SetRectangleRes(ProgressCanvas, "GreenSemiLight", "Border", false);
            ListCanvas = SetCanvasList(ListGrid, new Thickness(ScaleTo(10.0), 0, ScaleTo(10.0), 0));
            TrackTimeLabel = SetLabel(ProgressCanvas,0,0);

            // Применение событий
            ProgressCanvas.MouseDown += TrackLabel_MouseDown;
            ProgressCanvas.MouseDown += TrackLabel_MouseMove;
            ProgressCanvas.MouseUp += TrackLabel_MouseUp;
            ProgressCanvas.MouseMove += TrackLabel_MouseMove;
            ProgressCanvas.MouseEnter += TrackLabel_MouseEnter;
            ProgressCanvas.MouseLeave += TrackLabel_MouseLeave;

            DeskState = new PlayState(StatusColorRect);
            Balance = balance;
            Volume = volume;
            LabelClick = new ListClick(PressedTrackRect, ListHeightMargin);
            UpdateList();
        }

        /// <summary>
        /// Обновление списка треков
        /// </summary>
        protected void UpdateList()
        {
            if (TrackList.Count == 0)
            {
                foreach (Label L in ListLabels)
                    L.Content = "";
                return;
            }

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

        /// <summary>
        /// Обновление всех изменяющихся элементов интерфейса
        /// </summary>
        public void UpdateVisualElements()
        {
            if (CurrentTrack < 0)
            {
                if ("--:--" != PositionLabel.Content.ToString()) PositionLabel.Content = "--:--";
                if ("--:--" != LengthLabel.Content.ToString()) LengthLabel.Content = "--:--";
                if ((string)NameLabel.Content != "") NameLabel.Content = "";
                PositionRect.Width = 0;
                UpdateMiddleRect();
                return;
            }
            
            // Вычисление текущего времени
            string Pos = PositionTime();
            if (Pos != PositionLabel.Content.ToString()) PositionLabel.Content = Pos;

            //Вычисление времени длины
            Pos = LengthTime();
            if (Pos != LengthLabel.Content.ToString()) LengthLabel.Content = Pos;

            //Отображение позиции трека
            long PositionT = Position;
            PositionRect.Width = BackgroundRect.Width * PositionT / 1000;
            UpdateMiddleRect();

            // Отображение названия трека
            string NewName = TrackList[CurrentTrack].FullName();
            if ((string)NameLabel.Content != NewName) NameLabel.Content = NewName;
        }

        /// <summary>
        /// Обновление промежуточного прямугольника трека
        /// </summary>
        private void UpdateMiddleRect()
        {
            if (CurrentTrack < 0)
            {
                TrackTimeLabel.Content = "";
                MiddleRect.Width = 0;
                return;
            }

            TrackTimeLabel.Content = PositionTrackShow
                            ? PositionTrackLabel
                            : "";

            if (PositionTrackShow)
            {
                double NewPosition = MouseX * 1000 / ProgressCanvas.ActualWidth;
                string ForeStr = Position < (NewPosition)
                    ? "White"
                    : "Black";
                TrackTimeLabel.SetResourceReference(Label.ForegroundProperty, ForeStr);

                MiddleRect.Margin = Position < (NewPosition)
                    ? new Thickness(Position * ProgressCanvas.ActualWidth / 1000 - 0.5, 0, 0, 0)
                    : new Thickness(MouseX, 0, 0, 0);

                MiddleRect.Width = Position < (NewPosition)
                    ? MouseX - Position * ProgressCanvas.ActualWidth / 1000
                    : Position * ProgressCanvas.ActualWidth / 1000 - MouseX + 0.5;
            }
            else
            {
                MiddleRect.Width = 0;
            }
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

        /// <summary>
        /// Событие нажатия на кнопку Play
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Play();
            UpdateVisualElements();
        }

        /// <summary>
        /// Событие нажатия на кнопку Pause
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Pause();
            UpdateVisualElements();
        }

        /// <summary>
        /// Событие нажатия на кнопку Stop
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            UpdateVisualElements();
        }

        /// <summary>
        /// Событие нажатия на кнопку Next
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            CurrentTrack++;
            UpdateVisualElements();
        }

        /// <summary>
        /// Событие нажатия на кнопку Previous
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void PreviosButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            CurrentTrack--;
            UpdateVisualElements();
        }

        /// <summary>
        /// Событие списка треков. OnMouseUp
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void ListElement_Click(object sender, RoutedEventArgs e)
        {
            if (!LabelClick.MouseDown) return;
            LabelClick.Free();

            if ((string)((Label)sender).Content == "") return;
            if ((int)((Label)sender).Tag == 0) return;
            CurrentTrack += (int)((Label)sender).Tag;
        }

        /// <summary>
        /// Событие списка треков. OnMouseDown
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void ListElement_MouseDown(object sender, RoutedEventArgs e)
        {
            if ((string)((Label)sender).Content == "") return;

            LabelClick.Set((Label)sender);
        }

        /// <summary>
        /// Событие списка треков. OnMouseEnter
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void ListElement_MouseEnter(object sender, MouseEventArgs e)
        {
            if ((string)((Label)sender).Content == "") return;
            if (e.LeftButton == MouseButtonState.Pressed)
                LabelClick.Element = (Label)sender;
            else
                LabelClick.Free();
        }

        /// <summary>
        /// Событие списка треков. OnMouseLeave
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void List_MouseLeave(object sender, RoutedEventArgs e)
        {
            if (LabelClick.MouseDown) LabelClick.Bleach();
        }

        /// <summary>
        /// Событие полосы трекинга. OnMouseUp
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void TrackLabel_MouseUp(object sender, MouseEventArgs e)
        {
            PositionTrackShow = false;
            Position = Convert.ToInt32(e.GetPosition((Canvas)sender).X *
                1000 / ((Canvas)sender).ActualWidth);
        }

        /// <summary>
        /// Событие полосы трекинга. OnMouseDown
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void TrackLabel_MouseDown(object sender, MouseEventArgs e)
        {
            PositionTrackShow = true;
        }

        /// <summary>
        /// Событие полосы трекинга. OnMouseEnter
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void TrackLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            PositionTrackShow = e.LeftButton == MouseButtonState.Pressed;
        }

        /// <summary>
        /// Событие полосы трекинга. OnMouseLeave
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void TrackLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            PositionTrackShow = false;
        }

        /// <summary>
        /// Событие полосы трекинга. OnMouseMove
        /// </summary>
        /// <param name="sender">Элемент, выдающий событие</param>
        /// <param name="e">параметры события</param>
        private void TrackLabel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;

            MouseX = e.GetPosition((Canvas)sender).X;
            bool LabelPos = MouseX < TrackTimeLabel.ActualWidth + 10;
            double NewPosition = MouseX * 1000 / ((Canvas)sender).ActualWidth;

            PositionTrackLabel = PositionTime(Convert.ToInt32(NewPosition));
            UpdateMiddleRect();

            TrackTimeLabel.Margin = LabelPos
                ? new Thickness(MouseX + 5, 0, 0, 0)
                : new Thickness(MouseX - 5 - TrackTimeLabel.ActualWidth, 0, 0, 0);
            
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

        /// <summary>
        /// Событие Bass.dll. OnPlay
        /// </summary>
        /// <param name="InObject">Внутренний параметр</param>
        protected override void DoOnPlay(object InObject)
        {
            DeskState.State = PlayState.Play;
        }

        /// <summary>
        /// Событие Bass.dll. OnPause
        /// </summary>
        /// <param name="InObject">Внутренний параметр</param>
        protected override void DoOnPause(object InObject)
        {
            DeskState.State = PlayState.Pause;
        }

        /// <summary>
        /// Событие Bass.dll. OnStop
        /// </summary>
        /// <param name="InObject">Внутренний параметр</param>
        protected override void DoOnStop(object InObject)
        {
            DeskState.State = PlayState.Stop;
        }

        /// <summary>
        /// Рисует частотную зависимость для трека
        /// </summary>
        public void DrawFriq()
        {
            int Level;
            if (State)
            {
                Int32 Levels = Bass.BASS_ChannelGetLevel(Channel);
                Level = Balance == 1 ? Levels.HighWord() : Levels.LowWord();
            }
            else Level = 0;

            double db = 10 * Math.Log10(Level / 32768f);
            db = db < -23 ? -23 : db;

            double NewHeight =  FreqBkg.ActualHeight * ( 0 - db / 23);
            if (NewHeight < LevelsThickness) NewHeight = LevelsThickness;
            if (NewHeight > FreqBkg.ActualHeight - LevelsThickness)
                NewHeight = FreqBkg.ActualHeight - LevelsThickness;

            /*int speed = NewHeight > Freq.Margin.Top ? 90 : 10;
            if (NewHeight - Freq.Margin.Bottom > FreqBkg.ActualHeight * 0.2)
                NewHeight = Freq.Margin.Bottom + FreqBkg.ActualHeight * 0.2;*/
            Animator.Margin(Freq, new Thickness(LevelsThickness, LevelsThickness, LevelsThickness, 
                FreqBkg.ActualHeight - NewHeight), 0, 100);
        }

        public void SetLevels(Brush I, Brush O)
        {
            Freq.Fill = O;
            FreqBkg.Fill = I;
        }
    }

    //Класс одного муз. файла
    public class MusicTrack : IDisposable
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Data.Dispose();
                Data = null;
                this.Name = null;
                this.Number = null;
            }
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
        private Brush StopBrush;
        private Brush PlayBrush;
        private Brush PauseBrush;

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
                    Rect.SetResourceReference(Shape.FillProperty, "Red");
                    return;
                }

                if (value == Play)
                {
                    Rect.SetResourceReference(Shape.FillProperty, "Green");
                    return;
                }

                if (value == Pause)
                {
                    Rect.SetResourceReference(Shape.FillProperty, "Yellow");
                    return;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
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
        protected Rectangle PositionRect;
        protected Rectangle BackgroundRect;
        protected Label PositionLabel;
        protected Label LengthLabel;
        protected Button PlayButton;
        protected Button PauseButton;
        protected Button StopButton;
        protected Canvas ProgressCanvas;

        public Desk(Rectangle PositionRectangle, Rectangle BackgroundRectangle,
            Label Position_Label, Label Length_Label, int balance = 0, int volume = 100)
        {
            PositionRect = PositionRectangle;
            BackgroundRect = BackgroundRectangle;
            PositionLabel = Position_Label;
            LengthLabel = Length_Label;
            Balance = balance;
            Volume = volume;
        }

        public Desk(Grid CurGrid, int balance, int volume)
        {
            void SetBinding(object BindSource, FrameworkElement Element, string Property, 
                DependencyProperty DP)
            {
                Binding binding = new Binding();
                binding.Source = BindSource;
                binding.Path = new PropertyPath(Property);
                binding.Mode = BindingMode.OneWay;
                Element.SetBinding(DP, binding);
            }

            Button SetButton(Grid Parent, int Left, object Content, RoutedEventHandler OnClick)
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

            Label SetLabel(Grid Parent, int Left, int Top)
            {
                Label NewLabel = new Label();
                Parent.Children.Add(NewLabel);
                NewLabel.Margin = new Thickness(Left, Top, 0, 0);
                NewLabel.Content = "";
                NewLabel.VerticalAlignment = VerticalAlignment.Center;
                return NewLabel;
            }

            Canvas SetCanvas(Grid Parent, Thickness Margin, double Height, double Width = double.NaN)
            {
                Canvas NewCanvas = new Canvas();
                Parent.Children.Add(NewCanvas);
                NewCanvas.Margin = Margin;
                NewCanvas.Height = Height;
                NewCanvas.Width = Width;
                return NewCanvas;
            }

            Rectangle SetRectangle(Grid Parent, Color Fill, object Source, bool BindWidth)
            {
                Rectangle NewRect = new Rectangle();
                NewRect.Fill = new SolidColorBrush(Fill);
                SetBinding(Source, NewRect, "Height", FrameworkElement.HeightProperty);
                if (BindWidth)
                    SetBinding(Source, NewRect, "ActualWidth", FrameworkElement.WidthProperty);
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

            
            CurGrid.RowDefinitions.Add(RowDefenitionHeight(25));
            CurGrid.RowDefinitions.Add(RowDefenitionHeight(31));
            CurGrid.RowDefinitions.Add(RowDefenitionHeight());

            Grid Grid1 = new Grid();
            Grid Grid2 = new Grid();
            Grid Grid3 = new Grid();
            Grid.SetRow(Grid1, 0);
            Grid.SetRow(Grid2, 1);
            Grid.SetRow(Grid3, 2);
            CurGrid.Children.Add(Grid1);
            CurGrid.Children.Add(Grid2);
            CurGrid.Children.Add(Grid3);
            Grid1.ColumnDefinitions.Add(new ColumnDefinition());
            Grid1.ColumnDefinitions.Add(new ColumnDefinition());
            Grid1.ColumnDefinitions.Add(new ColumnDefinition());
            Grid Grid1_1 = new Grid();
            Grid Grid1_2 = new Grid();
            Grid Grid1_3 = new Grid();
            Grid.SetColumn(Grid1_1, 0);
            Grid.SetColumn(Grid1_2, 1);
            Grid.SetColumn(Grid1_3, 2);
            Grid1.Children.Add(Grid1_1);
            Grid1.Children.Add(Grid1_2);
            Grid1.Children.Add(Grid1_3);

            PlayButton = SetButton(Grid1_1, 0, "Play", PlayButton_Click);
            PauseButton = SetButton(Grid1_2, 0, "Pause", PauseButton_Click);
            StopButton = SetButton(Grid1_3, 0, "Stop", StopButton_Click);
            PositionLabel = SetLabel(Grid2, 0, 0);
            LengthLabel = SetLabel(Grid2, 45, 0);
            ProgressCanvas = SetCanvas(Grid3, new Thickness(10, 0, 10, 0), 25);
            BackgroundRect = SetRectangle(Grid3, Color.FromRgb(0, 77, 0), ProgressCanvas, true);
            PositionRect = SetRectangle(Grid3, Color.FromRgb(0, 255, 0), ProgressCanvas, false);

            //Добавление в Canvas
            ProgressCanvas.Children.Add(BackgroundRect);
            ProgressCanvas.Children.Add(PositionRect);

            Balance = balance;
            Volume = volume;
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
    }
}

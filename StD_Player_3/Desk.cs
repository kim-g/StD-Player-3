using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    }
}

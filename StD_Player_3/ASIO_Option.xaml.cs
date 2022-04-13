using System;
using System.Collections.Generic;
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

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для ASIO_Option.xaml
    /// </summary>
    public partial class ASIO_Option : UserControl
    {
        private int _desk = 0;
        private List<AudioDevice> _ASIOaudioDevices = new List<AudioDevice>();
        private List<AudioDevice> _BassaudioDevices = new List<AudioDevice>();
        private CheckBox[] Boxes = new CheckBox[8];
        public ASIO_Option[] OtherDesks = new ASIO_Option[2];

        /// <summary>
        /// Номер деки
        /// </summary>
        public int Desk
        {
            get => _desk;
            set
            {
                if (_desk == value) return;
                _desk = value;
                TitleLabel.Content = $"Дека {_desk}";
            }
        }

        /// <summary>
        /// Список доступных ASIO аудиокарт
        /// </summary>
        public List<AudioDevice> ASIOAudioDevices
        {
            get => _ASIOaudioDevices;
            set
            {
                _ASIOaudioDevices = value;
                CardCB.ItemsSource = _ASIOaudioDevices;
            }
        }

        /// <summary>
        /// Выбранная ASIO аудиокарта
        /// </summary>
        public AudioDevice SelectedASIODevice
        {
            get => CardCB.SelectedItem as AudioDevice;
        }

        /// <summary>
        /// Список доступных ASIO аудиокарт
        /// </summary>
        public List<AudioDevice> StandartAudioDevices
        {
            get => _BassaudioDevices;
            set
            {
                _BassaudioDevices = value;
                StandartCardCB.ItemsSource = _BassaudioDevices;
            }
        }

        /// <summary>
        /// Выбранная ASIO аудиокарта
        /// </summary>
        public AudioDevice SelectedStandartDevice
        {
            get => StandartCardCB.SelectedItem as AudioDevice;
        }

        /// <summary>
        /// Выбранные каналы как число
        /// </summary>
        public int Channels
        {
            get
            {
                int Res = 0;
                for (int i = 0; i < Boxes.Length; i++)
                    if (Boxes[i].IsChecked == true)
                        Res += (int)Math.Pow(2, i);
                return Res;
            }
            set
            {
                for (int i = 0; i < Boxes.Length; i++)
                    Boxes[i].IsChecked = (value & (int)Math.Pow( 2,  i)) > 0;
            }
        }

        public ASIO_Option()
        {
            InitializeComponent();

            Boxes[0] = C1;
            Boxes[1] = C2;
            Boxes[2] = C3;
            Boxes[3] = C4;
            Boxes[4] = C5;
            Boxes[5] = C6;
            Boxes[6] = C7;
            Boxes[7] = C8;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (ASIO_Option ASIO_Desk in OtherDesks)
                ASIO_Desk.SetCheckBoxes();
        }

        public void SetCheckBoxes()
        {
            for (int i = 0; i < 8; i++)
            {
                if (OtherDesks[0].Boxes[i].IsChecked == true || OtherDesks[1].Boxes[i].IsChecked == true)
                {
                    Boxes[i].IsChecked = false;
                    Boxes[i].IsEnabled = false;
                }
                else
                {
                    Boxes[i].IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Установка устройства по VEN ID
        /// </summary>
        /// <param name="VEN"></param>
        public void SetDevice(string VEN, bool ASIO = true)
        {
            if (ASIO)
            {
                foreach (AudioDevice device in ASIOAudioDevices)
                    if (device.ADID == VEN)
                        CardCB.SelectedItem = device;
            }
            else
                foreach (AudioDevice Sdevice in StandartAudioDevices)
                    if (Sdevice.ADID == VEN)
                        StandartCardCB.SelectedItem = Sdevice;
        }



        /// <summary>
        /// Возвращает параметры деки.
        /// </summary>
        public DeskOptions Options
        {
            get
            {
                DeskOptionsASIO DO = new DeskOptionsASIO();
                DO.DeviceID = (CardCB.SelectedItem as AudioDevice).ID;
                DO.DeviceName = (CardCB.SelectedItem as AudioDevice).ADID;
                DO.StandartDeviceID = (StandartCardCB.SelectedItem as AudioDevice).ID;
                DO.StandartDeviceName = (StandartCardCB.SelectedItem as AudioDevice).ADID;
                for (int i = 0; i < 8; i++)
                    DO.OutputChannels[i] = Boxes[i].IsChecked == true;
                return DO;
            }
        }
    }
}

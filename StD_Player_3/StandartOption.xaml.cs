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
    /// Логика взаимодействия для StandartOption.xaml
    /// </summary>
    public partial class StandartOption : UserControl
    {
        private int _desk = 0;
        private List<AudioDevice> _audioDevices = new List<AudioDevice>();

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
        /// Список доступных аудиокарт
        /// </summary>
        public List<AudioDevice> AudioDevices 
        {
            get => _audioDevices;
            set
            {
                _audioDevices = value;
                CardCB.ItemsSource = _audioDevices;
            }
        }

        /// <summary>
        /// Выбранная аудиокарта
        /// </summary>
        public AudioDevice SelectedDevice
        {
            get => CardCB.SelectedItem as AudioDevice;
        }

        /// <summary>
        /// Канал воспроизведения
        /// </summary>
        public int Balance
        {
            get
            {
                if (LeftRB.IsChecked == true) return -1;
                if (CenterRB.IsChecked == true) return 0;
                if (RightRB.IsChecked == true) return 1;
                return 0;
            }
            set
            {
                switch(value)
                {
                    case -1:
                        LeftRB.IsChecked = true;
                        break;
                    case 0:
                        CenterRB.IsChecked = true;
                        break;
                    case 1:
                        RightRB.IsChecked = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Возвращает параметры деки.
        /// </summary>
        public DeskOptions Options
        {
            get
            {
                DeskOptionsStandart DO = new DeskOptionsStandart();
                DO.DeviceID = (CardCB.SelectedItem as AudioDevice).ID;
                DO.DeviceName = (CardCB.SelectedItem as AudioDevice).ADID;
                DO.Balance = Balance;
                return DO;
            }
        }
        
        public StandartOption()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Установка устройства по VEN ID
        /// </summary>
        /// <param name="VEN"></param>
        public void SetDevice(string VEN)
        {
            foreach (AudioDevice device in AudioDevices)
                if (device.ADID == VEN)
                    CardCB.SelectedItem = device;
        }
    }
}

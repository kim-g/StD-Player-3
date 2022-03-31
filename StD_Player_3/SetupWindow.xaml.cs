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
using System.Windows.Shapes;
using Un4seen.Bass;
using Un4seen.BassAsio;
using SQLite;

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        SetupResult Result = null;
        Dictionary<string, int> BalanceDic = new Dictionary<string, int>() { { "left", -1 }, { "center", 0 }, { "right", 1 } };
        Dictionary<int, string> BalanceDicOut = new Dictionary<int, string>() { { -1, "left" }, { 0, "center" }, { 1, "right" } };
        StandartOption[] standartOptions = new StandartOption[3];
        ASIO_Option[] ASIOOptions = new ASIO_Option[3];  
        SQLiteConfig Config;
        
        public SetupWindow()
        {
            InitializeComponent();
            standartOptions[0] = Desk1;
            standartOptions[1] = Desk2;
            standartOptions[2] = Desk3;

            ASIOOptions[0] = ADesk1;
            ASIOOptions[1] = ADesk2;
            ASIOOptions[2] = ADesk3;

            ADesk1.OtherDesks[0] = ADesk2;
            ADesk1.OtherDesks[1] = ADesk3;
            ADesk2.OtherDesks[0] = ADesk1;
            ADesk2.OtherDesks[1] = ADesk3;
            ADesk3.OtherDesks[0] = ADesk1;
            ADesk3.OtherDesks[1] = ADesk2;
        }

        /// <summary>
        /// Открытие окна настроек и получение всех настроек
        /// </summary>
        /// <param name="config">Конфигурационный файл.</param>
        /// <returns></returns>
        public static SetupResult Open(SQLiteConfig config)
        {
            SetupWindow SW = new SetupWindow() { Config = config};

            List<AudioDevice> devices = new List<AudioDevice>();

            switch (SW.Config.GetConfigValue($"Device_Type"))
            {
                case "Standart":
                    SW.StandartRB.IsChecked = true;
                    break;
                case "ASIO":
                    SW.ASIORB.IsChecked = true;
                    break;
                default:
                    SW.StandartRB.IsChecked = true;
                    SW.Config.SetConfigValue("Device_Type", "Standart");
                    break;
            }

            BASS_DEVICEINFO[] Cards = Bass.BASS_GetDeviceInfos();
            for (int Card = 0; Card < Cards.Length; Card++)
            {
                if (Cards[Card].id == null) continue;
                AudioDevice device = new AudioDevice();
                device.ID = Card;
                device.Name = Cards[Card].name;
                device.ADID = Cards[Card].id;
                devices.Add(device);
            }

            foreach (StandartOption SO in SW.standartOptions)
            {
                SO.AudioDevices = devices;
                SO.SetDevice(SW.Config.GetConfigValue($"Desk_{SO.Desk}_Sound_Card"));
                SO.Balance = SW.BalanceDic[SW.Config.GetConfigValue($"Desk_{SO.Desk}_Pan")];
            }

            BASS_ASIO_DEVICEINFO[] ASIOCards = BassAsio.BASS_ASIO_GetDeviceInfos();
            for (int Card = 0; Card < ASIOCards.Length; Card++)
            {
                if (ASIOCards[Card].name == null) continue;
                AudioDevice device = new AudioDevice();
                device.ID = Card;
                device.Name = ASIOCards[Card].name;
                device.ADID = ASIOCards[Card].name;
                devices.Add(device);
            }

            foreach (ASIO_Option SO in SW.ASIOOptions)
            {
                SO.AudioDevices = devices;
                SO.SetDevice(SW.Config.GetConfigValue($"Desk_{SO.Desk}_ASIO_Sound_Card"));
                SO.Channels = SW.Config.GetConfigValueInt($"Desk_{SO.Desk}_Channels");
            }

            if (SW.ShowDialog() == true)
            {
                SW.Result = new SetupResult();
                if (SW.StandartRB.IsChecked == true)
                { 
                    SW.Result.Type = SoundType.Standart;
                    SW.Config.SetConfigValue("Device_Type", "Standart");
                    for (int i = 0; i < 3; i++)
                    {
                        SW.Result.Desks[i] = SW.standartOptions[i].Options;
                        SW.Config.SetConfigValue($"Desk_{SW.standartOptions[i].Desk}_Sound_Card", SW.standartOptions[i].Options.DeviceName);
                        SW.Config.SetConfigValue($"Desk_{SW.standartOptions[i].Desk}_Pan", SW.BalanceDicOut[(SW.standartOptions[i].Options as DeskOptionsStandart).Balance]);
                    }
                    return SW.Result;
                }
                if (SW.ASIORB.IsChecked == true)
                {
                    SW.Result.Type = SoundType.ASIO;
                    SW.Config.SetConfigValue("Device_Type", "ASIO");
                    for (int i = 0; i < 3; i++)
                    {
                        SW.Result.Desks[i] = SW.ASIOOptions[i].Options;
                        SW.Config.SetConfigValue($"Desk_{SW.ASIOOptions[i].Desk}_ASIO_Sound_Card", SW.ASIOOptions[i].Options.DeviceName);
                        SW.Config.SetConfigValue($"Desk_{SW.ASIOOptions[i].Desk}_Channels", SW.ASIOOptions[i].Channels);
                    }
                    return SW.Result;
                }
            }
            return SW.Result;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void StandartRB_Checked(object sender, RoutedEventArgs e)
        {
            StandartParametersGrid.Visibility = Visibility.Visible;
            ASIOParametersGrid.Visibility = Visibility.Collapsed;
        }

        private void ASIORB_Checked(object sender, RoutedEventArgs e)
        {
            StandartParametersGrid.Visibility = Visibility.Collapsed;
            ASIOParametersGrid.Visibility = Visibility.Visible;
        }
    }
}

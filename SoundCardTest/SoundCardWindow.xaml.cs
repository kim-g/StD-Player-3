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
using Un4seen.Bass;

namespace SoundCardTest
{
    /// <summary>
    /// Логика взаимодействия для SoundCardWindow.xaml
    /// </summary>
    public partial class SoundCardWindow : Window
    {
        public SoundCardWindow()
        {
            InitializeComponent();

            foreach (BASS_DEVICEINFO Card in Bass.BASS_GetDeviceInfos())
            {
                if (Card.id == null) continue;
                SoundCards.Items.Add(new ComboBoxItem() { Content = Card.name });
                if (Card.IsDefault) SoundCards.SelectedIndex = SoundCards.Items.Count - 1;
            }

            int channels = 0;
            float dummy;
            while (Bass.BASS_ChannelGetAttribute(music, (BASSAttribute)((int)BASS_ATTRIB_MUSIC_VOL_CHAN + channels), ref dummy))
            {
                channels++;
            }
        }
    }
}

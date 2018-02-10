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
using Un4seen.Bass;

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Desk Channel_1;
        Desk Channel_2;
        System.Windows.Threading.DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            SoundChannel.Initiate();

            timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal);
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            Channel_1 = new Desk(Desk1, -1, 100);
            Stream File1 = new FileStream(@"d:\Женитьба\Sound\G 5 2m21s.mp3", FileMode.Open);
            Channel_1.Open(@"d:\Женитьба\Sound\G 5 2m21s.mp3");

            Channel_2 = new Desk(Desk2,1,100);
            Stream File2 = new FileStream(@"d:\Женитьба\Sound\G 7 2m20s.mp3", FileMode.Open, FileAccess.Read);
            Channel_2.Open(File2);
        }

        private void timerTick(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            Channel_1.UpdateVisualElements();
            Channel_2.UpdateVisualElements();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            Channel_1.Play();
            Update();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Channel_1.Pause();
            Update();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Channel_1.Stop();
            Update();
        }
    }
}

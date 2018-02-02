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

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Desk Channel_1;
        System.Windows.Threading.DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            SoundChannel.Initiate();

            timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal);
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Start();

            Channel_1 = new Desk(Pos, Bkgd, Position, Length);
            Channel_1.Open(@"d:\Женитьба\Sound\G 5 2m21s.mp3");
        }

        private void timerTick(object sender, EventArgs e)
        {
            Update();
        }

        private void Update()
        {
            Channel_1.UpdateVisualElements();
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

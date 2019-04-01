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

namespace StD_Player_3
{
    /// <summary>
    /// Логика взаимодействия для Parameters.xaml
    /// </summary>
    public partial class Parameters : Window
    {
        SQLite.SQLiteConfig Config;
        DeskParams[] Desks = new DeskParams[2];

        public Parameters(Window DialogParentWindow)
        {
            InitializeComponent();
            Owner = DialogParentWindow;
            Config = ((MainWindow)DialogParentWindow).Config;

            Desks[0] = new DeskParams(Desk1, 1, Config.GetConfigValue("Desk_1_Sound_Card"),
                Config.GetConfigValue("Desk_1_Pan"));
            Desks[1] = new DeskParams(Desk2, 2, Config.GetConfigValue("Desk_2_Sound_Card"),
                Config.GetConfigValue("Desk_2_Pan"));
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        public static void Set(Window ParentWindow)
        {
            Parameters Params = new Parameters(ParentWindow);

            Params.ShowDialog();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (DeskParams D in Desks)
            {
                Config.SetConfigValue($"Desk_{D.NDesk}_Sound_Card", D.SoundCard);
                Config.SetConfigValue($"Desk_{D.NDesk}_Pan", D.Pan);
            }
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        
    }
}

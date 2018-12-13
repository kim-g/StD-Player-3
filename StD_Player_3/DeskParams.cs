using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Un4seen.Bass;

namespace StD_Player_3
{
    public class DeskParams
    {
        private StackPanel CorePanel;
        private StackPanel DeskLabelPanel;
        private Label DeskLabel;
        private ComboBox CardsList;
        private SQLite.SQLiteConfig Config;

        public byte NDesk { get; set; }
        public string SoundCard
        {
            get
            {
                string Combo = ((ComboBoxItem)CardsList.SelectedItem).Content.ToString();
                return Bass.BASS_GetDeviceInfos().Where
                    (x => x.name == Combo).ToArray()[0].id;
            }
        }

        public DeskParams(Grid CurGrid, byte ndesk, string SoundCard)
        {
            this.Config = Config;
            NDesk = ndesk;

            CorePanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10)
            };
            DeskLabelPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            DeskLabel = new Label()
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = $"Дека {ndesk}"
            };
            DeskLabel.SetResourceReference(Control.ForegroundProperty, "Foreground");
            DeskLabelPanel.Children.Add(DeskLabel);

            CardsList = new ComboBox()
            {
                FontSize = 14,
                Width = 250
            };
            
            CardsList.SetResourceReference(Control.ForegroundProperty, "Foreground");
            CardsList.SetResourceReference(Control.BackgroundProperty, "Background");
            bool SC_Found = false;
            foreach (BASS_DEVICEINFO Card in Bass.BASS_GetDeviceInfos())
            {
                if (Card.id == null) continue;
                CardsList.Items.Add(new ComboBoxItem() { Content = Card.name});
                if (Card.IsDefault && !SC_Found) CardsList.SelectedIndex = CardsList.Items.Count - 1;
                if (Card.id == SoundCard)
                {
                    CardsList.SelectedIndex = CardsList.Items.Count - 1;
                    SC_Found = true;
                }
            }

            DeskLabelPanel.Children.Add(CardsList);

            CorePanel.Children.Add(DeskLabelPanel);

            CurGrid.Children.Add(CorePanel);
        }

    }
}

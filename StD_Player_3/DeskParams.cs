using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Un4seen.Bass;

namespace StD_Player_3
{
    public class DeskParams
    {
        private StackPanel CorePanel;
        private StackPanel DeskLabelPanel;
        private Label DeskLabel;
        private ComboBox CardsList;
        private Label Channels;
        private Grid SoundPanPanel;
        private RadioButton LeftRB;
        private RadioButton CenterRB;
        private RadioButton RightRB;

        public byte NDesk { get; set; }
        public string SoundCard
        {
            get
            {
                try
                {
                    string Combo = ((ComboBoxItem)CardsList.SelectedItem).Content.ToString();
                    return Bass.BASS_GetDeviceInfos().Where
                        (x => x.name == Combo).ToArray()[0].id;
                }
                catch
                {
                    return "";
                }
            }
        }

        public string Pan
        {
            get
            {
                if (LeftRB.IsChecked == true) return "left";
                if (CenterRB.IsChecked == true) return "center";
                if (RightRB.IsChecked == true) return "right";
                return "NULL";
            }
            set
            {
                switch (value)
                {
                    case "left":
                        LeftRB.IsChecked = true;
                        CenterRB.IsChecked = false;
                        RightRB.IsChecked = false;
                        break;
                    case "center":
                        LeftRB.IsChecked = false;
                        CenterRB.IsChecked = true;
                        RightRB.IsChecked = false;
                        break;
                    case "right":
                        LeftRB.IsChecked = false;
                        CenterRB.IsChecked = false;
                        RightRB.IsChecked = true;
                        break;
                    default:
                        LeftRB.IsChecked = false;
                        CenterRB.IsChecked = true;
                        RightRB.IsChecked = false;
                        break;
                }
            }
        }

        public DeskParams(Grid CurGrid, byte ndesk, string SoundCard, string Pan)
        {
            NDesk = ndesk;

            Channels = new Label();
            Channels.SetResourceReference(Control.ForegroundProperty, "Foreground");
            Channels.Visibility = Visibility.Collapsed;
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
            CardsList.SelectionChanged += ComboBoxSelectionChanged;


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

            SoundPanPanel = new Grid();
            for (byte i=0; i<3; i++)
                SoundPanPanel.ColumnDefinitions.Add(new ColumnDefinition()
                    { Width = new GridLength(1, GridUnitType.Star) });


            LeftRB = new RadioButton()
            {
                Content = "Левый канал",
                IsChecked = Pan == "left"
            };
            LeftRB.SetResourceReference(Control.ForegroundProperty, "Foreground");
            CenterRB = new RadioButton()
            {
                Content = "Оба канала",
                IsChecked = Pan == "center"
            };
            CenterRB.SetResourceReference(Control.ForegroundProperty, "Foreground");
            RightRB = new RadioButton()
            {
                Content = "Правый канал",
                IsChecked = Pan == "right"
            };
            RightRB.SetResourceReference(Control.ForegroundProperty, "Foreground");
            Grid.SetColumn(LeftRB, 0);
            Grid.SetColumn(CenterRB, 1);
            Grid.SetColumn(RightRB, 2);
            SoundPanPanel.Children.Add(LeftRB);
            SoundPanPanel.Children.Add(CenterRB);
            SoundPanPanel.Children.Add(RightRB);

            DeskLabelPanel.Children.Add(CardsList);

            CorePanel.Children.Add(DeskLabelPanel);
            CorePanel.Children.Add(Channels);
            CorePanel.Children.Add(SoundPanPanel);

            CurGrid.Children.Add(CorePanel);
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BASS_INFO info = new BASS_INFO();
            if (Bass.BASS_GetInfo(info))
            {
                Channels.Content = info.ToString();
                int speakers = info.speakers;
                //if speakers
            }
        }

    }
}

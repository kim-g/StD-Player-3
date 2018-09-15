using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    public enum Theme { Dark = 0, Light = 1}

    public class ProjectColors
    {
        public Color White;
        public Color Black;

        public Color SelectedElement;
        public Color SelectedElementFont;
        public Color TimeFont;

        public Color FontColor;
        public Color TitleRect;
        public Color Border;

        public Color Background;
        public Color Gray;
        public Color DarkGray;
        public Color Button;

        public Color LoadingBackground;

        // Постоянные цвета
        public Color ConstantWhite;
        public Color ConstantBlack;
        public Color Green;
        public Color GreenLight;
        public Color GreenSemiLight;
        public Color Yellow;
        public Color YellowLight;
        public Color Red;
        public Color RedLight;

        public ProjectColors(Theme WindowTheme)
        {
            switch (WindowTheme)
            {
                case Theme.Dark:
                    White = Color.FromRgb(0x00, 0x00, 0x00);
                    Black = Color.FromRgb(0xFF, 0xFF, 0xFF);

                    SelectedElement = Color.FromRgb(0xA0, 0xA0, 0xA0);
                    SelectedElementFont = White;
                    TimeFont = White;

                    FontColor = Black;
                    TitleRect = White;
                    Border = Black;

                    Background = Color.FromRgb(0x00, 0x00, 0x00); //0xF0
                    Gray = Color.FromRgb(0x80, 0x80, 0x80);
                    DarkGray = Color.FromRgb(0x70, 0x70, 0x70);
                    Button = Color.FromRgb(0x22, 0x22, 0x22);

                    LoadingBackground = Color.FromArgb(0xA0, 0x00, 0x00, 0x00);
                    break;

                case Theme.Light:
                    White = Color.FromRgb(0xFF, 0xFF, 0xFF);
                    Black = Color.FromRgb(0x00, 0x00, 0x00);

                    SelectedElement = Color.FromRgb(0x00, 0x00, 0x88);
                    SelectedElementFont = Colors.White;
                    TimeFont = Colors.White;

                    FontColor = Colors.Black;
                    TitleRect = Colors.White;
                    Border = Colors.Black;

                    Background = Color.FromRgb(0xF0, 0xF0, 0xF0);
                    Gray = Color.FromRgb(0x80, 0x80, 0x80);
                    DarkGray = Color.FromRgb(0x8f, 0x8f, 0x8f);
                    Button = Color.FromRgb(0xDD, 0xDD, 0xDD);

                    LoadingBackground = Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF);
                    break;
            }

            // Устанавливаем постоянные цвета
            Green = Color.FromRgb(0x25, 0x5b, 0x25);
            GreenLight = Color.FromRgb(0x00, 0xE0, 0x3F);
            GreenSemiLight = Color.FromRgb(0x12, 0x9d, 0x32);
            Yellow = Color.FromRgb(0x9D, 0x99, 0x00);
            YellowLight = Color.FromRgb(0xFF, 0xFF, 0x00);
            Red = Color.FromRgb(0x86, 0x28, 0x28);
            RedLight = Color.FromRgb(0xFF, 0x00, 0x00);
            ConstantBlack = Colors.Black;
            ConstantWhite = Colors.White;
        }
    }

    public class ProjectSolids
    {
        public Brush Black;
        public Brush White;
        public Brush Gray;
        public Brush SelectedElement;
        public Brush Transparent;
        public Brush LoadingBackground;
        public Brush Green;
        public Brush GreenLight;
        public Brush GreenSemiLight;
        public Brush LevelsOut;
        public Brush LevelsOutLight;
        public Brush Red;
        public Brush Yellow;
        public Brush Button;
        public Brush DarkGray;
        public Brush ConstantWhite;
        public Brush ConstantBlack;
        public Brush TitleRect;
        public Brush Background;

        public ProjectSolids(ProjectColors WindowTheme)
        {
            Black = new SolidColorBrush(WindowTheme.Black);
            White = new SolidColorBrush(WindowTheme.TimeFont);
            Gray = new SolidColorBrush(WindowTheme.Gray);
            DarkGray = new SolidColorBrush(WindowTheme.DarkGray);
            SelectedElement = new SolidColorBrush(WindowTheme.SelectedElement);
            Transparent = new SolidColorBrush(Colors.Transparent);
            LoadingBackground = new SolidColorBrush(WindowTheme.LoadingBackground);
            Green = new SolidColorBrush(WindowTheme.Green);
            GreenLight = new SolidColorBrush(WindowTheme.GreenLight);
            GreenSemiLight = new SolidColorBrush(WindowTheme.GreenSemiLight);
            Red = new SolidColorBrush(WindowTheme.Red);
            Yellow = new SolidColorBrush(WindowTheme.Yellow);
            Button = new SolidColorBrush(WindowTheme.Button);
            ConstantWhite = new SolidColorBrush(WindowTheme.ConstantWhite);
            ConstantBlack = new SolidColorBrush(WindowTheme.ConstantBlack);
            TitleRect = new SolidColorBrush(WindowTheme.TitleRect);
            Background = new SolidColorBrush(WindowTheme.Background);
            LevelsOut = new LinearGradientBrush(new GradientStopCollection(
                new List<GradientStop>()
                {
                new GradientStop(WindowTheme.Green, 1.0),
                new GradientStop(WindowTheme.Green, 0.5),
                new GradientStop(WindowTheme.Yellow, 0.2),
                new GradientStop(WindowTheme.Red, 0.0)
                }), 90.0)
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 200)
            };
            LevelsOutLight = new LinearGradientBrush(new GradientStopCollection(
                new List<GradientStop>()
                {
                    new GradientStop(WindowTheme.GreenLight, 1.0),
                    new GradientStop(WindowTheme.GreenLight, 0.5),
                    new GradientStop(WindowTheme.YellowLight, 0.2),
                    new GradientStop(WindowTheme.RedLight, 0.0)
                }), 90.0)
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 200)
            };
        }
    }
}

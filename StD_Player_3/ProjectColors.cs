using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    class ProjectColors
    {
        public static Color Green = Color.FromRgb(0x25, 0x5b, 0x25);
        public static Color GreenLight = Color.FromRgb(0x00, 0xE0, 0x3F);
        public static Color GreenSemiLight = Color.FromRgb(0x12, 0x9d, 0x32);
        public static Color Yellow = Color.FromRgb(0x9D, 0x99, 0x00);
        public static Color Red = Color.FromRgb(0x86, 0x28, 0x28);
        public static Color Black = Color.FromRgb(0x00, 0x00, 0x00);

        public static Color SelectedElement = Color.FromRgb(0x00, 0x00, 0x88);
        public static Color SelectedElementFont = Colors.White;
        public static Color TimeFont = Colors.White;

        public static Color FontColor = Colors.Black;
        public static Color TitleRect = Colors.White;
        public static Color Border = Colors.Black;

        public static Color Background = Color.FromRgb(0xF0, 0xF0, 0xF0);
        public static Color Gray = Color.FromRgb(0x80, 0x80, 0x80);

        public static Color LoadingBackground = Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF);
    }

    class ProjectSolids
    {
        public static Brush Black = new SolidColorBrush(ProjectColors.Black);
        public static Brush White = new SolidColorBrush(ProjectColors.TimeFont);
        public static Brush Gray = new SolidColorBrush(ProjectColors.Gray);
        public static Brush SelectedElement = new SolidColorBrush(ProjectColors.SelectedElement);
        public static Brush Transparent = new SolidColorBrush(Colors.Transparent);
        public static Brush LoadingBackground = new SolidColorBrush(ProjectColors.LoadingBackground);
        public static Brush Green = new SolidColorBrush(ProjectColors.Green);
        public static Brush GreenLight = new SolidColorBrush(ProjectColors.GreenLight);
        public static Brush GreenSemiLight = new SolidColorBrush(ProjectColors.GreenSemiLight);
    }
}

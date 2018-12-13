using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StD_Player_3
{
    public static class AppResources
    {
        public static BitmapImage LoadBitmapFromResources(string URI)
        {
            Uri Link = new Uri($"pack://application:,,,/{URI}", UriKind.Absolute);
            return new BitmapImage(Link)
            { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    class Drag
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Drag(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Drag(Point point)
        {
            Set(point);
        }

        public void Set(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
    }
}

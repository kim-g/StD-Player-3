using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Extentions
{
    public class Animator
    {
        /// <summary>
        /// Анимация возникновения элемента
        /// </summary>
        /// <param name="Element">Элемент интерфейса</param>
        /// <param name="Time">Время анимации</param>
        public static void Show(FrameworkElement Element, int Time = 10, int Delay = 0)
        {
            ChangeOpacity(Element, 1, Time, Delay);
        }

        /// <summary>
        /// Анимация исчезновения элемента
        /// </summary>
        /// <param name="Element">Элемент интерфейса</param>
        /// <param name="Time">Время анимации</param>
        public static void Hide(FrameworkElement Element, int Time = 10, int Delay = 0)
        {
            ChangeOpacity(Element, 0, Time, Delay);
        }

        public static void ChangeOpacity(FrameworkElement Element, double ToOpacity, int Time = 10, 
            int Delay = 0)
        {
            DoubleAnimation Animation = new DoubleAnimation(ToOpacity, TimeSpan.FromMilliseconds(Time));
            Animation.From = Element.Opacity;
            Animation.BeginTime = TimeSpan.FromMilliseconds(Delay);
            Element.BeginAnimation(UIElement.OpacityProperty, Animation);
        }

        /// <summary>
        /// Анимация изменения размера RenderTransform.Scale
        /// </summary>
        /// <param name="Element">Элемент интерфейса</param>
        /// <param name="ScaleX">Масштаб по X</param>
        /// <param name="ScaleY">Масштаб по Y</param>
        /// <param name="Time">Время анимации</param>
        public static void ScaleTo(FrameworkElement Element, double ScaleX, double ScaleY, int Time = 10, int Wait = 0)
        {


            LinearDoubleKeyFrame keyFrame1 = new LinearDoubleKeyFrame { Value = ScaleX,
                KeyTime = TimeSpan.FromMilliseconds(Time) };
            LinearDoubleKeyFrame keyFrame2 = new LinearDoubleKeyFrame { Value = ScaleY,
                KeyTime = TimeSpan.FromMilliseconds(Time) };
            var dub1 = new DoubleAnimationUsingKeyFrames();
            var dub2 = new DoubleAnimationUsingKeyFrames();
            Storyboard sb = new Storyboard();
            dub1.KeyFrames.Add(keyFrame1);
            dub2.KeyFrames.Add(keyFrame2);
            sb.Children.Add(dub1);
            sb.Children.Add(dub2);
            Storyboard.SetTarget(dub1, Element);
            Storyboard.SetTarget(dub2, Element);
            Storyboard.SetTargetProperty(dub1, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(dub2, new PropertyPath("RenderTransform.ScaleY"));
            sb.BeginTime = TimeSpan.FromMilliseconds(Wait);
            sb.Begin();
        }

        /// <summary>
        /// Анимация изменения размера RenderTransform.Scale
        /// </summary>
        /// <param name="Element">Элемент интерфейса</param>
        /// <param name="X">Конечное положение по X</param>
        /// <param name="Y">Конечное положение по Y</param>
        /// <param name="Time">Время анимации</param>
        public static void MoveTo(FrameworkElement Element, double X, double Y, int Wait, int Time = 10)
        {
            ThicknessAnimation TA = new ThicknessAnimation(new Thickness(X, Y, 0, 0),
                TimeSpan.FromMilliseconds(Time));
            TA.BeginTime = TimeSpan.FromMilliseconds(Wait);
            Element.BeginAnimation(FrameworkElement.MarginProperty, TA);
        }

        /// <summary>
        /// Анимация возникновения Grid с элементами с увеличением и возникновением
        /// </summary>
        /// <param name="ParentGrid"></param>
        /// <param name="Time"></param>
        public static void GridShow(Grid ParentGrid, int Time, EventHandler OnStop = null)
        {
            GridScaleOpacity(ParentGrid, 0.8, 1, 1, 10, Time, Time, OnStop);
        }

        /// <summary>
        /// Анимация возникновения Grid с элементами с увеличением и возникновением
        /// </summary>
        /// <param name="ParentGrid"></param>
        /// <param name="Time"></param>
        public static void GridHide(Grid ParentGrid, int Time, EventHandler OnStop = null)
        {
            GridScaleOpacity(ParentGrid, 1.0, 0.8, 0,  10, Time, Time, OnStop);
        }

        private static void GridScaleOpacity(Grid ParentGrid, double Val1, double Val2, double Opacity, 
            int Time1, int Time2, int OpacityTime, EventHandler OnStop = null)
        {
            LinearDoubleKeyFrame keyFrame1 = new LinearDoubleKeyFrame
            {
                Value = Val1,
                KeyTime = TimeSpan.FromMilliseconds(Time1)
            };
            LinearDoubleKeyFrame keyFrame2 = new LinearDoubleKeyFrame
            {
                Value = Val2,
                KeyTime = TimeSpan.FromMilliseconds(Time2)
            };
            LinearDoubleKeyFrame keyFrameOp = new LinearDoubleKeyFrame
            {
                Value = Opacity,
                KeyTime = TimeSpan.FromMilliseconds(OpacityTime)
            };
            var dub1 = new DoubleAnimationUsingKeyFrames();
            var dub2 = new DoubleAnimationUsingKeyFrames();
            var dubOp = new DoubleAnimationUsingKeyFrames();
            Storyboard sb = new Storyboard();
            dub1.KeyFrames.Add(keyFrame1);
            dub1.KeyFrames.Add(keyFrame2);
            dub2.KeyFrames.Add(keyFrame1);
            dub2.KeyFrames.Add(keyFrame2);
            dubOp.KeyFrames.Add(keyFrameOp);
            sb.Children.Add(dub1);
            sb.Children.Add(dub2);
            sb.Children.Add(dubOp);
            Storyboard.SetTarget(dub1, ParentGrid);
            Storyboard.SetTarget(dub2, ParentGrid);
            Storyboard.SetTarget(dubOp, ParentGrid);
            Storyboard.SetTargetProperty(dub1, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(dub2, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTargetProperty(dubOp, new PropertyPath("Opacity"));
            if (OnStop != null) sb.Completed += OnStop;
            sb.Begin();  
        }

        public static void NullAnimation(FrameworkElement Element, int Time = 10, EventHandler OnStop = null, string Property= "MinHeight", int Session = 0)
        {
            DispatcherTimer DT = new DispatcherTimer();
            DT.Interval = TimeSpan.FromMilliseconds(Time);
            DT.Tick += OnStop;
            DT.Tag = Session;
            DT.Tick += (object sender, EventArgs e) => { ((DispatcherTimer)sender).Stop(); };
            DT.Start();           
        }
    }
}

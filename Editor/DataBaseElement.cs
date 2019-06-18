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

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для DataBaseElement.xaml
    /// </summary>
    public abstract class DataBaseElement : UserControl
    {
        private bool selected = false;
        protected Rectangle SelectedRect;

        public DataBaseElement()
        {
            
        }

        /// <summary>
        /// Объект выделен
        /// </summary>
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                SelectedRect.Visibility = selected ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Запуск / остановка воспроизведения
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// Удаление объекта
        /// </summary>
        public abstract void Delete();
    }
}


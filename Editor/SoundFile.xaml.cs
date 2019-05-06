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
    /// Логика взаимодействия для SoundFile.xaml
    /// </summary>
    public partial class SoundFile : UserControl
    {
        private string _Title;
        private string _Comment;

        /// <summary>
        /// Заголовок файла
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                TitleText.Text = _Title;
            }
        }

        /// <summary>
        /// Заголовок файла
        /// </summary>
        public string Comment
        {
            get { return _Comment; }
            set
            {
                _Comment = value;
                TitleText.Text = _Comment;
            }
        }

        public SoundFile()
        {
            InitializeComponent();
        }
    }
}

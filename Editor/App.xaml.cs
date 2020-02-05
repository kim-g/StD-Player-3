using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Editor
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            string fileName = e.Args?.FirstOrDefault();
            EditorWindow mainWindow;
            if (!string.IsNullOrWhiteSpace(fileName))
                mainWindow = new EditorWindow(fileName);
            else
                mainWindow = new EditorWindow();

            mainWindow.Show();
        }
    }
}

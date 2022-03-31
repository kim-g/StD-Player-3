using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    /// <summary>
    /// Результаты настройки программы.
    /// </summary>
    public class SetupResult
    {
        /// <summary>
        /// Тип протокола передачи данных
        /// </summary>
        public SoundType Type { get; set; }

        /// <summary>
        /// Список настроек дек.
        /// </summary>
        public DeskOptions[] Desks { get; set; } = new DeskOptions[3];
    }
}

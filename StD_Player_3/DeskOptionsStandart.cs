using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    /// <summary>
    /// Сохраняет и передаёт настройки аппаратной части деки в режиме Standart
    /// </summary>
    public class DeskOptionsStandart: DeskOptions
    {
        /// <summary>
        /// Настройка каналов устройства
        /// </summary>
        public int Balance { get; set; }
    }
}

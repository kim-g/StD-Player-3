using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    /// <summary>
    /// Сохраняет и передаёт настройки аппаратной части деки
    /// </summary>
    public class DeskOptions
    {
        /// <summary>
        /// Номер устройства в списке устройств
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// Имя устройства для БД
        /// </summary>
        public string DeviceName { get; set; }
    }
}

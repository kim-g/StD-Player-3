using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    /// <summary>
    /// Звуковое устройство. Класс для элементов ComboBox
    /// </summary>
    public class AudioDevice
    {
        /// <summary>
        /// Номер устройства в списке
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Название устройства
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Индивидуальный номер устройства в системе VEN
        /// </summary>
        public string ADID { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

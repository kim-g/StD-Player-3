using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StD_Player_3
{
    /// <summary>
    /// Сохраняет и передаёт настройки аппаратной части деки в режиме ASIO
    /// </summary>
    public class DeskOptionsASIO: DeskOptions
    {
        public byte ChannelsNumber = 8;
        
        /// <summary>
        /// Список выводных каналов
        /// </summary>
        public bool[] OutputChannels { get; set; }

        /// <summary>
        /// Номер устройства воспроизведения в списке устройств
        /// </summary>
        public int StandartDeviceID { get; set; }

        /// <summary>
        /// Имя устройства воспроизведения для БД
        /// </summary>
        public string StandartDeviceName { get; set; }

        public DeskOptionsASIO(byte Number = 8)
        {
            ChannelsNumber = Number;
            OutputChannels = new bool[Number];
        }

        /// <summary>
        /// Получение списка номеров каналов для вывода
        /// </summary>
        /// <returns></returns>
        public int[] GetChannels()
        {
            List<int> channels = new List<int>();
            for (int i = 0; i < ChannelsNumber; i++)
                if (OutputChannels[i])
                    channels.Add(i);
            return channels.ToArray();
        }

        /// <summary>
        /// Выдаёт список каналов в виде числа
        /// </summary>
        /// <returns></returns>        
        public int ChannelsToInt()
        {
            int Result = 0;
            for (int i = 0; i < ChannelsNumber; i++)
                Result += OutputChannels[i] ? (int)Math.Pow(2, i) : 0;
            return Result;
        }
    }
}

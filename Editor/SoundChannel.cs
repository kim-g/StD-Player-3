using System;
using System.Windows;
using Un4seen.Bass;

namespace Editor
{
    public class SoundChannel : SoundBase
    {
        /// <summary>
        /// Инициализация звуковой карты в стандартном режиме.
        /// </summary>
        /// <param name="SoundCard"></param>
        /// <param name="BitRate"></param>
        /// <param name="DeviceProperties"></param>
        public static void Initiate(int SoundCard = -1, int BitRate = 44100,
            BASSInit DeviceProperties = BASSInit.BASS_DEVICE_DEFAULT)
        {
            Bass.BASS_Init(SoundCard, BitRate, DeviceProperties, IntPtr.Zero);
        }

        /// <summary>
        /// Создание нового экземпляра SoundChannel
        /// </summary>
        /// <param name="balance"></param>
        /// <param name="volume"></param>
        public SoundChannel(int balance=0, int volume=100): base(balance, volume)
        {
           
        }

        /// <summary>
        /// Получение текущей позиции.
        /// </summary>
        /// <param name="Chanel">Канал</param>
        /// <returns></returns>
        protected override long GetPosition()
        {
            return Bass.BASS_ChannelGetPosition(Channel);
        }

        /// <summary>
        /// Задание новой позиции.
        /// </summary>
        /// <param name="Chanel">Канал</param>
        /// <param name="NewPosition">Новоя позиция</param>
        protected override void SetPosition(long NewPosition)
        {
            Bass.BASS_ChannelSetPosition(Channel, NewPosition);
        }

        /// <summary>
        /// Установка устройства для воспроизведения
        /// </summary>
        /// <param name="device">Номер устройства</param>
        /// <returns></returns>
        protected override bool SetDevice(int device)
        {
            if (device == -1) return true;
            if (Bass.BASS_SetDevice(device))
                return true;
            MessageBox.Show(Bass.BASS_ErrorGetCode().ToString());
            return false;
        }

        /// <summary>
        /// Выдаёт длину трека
        /// </summary>
        /// <returns></returns>
        protected override long GetLength()
        {
            return Bass.BASS_ChannelGetLength(Channel);
        }

        /// <summary>
        /// Выполнение функции Play библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected override bool ChannelPlay(int Channel, bool repeate)
        {
            return Bass.BASS_ChannelPlay(Channel, repeate);
        }

        /// <summary>
        /// Выполнение функции Play библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected override bool ChannelPause(int Channele)
        {
            return Bass.BASS_ChannelPause(Channel);
        }

        /// <summary>
        /// Выполнение функции Stop библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected override bool ChannelStop(int Channel)
        {
            return Bass.BASS_ChannelStop(Channel);
        }

        /// <summary>
        /// Задаёт громкость в процентах
        /// </summary>
        /// <param name="volume">Громкость</param>
        public override void SetVolume(int volume)
        {
            Volume = volume;
            Bass.BASS_SetVolume(Volume / 100f);
        }
    }

}

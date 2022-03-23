using System;
using Un4seen.Bass;
using Un4seen.BassAsio;

namespace Sound
{
    public class ASIO_Channel : SoundBase
    {
        private ASIOPROC _myAsioProc; // make it global, so that it can not be removed by the Garbage Collector
        private BassAsioHandler AsioChannel;
        private BassAsioHandler _asio1;

        public int[] OutputChannels;


        /// <summary>
        /// Инициализация звуковой карты в стандартном режиме.
        /// </summary>
        /// <param name="SoundCard"></param>
        /// <param name="BitRate"></param>
        /// <param name="DeviceProperties"></param>
        public static void Initiate(int SoundCard = -1, int BitRate = 44100,
            BASSInit DeviceProperties = BASSInit.BASS_DEVICE_DEFAULT)
        {
            Bass.BASS_Free();
            BassAsio.BASS_ASIO_Free();
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0);
            Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            int ASIOCard = SoundCard == -1 ? 0 : SoundCard - 1;
            BassAsio.BASS_ASIO_Init(0, BASSASIOInit.BASS_ASIO_THREAD);
            BASSError Error = BassAsio.BASS_ASIO_ErrorGetCode();

        }

        /// <summary>
        /// Создание нового экземпляра SoundChannel
        /// </summary>
        /// <param name="outputChannels">Список выводных каналов ASIO. Первый смитается основным, остальные - зеркала. </param>
        /// <param name="volume">Громкость</param>
        public ASIO_Channel(int[] outputChannels, int volume = 100) : base(0, volume)
        {
            OutputChannels =  outputChannels;
            AudioChannel = BASSFlag.BASS_STREAM_DECODE;
        }

        /// <summary>
        /// Создание нового экземпляра SoundChannel
        /// </summary>
        /// <param name="OutputChannel">Выводной канал ASIO </param>
        /// <param name="volume">Громкость</param>
        public ASIO_Channel(int OutputChannel, int volume = 100) : base(0, volume)
        {
            OutputChannels = new int[1] { OutputChannel };
            AudioChannel = BASSFlag.BASS_STREAM_DECODE;
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
            return BassAsio.BASS_ASIO_SetDevice(device);
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
            return AsioChannel.Start(1000, 2);
        }

        /// <summary>
        /// Выполнение функции Play библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected override bool ChannelPause(int Channele)
        {
            return AsioChannel.Pause(true);
        }

        /// <summary>
        /// Выполнение функции Stop библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected override bool ChannelStop(int Channel)
        {
            return AsioChannel.Stop();
        }

        /// <summary>
        /// Задаёт громкость в процентах
        /// </summary>
        /// <param name="volume">Громкость</param>
        public override void SetVolume(int volume)
        {
            Volume = volume;
            if (AsioChannel != null) AsioChannel.Volume = volume / 100f;
        }

        public override void ConnectToSoundProtocol()
        {
            if (Channel == 0)
            {
                new Exception("Ошибка файла. Файл не был загружен.");
                return;
            }

            AsioChannel = new BassAsioHandler(0, 0, Channel);
            /*if (OutputChannels.Length > 1)
                for ( int i = 1; i < OutputChannels.Length; i++)    
                    AsioChannel.SetMirror(OutputChannels[i]);*/
            AsioChannel.Volume = Volume;
            AsioChannel.VolumeMirror = Volume;
        }
    }
}

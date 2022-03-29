using System;
using Un4seen.Bass;
using Un4seen.BassAsio;

namespace Sound
{
    public class ASIO_Channel : SoundBase
    {
        private BassAsioHandler AsioChannel;
        private int Device = 0;
        public int[] OutputChannels;


        /// <summary>
        /// Инициализация звуковой карты в стандартном режиме.
        /// </summary>
        /// <param name="SoundCard"></param>
        /// <param name="BitRate"></param>
        /// <param name="DeviceProperties"></param>
        public static void Initiate(int SoundCard = -1, int BitRate = 48000,
            BASSInit DeviceProperties = BASSInit.BASS_DEVICE_DEFAULT)
        {
            Bass.BASS_Free();
            BassAsio.BASS_ASIO_Free();
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 0);
            Bass.BASS_Init(SoundCard, BitRate, DeviceProperties, IntPtr.Zero);
            int ASIOCard = SoundCard == -1 ? 0 : SoundCard - 1;
            BassAsio.BASS_ASIO_Init(SoundCard, BASSASIOInit.BASS_ASIO_THREAD);
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
            Bass.BASS_SetDevice(device);
            Device = device - 1;
            return BassAsio.BASS_ASIO_SetDevice(Device);
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
            if (!State) return AsioChannel.Pause(false);
            return true;
        }

        /// <summary>
        /// Выполнение функции Play библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected override bool ChannelPause(int Channele)
        {
            return AsioChannel.Pause(State);
        }

        /// <summary>
        /// Выполнение функции Stop библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected override bool ChannelStop(int Channel)
        {
            AsioChannel.Pause(true);
            return true;
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
            AsioChannel = new BassAsioHandler(Device, OutputChannels[0], Channel);
            AsioChannel.Start(0, 0);
            AsioChannel.Pause(true);
            if (OutputChannels.Length > 1)
                for ( int i = 1; i < OutputChannels.Length; i++)    
                    AsioChannel.SetMirror(OutputChannels[i]);
            AsioChannel.Volume = Volume / 100f;
            AsioChannel.VolumeMirror = AsioChannel.Volume;
        }

        public override int[] Levels()
        {
            float Level = BassAsio.BASS_ASIO_ChannelGetLevel(false, OutputChannels[0]);
            int[] levels = new int[2];
            int level = Convert.ToInt32(Level * 32768);
            levels[0] = level;
            levels[1] = level;
            return levels;
        }
    }
}

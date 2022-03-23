using System;
using Un4seen.Bass;
using Un4seen.BassAsio;

namespace Sound
{
    public class ASIO_Channel : SoundBase
    {
        private ASIOPROC _myAsioProc; // make it global, so that it can not be removed by the Garbage Collector
        private BassAsioHandler AsioChannel;

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
            Bass.BASS_Init(SoundCard, BitRate, DeviceProperties, IntPtr.Zero);
            int ASIOCard = SoundCard == -1 ? 0 : SoundCard - 1;
            BassAsio.BASS_ASIO_Init(ASIOCard, BASSASIOInit.BASS_ASIO_DEFAULT);
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
            AudioChannel = BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT;
        }

        /// <summary>
        /// Создание нового экземпляра SoundChannel
        /// </summary>
        /// <param name="OutputChannel">Выводной канал ASIO </param>
        /// <param name="volume">Громкость</param>
        public ASIO_Channel(int OutputChannel, int volume = 100) : base(0, volume)
        {
            OutputChannels = new int[1] { OutputChannel };
            AudioChannel = BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT;
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
            return BassAsio.BASS_ASIO_SetDevice(0);
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

        public override void ConnectToSoundProtocol()
        {
            if (Channel == 0)
            {
                new Exception("Ошибка файла. Файл не был загружен.");
                return;
            }

            AsioChannel = new BassAsioHandler(SoundCard, 0, Channel);
            if (OutputChannels.Length > 1)
                for ( int i = 1; i < OutputChannels.Length; i++)    
                    AsioChannel.SetMirror(OutputChannels[i]);
        }

        protected override void MethodPlay()
        {
            AsioChannel.Start(0,0);
        }

        protected override void MethodPause()
        {
            AsioChannel.Pause(true);
        }

        protected override void MethodStop()
        {
            AsioChannel.Stop();
        }
    }
}

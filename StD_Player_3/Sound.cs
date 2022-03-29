using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Un4seen.Bass;

namespace Sound
{
    public abstract class SoundBase
    {
        public int Channel { get; protected set; } = 0;
        public int Balance { get; protected set; } = 0;
        public int Volume { get; protected set; } = 100;
        protected bool Repeate = false;
        public bool State { get; protected set; } = false;
        protected long Length = 0;
        protected System.Windows.Threading.DispatcherTimer timer;
        public event EventHandler AutoStop;
        public event EventHandler OnStop;
        public event EventHandler OnPause;
        public event EventHandler OnPlay;
        protected int _SoundCard;
        protected long LastPos;
        protected BASSFlag AudioChannel = BASSFlag.BASS_SPEAKER_FRONT;

        /// <summary>
        /// Получение текущей позиции.
        /// <returns></returns>
        protected abstract long GetPosition();

        /// <summary>
        /// Задание новой позиции.
        /// </summary>
        /// <param name="Chanel">Канал</param>
        /// <param name="NewPosition">Новоя позиция</param>
        protected abstract void SetPosition(long NewPosition);

        /// <summary>
        /// Установка устройства для воспроизведения
        /// </summary>
        /// <param name="device">Номер устройства</param>
        /// <returns></returns>
        protected abstract bool SetDevice(int device);

        /// <summary>
        /// Выдаёт длину трека
        /// </summary>
        /// <returns></returns>
        protected abstract long GetLength();

        /// <summary>
        /// Выполнение функции Play библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected abstract bool ChannelPlay(int Channel, bool repeate);

        /// <summary>
        /// Выполнение функции Pause библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected abstract bool ChannelPause(int Channel);

        /// <summary>
        /// Выполнение функции Stop библиотеки BASS
        /// </summary>
        /// <returns></returns>
        protected abstract bool ChannelStop(int Channel);

        /// <summary>
        /// Задаёт громкость в процентах
        /// </summary>
        /// <param name="volume">Громкость</param>
        public abstract void SetVolume(int volume);

        /// <summary>
        /// Добавляет канал в протокол воспроизведения, если это требуется.
        /// </summary>
        public abstract void ConnectToSoundProtocol();

        /// <summary>
        /// Определяет уровень громкости
        /// </summary>
        /// <returns></returns>
        public abstract int[] Levels();

        /// <summary>
        /// Определяет позицию трека в промилле (десятые процента)
        /// </summary>
        public int Position
        {
            get
            {
                if (Channel == 0) return 0;
                return Convert.ToInt32(Math.Round(GetPosition() * 1000f / Length));
            }

            set
            {
                if (Channel == 0) return;
                int Pos = value;
                if (value < 0) Pos = 0;
                if (value > 1000) Pos = 1000;

                SetPosition(Pos * Length / 1000);
            }
        }

        public int SoundCard
        {
            get { return _SoundCard; }
            set
            {
                _SoundCard = value;
                SetDevice(_SoundCard);
            }
        }

        public SoundBase(int balance = 0, int volume = 100)
        {
            Balance = balance;
            Volume = volume;

            timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal);
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timer.Start();
        }

        /// <summary>
        /// Событие для определения остановки по достижении финала
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerTick(object sender, EventArgs e)
        {
            if (!State) return;

            long Pos = GetPosition();
            if (Pos >= Length/* || Pos == LastPos*/)
            {
                State = false;
                Stop();
                OnAutoStop(new EventArgs());
            }
            LastPos = Pos;
        }

        /// <summary>
        /// Событие остановки по окончанию.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAutoStop(EventArgs e)
        {
            AutoStop?.Invoke(this, e);
        }

        /// <summary>
        /// Открыть файл для проигрывания из файла.
        /// </summary>
        /// <param name="FileName">Имя файла для открытия</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public void Open(string FileName, bool repeate = false)
        {
            SetDevice(SoundCard);
            Repeate = repeate;
            BASSFlag Loop = repeate
                ? BASSFlag.BASS_MUSIC_LOOP
                : BASSFlag.BASS_DEFAULT;
            Channel = Bass.BASS_StreamCreateFile(FileName, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | Loop | AudioChannel);
            SetOpenParameters();

            ConnectToSoundProtocol();
        }

        /// <summary>
        /// Настройка параметров при открытии
        /// </summary>
        private void SetOpenParameters()
        {
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100f);
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_PAN, Balance);
            SetBalance(Balance);
            Length = GetLength();
        }

        /// <summary>
        /// Открыть файл для проигрывания из потока.
        /// </summary>
        /// <param name="FileStream">Поток</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public void Open(Stream FileStream, bool repeate = false)
        {
            Bass.BASS_StreamFree(Channel);
            long length = FileStream.Length;
            // create the buffer which will keep the file in memory
            byte[] buffer = new byte[length];
            // read the file into the buffer
            FileStream.Position = 0;
            FileStream.Read(buffer, 0, (int)length);
            // buffer is filled, file can be closed

            Open(buffer, repeate);
        }

        /// <summary>
        /// Открыть файл для проигрывания из массива байтов (byte[]).
        /// </summary>
        /// <param name="ByteStream">массива байтов</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public void Open(byte[] ByteStream, bool repeate = false)
        {
            SetDevice(SoundCard);
            GCHandle _hGCFile;
            Repeate = repeate;
            BASSFlag Loop = repeate
                ? BASSFlag.BASS_MUSIC_LOOP
                : BASSFlag.BASS_DEFAULT;

            _hGCFile = GCHandle.Alloc(ByteStream, GCHandleType.Pinned);
            // create the stream (AddrOfPinnedObject delivers the necessary IntPtr)
            Channel = Bass.BASS_StreamCreateFile(_hGCFile.AddrOfPinnedObject(),
                              0L, ByteStream.Length, BASSFlag.BASS_SAMPLE_FLOAT | Loop | AudioChannel);
            Bass.BASS_ErrorGetCode();
            SetOpenParameters();

            ConnectToSoundProtocol();
        }

        /// <summary>
        /// Запуск воспроизведения
        /// </summary>
        public void Play()
        {
            if (Channel == 0) return;
            ChannelPlay(Channel, false);
            State = true;
            DoOnPlay(null);
        }

        protected virtual void DoOnPlay(object InObject)
        {
            OnPlay?.Invoke(this, new EventArgs());
        }

        public void Pause()
        {
            if (State)
            {
                ChannelPause(Channel);
                State = false;
                DoOnPause(null);
            }
            else
            {
                ChannelPlay(Channel, false);
                State = true;
                DoOnPlay(null);
            }
        }

        protected virtual void DoOnPause(object InObject)
        {
            OnPause?.Invoke(this, new EventArgs());
        }

        public void Stop()
        {
            if (State)
            {
                ChannelStop(Channel);
                State = false;
            }

            SetPosition(0);
            DoOnStop(null);
        }

        protected virtual void DoOnStop(object InObject)
        {
            OnStop?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Возвращает текщее время трека в формате MM:SS или "--:--", если трек не загружен
        /// </summary>
        /// <returns></returns>
        public string PositionTime()
        {
            if (Channel == 0) return "--:--";

            double PosSeconds = Bass.BASS_ChannelBytes2Seconds(Channel, GetPosition());
            int PosMinute = Convert.ToInt32(Math.Floor(PosSeconds / 60));
            int PosSecond = Convert.ToInt32(Math.Floor(PosSeconds - (PosMinute * 60)));
            string Pos = PosMinute.ToString("D2") + ":" + PosSecond.ToString("D2");
            return Pos;
        }

        /// <summary>
        /// Возвращает время трека в формате MM:SS или "--:--", если трек не загружен
        /// </summary>
        /// <param name="position">Время в промилле (десятые процента)</param>
        public string PositionTime(int position)
        {
            if (Channel == 0) return "--:--";

            double PosSeconds = Bass.BASS_ChannelBytes2Seconds(Channel,
                position * Length / 1000);
            int PosMinute = Convert.ToInt32(Math.Floor(PosSeconds / 60f));
            int PosSecond = Convert.ToInt32(Math.Floor(PosSeconds - (PosMinute * 60)));
            string Pos = PosMinute.ToString("D2") + ":" + PosSecond.ToString("D2");
            return Pos;
        }

        /// <summary>
        /// Возвращает длину трека в формате MM:SS или "--:--", если трек не загружен
        /// </summary>
        /// <returns></returns>
        public string LengthTime()
        {
            if (Channel == 0) return "--:--";

            double PosSeconds = Bass.BASS_ChannelBytes2Seconds(Channel, Length);
            int PosMinute = Convert.ToInt32(Math.Floor(PosSeconds / 60f));
            int PosSecond = Convert.ToInt32(Math.Round(PosSeconds - (PosMinute * 60)));
            string Pos = PosMinute.ToString("D2") + ":" + PosSecond.ToString("D2");
            return Pos;
        }

        /// <summary>
        /// Установка баланса
        /// </summary>
        /// <param name="balance"></param>
        public void SetBalance(int balance)
        {
            Balance = balance;
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_PAN, Balance);
            //AudioChannel = BASSFlag.BASS_SPEAKER_FRONT;
            /*switch (Balance)
            {
                case -1: AudioChannel = BASSFlag.BASS_SPEAKER_FRONTLEFT; break;
                case 0: AudioChannel = BASSFlag.BASS_SPEAKER_FRONT; break;
                case 1: AudioChannel = BASSFlag.BASS_SPEAKER_FRONTRIGHT; break;

            }*/
        }

        /// <summary>
        /// Выдаёт номер канала
        /// </summary>
        /// <returns></returns>
        public int GetChannel()
        {
            return Channel;
        }
    }
}

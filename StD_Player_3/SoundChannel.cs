using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace StD_Player_3
{
    class SoundChannel
    {
        private int Channel = 0;
        int Balance = 0;
        int Volume = 100;

        public static void Initiate(int SoundCard = -1, int BitRate = 44100,
            BASSInit DeviceProperties = BASSInit.BASS_DEVICE_DEFAULT)
        {
            Bass.BASS_Init(SoundCard, BitRate, DeviceProperties, IntPtr.Zero);
        }

        public SoundChannel(int balance=0, int volume=100)
        {
            Balance = balance;
            Volume = volume;
        }

        public void Open(string FileName)
        {
            Channel = Bass.BASS_StreamCreateFile(FileName, 0, 0, BASSFlag.BASS_DEFAULT);
        }

        public void Play(bool Repeate = false)
        {
            if (Channel == 0) return;
            Bass.BASS_ChannelPlay(Channel, Repeate);
        }
    }
}

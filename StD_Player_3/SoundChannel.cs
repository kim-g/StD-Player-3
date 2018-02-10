﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace StD_Player_3
{
    class SoundChannel
    {
        protected int Channel = 0;
        protected int Balance = 0;
        protected int Volume = 100;
        protected bool Repeate = false;
        protected bool State = false;
        protected long Length = 0;

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

        /// <summary>
        /// Открыть файл для проигрывания из файла.
        /// </summary>
        /// <param name="FileName">Точка, от которой считаем расстояние</param>
        public void Open(string FileName, bool repeate = false)
        {
            Repeate = repeate;
            Channel = Bass.BASS_StreamCreateFile(FileName, 0, 0, BASSFlag.BASS_DEFAULT);
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100f);
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_PAN, Balance);
            Length = Bass.BASS_ChannelGetLength(Channel);
        }

        public void Open(Stream FileStream, bool repeate = false)
        {
            GCHandle _hGCFile;
            Repeate = repeate;
            long length = FileStream.Length;
            // create the buffer which will keep the file in memory
            byte[] buffer = new byte[length];
            // read the file into the buffer
            FileStream.Read(buffer, 0, (int)length);
            // buffer is filled, file can be closed
            FileStream.Close();

            _hGCFile = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            // create the stream (AddrOfPinnedObject delivers the necessary IntPtr)
            Channel = Bass.BASS_StreamCreateFile(_hGCFile.AddrOfPinnedObject(),
                              0L, length, BASSFlag.BASS_SAMPLE_FLOAT);
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100f);
            Bass.BASS_ChannelSetAttribute(Channel, BASSAttribute.BASS_ATTRIB_PAN, Balance);
            Length = Bass.BASS_ChannelGetLength(Channel);
        }

        public void Play()
        {
            if (Channel == 0) return;
            Bass.BASS_ChannelPlay(Channel, Repeate);
            State = true;
        }

        public void Pause()
        {
            if (State)
            {
                Bass.BASS_ChannelPause(Channel);
                State = false;
            }
            else
            {
                Bass.BASS_ChannelPlay(Channel, Repeate);
                State = true;
            }
        }

        public void Stop()
        {
            if (State)
            {
                Bass.BASS_ChannelStop(Channel);
                State = false;
            }

            Bass.BASS_ChannelSetPosition(Channel, 0f);
        }

        protected long BytePosition()
        {
            return Bass.BASS_ChannelGetPosition(Channel);
        }

        public int Position()
        {
            if (Channel == 0) return 0;
            return Convert.ToInt32(Bass.BASS_ChannelGetPosition(Channel) * 1000 / Length);
        }

        public string LengthTime()
        {
            if (Channel == 0) return "--:--";

            return "";
        }

        public void SetVolume(int volume)
        {
            Volume = volume;
            Bass.BASS_SetVolume(Volume / 100f);
        }
    }

}

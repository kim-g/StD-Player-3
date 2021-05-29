using System;
using System.IO;

namespace SQLite
{
    //Класс одного муз. файла
    public class MusicTrack : IDisposable
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public Stream Data { get; set; }
        public bool Repeat { get; set; }

        /// <summary>
        /// Открыть файл из файла
        /// </summary>
        /// <param name="FileName">Имя файла для открытия</param>
        /// <param name="number">Номер трека</param>
        /// <param name="name">Название трека</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public MusicTrack(string FileName, string number, string name, bool repeat)
        {
            Number = number;
            Name = name;
            Repeat = repeat;
            using (FileStream FS = new FileStream(FileName, FileMode.Open))
            {
                Data = new MemoryStream();
                FS.CopyTo(Data);
                FS.Close();
            }
        }

        /// <summary>
        /// Открыть файл из файла
        /// </summary>
        /// <param name="FileStream">Имя файла для открытия</param>
        /// <param name="number">Номер трека</param>
        /// <param name="name">Название трека</param>
        /// <param name="repeate">Повторять ли звук после окончания</param>
        public MusicTrack(Stream FileStream, string number, string name, bool repeat)
        {
            Number = number;
            Name = name;
            Repeat = repeat;
            Data = new MemoryStream();
            if (FileStream != null) FileStream.CopyTo(Data);
        }

        /// <summary>
        /// Получить номер трека и его название
        /// </summary>
        public string FullName()
        {
            return Number + " — " + Name;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Data.Dispose();
                Data = null;
                this.Name = null;
                this.Number = null;
            }
        }
    }
}

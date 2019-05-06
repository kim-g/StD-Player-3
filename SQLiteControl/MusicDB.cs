using System.Collections.Generic;
using SQLite;
using System.Data;
using System.IO;
using System;

namespace SQLite
{
    public class MusicDB : SQLiteDataBase
    {
        const string CreationQuery = @"BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS `info` (
	`id`	INTEGER PRIMARY KEY AUTOINCREMENT,
	`name`	TEXT,
	`description`	TEXT,
	`version`	INTEGER DEFAULT 1,
	`last_d_1`	INTEGER,
	`last_d_2`	INTEGER
);
CREATE TABLE IF NOT EXISTS `files` (
	`id`	INTEGER PRIMARY KEY AUTOINCREMENT,
	`title`	TEXT,
	`comment`	TEXT,
	`cycle`	INTEGER,
	`file`	BLOB
);
CREATE TABLE IF NOT EXISTS `desk` (
	`id`	INTEGER PRIMARY KEY AUTOINCREMENT,
	`desk_n`	INTEGER,
	`number`	TEXT,
	`file`	INTEGER,
	`title`	TEXT,
	`order`	INTEGER
);
COMMIT;";

        public MusicDB(string FileName) : base (FileName)
        {
            if (File.Exists(FileName))
                OpenDB();
            else
                CreateDB(CreationQuery);
        }

        public string Name
        {
            get
            {
                DataTable DT = ReadTable("SELECT name FROM info ORDER BY id DESC LIMIT 1");
                return DT.Rows[0].ItemArray[0].ToString();
            }
        }

        public List<MusicTrack> LoadDesk(byte DeskN)
        {
            List<MusicTrack> DeskList = new List<MusicTrack>();

            DataTable DeskData = ReadTable(@"SELECT desk.id, desk.number, files.file, 
	case when desk.title='' then files.title else desk.title end title, files.cycle  
FROM files INNER JOIN desk ON (desk.file = files.id) 
WHERE desk.desk_n = " + DeskN.ToString() + @" 
ORDER BY desk.`order`");

            for (int i=0; i< DeskData.Rows.Count; i++)
            {
                Stream MS = new MemoryStream();
                MS.Write((byte[])DeskData.Rows[i].ItemArray[2], 0,
                    ((byte[])DeskData.Rows[i].ItemArray[2]).Length);
                MS.Position = 0;

                MusicTrack MT = new MusicTrack(MS,
                    DeskData.Rows[i].ItemArray[1].ToString(), DeskData.Rows[i].ItemArray[3].ToString(),
                    DeskData.Rows[i].ItemArray[4].ToString() == "1");
                DeskList.Add(MT);
            }

            return DeskList;
        }
    }

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
            FileStream FS = new FileStream(FileName, FileMode.Open);
            Data = new MemoryStream();
            FS.CopyTo(Data);
            FS.Close();
            FS.Dispose();
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
            FileStream.CopyTo(Data);
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

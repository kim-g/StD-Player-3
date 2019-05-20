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

        /// <summary>
        /// Название спектакля
        /// </summary>
        public string Name
        {
            get
            {
                DataTable DT = ReadTable("SELECT name FROM info ORDER BY id DESC LIMIT 1");
                return DT?.Rows[0]?.ItemArray[0]?.ToString();
            }
            set
            {
                Execute($"UPDATE info SET name='{value}'");
            }
        }

        /// <summary>
        /// Комментарий к спектаклю
        /// </summary>
        public string Comment
        {
            get
            {
                DataTable DT = ReadTable("SELECT description FROM info ORDER BY id DESC LIMIT 1");
                return DT?.Rows[0]?.ItemArray[0]?.ToString();
            }
            set
            {
                Execute($"UPDATE info SET description='{value}'");
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

        /// <summary>
        /// Выдаёт список файлов в таблице files
        /// </summary>
        /// <returns></returns>
        public List<MusicFile> GetFiles(string Where = null)
        {
            List<MusicFile> Result = new List<MusicFile>();
            string WhereCase = Where == null ? "" : $" WHERE {Where}";
            DataTable DT = ReadTable($"SELECT `id`, `title`, `comment`, `cycle` FROM `files`{Where};");

            foreach (DataRow row in DT.Rows)
            {
                MusicFile MF = new MusicFile(this, 
                    Convert.ToInt32( row.ItemArray[DT.Columns.IndexOf("id")]),
                    row.ItemArray[DT.Columns.IndexOf("title")].ToString(),
                    row.ItemArray[DT.Columns.IndexOf("comment")].ToString(),
                    row.ItemArray[DT.Columns.IndexOf("cycle")].ToString() == "1");
                Result.Add(MF);
            }

            return Result;
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

    public class MusicFile : IDisposable
    {
        private MusicDB DB;
        private string title;
        private string comment;
        private bool cycle;

        /// <summary>
        /// Создание пустого экземпляра
        /// </summary>
        public MusicFile(MusicDB db, int id, string title, string comment, bool cycle)
        {
            DB = db;
            ID = id;
            this.title = title;
            this.comment = comment;
            this.cycle = cycle;
        }

        public MusicFile(MusicDB db, int id)
        {
            DB = db;
            ID = id;
            Update();
        }

        /// <summary>
        /// Номер в БД
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Заголовок файла
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                DB.Execute($"UPDATE `files` SET `title`='{title}' WHERE `id`={ID};");
            }
        }

        /// <summary>
        /// Комментарий к файлу
        /// </summary>
        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value;
                DB.Execute($"UPDATE `files` SET `comment`='{comment}' WHERE `id`={ID};");
            }
        }

        /// <summary>
        /// Указывает, что звук должен быть зациклен
        /// </summary>
        public bool Cycle
        {
            get { return cycle; }
            set
            {
                cycle = value;
                string C = cycle ? "1" : "0";
                DB.Execute($"UPDATE `files` SET `cycle`={C} WHERE `id`={ID};");
            }
        }

        public MemoryStream Data
        {
            get
            {
                DataTable DeskData = DB.ReadTable($"SELECT `file` FROM `files` WHERE `id`={ID} LIMIT 1");
                if (DeskData.Rows.Count < 0) return null;

                MemoryStream MS = new MemoryStream();
                MS.Write((byte[])DeskData.Rows[0].ItemArray[0], 0,
                    ((byte[])DeskData.Rows[0].ItemArray[0]).Length);
                MS.Position = 0;
                return MS;
            }

            set
            {
                DB.ExecuteBLOB($"UPDATE `files` SET `file`=@BLOB WHERE `id`={ID}", value);
            }
        }

        /// <summary>
        /// Загрузить все данные из БД
        /// </summary>
        public void Update()
        {
            DataTable dt = DB.ReadTable($"SELECT `title`, `comment`, `cycle` FROM `files` WHERE `id`={ID} LIMIT 1;");
            if (dt.Rows.Count == 0)
            {
                title = "";
                comment = "";
                cycle = false;
                return;
            }
            title = dt.Rows[0].ItemArray[dt.Columns.IndexOf("title")].ToString();
            comment = dt.Rows[0].ItemArray[dt.Columns.IndexOf("comment")].ToString();
            cycle = dt.Rows[0].ItemArray[dt.Columns.IndexOf("comment")].ToString() == "1";
        }

        public void Delete()
        {
            DB.Execute($"DELETE FROM `desk` WHERE `file`={ID};");
            DB.Execute($"DELETE FROM `files` WHERE `id`={ID};");
            Dispose();
        }


        /// <summary>
        /// Удаление экземпляра
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                title = null;
                comment = null;
                DB = null;
            }
        }
    }

}

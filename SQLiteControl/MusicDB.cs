using System.Collections.Generic;
using SQLite;
using System.Data;
using System.IO;
using System;
using System.Globalization;

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
        readonly CultureInfo EnCI = new CultureInfo("en-US");

        public MusicDB(string FileName) : base (FileName)
        {
            if (File.Exists(FileName))
                OpenDB();
            else
            {
                CreateDB(CreationQuery);
                Initiate();
            }
        }

        /// <summary>
        /// Название спектакля
        /// </summary>
        public string Name
        {
            get
            {
                DataTable DT = ReadTable("SELECT name FROM info ORDER BY id DESC LIMIT 1");
                if (DT?.Rows.Count == 0)
                {
                    Initiate();
                    DT.Dispose();
                    return "";
                }
                string Out = DT?.Rows[0]?.ItemArray[0]?.ToString();
                DT.Dispose();
                return Out;
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
                if (DT?.Rows.Count == 0)
                {
                    Initiate();
                    DT.Dispose();
                    return "";
                }
                string Out = DT?.Rows[0]?.ItemArray[0]?.ToString();
                DT.Dispose();
                return Out;
            }
            set
            {
                Execute($"UPDATE info SET description='{value}'");
            }
        }

        /// <summary>
        /// Производит загрузку деки
        /// </summary>
        /// <param name="DeskN"></param>
        /// <returns></returns>
        public List<MusicTrack> LoadDesk(byte DeskN)
        {
            List<MusicTrack> DeskList = new List<MusicTrack>();

            DataTable DeskData = ReadTable(@"SELECT desk.id, desk.number, files.file, 
	case when desk.title='' then files.title else desk.title end title, files.cycle  
FROM files INNER JOIN desk ON (desk.file = files.id) 
WHERE desk.desk_n = " + DeskN.ToString(EnCI) + @" 
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
                MS.Dispose();
            }

            DeskData.Dispose();

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
            DataTable DT = ReadTable($"SELECT `id`, `title`, `comment`, `cycle` FROM `files`{WhereCase};");

            foreach (DataRow row in DT.Rows)
            {
                MusicFile MF = new MusicFile(this, 
                    Convert.ToInt32( row.ItemArray[DT.Columns.IndexOf("id")], EnCI),
                    row.ItemArray[DT.Columns.IndexOf("title")].ToString(),
                    row.ItemArray[DT.Columns.IndexOf("comment")].ToString(),
                    row.ItemArray[DT.Columns.IndexOf("cycle")].ToString() == "1");
                Result.Add(MF);
            }

            DT.Dispose();

            return Result;
        }

        /// <summary>
        /// Позволяет получить все треки определённой деки
        /// </summary>
        /// <param name="Desk"></param>
        /// <param name="Where"></param>
        /// <returns></returns>
        public List<Track> GetTracks(int Desk, string Where=null)
        {
            List<Track> Result = new List<Track>();

            string WhereCase = Where == null ? $" WHERE `desk_n`={Desk}" : $" WHERE `desk_n`={Desk} AND {Where}";
            DataTable DT = ReadTable($"SELECT `id`, `number`, `file`, `title`, `order` FROM `desk`{WhereCase} ORDER BY `order`;");

            foreach (DataRow row in DT.Rows)
            {
                Track track = new Track(this,
                    Convert.ToInt32(row.ItemArray[DT.Columns.IndexOf("id")], EnCI),
                    Desk,
                    row.ItemArray[DT.Columns.IndexOf("number")].ToString(),
                    row.ItemArray[DT.Columns.IndexOf("title")].ToString(),
                    Convert.ToInt32(row.ItemArray[DT.Columns.IndexOf("file")], EnCI),
                    Convert.ToInt32(row.ItemArray[DT.Columns.IndexOf("order")], EnCI)
                    );
                Result.Add(track);
            }

            DT.Dispose();
            return Result;
        }

        /// <summary>
        /// Инициация новой базы с нужной информацией о названии и комментарии
        /// </summary>
        public void Initiate()
        {
            Execute("INSERT INTO `info`(`name`, `description`) VALUES ('', '');");
        }

        /// <summary>
        /// Показывает, имеется ли номер на треке
        /// </summary>
        /// <param name="Number">Номер трека</param>
        /// <param name="Desk">Номер деки</param>
        /// <returns></returns>
        public bool NumberExists(string Number, int Desk)
        {
            return NumberExists(Number, Desk, 0);
        }

        /// <summary>
        /// Показывает, имеется ли номер на треке
        /// </summary>
        /// <param name="Number">Номер трека</param>
        /// <param name="Desk">Номер деки</param>
        /// <param name="TrackToExclude">ID трека, который нужно игнорировать (0 - ничего не игнорировать)</param>
        /// <returns></returns>
        public bool NumberExists(string Number, int Desk, long TrackToExclude)
        {
            string Condition = TrackToExclude == 0 ? "" : $" AND (`id`<>{TrackToExclude})";
            return GetCount("desk", $"(`desk_n`={Desk}) AND (`number`=\"{Number}\"){Condition}") > 0;
        }
    }
}

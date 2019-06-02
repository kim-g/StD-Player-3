﻿using System.Collections.Generic;
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
                    return "";
                }
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
                if (DT?.Rows.Count == 0)
                {
                    Initiate();
                    return "";
                }
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
            DataTable DT = ReadTable($"SELECT `id`, `title`, `comment`, `cycle` FROM `files`{WhereCase};");

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

        public List<Track> GetTracks(int Desk, string Where=null)
        {
            List<Track> Result = new List<Track>();

            string WhereCase = Where == null ? $" WHERE `desk_n`={Desk}" : $" WHERE `desk_n`={Desk} AND {Where}";
            DataTable DT = ReadTable($"SELECT `id`, `number`, `file`, `title`, `order` FROM `desk`{WhereCase} ORDER BY `order`;");

            foreach (DataRow row in DT.Rows)
            {
                Track track = new Track(this,
                    Convert.ToInt32(row.ItemArray[DT.Columns.IndexOf("id")]),
                    Desk,
                    row.ItemArray[DT.Columns.IndexOf("number")].ToString(),
                    row.ItemArray[DT.Columns.IndexOf("title")].ToString(),
                    Convert.ToInt32(row.ItemArray[DT.Columns.IndexOf("file")]),
                    Convert.ToInt32(row.ItemArray[DT.Columns.IndexOf("order")])
                    );
                Result.Add(track);
            }

            return Result;
        }

        public void Initiate()
        {
            Execute("INSERT INTO `info`(`name`, `description`) VALUES ('', '');");
        }
    }
}

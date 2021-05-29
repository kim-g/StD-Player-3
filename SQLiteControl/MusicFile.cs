using System;
using System.Data;
using System.IO;

namespace SQLite
{
    /// <summary>
    /// Музыкальный файл в БД. Файл для работы. 
    /// </summary>
    public class MusicFile : IDisposable
    {
        private MusicDB DB;
        private string title;
        private string comment;
        private bool cycle;

        /// <summary>
        /// Создание пустого экземпляра
        /// </summary>
        public MusicFile(MusicDB db, long id, string title, string comment, bool cycle)
        {
            DB = db;
            ID = id;
            this.title = title;
            this.comment = comment;
            this.cycle = cycle;
        }

        public MusicFile(MusicDB db, long id)
        {
            DB = db;
            ID = id;
            Update();
        }

        /// <summary>
        /// Номер в БД
        /// </summary>
        public long ID { get; private set; }

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
                using (DataTable DeskData = DB.ReadTable($"SELECT `file` FROM `files` WHERE `id`={ID} LIMIT 1"))
                {
                    if (DeskData.Rows.Count < 0) return null;

                    MemoryStream MS = new MemoryStream();
                    if (DeskData.Rows.Count > 0)
                    {
                        MS.Write((byte[])DeskData.Rows[0].ItemArray[0], 0,
                            ((byte[])DeskData.Rows[0].ItemArray[0]).Length);
                        MS.Position = 0;
                    }
                    DeskData.Dispose();
                    return MS;
                }
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
            using (DataTable dt = DB.ReadTable($"SELECT `title`, `comment`, `cycle` FROM `files` WHERE `id`={ID} LIMIT 1;"))
            {
                if (dt.Rows.Count == 0)
                {
                    title = "";
                    comment = "";
                    cycle = false;
                    return;
                }
                title = dt.Rows[0].ItemArray[dt.Columns.IndexOf("title")].ToString();
                comment = dt.Rows[0].ItemArray[dt.Columns.IndexOf("comment")].ToString();
                cycle = dt.Rows[0].ItemArray[dt.Columns.IndexOf("cycle")].ToString() == "1";
            }
        }

        public void Delete()
        {
            DB.Execute($"DELETE FROM `desk` WHERE `file`={ID};");
            DB.Execute($"DELETE FROM `files` WHERE `id`={ID};");
            Dispose();
        }

        public static MusicFile CreateNewRecord(MusicDB db, string Title, string Comment, bool Cycle, MemoryStream Data)
        {
            if (db == null) return null;
            
            string CycleS = Cycle ? "1" : "0";
            db.Execute($"INSERT INTO `files` (`title`, `comment`, `cycle`) VALUES ('{Title}', '{Comment}', {CycleS});");
            long ID = db.LastID;
            db.ExecuteBLOB($"UPDATE `files` SET `file`=@BLOB WHERE `id`={ID};", Data);
            return new MusicFile(db, ID);
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

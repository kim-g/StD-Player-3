using System;
using System.Data;
using System.IO;

namespace SQLite
{
    /// <summary>
    /// Музыкальный файл в БД. Файл для работы. 
    /// </summary>
    public class Track : IDisposable
    {
        private MusicDB DB;
        private MusicFile sound;
        private int desk;
        private string title;
        private string number;
        private int file;
        private int order;

        /// <summary>
        /// Создание пустого экземпляра
        /// </summary>
        public Track(MusicDB db, long id, int desk, string number, string title, int file, int order)
        {
            DB = db;
            ID = id;
            this.desk = desk;
            this.number = number;
            this.title = title;
            this.file = file;
            this.order = order;
            Sound = new MusicFile(db, file);
        }

        /// <summary>
        /// Загрузка трека из БД по ID
        /// </summary>
        /// <param name="db">База данных</param>
        /// <param name="id">Номер записи</param>
        public Track(MusicDB db, long id)
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
        /// Номер деки
        /// </summary>
        public int Desk
        {
            get => desk;
            set
            {
                desk = value;
                DB.Execute($"UPDATE `desk` SET `desk_n`='{desk}' WHERE `id`={ID};");
            }
        }

        /// <summary>
        /// Метка трека
        /// </summary>
        public string Number
        {
            get => number;
            set
            {
                number = value;
                DB.Execute($"UPDATE `desk` SET `number`='{number}' WHERE `id`={ID};");
            }
        }

        /// <summary>
        /// Заголовок файла. Пустой заголовок означает, что заголовок совпадает с заголовком файла
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                title = value;
                DB.Execute($"UPDATE `desk` SET `title`='{title}' WHERE `id`={ID};");
            }
        }

        public int Order
        {
            get => order;
            set
            {
                order = value;
                DB.Execute($"UPDATE `desk` SET `order`='{order}' WHERE `id`={ID};");
            }
        }

        public MusicFile Sound
        {
            get => sound;
            private set => sound = value;
        }

        /// <summary>
        /// Номер файла в БД
        /// </summary>
        public int File { get => file; }

        /// <summary>
        /// Комментарий к файлу
        /// </summary>
        public string Comment
        {
            get => Sound.Comment;
        }

        /// <summary>
        /// Указывает, что звук должен быть зациклен
        /// </summary>
        public bool Cycle
        {
            get => Sound.Cycle;
        }

        /// <summary>
        /// Поток звукового файла
        /// </summary>
        public MemoryStream Data
        {
            get => Sound.Data;
        }

        public static Track Create(MusicDB db, int desk, string number, string title, 
            long file, int order)
        {
            db.Execute("INSERT INTO `desk` (`desk_n`, `number`, `file`, `title`, `order`) "+
                $"VALUES ({desk}, '{number}', {file}, '{title}', {order})");

            long ID = db.LastID;

            return new Track(db, ID);
        }

        /// <summary>
        /// Загрузить все данные из БД
        /// </summary>
        public void Update()
        {
            DataTable dt = DB.ReadTable($"SELECT `desk_n`, `number`, `file`, `title`, `order` FROM `desk` WHERE `id`={ID} LIMIT 1;");
            if (dt.Rows.Count == 0)
            {
                title = "";
                number = "";
                file = 0;
                desk = 0;
                order = 0;
                return;
            }
            title = dt.Rows[0].ItemArray[dt.Columns.IndexOf("title")].ToString();
            number = dt.Rows[0].ItemArray[dt.Columns.IndexOf("number")].ToString();
            file = Convert.ToInt32(dt.Rows[0].ItemArray[dt.Columns.IndexOf("file")]);
            desk = Convert.ToInt32(dt.Rows[0].ItemArray[dt.Columns.IndexOf("desk_n")]);
            order = Convert.ToInt32(dt.Rows[0].ItemArray[dt.Columns.IndexOf("order")]);
            Sound = new MusicFile(DB, file);
        }

        /// <summary>
        /// Удалить запись
        /// </summary>
        public void Delete()
        {
            DB.Execute($"DELETE FROM `desk` WHERE `id`={ID};");
            Dispose();
        }

        /// <summary>
        /// Создать новую запись
        /// </summary>
        /// <param name="db"></param>
        /// <param name="desk_n"></param>
        /// <param name="number"></param>
        /// <param name="file"></param>
        /// <param name="title"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static Track CreateNewRecord(MusicDB db, int desk_n, string number, int file, string title, int order)
        {
            db.Execute($"INSERT INTO `desk` (`desk_n`, `number`, `file`, `title`, `order`) VALUES ('{desk_n}', '{number}', {file}, '{title}', {order});");
            long ID = db.LastID;
            return new Track(db, ID);
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
                number = null;
                Sound = null;
                DB = null;
            }
        }

        public bool IsNumberUnique(string number = null)
        {
            return number == null
                ? !DB.NumberExists(Number, Desk)
                : !DB.NumberExists(number, Desk, ID);
        }
    }
}

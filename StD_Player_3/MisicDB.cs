using System.Collections.Generic;
using SQLite;
using System.Data;
using System.IO;

namespace StD_Player_3
{
    public class MusicDB : SQLiteDataBase
    {
        public MusicDB(string FileName) : base (FileName)
        {
            OpenDB();
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
}

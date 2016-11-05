using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hTunes
{
    class Playlist
    {
        public string Name { set; get; }

        private DataTable playlist;

        public Playlist(string name)
        {
            Name = name;
            playlist = new DataTable();
            playlist.Columns.Add("songId", typeof(String));
        }

        public void AddSong(int id)
        {
            DataRow row = playlist.NewRow();
            row["songId"] = id;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/*
Application name	hTunes
API key 08450c1548807ba37a988798aea2174e
Shared secret b784ecf1c70bd88997e3afbe052eb7c5
Registered to   OtmaroE

*/
namespace hTunes
{
    public class MusicLib
    {
        private string API_KEY = "08450c1548807ba37a988798aea2174e";

        private DataSet musicDataSet;
        public DataSet MusicDataSet { get { return musicDataSet; } }

        //public List<string> Playlists { get { return playlists; } }
        public IEnumerable<string> Playlists {
            get
            {
                IEnumerable<string> playlists = new List<string>() { "All Music" }.AsEnumerable();
                var names = from row in musicDataSet.Tables["playlist"].AsEnumerable()
                          orderby row["name"]
                          select row["name"].ToString();

                playlists = playlists.Concat(names);
                return playlists;
            }
        }
        
        public EnumerableRowCollection<string> SongIds
        {
            get
            {
                var ids = from row in musicDataSet.Tables["song"].AsEnumerable()
                          orderby row["id"]
                          select row["id"].ToString();
                return ids;
            }
        }

        // The constructor reads the music.xsd and music.xml files into the DataSet
        public MusicLib()
        {
            musicDataSet = new DataSet();

            musicDataSet.ReadXmlSchema("music.xsd");

            musicDataSet.ReadXml("music.xml");

            Console.WriteLine("Total songs = " + musicDataSet.Tables["song"].Rows.Count);
        }

        public void PrintAllTables()
        {
            foreach (DataTable table in musicDataSet.Tables)
            {
                Console.WriteLine("Table name = " + table.TableName);
                foreach (DataRow row in table.Rows)
                {
                    Console.WriteLine("Row:");
                    int i = 0;
                    foreach (Object item in row.ItemArray)
                    {
                        Console.WriteLine(" " + table.Columns[i].Caption + "=" + item);
                        i++;
                    }
                }
                Console.WriteLine();
            }
        }

        public bool AddPlaylist(string name)
        {
            //ensure playlist name doesnt already exist
            int count = musicDataSet.Tables["playlist"].Select("name = '" + name + "'" ).Length;
            if (count == 0)
            {
                DataTable table = musicDataSet.Tables["playlist"];
                DataRow row = table.NewRow();
                row["name"] = name;
                table.Rows.Add(row);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeletePlaylist(string playlist)
        {
            // Search the primary key for this playlist and delete it from the playlist table
            DataTable table = musicDataSet.Tables["playlist"];
            DataRow row = table.Rows.Find(playlist);
            if (row == null)
                return false;

            table.Rows.Remove(row);

            // Remove from playlist_song every occurance of this playlist
            List<DataRow> rows = new List<DataRow>();
            table = musicDataSet.Tables["playlist_song"];
            foreach (DataRow r in table.Rows)
                if ((string)r["playlist_name"] == playlist)
                    rows.Add(r);

            foreach (DataRow r in rows)
                r.Delete();

            return true;
        }

        public bool RenamePlaylist(string oldPlaylistName, string newPlaylistName)
        {
            Console.WriteLine("RenamePlaylist: " + oldPlaylistName + " to " + newPlaylistName);

            // Update playlist name in playlist and playlist_song tables

            DataTable table = musicDataSet.Tables["playlist"];
            DataRow row = table.Rows.Find(oldPlaylistName);
            if (row == null || PlaylistExists(newPlaylistName))
                return false;

            row["name"] = newPlaylistName;

            table = musicDataSet.Tables["playlist_song"];
            foreach (DataRow r in table.Rows)
                if ((string)r["playlist_name"] == oldPlaylistName)
                    r["playlist_name"] = newPlaylistName;

            return true;
        }

        public bool PlaylistExists(string playlist)
        {
            DataTable table = musicDataSet.Tables["playlist"];
            DataRow row = table.Rows.Find(playlist);
            return row != null;
        }

        public void AddSongToPlaylist(string playlistName, int id)
        {
            DataTable table = musicDataSet.Tables["playlist_song"];
            DataRow row = table.NewRow();
            row["playlist_name"] = playlistName;
            row["song_id"] = id;
            row["position"] = table.AsEnumerable().Count() + 1;
            table.Rows.Add(row);
        }

        public void RemoveSongFromPlaylist(int position, int songId, string playlist)
        {
            Console.WriteLine("RemoveSongFromPlaylist: id=" + songId + ", pos=" + position +
                ", playlist=" + playlist);

            // Search the primary key for this playlist and delete it from the playlist table
            DataTable table = musicDataSet.Tables["playlist_song"];
            ArrayList primaryKeys = new ArrayList();

            // Order of keys must match column order
            primaryKeys.Add(songId);
            primaryKeys.Add(playlist);
            primaryKeys.Add(position);
            table.Rows.Remove(table.Rows.Find(primaryKeys.ToArray()));

            // Decrement position by 1 for each song in this playlist that is positioned after
            // this one
            table = musicDataSet.Tables["playlist_song"];
            foreach (DataRow r in table.Rows)
            {
                int pos = Convert.ToInt32(r["position"]);
                if ((string)r["playlist_name"] == playlist && pos > position)
                    r["position"] = pos - 1;
            }

        }

        public DataTable GetPlaylist(string name)
        {
            if(name == "All Music")
            {
                return musicDataSet.Tables["song"];
            }
            else
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("title", typeof(string));
                dt.Columns.Add("artist", typeof(string));
                dt.Columns.Add("album", typeof(string));
                dt.Columns.Add("filename", typeof(string));
                dt.Columns.Add("length", typeof(string));
                dt.Columns.Add("genre", typeof(string));
                dt.Columns.Add("id", typeof(string));
                dt.Columns.Add("position", typeof(string));
                dt.Columns.Add("playlist_name", typeof(string));

                var rows = from rowPlaylistSong in musicDataSet.Tables["playlist_song"].AsEnumerable()
                           join rowSong in musicDataSet.Tables["song"].AsEnumerable() on rowPlaylistSong["song_id"] equals rowSong["id"]
                           orderby rowPlaylistSong["position"]
                           where rowPlaylistSong["playlist_name"].ToString() == name
                           select new { 
                               title = rowSong["title"], 
                               artist=rowSong["artist"],
                               album=rowSong["album"],
                               filename=rowSong["filename"],
                               length=rowSong["length"],
                               genre=rowSong["genre"],
                               id=rowSong["id"],
                               position = rowPlaylistSong["position"],
                               playlist_name = rowPlaylistSong["playlist_name"]
                            };
                foreach (var row in rows)
                {
                    DataRow dtRow = dt.NewRow();
                    dtRow["title"] = row.title;
                    dtRow["artist"] = row.artist;
                    dtRow["album"] = row.album;
                    dtRow["filename"] = row.filename;
                    dtRow["length"] = row.length;
                    dtRow["genre"] = row.genre;
                    dtRow["id"] = row.id;
                    dtRow["position"] = row.position;
                    dtRow["playlist_name"] = row.playlist_name;
                    dt.Rows.Add(dtRow);
                }
                return dt;
            }
                        
        }

        public int AddSong(Song s)
        {
            DataTable table = musicDataSet.Tables["song"];
            DataRow row = table.NewRow();
            row["title"] = s.Title;
            row["artist"] = s.Artist;
            row["album"] = s.Album;
            row["filename"] = s.Filename;
            row["length"] = s.Length;
            row["genre"] = s.Genre;
            table.Rows.Add(row);
            // Update this song's ID
            s.Id = Convert.ToInt32(row["id"]);
            return s.Id;
        }

        // Return a Song for the given song ID or null if no song was not found.
        public Song GetSong(int songId)
        {
            DataTable table = musicDataSet.Tables["song"];

            Song returnSong  = new Song();

            foreach (DataRow row in table.Select("id=" + songId))
            {
                returnSong.Title = row["title"].ToString();
                returnSong.Artist = row["artist"].ToString();
                returnSong.Album = row["album"].ToString();
                returnSong.Genre = row["genre"].ToString();
                returnSong.Length = row["length"].ToString();
                returnSong.Filename = row["filename"].ToString();

                return returnSong;
            }
            return null;
        }

        // Update the given song with the given song ID.  Returns true if the song
        // was updated, false if it could not because the song ID was not found.
        public bool UpdateSong(int songId, Song song)
        {
            DataTable table = musicDataSet.Tables["song"];
            // Only one row should be selected
            foreach (DataRow row in table.Select("id=" + songId))
            {
                row["title"] = song.Title;
                row["artist"] = song.Artist;
                row["album"] = song.Album;
                row["genre"] = song.Genre;
                row["length"] = song.Length;
                row["filename"] = song.Filename;

                return true;
            }
            return false;
        }

        // Delete a song given the song's ID. Return true if the song was  
        // successfully deleted, false if the song ID was not found.
        public bool DeleteSong(int songId)
        {
            Console.WriteLine("Deleting song " + songId);
            try
            {
                // Search the primary key for the selected song and delete it from 
                // the song table
                DataTable table = musicDataSet.Tables["song"];
                table.Rows.Remove(table.Rows.Find(songId));

                // Remove from playlist_song every occurance of songId.
                // Add rows to a separate list before deleting because we'll get an exception
                // if we try to delete more than one row while looping through table.Rows

                List<DataRow> rows = new List<DataRow>();
                table = musicDataSet.Tables["playlist_song"];
                foreach (DataRow row in table.Rows)
                    if (row["song_id"].ToString() == songId.ToString())
                        rows.Add(row);

                foreach (DataRow row in rows)
                    row.Delete();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Save the song database to the music.xml file
        public void Save()
        {
            // Save music.xml in the same directory as the exe
            string filename = "music.xml";
            Console.WriteLine("Saving " + filename);
            musicDataSet.WriteXml(filename);
        }
        //CODE provided in class project description
        public void GetSongImage(Song song)
        {
            String url = "http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key=" + API_KEY + "&" + "artist=" + WebUtility.UrlEncode(song.Artist) + "&track=" + WebUtility.UrlEncode(song.Title);
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                using (WebResponse response = request.GetResponse())
                {
                    Stream strm = response.GetResponseStream();
                    using (XmlTextReader reader = new XmlTextReader(strm))
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                Console.WriteLine("name = " + reader.Name);
                                if (reader.Name == "image")
                                {
                                    if (reader.GetAttribute("size") == "medium")
                                        Console.WriteLine("Image URL = " + reader.ReadString());
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                // A 400 response is returned when the song is not in their library
                Console.WriteLine("Error: " + e.Message);
            }
        }

    }    
}

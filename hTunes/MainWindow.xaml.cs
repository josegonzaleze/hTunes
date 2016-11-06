using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MusicLib musicLib;
        private MediaPlayer mediaPlayer;

        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();

            try
            {
                musicLib = new MusicLib();
            }
            catch (Exception e)
            {
                MessageBox.Show("There was a problem: " + e.Message);
            }

            dataGrid.ItemsSource = musicLib.MusicDataSet.Tables["song"].DefaultView;
            //dataGrid.ItemsSource = musicLib.GetPlaylist("Cool stuff").DefaultView;
            playlistBox.ItemsSource = musicLib.Playlists;
        }

        private void addPlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddPlaylistDialogBox newPlaylistDialogBox = new AddPlaylistDialogBox();

            newPlaylistDialogBox.ShowDialog();
            if(newPlaylistDialogBox.DialogResult == true)
            {
                bool successfulAdd = musicLib.AddPlaylist(newPlaylistDialogBox.PlaylistName);
                newPlaylistDialogBox.Close();
                if(successfulAdd)
                {
                    //playlistBox.Items.Refresh();
                    playlistBox.ItemsSource = musicLib.Playlists;
                }
                else
                {
                    MessageBox.Show("A playlist with this name already exists.", "Uh Oh!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }            
        }

        private void playlistBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                dataGrid.ItemsSource = musicLib.GetPlaylist(e.AddedItems[0] as string).DefaultView;
            }
            
        }

        private void dataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            // Start the drag-drop if mouse has moved far enough
            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Initiate dragging the text from the textbox
                if (dataGrid.SelectedItems.Count > 0)
                {
                    List<int> ids = new List<int>();
                    foreach (var item in dataGrid.SelectedItems)
                    {
                        DataRowView row = (DataRowView)item;
                        ids.Add(Int16.Parse(row["id"].ToString()));
                    }
                    DragDrop.DoDragDrop(dataGrid, ids, DragDropEffects.Copy);
                }
            }

        }
        private Point startPoint;
        private void dataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        private void playlist_Drop(object sender, DragEventArgs e)
        {
            string playlistName = ((ListBoxItem)sender).Content.ToString();
            if (e.Data.GetDataPresent(typeof(List<int>)))
            {
                List<int> songIds = (List<int>)e.Data.GetData(typeof(List<int>));
                foreach(var id in songIds)
                {
                    musicLib.AddSongToPlaylist(playlistName, id);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            DataRowView selectedsongaddress = dataGrid.SelectedItem as DataRowView;
            Song selectedsongobject = musicLib.GetSong(Convert.ToInt32(selectedsongaddress.Row.ItemArray[0]));

            if (selectedsongobject != null)
            {
                
                mediaPlayer.Open(new Uri(selectedsongobject.Filename));
                mediaPlayer.Play();
            }
            else
            {
                MessageBox.Show("Please select a song before you hit play");
            }
        }

        //This is used in previous projects!!!
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.DefaultExt = "*mp3;*.m4a;*.wma;*.wav";
            openFileDialog.Filter = "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*";

            // Show open file dialog box
            bool? result = openFileDialog.ShowDialog();

            // Load the selected song
            if (result == true)
            {
                AddSong(openFileDialog.FileName);
            }
        }

        //This is used in previous projects!!!
        public void AddSong(string sondata)
        {
            // Add the selected file to the music library
            Song s = GetSongDetails(sondata);
            musicLib.AddSong(s);
            DataView temp = dataGrid.ItemsSource as DataView;
            temp.Sort = "id";
            int songID = temp.Find(s.Id);
            dataGrid.SelectedIndex = songID;
        }

        //This is used in previous projects!!!
        private Song GetSongDetails(string filename)
        {
            Song s = null;

            try
            {
                // PM> Install-Package taglib
                // http://stackoverflow.com/questions/1750464/how-to-read-and-write-id3-tags-to-an-mp3-in-c
                TagLib.File file = TagLib.File.Create(filename);

                s = new Song
                {
                    Title = file.Tag.Title,
                    Artist = file.Tag.AlbumArtists.Length > 0 ? file.Tag.AlbumArtists[0] : "",
                    Album = file.Tag.Album,
                    Genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "",
                    Length = file.Properties.Duration.Minutes + ":" + file.Properties.Duration.Seconds,
                    Filename = filename
                };

                return s;
            }
            catch (TagLib.UnsupportedFormatException)
            {
                MessageBox.Show("You did not select a valid song file.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return s;
        }

        private void deleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (playlistBox.SelectedItem.ToString() != "All Music");
        }

        private void deleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            musicLib.DeletePlaylist(playlistBox.SelectedItem.ToString());
            playlistBox.ItemsSource = musicLib.Playlists;
        }
    }
}

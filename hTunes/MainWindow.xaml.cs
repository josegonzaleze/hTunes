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

            dataGrid.ItemsSource = musicLib.MusicDataSet.Tables[0].DefaultView;
            playlistBox.ItemsSource = musicLib.Playlists;
        }

        private void addPlaylist_Click(object sender, RoutedEventArgs e)
        {
            AddPlaylistDialogBox newPlaylistDialogBox = new AddPlaylistDialogBox();

            newPlaylistDialogBox.ShowDialog();
            if(newPlaylistDialogBox.DialogResult == true)
            {
                musicLib.AddPlaylist(newPlaylistDialogBox.PlaylistName);
                newPlaylistDialogBox.Close();
                playlistBox.Items.Refresh();
            }            
        }
    }
}

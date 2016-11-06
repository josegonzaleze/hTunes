using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace hTunes
{
    public enum PlaylistDialogType { Create, Rename};
    /// <summary>
    /// Interaction logic for AddPlaylistDialogBox.xaml
    /// </summary>
    public partial class AddPlaylistDialogBox : Window
    {
        public String PlaylistName { get { return playlistNameTextBox.Text; } }

        public AddPlaylistDialogBox(PlaylistDialogType dt)
        {
            InitializeComponent();
            if(dt == PlaylistDialogType.Create)
            {
                window.Title = "New Playlist";
                submitButton.Content = "Create";
            }
            else
            {
                window.Title = "Rename Playlist";
                submitButton.Content = "Rename";
            }
        }

        private void CreatePlaylist_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrWhiteSpace(playlistNameTextBox.Text);
        }

        private void CreatePlaylist(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

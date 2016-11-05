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
    /// <summary>
    /// Interaction logic for AddPlaylistDialogBox.xaml
    /// </summary>
    public partial class AddPlaylistDialogBox : Window
    {
        public String PlaylistName { get { return playlistNameTextBox.Text; } }

        public AddPlaylistDialogBox()
        {
            InitializeComponent();
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

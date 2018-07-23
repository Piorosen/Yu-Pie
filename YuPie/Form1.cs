using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;




namespace YuPie
{
    public partial class Form1 : Form
    {
        private YouTubeService Service = null;

        Dictionary<string, Playlist> playlists = new Dictionary<string, Playlist>();
        Dictionary<string, PlaylistItem> musiclists = new Dictionary<string, PlaylistItem>();


        string Global_DriveAuth = Application.StartupPath + "\\";

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientSecret = "6z6zVtE5GaUY8h2ySRKj95Co", ClientId = "1032108029526-bgim3s61a8kqd7jfq1jdoshm8avh0i9q.apps.googleusercontent.com" }
                , new string[] { YouTubeService.Scope.Youtube }
                , "user"
                , CancellationToken.None
                , new FileDataStore(Global_DriveAuth, true)).Result;

            Service = new YouTubeService(new BaseClientService.Initializer()
            {

                HttpClientInitializer = credential,
                ApplicationName = GetType().ToString()
            });



            var list = Service.Playlists.List("snippet");
            list.Mine = true;

            var playlist = list.Execute();
            var tmp = playlist.Items.ToList();

            foreach (var data in tmp)
            {
                playlists[data.Snippet.Title] = data;
                listBox_Playlist.Items.Add(data.Snippet.Title);
            }
        }

        private void listBox_Playlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Playlist.SelectedIndex != -1)
            {

                listBox_MusicList.Items.Clear();

                var d = Service.PlaylistItems.List("snippet");
                d.PlaylistId = playlists[listBox_Playlist.SelectedItem.ToString()].Id;
                d.MaxResults = 50;

                var Playitems = d.Execute();

                foreach (var data in Playitems.Items)
                {
                    listBox_MusicList.Items.Add(data.Snippet.Title);
                    musiclists[data.Snippet.Title] = data;
                }

            }
        }

        private void Btn_Oath_Click(object sender, EventArgs e)
        {
            

            var fi = new FileInfo(Global_DriveAuth + "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");
            if (fi.Exists)
            {
                fi.Delete();
            }

            Application.Restart();
        }

        private const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";

        string GetVideoId(string input)
        {
            var regex = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
            foreach (Match match in regex.Matches(input))
            {
                //Console.WriteLine(match);
                foreach (var groupdata in match.Groups.Cast<Group>().Where(groupdata => !groupdata.ToString().StartsWith("http://") && !groupdata.ToString().StartsWith("https://") && !groupdata.ToString().StartsWith("youtu") && !groupdata.ToString().StartsWith("www.")))
                {
                    return groupdata.ToString();
                }
            }
            return string.Empty;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var count = Regex.Split(textBox1.Text, "\r\n");

            try
            {
                if (listBox_Playlist.SelectedIndex == -1)
                {
                    MessageBox.Show("어느 플레이 리스트 에 추가할것인지 체크 해주세요.");
                }

                for (int i = 0; i < count.Length; i++)
                {
                    if (count[i] == String.Empty)
                    {
                        continue;
                    }
                    PlaylistItem playlistItem = new PlaylistItem();
                    playlistItem.Snippet = new PlaylistItemSnippet();
                    playlistItem.Snippet.PlaylistId = playlists[listBox_Playlist.SelectedItem.ToString()].Id;
                    playlistItem.Snippet.ResourceId = new ResourceId();
                    playlistItem.Snippet.ResourceId.VideoId = GetVideoId(count[i]).Trim();
                    playlistItem.Snippet.ResourceId.Kind = "youtube#video";

                    playlistItem = Service.PlaylistItems.Insert(playlistItem, "snippet").Execute();
                    listBox_MusicList.Items.Add(playlistItem.Snippet.Title);
                    musiclists[playlistItem.Snippet.Title] = playlistItem;
                }
            }catch (Exception e1)
            {
                MessageBox.Show(e1.ToString());
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Playlist playlist = new Playlist();
            playlist.Snippet = new PlaylistSnippet();
            playlist.Snippet.Title = TextBox_PlayListCreate.Text;
            playlist = Service.Playlists.Insert(playlist, "snippet").Execute();
            listBox_Playlist.Items.Add(playlist.Snippet.Title);
            playlists[playlist.Snippet.Title] = playlist;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Service.Playlists.Delete(playlists[listBox_Playlist.SelectedItem.ToString()].Id).Execute();
            playlists.Remove(listBox_Playlist.SelectedItem.ToString());
            listBox_Playlist.Items.Remove(listBox_Playlist.SelectedItem);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox_MusicList.SelectedIndex != -1)
            {
                Service.PlaylistItems.Delete(musiclists[listBox_MusicList.SelectedItem.ToString()].Id).Execute();
                musiclists.Remove(listBox_MusicList.SelectedItem.ToString());
                listBox_MusicList.Items.Remove(listBox_MusicList.SelectedItem.ToString());
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
        }
    }
}

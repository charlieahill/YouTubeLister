using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VideoLibrary;
using YT.Helpers;

namespace YT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            AllVideos = VideoSavingController.Load();

            UpdateVideoList();
        }

        private void UpdateVideoList(string scrollToURL = "")
        {
            if (ShowWatchedVideos)
                FilteredVideos = new ObservableCollection<YouTubeVideoDataModel>(AllVideos);
            else
             FilteredVideos = new ObservableCollection<YouTubeVideoDataModel>(AllVideos.Where(x => x.Watched == false));

            if (SortByUploadDate)
                DisplayVideos = new ObservableCollection<YouTubeVideoDataModel>(FilteredVideos.OrderByDescending(x => x.DateAdded));
            else
                DisplayVideos = new ObservableCollection<YouTubeVideoDataModel>(FilteredVideos.OrderByDescending(x => x.PriorityIsSet).ThenBy(x => x.Priority).ThenBy(x => x.Channel).ThenBy(x => x.Series).ThenBy(x => x.UploadDate));

            OnPropertyChanged("DisplayVideos");

            if (scrollToURL != "")
            {
                YouTubeVideoDataModel targetVideo = DisplayVideos.Where(x => x.URL == scrollToURL).First();
                VideosListView.SelectedItem = targetVideo;
                VideosListView.ScrollIntoView(targetVideo);
            }
        }

        /* Observable Collection Properties */
        public ObservableCollection<YouTubeVideoDataModel> DisplayVideos { get; set; }
        public ObservableCollection<YouTubeVideoDataModel> AllVideos { get; set; }
        public ObservableCollection<YouTubeVideoDataModel> FilteredVideos { get; set; }
        public ObservableCollection<MenuItem> ContextMenuItems { get; set; } = new ObservableCollection<MenuItem>();

        /* Selected Item Properties */
        private YouTubeVideoDataModel _selectedVideo;

        public YouTubeVideoDataModel SelectedVideo
        {
            get { return _selectedVideo; }
            set { _selectedVideo = value; OnPropertyChanged(); }
        }

        public bool DeleteOnOpen { get; set; } = true;

        private bool sortByUploadDate = false;

        public bool SortByUploadDate
        {
            get { return sortByUploadDate; }
            set { sortByUploadDate = value; OnPropertyChanged("SortByUploadDate"); }
        }

        private bool showWatchedVideos = false;
        public bool ShowWatchedVideos
        {
            get { return showWatchedVideos; }
            set { showWatchedVideos = value; OnPropertyChanged("ShowWatchedVideos"); }
        }


        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            int addedVideos = 0;

            try
            {
                string textfromClipboard = "";
                textfromClipboard = Clipboard.GetText();

                string[] URLsFromClipboard = textfromClipboard.Split('\n');
                string lastURL = "";


                foreach (string URLfromClipboard in URLsFromClipboard)
                {
                    if (!VideoIsDuplicate(URLfromClipboard, AllVideos))
                    {
                        AllVideos.Add(new YouTubeVideoDataModel(URLfromClipboard.Trim()));
                        addedVideos++;
                        lastURL = URLfromClipboard.Trim();
                    }
                    else
                    {
                        AllVideos.Where(x => x.URL == URLfromClipboard).First().Watched = false;
                    }
                }

                SortByUploadDate = true;

                UpdateVideoList(lastURL);
            }
            catch(Exception ex)
            {
                _ = MessageBox.Show($"Error adding video.{Environment.NewLine}{Environment.NewLine}{ex}");
            }

            MessageBox.Show($"{addedVideos} videos added.");
        }

        private bool VideoIsDuplicate(string uRLfromClipboard, ObservableCollection<YouTubeVideoDataModel> videos)
        {
            foreach (YouTubeVideoDataModel video in videos)
                if (uRLfromClipboard.Trim() == video.URL.Trim())
                {
                    MessageBox.Show("Video not added, as video is already in list.");
                    return true;
                }

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "DisplayVideos")
                UpdateVideoCount();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVideo != null)
            {
                Process.Start(SelectedVideo.URL);

                if (DeleteOnOpen)
                {
                    SelectedVideo.Watched = true;
                    SelectedVideo.WatchedDate = DateTime.Now;
                    SelectedVideo.Priority = string.Empty;
                    //DisplayVideos.Remove(SelectedVideo);
                }

                UpdateVideoList();
            }
        }

        private void RemoveFromListButton_Click(object sender, RoutedEventArgs e)
        {
            //ideas from https://social.msdn.microsoft.com/Forums/en-US/c12a625f-b994-4b6e-9050-f66fa0434cdc/how-to-delete-a-row-in-a-listview-with-a-delete-button-in-each-row?forum=wpf
            //ideas from https://stackoverflow.com/questions/26144400/find-parent-listviewitem-of-button-on-click-event
            Button btn = (Button)sender;
            Console.WriteLine($"Removing {btn.Tag}");
            AllVideos.Where(x => x.URL == btn.Tag.ToString()).First().Watched = true;
            AllVideos.Where(x => x.URL == btn.Tag.ToString()).First().WatchedDate = DateTime.Now;
            AllVideos.Where(x => x.URL == btn.Tag.ToString()).First().Priority = string.Empty;
            UpdateVideoList();
        }

        public T GetAncesterOfType<T>(FrameworkElement child) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent != null && !(parent is T))
                return (T)GetAncesterOfType<T>((FrameworkElement)parent);
            return (T)parent;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            AllVideos.Save();
        }

        /// <summary>
        /// Handles right click on an already previously selected ListViewItem
        /// </summary>
        private void VideosListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if(item!= null && item.IsSelected)
            {
                VideosListView.SelectedItems.Clear();
                VideosListView.SelectedItem = item;
            }
        }

        /// <summary>
        /// Allows the user to specify (text) a new category to add the video to
        /// </summary>
        private void CreateCategoryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InputBox inputBox = new InputBox("New Category", "Please enter the name of the new category...");
            if (inputBox.ShowDialog() == true)
                ((YouTubeVideoDataModel)VideosListView.SelectedItem).Series = inputBox.UserInput;

            OnPropertyChanged("Videos");
        }

        private string AssignPriority(YouTubeVideoDataModel selectedItem)
        {
            //If the priority is already set, then do not change it
            if (selectedItem.PriorityIsSet)
            {
                Console.WriteLine("Item has priority previously set.");
                return selectedItem.Priority;
            }

            //No other videos in display list with current priority
            if (DisplayVideos.Where(x => x.Series == selectedItem.Series).Count() == 0)
            {
                Console.WriteLine("No other videos in display list with current priority");
                return "";
            }

            List<string> priorities = new List<string>();
            foreach (YouTubeVideoDataModel video in DisplayVideos.Where(x => x.Series == selectedItem.Series))
                if (video.PriorityIsSet)
                    priorities.Add(video.Priority);

            //No video with the priority set
            if (priorities.Count == 0)
            {
                Console.WriteLine("No video with the priority set");
                return "";
            }

            priorities = priorities.OrderBy(x => x).ToList();
            Console.WriteLine($"Setting priority {priorities.First()}");
            return priorities.First();
        }

        /// <summary>
        /// Populates the series list view with a list of previously declared series for this video channel
        /// </summary>
        private void VideoCategoryContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Console.WriteLine("Generating Context Menu...");

            if (VideosListView.SelectedItem != null)
                Console.WriteLine(VideosListView.SelectedItem);

            PopulateContextMenu();
        }

        /// <summary>
        /// Gest a list of all of the availalbe series
        /// </summary>
        private void PopulateContextMenu()
        {
            if (VideosListView.SelectedItem == null)
                return;

            ContextMenuItems = new ObservableCollection<MenuItem>();
            
            string Channel = ((YouTubeVideoDataModel)VideosListView.SelectedItem).Channel;
            ObservableCollection<string> channelSeries = new ObservableCollection<string>();

            Console.WriteLine(Channel);

            //Loop through all videos belonging to the same channel
            //Compile a list of all of the (unique) series
            foreach (YouTubeVideoDataModel video in AllVideos)
                if (video.Channel == Channel)
                    if (video.Series.Trim() != string.Empty)
                        if (SeriesIsNew(video.Series.Trim(), channelSeries))
                            channelSeries.Add(video.Series.Trim());

            //Add those unique series items to the list available to the user in the context menu
            foreach (string series in channelSeries)
            {
                MenuItem AssignToChannelMenuItem = new MenuItem() { Header = series };
                AssignToChannelMenuItem.Click += AssignToChannelMenuItem_Click;
                ContextMenuItems.Add(AssignToChannelMenuItem);
            }

            MenuItem AddNewCategoryMenuItem = new MenuItem()
            {
                Header = "+ Create Category"
            };
            AddNewCategoryMenuItem.Click += CreateCategoryMenuItem_Click;
            ContextMenuItems.Add(AddNewCategoryMenuItem);

            for (int i = 1; i < 6; i++)
            {
                MenuItem PriorityMenuItem = new MenuItem()
                {
                    Header = i.ToString()
                };
                PriorityMenuItem.Click += PriorityMenuItem_Click;
                ContextMenuItems.Add(PriorityMenuItem);
            }

            OnPropertyChanged("ContextMenuItems");
        }

        private void PriorityMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((YouTubeVideoDataModel)VideosListView.SelectedItem).Priority = (sender as MenuItem).Header.ToString();
            OnPropertyChanged("Videos");
            UpdateVideoList();
        }

        /// <summary>
        /// Assigns a video to a specified series
        /// </summary>
        private void AssignToChannelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ((YouTubeVideoDataModel)VideosListView.SelectedItem).Series = (sender as MenuItem).Header.ToString();
            ((YouTubeVideoDataModel)VideosListView.SelectedItem).Priority = AssignPriority((YouTubeVideoDataModel)VideosListView.SelectedItem);
            OnPropertyChanged("Videos");
            UpdateVideoList();
        }

        /// <summary>
        /// Compares a string to an array of strings, to see if that string is already in the array
        /// </summary>
        /// <param name="series">The series to compare against the list</param>
        /// <param name="channelSeries">The list of strings</param>
        /// <returns>True if the series is not contained in the list, false otherwise</returns>
        private bool SeriesIsNew(string series, ObservableCollection<string> channelSeries)
        {
            if (channelSeries.Contains(series))
                return false;

            return true;
        }

        /// <summary>
        /// Updates the number of videos left in the list
        /// </summary>
        private void UpdateVideoCount()
        {
            VideoCountLabel.Content = $"{DisplayVideos.Count} videos...";
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateVideoList();
            Console.WriteLine("Sort by upload date: " + SortByUploadDate);
        }

        private void CheckBoxWatched_Checked(object sender, RoutedEventArgs e)
        {
            UpdateVideoList();
            Console.WriteLine("Show watched videos: " + ShowWatchedVideos);
        }

        //todo: Wishlist: Can I download thumbnails (delete thumbnail when deleting video)
    }
}
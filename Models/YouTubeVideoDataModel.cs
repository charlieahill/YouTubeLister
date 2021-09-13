using System;
using System.Windows;
using VideoLibrary;
using YT.Helpers;

namespace YT
{
    [Serializable]
    public class YouTubeVideoDataModel : ObservableObject
    {
        private string _url;
        /// <summary>
        /// The URL link to the YouTube video
        /// </summary>
        public string URL
        {
            get { return _url; }
            set { _url = value; OnPropertyChanged(); }
        }

        private string _channel;
        /// <summary>
        /// The uploading channel of the YT video
        /// </summary>
        public string Channel
        {
            get { return _channel; }
            set { _channel = value; OnPropertyChanged(); }
        }

        private string _series;
        /// <summary>
        /// The series of that YouTuber to which the video belongs (user defined)
        /// </summary>
        public string Series
        {
            get { return _series; }
            set { _series = value; OnPropertyChanged(); }
        }

        private string _title;
        /// <summary>
        /// The title of the YT video
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }

        private DateTime _uploadDate;
        /// <summary>
        /// The date when the video was uploaded
        /// </summary>
        public DateTime UploadDate
        {
            get { return _uploadDate; }
            set { _uploadDate = value; OnPropertyChanged(); }
        }

        private DateTime _dateAdded;
        /// <summary>
        /// The date when the video was added to this list
        /// </summary>
        public DateTime DateAdded
        {
            get { return _dateAdded; }
            set { _dateAdded = value; }
        }

        private string priority = "";
        /// <summary>
        /// The video priority for ordering videos
        /// </summary>
        public string Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        private bool watched;
        /// <summary>
        /// True if the video has been watched, false otherwise
        /// </summary>
        public bool Watched
        {
            get { return watched; }
            set { watched = value; }
        }

        private DateTime watchedDate;
        /// <summary>
        /// The date time when the video was watched
        /// </summary>
        public DateTime WatchedDate
        {
            get { return watchedDate; }
            set { watchedDate = value; }
        }

        private int duration;
        /// <summary>
        /// The length of the video in seconds
        /// </summary>
        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        /// <summary>
        /// Returns a visibility that hides the watched date if the item has not been viewed
        /// </summary>
        public Visibility WatchedVisibility
        {
            get
            {
                if (Watched)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
        }

        /// <summary>
        /// Returns true if the item has a priority set
        /// </summary>
        public bool PriorityIsSet
        {
            get
            {
                if (Priority == "")
                    return false;

                return true;
            }
        }

        public string DurationDisplay
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(Duration);
                return "Duration: " + time.ToString(@"hh\:mm\:ss");
            }
        }

        /// <summary>
        /// Creates a YouTube data model from a string URL
        /// </summary>
        /// <param name="url">The URL to create the data model from</param>
        public YouTubeVideoDataModel(string url)
        {
            URL = url;

            YouTube yt = YouTube.Default;
            YouTubeVideo vid = yt.GetVideo(url);
            VideoInfo info = vid.Info;

            Channel = info.Author;
            Series = "";
            Title = vid.Title;
            Duration = (int)info.LengthSeconds;
            Watched = false;
            WatchedDate = DateTime.Now;
            if (DateTime.TryParse(info.UploadDate, out DateTime _uploadDate))
                UploadDate = _uploadDate;
            else
                UploadDate = DateTime.Now;

            DateAdded = DateTime.Now;
        }

        public YouTubeVideoDataModel() {}

        public override string ToString()
        {
            return $"{Title}, {Channel}";
        }
    }
}

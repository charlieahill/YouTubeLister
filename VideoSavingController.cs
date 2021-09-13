using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace YT
{
    public static class VideoSavingController
    {
        /// <summary>
        /// Saves the given data to file
        /// </summary>
        /// <param name="data">Data to be saved to file</param>
        public static void Save(this ObservableCollection<YouTubeVideoDataModel> data)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(programDataFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(programDataFilePath));

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<YouTubeVideoDataModel>));
                using (StreamWriter writer = new StreamWriter(programDataFilePath))
                    xmlSerializer.Serialize(writer, data);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save program data. Changes to the program data made in this session will be lost." + Environment.NewLine + Environment.NewLine + ex.ToString());
            }
        }

        /// <summary>
        /// Loads data from file
        /// </summary>
        /// <returns>The loaded data</returns>
        public static ObservableCollection<YouTubeVideoDataModel> Load()
        {
            try
            {
                if (!File.Exists(programDataFilePath)) return new ObservableCollection<YouTubeVideoDataModel>();

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<YouTubeVideoDataModel>));
                using (StreamReader reader = new StreamReader(programDataFilePath))
                    return (ObservableCollection<YouTubeVideoDataModel>)xmlSerializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load data. Blank data will be loaded in for this session." + Environment.NewLine + Environment.NewLine + ex.ToString());
                return new ObservableCollection<YouTubeVideoDataModel>();
            }
        }

        /// <summary>
        /// The file path for the program data
        /// </summary>
        private static string programDataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CHillSW/youTube/videos.xml";
    }
}

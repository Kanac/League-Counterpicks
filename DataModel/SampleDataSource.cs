using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace League_of_Legends_Counterpicks.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    /// 

    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String imagePath, List<string> Counters)
        {
            this.UniqueId = uniqueId;
            this.ImagePath = imagePath;
            this.Counters = Counters;
        }

        public string UniqueId { get; private set; }
        public string ImagePath { get; private set; }
        public List<string> Counters { get; private set; }


    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String imagePath)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        public static SampleDataSource _sampleDataSource = new SampleDataSource();         //static property of class itself allows its properties and methods to be referenced 
                                                                                            //without initializaiton (singleton)
        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();        //Initalize a collection of groups
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<ObservableCollection<SampleDataGroup>> GetGroupsAsync()     
        {                                                                          
            await _sampleDataSource.GetSampleDataAsync();                 //Await methods are only when the code needs the information provided. If already contains it, don't need it anymore          

            return _sampleDataSource.Groups;            
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)    
        {
            await _sampleDataSource.GetSampleDataAsync();       
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group => group.UniqueId.Equals(uniqueId)));       //Cycle through the collection of groups, returning first group that matches the ID (Lambda expression)
            if (matches.Count() == 1) return matches.First();                                               
            return null;
        }
        public static string GetGroupId(string champId)
        {
            foreach(var group in _sampleDataSource.Groups){
                foreach (var item in group.Items){
                    if (item.UniqueId.ToLower() == champId.ToLower() && group.UniqueId != "All" && group.UniqueId != "Filter")
                        return group.UniqueId;
                }
            }
            return null;
        }


        public static async Task<SampleDataItem> GetItemAsync(string champId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            var appId = Windows.ApplicationModel.Store.CurrentApp.AppId;
            Debug.WriteLine("{0}",appId);
            // Simple linear search is acceptable for small data sets
            var matches = new List<SampleDataItem>();
            foreach(var group in _sampleDataSource.Groups){
                foreach (var item in group.Items){
                    if (item.UniqueId.ToLower().Contains(champId.ToLower()) && group.UniqueId != "All" && group.UniqueId != "Filter")
                        matches.Add(item);
                    if (champId == "Vi" && item.UniqueId == "Vi")   //Vi could be mistaken for filter or actual champion, we'll choose the latter case
                        return item;
                }
            }
            //Select all the items of each group, and return first item that matches the item id 
            if (matches.Count() == 1) return matches.First();
            else return null;
        }


        private async Task GetSampleDataAsync()
        {
            Debug.WriteLine("GetSampleDataAsync Called");
            if (this._groups.Count != 0)            //HERE IS THE ANSWER. IF LOADED, DO NOT LOAD AGAIN
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");      //Get location of data 

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);       //Get the file from where the data is located
            string jsonText = await FileIO.ReadTextAsync(file);     //Returns the json text in which it was saved as 
            JsonObject jsonObject = JsonObject.Parse(jsonText);     //Parse the json text into object 
            JsonArray jsonArray = jsonObject["Groups"].GetArray();     

            foreach (JsonValue groupValue in jsonArray)
            {
                Debug.WriteLine("Going In");
                JsonObject groupObject = groupValue.GetObject();
                SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["ImagePath"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    var counterList = new List<String>();
                    
                    foreach (JsonValue counterValue in itemObject["Counters"].GetArray())
                    {
                        var counterString = counterValue.GetString();
                        counterList.Add(counterString);
                    }
                    group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       counterList));
                    

                }
                this.Groups.Add(group);
            }
            Debug.WriteLine("Amount of Counters for Ashe: {0}", Groups[1].Items[0].Counters.Count);

        }

        //public async Task saveDataAsync()
        //{
        //    Uri dataUri = new Uri("ms-appx://DataModel/LeagueChamp.json");          //Parse the file into an object to work with 
        //    StorageFile LeagueFile = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
        //    string jsonText = await FileIO.ReadTextAsync(LeagueFile);
        //    JsonObject jsonObject = JsonObject.Parse(jsonText);
        //    JsonArray jsonArray = jsonObject.GetArray();

        //    foreach (JsonValue champ in jsonArray)
        //    {
        //        JsonObject champObject = champ.GetObject();

        //        champObject.Add(new SampleDataItem(champObject["desc"].GetString(),
        //                                           champObject["id"].GetString(),
        //                                           champObject["name"].GetString(),
        //                                           champObject["tags"].GetString()));
        //    }


        //}
    }

}
using League_of_Legends_Counterpicks.Common;
using League_of_Legends_Counterpicks.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.System;  

// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace League_of_Legends_Counterpicks
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ChampionPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly String APP_ID = "3366702e-67c7-48e7-bc82-d3a4534f3086";
        private List<Image> ChampList = new List<Image>();
        private List<StackPanel> StackList = new List<StackPanel>();
        private ObservableCollection<String> counters = new ObservableCollection<string>();
        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        

        public ChampionPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            
        } 

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        //Iterate view count of champion page each time its viewed for purposes of how often to show rate and review page
        private async void reviewApp() {
            if (!localSettings.Values.ContainsKey("Views"))
                localSettings.Values.Add(new KeyValuePair<string, object>("Views", 0));
            else
                localSettings.Values["Views"] = 1 + Convert.ToInt32(localSettings.Values["Views"]);
            
            int viewCount = Convert.ToInt32(localSettings.Values["Views"]);
            
            //Only ask for review up to 10 times, once every 5 times this page is visited, and do not ask anymore once reviewed
            if (viewCount % 5 == 0 && viewCount <= 50 && Convert.ToInt32(localSettings.Values["Rate"]) != 1 )
            {
                var reviewBox = new MessageDialog("Please rate this app 5 stars to support us!");
                reviewBox.Commands.Add(new UICommand { Label = "Yes! :)", Id = 0 });
                reviewBox.Commands.Add(new UICommand { Label = "Maybe later :(", Id = 1 });

                var reviewResult = await reviewBox.ShowAsync();
               
                if ((int)reviewResult.Id == 0)
                {
                    await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + APP_ID));
                    localSettings.Values["Rate"] = 1;
                }
            }
        }
        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            reviewApp();
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            //If navigating via a counterpick, on loading that page, remove the previous history so the back page will go to main or role, not champion
            var prevPage = Frame.BackStack.ElementAt(Frame.BackStackDepth - 1);
            if (prevPage.SourcePageType.Equals(typeof(ChampionPage)))
            {
                Frame.BackStack.RemoveAt(Frame.BackStackDepth - 1);
                Debug.WriteLine("Done");
            }
            String championId = (string)e.NavigationParameter;
            Champion champion = DataSource.GetChampion(championId);
            this.DefaultViewModel["Champion"] = champion;
            this.DefaultViewModel["Role"] = DataSource.GetRoleId(champion.UniqueId);

            
            ChampList.Add(Champ1); ChampList.Add(Champ2); ChampList.Add(Champ3); ChampList.Add(Champ4); ChampList.Add(Champ5);
            StackList.Add(Stack1);StackList.Add(Stack2);StackList.Add(Stack3);StackList.Add(Stack4);StackList.Add(Stack5);
            //var champlist = new Image[5];
            //champlist[0] = Champ1; champlist[1] = Champ2; champlist[2] = Champ3; champlist[3] = Champ4; champlist[4] = Champ5;

            int i = 0;
            counters = champion.Counters;
            foreach (var counter in counters){
                var uri = "ms-appx:///Assets/" + counter + "_Square_0.png";
                ChampList[i].Source = new BitmapImage(new Uri(uri, UriKind.Absolute));
                int f = i;
                while (f != 5){
                    Image image = new Image(); ;
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Short_Fuse.jpg", UriKind.Absolute));
                    StackList[i].Children.Add(image);       //Recall that nothing is saved, so when viewing again, it'll be a fresh stacklist
                    f++;
                }
                int d = i;
                while (d != 0)
                {
                    Image image = new Image(); ;
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Short_Fuse.jpg", UriKind.Absolute));
                    StackList[i].Children.Add(image);
                    image.Opacity = 0.2;
                    d--;
                }
                i++;
            }
            
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion



        private void Champ_Tapped(object sender, TappedRoutedEventArgs e)
        {
            String champ = (sender as Image).Name.Substring("Champ".Length);
            int champIndex = Int32.Parse(champ) - 1;
            var championId = counters.ElementAt(champIndex);
            Frame.Navigate(typeof(ChampionPage), championId);
            
        }
    }
}

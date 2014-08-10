using League_of_Legends_Counterpicks.Common;
using League_of_Legends_Counterpicks.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace League_of_Legends_Counterpicks
{
    public sealed partial class SectionPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();


        public SectionPage()
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

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)  //e is the unique ID
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data.
            var list = (List<String>)e.NavigationParameter;
            var id = list[1];               //Group ID or multiple champ ID
            var filter = list[0];
            Debug.WriteLine("Section loaded");
            var group = await SampleDataSource.GetGroupAsync(id);

            if (filter == "No")
            {
                if (id == "All" && group.Items.Count == 0)     //ONE TIME LOAD ALL ONLY
                {
                    Debug.WriteLine("All called");
                    var groups = await SampleDataSource.GetGroupsAsync();       //Returns groups part of the singleton Static object, same group everytime.
                    foreach (var groupa in groups)      //References to groups inside _sampleDataSource, not new objects
                    {
                        foreach (var item in groupa.Items)
                        {
                            group.Items.Add(item);

                        }
                    }
                    group.Items = new ObservableCollection<SampleDataItem>(group.Items.OrderBy(p => p.UniqueId));   //Sort into alphabetical order
                    Debug.WriteLine("Loaded All Group");

                }
                
                DefaultViewModel["Group"] = group;
                
            }
            else
            {
                var groups = await SampleDataSource.GetGroupsAsync();
                var groupFilter = new SampleDataGroup("Filter", "Filter", "/Assets/Urgot_Square_0.png");        //Group not part of _sampleDataSource
                foreach (var groupa in groups)
                {
                    if (groupa.UniqueId != "All")
                    {
                        foreach (var item in groupa.Items)
                        {
                            if (item.UniqueId.ToLower().Contains(id.ToLower()))
                                groupFilter.Items.Add(item);

                        }
                    }
                }
                if (groupFilter.Items.Count > 1)
                    groupFilter.Items = new ObservableCollection<SampleDataItem>(groupFilter.Items.OrderBy(p => p.UniqueId));

                DefaultViewModel["Group"] = groupFilter;
    
            }

        }
    


            //Data context set to the individual group (dictionary key to value)
        

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        /// </summary>
        /// <param name="sender">The GridView displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;

            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                var resourceLoader = ResourceLoader.GetForCurrentView("Resources");
                throw new Exception(resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}

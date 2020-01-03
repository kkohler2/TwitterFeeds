using System.ComponentModel;
using TwitterFeeds.Interfaces;
using TwitterFeeds.Models;
using TwitterFeeds.Services;
using TwitterFeeds.ViewModel;
using Xamarin.Forms;

namespace TwitterFeeds
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        MainPageViewModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            DependencyService.Register<IDataService, DataService>();
            BindingContext = viewModel = new MainPageViewModel();

            listView.ItemSelected += (sender, args) =>
            {
                if (listView.SelectedItem == null)
                    return;

                var tweet = listView.SelectedItem as Tweet;
                viewModel.OpenTweetCommand.Execute(tweet.StatusID);
                listView.SelectedItem = null;
            };

        }
    }
}

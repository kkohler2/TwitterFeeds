﻿using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using TwitterFeeds.Interfaces;
using TwitterFeeds.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TwitterFeeds.ViewModel
{
    public class MainPageViewModel : BaseViewModel
    {
        public static MainPageViewModel Instance { get; set; }
        IDataService dataService;
        protected IDataService DataService => dataService ?? (dataService = DependencyService.Get<IDataService>());

        bool includeRetweets;
        public bool IncludeRetweets 
        { 
            get { return includeRetweets; }
            set 
            { 
                includeRetweets = value;
                RefreshCommand.Execute(null);
            }
        }
        public List<Feed> Feeds { get; set; }
        public ObservableRangeCollection<Tweet> Tweets { get; set; }
        public Command<string> OpenTweetCommand { get; }
        public Command RefreshCommand { get; }

        public ICommand UpdateCommand => new Command(() => UpdateSubscribeListAsync());

        public Callback handler = UpdateComplete;

        public MainPageViewModel()
        {
            Tweets = new ObservableRangeCollection<Tweet>();
            Feeds = DataService.GetFeedsAsync().Result;
            //SelectedFeed = Feeds[0];
            OpenTweetCommand = new Command<string>(async (s) => await ExecuteOpenTweetCommand(s));
            RefreshCommand = new Command(async () =>
                {
                    if (IsBusy) return;
                    await SelectedFeedChanged();
                });
            Instance = this;
        }

        public async void UpdateSubscribeListAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            await Task.Delay(10);
            await DataService.DownloadFeedList(UpdateComplete);
        }

        public void UpdateCompleted(List<Feed> feeds)
        {
            IsBusy = false;
            if (feeds != null)
            {
                Feeds = feeds;
            }
        }

        public static void UpdateComplete(List<Feed> feeds)
        {
            Instance.UpdateCompleted(feeds);
        }

        private Feed _selectedFeed;

        public Feed SelectedFeed
        {
            get { return _selectedFeed; }
            set { SetProperty(ref _selectedFeed, value, "SelectedFeed", new Action(async () => await SelectedFeedChanged())); }
        }

        private async Task SelectedFeedChanged()
        {
            if (SelectedFeed == null) return;

            IsBusy = true;
            var items = await DataService.GetTweetsAsync(SelectedFeed.UserName, IncludeRetweets);
            if (items == null)
            {
                //await DisplayAlert("Error", "Unable to load tweets.", "OK");
            }
            else
            {
                Tweets.ReplaceRange(items);
            }
            IsBusy = false;
        }

        async Task ExecuteOpenTweetCommand(string statusId)
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                if (await Launcher.CanOpenAsync("twitter://"))
                {
                    await Launcher.OpenAsync($"twitter://status?id={statusId}");
                    return;
                }
                else if (await Launcher.CanOpenAsync("tweetbot://"))
                {
                    await Launcher.OpenAsync($"tweetbot:///status/{statusId}");
                    return;
                }
            }

            await OpenBrowserAsync($"http://twitter.com/{SelectedFeed.UserName}/status/" + statusId);
        }

        public static Task OpenBrowserAsync(string url) =>
            Browser.OpenAsync(url, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show,
                PreferredControlColor = Color.White,
                PreferredToolbarColor = (Color)Application.Current.Resources["PrimaryColor"]
            });
    }
}

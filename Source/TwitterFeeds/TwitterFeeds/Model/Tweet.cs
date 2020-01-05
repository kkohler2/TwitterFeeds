using TwitterFeeds.Helpers;
using System;

namespace TwitterFeeds.Models
{
    public partial class Tweet
    {
        public Tweet()
        {
        }

        public bool IsReTweet { get; set; }
        public string StatusID { get; set; }
        public string ScreenName { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public long RetweetCount { get; set; }
        public long FavoriteCount { get; set; }
        public string MediaUrl { get; set; }
        public bool HasMedia => !string.IsNullOrWhiteSpace(MediaUrl);
        public string Date => CreatedAt.ToString("g");
        public string DateHumanized => CreatedAt.TwitterHumanize();
        public string RTCount => RetweetCount == 0 ? string.Empty : RetweetCount + " RT";
    }
}

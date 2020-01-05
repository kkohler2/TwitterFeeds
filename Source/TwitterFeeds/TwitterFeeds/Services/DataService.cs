using TwitterFeeds.Interfaces;
using TwitterFeeds.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Essentials;

namespace TwitterFeeds.Services
{
    public class DataService : IDataService
    {
        private readonly HttpClient client;

        public DataService()
        {
            client = new HttpClient();
        }

        public Task<List<Feed>> GetFeedsAsync()
        {
            List<Feed> feeds = new List<Feed>();
            string indexFile = "feeds.txt";
            // Get Assembly
            var assembly = Assembly.GetExecutingAssembly();

            // Get Resources
            var resources = assembly.GetManifestResourceNames();

            // Get File Path from FileName
            string filePath = resources?.Single(resource => resource.EndsWith(indexFile, StringComparison.Ordinal));

            using (var stream = assembly.GetManifestResourceStream(filePath))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string l;
                    while ((l = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(l))
                        {
                            string[] parts = l.Split(',');
                            if (parts.Length == 2)
                            {
                                feeds.Add(new Feed { Name = parts[0], UserName = parts[1] });
                            }
                        }
                    }
                }
            }
            return Task.FromResult(feeds.OrderBy(f => f.Name).ToList());
        }


        public async Task<IEnumerable<Tweet>> GetTweetsAsync(string screenName, bool includeRetweets)
        {
            var accessToken = await GetAccessToken(client);

            var requestUserTimeline = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.twitter.com/1.1/statuses/user_timeline.json?count=100&screen_name={screenName}&trim_user=0&exclude_replies=1");

            requestUserTimeline.Headers.Add("Authorization", "Bearer " + accessToken);

            var responseUserTimeLine = await client.SendAsync(requestUserTimeline);
            var json = await responseUserTimeLine.Content.ReadAsStringAsync();

            var tweetsRaw = TweetRaw.FromJson(json);

            List<Tweet> tweets = tweetsRaw.Select(t => new Tweet
            {
                IsReTweet = t?.RetweetedStatus != null,
                StatusID = t?.RetweetedStatus?.User?.ScreenName == screenName ? t.RetweetedStatus.IdStr : t.IdStr,
                ScreenName = t?.RetweetedStatus?.User?.ScreenName ?? t.User.ScreenName,
                Text = t?.Text,
                RetweetCount = t.RetweetCount,
                FavoriteCount = t.FavoriteCount,
                CreatedAt = GetDate(t.CreatedAt, DateTime.MinValue),
                Image = t?.RetweetedStatus != null && t?.RetweetedStatus?.User != null ?
                                     t.RetweetedStatus.User.ProfileImageUrlHttps.ToString() : (t?.User?.ScreenName == screenName ? "liar_in_chief.png" : t?.User?.ProfileImageUrlHttps.ToString()),
                MediaUrl = t?.Entities?.Media?.FirstOrDefault()?.MediaUrlHttps?.AbsoluteUri
            }).ToList();
            if (!includeRetweets)
            {
                tweets = tweets.Where(t => !t.IsReTweet).ToList();
            }
            return tweets;
        }

        public readonly string[] DateFormats = { "ddd MMM dd HH:mm:ss %zzzz yyyy",
                                                         "yyyy-MM-dd\\THH:mm:ss\\Z",
                                                         "yyyy-MM-dd HH:mm:ss",
                                                         "yyyy-MM-dd HH:mm"};
        private DateTime GetDate(string date, DateTime defaultValue)
        {
            return string.IsNullOrWhiteSpace(date) ||
                !DateTime.TryParseExact(date,
                        DateFormats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result)
                    ? defaultValue
                    : result;
        }

        private static async Task<string> GetAccessToken(HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/oauth2/token ");
            var customerInfo = Convert.ToBase64String(new UTF8Encoding()
                                      .GetBytes("ZTmEODUCChOhLXO4lnUCEbH2I:Y8z2Wouc5ckFb1a0wjUDT9KAI6DUat5tFNdmIkPLl8T4Nyaa2J"));
            request.Headers.Add("Authorization", "Basic " + customerInfo);
            request.Content = new StringContent("grant_type=client_credentials",
                                                    Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonValue.Parse(json);

            return result["access_token"];
        }

    }
}

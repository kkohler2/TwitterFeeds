using TwitterFeeds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwitterFeeds.Interfaces
{
    public delegate void Callback(List<Feed> feeds);
    public interface IDataService
    {
        Task<List<Feed>> GetFeedsAsync();
        Task<IEnumerable<Tweet>> GetTweetsAsync(string screenName, bool includeRetweets);
        Task DownloadFeedList(Callback callback);
    }
}

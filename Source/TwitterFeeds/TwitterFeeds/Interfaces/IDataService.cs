using TwitterFeeds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwitterFeeds.Interfaces
{
    public interface IDataService
    {
        Task<List<Feed>> GetFeedsAsync();
        Task<IEnumerable<Tweet>> GetTweetsAsync(string screenName);
    }
}

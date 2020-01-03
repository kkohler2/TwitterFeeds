using TwitterFeeds.Models;
using Xamarin.Forms;

namespace TwitterFeeds.Views
{
    public class TweetDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TweetTemplate { get; set; }
        public DataTemplate TweetWithMediaTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((Tweet)item).HasMedia ? TweetWithMediaTemplate : TweetTemplate;
        }
    }
}

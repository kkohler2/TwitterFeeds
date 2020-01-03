using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TwitterFeeds.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TweetCell : ViewCell
    {
        public TweetCell()
        {
            InitializeComponent();
        }
    }
}
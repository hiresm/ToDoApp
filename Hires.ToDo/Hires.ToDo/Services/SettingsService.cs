using Windows.Storage;

namespace Hires.ToDo.Services
{
    public class SettingsService : ISettingsService
    {

        private string language;
        public string Language
        {
            get
            {
                if(language == null)
                {
                    language = ApplicationData.Current.LocalSettings.Values[SettingsKeys.Language].ToString();
                }

                return language;
            }
        }

        private string subscription;
        public string Subscription
        {
            get
            {
                if(subscription == null)
                {
                    subscription = ApplicationData.Current.LocalSettings.Values[SettingsKeys.SubscriptionKey].ToString();
                }

                return subscription;
            }
        }

        private string region;
        public string Region
        {
            get
            {
                if(region == null)
                {
                    region = ApplicationData.Current.LocalSettings.Values[SettingsKeys.Region].ToString();
                }

                return region;
            }
        }
    }

    public static class SettingsKeys
    {
        public const string Language = "language";
        public const string Region = "region";
        public const string SubscriptionKey = "subscriptionKey";
    }
}

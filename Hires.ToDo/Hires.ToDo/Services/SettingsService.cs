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
                    if (ApplicationData.Current.LocalSettings.Values.TryGetValue(SettingsKeys.Language, out var value))
                        language = value.ToString();
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
                    if (ApplicationData.Current.LocalSettings.Values.TryGetValue(SettingsKeys.SubscriptionKey, out var value))
                        subscription = value.ToString();
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
                    if (ApplicationData.Current.LocalSettings.Values.TryGetValue(SettingsKeys.Region, out var value))
                        region = value.ToString();
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

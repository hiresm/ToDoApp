using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Hires.ToDo.Models;
using Hires.ToDo.Services;
using Windows.Storage;

namespace Hires.ToDo.ViewModels
{
    public class SettingPageViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly ApplicationDataContainer applicationDataContainer;

        public RelayCommand BackCommand { get; set; }

        public List<KeyValuePair<string, string>> Regions
        {
            get { return RegionModel.Regions; }
        }

        private KeyValuePair<string, string> selectedRegion;
        public KeyValuePair<string, string> SelectedRegion
        {
            get { return selectedRegion; }
            set
            {
                selectedRegion = value;
                RaisePropertyChanged();
            }
        }

        public List<KeyValuePair<string, string>> Languages
        {
            get { return LanguageModel.Languages; }
        }

        private KeyValuePair<string, string> selectedLanguage;
        public KeyValuePair<string, string> SelectedLanguage
        {
            get { return selectedLanguage; }
            set
            {
                selectedLanguage = value;
                RaisePropertyChanged();
            }
        }

        private string subscriptionKey;
        public string SubscriptionKey
        {
            get { return subscriptionKey; }
            set
            {
                subscriptionKey = value;
                RaisePropertyChanged();
            }
        }

        public SettingPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            applicationDataContainer = ApplicationData.Current.LocalSettings;

            BackCommand = new RelayCommand(NavigateBack);

            //TODO OnNavigate method
            LoadSettings();
        }

        private void NavigateBack()
        {
            SaveSettings();
            navigationService.NavigateTo("MainPage");
        }

        private void SaveSettings()
        {
            applicationDataContainer.Values[SettingsKeys.SubscriptionKey] = SubscriptionKey;
            applicationDataContainer.Values[SettingsKeys.Region] = SelectedRegion.Key ?? "";
            applicationDataContainer.Values[SettingsKeys.Language] = SelectedLanguage.Key ?? "";
        }

        private void LoadSettings()
        {
            if (applicationDataContainer.Values.TryGetValue(SettingsKeys.SubscriptionKey, out var subscriptionKey))
                SubscriptionKey = subscriptionKey.ToString();
            if (applicationDataContainer.Values.TryGetValue(SettingsKeys.Region, out var region))
                SelectedRegion = RegionModel.Regions.FirstOrDefault(x => x.Key == region.ToString());
            if (applicationDataContainer.Values.TryGetValue(SettingsKeys.Language, out var language))
                SelectedLanguage = LanguageModel.Languages.FirstOrDefault(x => x.Key == language.ToString());
        }
    }
}

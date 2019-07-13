using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Hires.ToDo.Helpers;
using Hires.ToDo.Models;
using Hires.ToDo.Services;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Hires.ToDo.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private const string FileName = "data.xml";

        private readonly IPersistationService persistationService;
        private readonly ISettingsService settingsService;
        private readonly INavigationService navigationService;
        private readonly ApplicationDataContainer applicationDataContainer;

        private TaskCompletionSource<bool> stopCompletionSource;
        private bool canRead;
        private bool canListen;


        public RelayCommand AddCommand { get; set; }
        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand SettingCommand { get; set; }
        public RelayCommand ListenCommand { get; set; }
        public RelayCommand ReadCommand { get; set; }

        public NotifyTaskCompletion<ObservableCollection<Item>> Items { get; set; }
        private Item selectedItem;
        public Item SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; RaisePropertyChanged(); }
        }

        public MainPageViewModel(INavigationService navigationService, IPersistationService persistationService, ISettingsService settingsService)
        {
            this.persistationService = persistationService;
            this.settingsService = settingsService;
            this.navigationService = navigationService;

            Application.Current.Suspending += Current_Suspending;

            AddCommand = new RelayCommand(AddItem);
            RemoveCommand = new RelayCommand(RemoveItem);
            SettingCommand = new RelayCommand(NavigateSetting);
            ListenCommand = new RelayCommand(Listen);
            ReadCommand = new RelayCommand(Read, CanRead);

            Items = new NotifyTaskCompletion<ObservableCollection<Item>>(persistationService.LoadData<ObservableCollection<Item>>(FileName));
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            persistationService.SaveData(Items.Result, FileName);
        }

        #region Commands
        private void RemoveItem()
        {
            if (Items.Result == null) return;

            Items.Result.Remove(SelectedItem);
        }

        private void AddItem()
        {
            if (Items.Result == null) return;

            SelectedItem = new Item { Created = DateTime.Now };
            Items.Result.Insert(0, SelectedItem);
        }

        private void NavigateSetting()
        {
            navigationService.NavigateTo("SettingPage");
        }

        private async void Read() //Ugly code
        {
            using (var synthesizer = new SpeechSynthesizer(SpeechConfig.FromSubscription(settingsService.Subscription, settingsService.Region), null))
            {
                using (var result = await synthesizer.SpeakTextAsync(SelectedItem.Text).ConfigureAwait(false))
                {
                    if(result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        using (var audioStream = AudioDataStream.FromResult(result))
                        {
                            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputaudio.wav");
                            await audioStream.SaveToWaveFileAsync(path);
                            var player = new MediaPlayer();
                            player.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(path));
                            player.Play();
                        }
                    }
                    else
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    }
                }
            }
        }

        private bool CanRead()
        {
            return SelectedItem != null;
        }

        private async void Listen() //Ugly code
        {
            string text;
            var mediaCapture = new Windows.Media.Capture.MediaCapture();
            var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
            await mediaCapture.InitializeAsync(settings);

            using (var recognizer = new SpeechRecognizer(GetSpeechConfig()))
            {
                text = await RunRecognizer(recognizer);
            }

            SelectedItem.Text = text;
        }

        private SpeechConfig GetSpeechConfig()
        {
            var config = SpeechConfig.FromSubscription(settingsService.Subscription, settingsService.Region);
            config.SpeechRecognitionLanguage = settingsService.Language;

            return config;
        }

        private async Task<string> RunRecognizer(SpeechRecognizer speechRecognizer)
        {
            
            
            var result = await speechRecognizer.RecognizeOnceAsync().ConfigureAwait(false);
            return result.Text;
            //EventHandler<SpeechRecognitionCanceledEventArgs> canceledHandler = (sender, e) => CanceledEventHandler(e, source);
            //EventHandler<SessionEventArgs> sessionStartedHandler = (sender, e) => SessionStartedEventHandler(e);
            //EventHandler<SessionEventArgs> sessionStoppedHandler = (sender, e) => SessionStoppedEventHandler(e, source);
            //EventHandler<RecognitionEventArgs> speechStartDetectedHandler = (sender, e) => SpeechDetectedEventHandler(e, "start");
            //EventHandler<RecognitionEventArgs> speechEndDetectedHandler = (sender, e) => SpeechDetectedEventHandler(e, "end");
        }

        //private void NotifyUser(string strMessage, NotifyType type)
        //{
        //    // If called from the UI thread, then update immediately.
        //    // Otherwise, schedule a task on the UI thread to perform the update.
        //    if (Dispatcher.HasThreadAccess)
        //    {
        //        UpdateStatus(strMessage, type);
        //    }
        //    else
        //    {
        //        var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
        //    }
        //}

        #endregion

    }
}

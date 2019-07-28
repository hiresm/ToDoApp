using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Hires.ToDo.Helpers;
using Hires.ToDo.Models;
using Hires.ToDo.Services;
using Microsoft.CognitiveServices.Speech;
using System;
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
        private readonly MediaPlayer mediaPlayer;

        private TaskCompletionSource<int> stopListening;
        private DispatcherTimer timer;
        private int timeCounter;

        public RelayCommand AddCommand { get; set; }
        public RelayCommand RemoveCommand { get; set; }
        public RelayCommand SettingCommand { get; set; }
        public RelayCommand ListenCommand { get; set; }
        public RelayCommand ReadCommand { get; set; }

        public NotifyTaskCompletion<ObservableCollectionWithItemNotify<Item>> Items { get; set; }

        private bool isListening;
        public bool IsListening
        {
            get { return isListening; }
            set
            {
                isListening = value;
                RaisePropertyChanged();
            }
        }

        private Item selectedItem;
        public Item SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                RaisePropertyChanged();
                CanExecuteChanged();
            }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                timer.Tick += Timer_Tick;
                timer.Start();
                RaisePropertyChanged();
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            if(timeCounter == 3)
            {
                Status = string.Empty;
                timeCounter = 0;
                timer.Stop();
            }

            timeCounter++;
        }

        public MainPageViewModel(INavigationService navigationService, IPersistationService persistationService, ISettingsService settingsService)
        {
            this.persistationService = persistationService;
            this.settingsService = settingsService;
            this.navigationService = navigationService;

            mediaPlayer = new MediaPlayer();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);

            Application.Current.Suspending += Current_Suspending;

            AddCommand = new RelayCommand(AddItem);
            RemoveCommand = new RelayCommand(RemoveItem, IsItemSelected);
            SettingCommand = new RelayCommand(NavigateSetting);
            ListenCommand = new RelayCommand(Listen, IsItemSelected);
            ReadCommand = new RelayCommand(Read, IsItemSelected);

            Items = new NotifyTaskCompletion<ObservableCollectionWithItemNotify<Item>>(persistationService.LoadData<ObservableCollectionWithItemNotify<Item>>(FileName));
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            persistationService.SaveData(Items.Result, FileName);
        }

        #region Commands
        private void CanExecuteChanged()
        {
            ReadCommand.RaiseCanExecuteChanged();
            RemoveCommand.RaiseCanExecuteChanged();
            ListenCommand.RaiseCanExecuteChanged();
        }

        private void RemoveItem()
        {
            if (Items.Result == null) return;

            Items.Result.Remove(SelectedItem);
        }

        private bool IsItemSelected()
        {
            return SelectedItem != null;
        }

        private void AddItem()
        {
            if (Items.Result == null) return;

            var item = new Item { Created = DateTime.Now };
            Items.Result.Insert(0, item);
            SelectedItem = item;
        }

        private void NavigateSetting()
        {
            navigationService.NavigateTo("SettingPage");
        }

        private async void Read() //Ugly code
        {
            if (string.IsNullOrEmpty(SelectedItem.Text)) return;

            using (var synthesizer = new SpeechSynthesizer(GetSpeechConfig(), null))
            {
                using (var result = await synthesizer.SpeakTextAsync(SelectedItem.Text).ConfigureAwait(false))
                {
                    if(result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        using (var audioStream = AudioDataStream.FromResult(result))
                        {
                            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputaudio.wav");
                            await audioStream.SaveToWaveFileAsync(path);
                            mediaPlayer.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(path));
                            mediaPlayer.Play();
                        }
                    }
                    else
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result); //TODO Notify user
                    }
                }
            }
        }

        private async void Listen() //Ugly code
        {
            if (isListening)
            {
                IsListening = false;
                stopListening.TrySetResult(0);
            }
            else
            {
                IsListening = true;
                stopListening = new TaskCompletionSource<int>();

                await SetupCapturMedia();

                using (var recognizer = new SpeechRecognizer(GetSpeechConfig()))
                {
                    await RunRecognizer(recognizer, stopListening).ConfigureAwait(false);
                }
            }
        }

        private static async Task SetupCapturMedia()
        {
            var mediaCapture = new Windows.Media.Capture.MediaCapture();
            var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
            await mediaCapture.InitializeAsync(settings);
        }

        private SpeechConfig GetSpeechConfig()
        {
            var config = SpeechConfig.FromSubscription(settingsService.Subscription, settingsService.Region);
            config.SpeechRecognitionLanguage = settingsService.Language;
            config.OutputFormat = OutputFormat.Detailed;
            config.EnableDictation();

            return config;
        }

        private async Task RunRecognizer(SpeechRecognizer speechRecognizer, TaskCompletionSource<int> source)
        {
            EventHandler<SpeechRecognitionCanceledEventArgs> canceledHandler = (sender, e) => CanceledEventHandler(e, source);
            EventHandler<SessionEventArgs> sessionStartedHandler = (sender, e) => SessionStartedEventHandler(e);
            EventHandler<SessionEventArgs> sessionStoppedHandler = (sender, e) => SessionStoppedEventHandler(e, source);

            speechRecognizer.Recognized += SpeechRecognizer_Recognized;
            speechRecognizer.Canceled += canceledHandler;
            speechRecognizer.SessionStarted += sessionStartedHandler;
            speechRecognizer.SessionStopped += sessionStoppedHandler;

            await speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            await source.Task.ConfigureAwait(false);
            await speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

            speechRecognizer.Recognized -= SpeechRecognizer_Recognized;
            speechRecognizer.Canceled -= canceledHandler;
            speechRecognizer.SessionStarted -= sessionStartedHandler;
            speechRecognizer.SessionStopped -= sessionStoppedHandler;
            
        }

        private void SpeechRecognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                SelectedItem.Text += $" {e.Result.Text}";
            });
        }

        private void CanceledEventHandler(SpeechRecognitionCanceledEventArgs e, TaskCompletionSource<int> source)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                IsListening = false;
                Status = "Recognition canceled";
            });
            
            source.TrySetResult(0);
        }

        private void SessionStoppedEventHandler(SessionEventArgs e, TaskCompletionSource<int> source)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                IsListening = false;
                Status = "Recognition session stopped";
            });

            source.TrySetResult(0);
        }

        private void SessionStartedEventHandler(SessionEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                Status = "Recognition session started";
            });
        }

        #endregion

    }
}

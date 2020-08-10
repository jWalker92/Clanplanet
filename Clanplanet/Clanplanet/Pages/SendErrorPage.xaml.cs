using Clanplanet.Dependencies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Clanplanet.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendErrorPage : ContentPage
    {
        public static List<ErrorDataPackage> EDPackages;

        private string buttonText;
        private string description;
        private bool canSend;
        private string errorText;

        public SendErrorPage()
        {
            if (EDPackages == null)
            {
                EDPackages = new List<ErrorDataPackage>();
            }
            SetTexts();
            InitializeComponent();
            BindingContext = this;
        }

        private void SetTexts()
        {
            if (GlobalErrorValues.Current.IsCollectingReportData)
            {
                Description = string.Format("Der Fehlerbericht wird momentan gesammelt. Es befinden sich {0} aufgenommene Schritte im Speicher.\r\n" +
                    "Der Vorgang kann einen Moment dauern (je nach Menge der gesammelten Daten)", EDPackages.Count);
                ButtonText = "Absenden";
                CanSend = true;
            }
            else
            {
                Description = "Wenn Probleme auftreten sollten, dann kann mit dieser Funktion eine Logdatei zum Entwickler gesendet werden.\r\n" +
                    "Durch einen Klick auf den Button \"Starten\" wird der Aufnahme-Prozess gestartet. Während dieser Aufnahme muss der gefundene Fehler noch einmal reproduziert (nachgestellt) werden.\r\n" +
                    "Also wenn man sich zum Beispiel bei einem Event nicht anmelden kann, dann versucht man sich während der Aufnahme anzumelden.\r\n" +
                    "Wenn der Fehler reproduziert wurde, kann auf dieser Seite dieser Bericht letztendlich abgeschickt werden.\r\n" +
                    "Im besten Fall sollten nicht zu viele unnötige Schritte aufgenommen werden.";
                ButtonText = "Starten";
                ErrorText = "";
                CanSend = false;
            }
            OnPropertyChanged("Description");
            OnPropertyChanged("ButtonText");
            OnPropertyChanged("ErrorText");
            OnPropertyChanged("CanSend");
        }

        public string ButtonText { get => buttonText; set => buttonText = value; }
        public string Description { get => description; set => description = value; }
        public bool CanSend { get => canSend; set => canSend = value; }
        public string ErrorText { get => errorText; set => errorText = value; }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (GlobalErrorValues.Current.IsCollectingReportData)
            {
                if (EDPackages.Count > 0)
                {
                    DependencyService.Get<ISendMail>().Send(ErrorText, CompressString(JsonConvert.SerializeObject(EDPackages)));
                    EDPackages.Clear();
                    GlobalErrorValues.Current.IsCollectingReportData = false;
                    Device.BeginInvokeOnMainThread(() => { App.CurrentApp.MasterDetail.NavPage.PopAsync(); });
                }
            }
            else
            {
                GlobalErrorValues.Current.IsCollectingReportData = true;
                Device.BeginInvokeOnMainThread(() => {App.CurrentApp.MasterDetail.NavPage.PopAsync(); }); //App.CurrentApp.MasterDetail.NavPage = new NavigationPage(new MainPage());  
            }
        }

        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            GlobalErrorValues.Current.IsCollectingReportData = false;
            EDPackages.Clear();
            SetTexts();
        }
    }

    public class GlobalErrorValues : INotifyPropertyChanged
    {
        public bool IsCollectingReportData { get => isCollectingReportData; set { isCollectingReportData = value;
                OnPropertyChanged("IsCollectingReportData");
                OnPropertyChanged("SLPadding");
                OnPropertyChanged("SLBGColor");
            } }
        public Thickness SLPadding { get { return isCollectingReportData ? new Thickness(4) : new Thickness(0); } }
        public Color SLBGColor { get { return isCollectingReportData ? Color.Red : Color.Transparent; } }

        private static bool isCollectingReportData;

        public static GlobalErrorValues Current = new GlobalErrorValues();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ErrorDataPackage
    {
        public string ErrorData;
        public string Origin;
        public DateTime TimeStamp;
    }
}
using System;
using System.IO;
using System.Net;
using System.Text;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int motionPin = 4;
        private GpioPin pin;

        public MainPage()
        {
            this.InitializeComponent();
            InitGPIO();
            UiNoMotion();
        }


        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                message.Text = "No GPIO on Device";
            }
            else
            {
                pin = gpio.OpenPin(motionPin);
                pin.SetDriveMode(GpioPinDriveMode.Input);
                pin.ValueChanged += pin_ValueChanged;
            }
            
        }

        private async void pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (args.Edge.CompareTo(GpioPinEdge.RisingEdge) == 0)
                {
                    //Motion Detected UI
                    UiAlert();

                    //Create JSON payload
                    var json = string.Format("{{sensor:Motion,  room:MsConfRoom1,  utc:{0}}}", DateTime.UtcNow.ToString("MM/dd/yyyy_HH:mm:ss"));
                    var data = new ASCIIEncoding().GetBytes(json);

                    //POST Data
                    string url = "https://rrpiot.azurewebsites.net/SensorData";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    using (Stream myStream = await request.GetRequestStreamAsync())
                    {
                        myStream.Write(data, 0, data.Length);
                    }
                    await request.GetResponseAsync();
                }
                else
                {
                    //Display No Motion Detected UI
                    UiNoMotion();
                }
            });

        }

        private void UiAlert()
        {
            grid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
            message.Text = "Motion Detected";
        }

        private void UiNoMotion()
        {
            grid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 255, 0));
            message.Text = "Monitoring";
        }

    }
}

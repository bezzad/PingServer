using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace PingServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool cancel;
        private int counter = 0;
        private string lastAnimate = "";
        private NotifyIcon nIcon;

        private Storyboard stbFail;
        private Storyboard stbFailRemove;
        private Storyboard stbSuccess;
        private Storyboard stbSuccessRemove;


        public MainWindow()
        {
            InitializeComponent();

            txtURL.Text = "www.Google.com";

            stbFail = (Storyboard)TryFindResource("FailConnect");
            stbFailRemove = (Storyboard)TryFindResource("FailConnectRemove");
            stbSuccess = (Storyboard)TryFindResource("SuccessConnect");
            stbSuccessRemove = (Storyboard)TryFindResource("SuccessConnectRemove");

            this.nIcon = new NotifyIcon();
            this.nIcon.Click += (source, e) => this.Show();
            this.nIcon.Visible = true;
            this.nIcon.Icon = global::PingServer.Properties.Resources.favicon;
            this.nIcon.ShowBalloonTip(10000, "Ping Server", "Application is exactly running", ToolTipIcon.Info);
        }

        public bool Cancel
        {
            get { return cancel; }
            set
            {
                cancel = value;
                if (value)
                    btnPing.IsEnabled = true;
                else btnPing.IsEnabled = false;
            }
        }

        public int Counter
        {
            get { return counter; }
            set
            {
                counter = value;
                lblCounter.Content = value.ToString();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            try
            {
                base.OnMouseLeftButtonDown(e);

                // Begin dragging the window
                // if mouse down on LayoutRoot
                this.DragMove();
            }
            catch { }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (this.WindowState == System.Windows.WindowState.Minimized)
                this.Hide();
        }
        private void btnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Cancel = true;

            switch (lastAnimate)
            {
                case "Success":
                    {
                        this.nIcon.Icon = global::PingServer.Properties.Resources.Inactive;
                        this.nIcon.ShowBalloonTip(3000, "Ping Server", "Application inactivated by user", ToolTipIcon.Info);

                        stbSuccessRemove.Begin();

                        lastAnimate = "";
                    } break;

                case "Fail":
                    {
                        this.nIcon.Icon = global::PingServer.Properties.Resources.Inactive;
                        this.nIcon.ShowBalloonTip(3000, "Ping Server", "Application inactivated by user", ToolTipIcon.Info);

                        stbFailRemove.Begin();

                        lastAnimate = "";
                    } break;
            }
        }

        private void btnExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Cancel = true;
            System.Windows.Application.Current.Shutdown();
        }

        private async void btnPing_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Cancel = false;

            do
            {
                await TicPing();

                Counter++;

                await Task.Delay(3000);
            } while (!Cancel);
        }
        private async void Fail()
        {
            switch (lastAnimate)
            {
                case "Fail": break;

                case "Success":
                    {
                        this.nIcon.Icon = global::PingServer.Properties.Resources.Error;
                        this.nIcon.ShowBalloonTip(3000, "Ping Server", "Internet Disconnected!", ToolTipIcon.Error);

                        stbSuccessRemove.Begin();

                        lastAnimate = "Fail";

                        await Task.Delay(1000);

                        stbFail.Begin();
                    } break;

                default:
                    {
                        lastAnimate = "Fail";

                        this.nIcon.Icon = global::PingServer.Properties.Resources.Error;
                        this.nIcon.ShowBalloonTip(3000, "Ping Server", "Internet Disconnected!", ToolTipIcon.Error);

                        stbFail.Begin();
                    } break;
            }
        }

        private async Task<bool> PingRequest()
        {
            try
            {
                Ping ping = new Ping();

                PingReply p = await ping.SendPingAsync(txtURL.Text, 3000);

                return p.Status == IPStatus.Success;
            }
            catch { return false; }
        }

        private async void Success()
        {
            switch (lastAnimate)
            {
                case "Success": break;

                case "Fail":
                    {
                        this.nIcon.Icon = global::PingServer.Properties.Resources.Computers;
                        this.nIcon.ShowBalloonTip(3000, "Ping Server", "Internet Connected", ToolTipIcon.Info);

                        stbFailRemove.Begin();

                        lastAnimate = "Success";

                        await Task.Delay(1000);

                        stbSuccess.Begin();
                    } break;

                default:
                    {
                        lastAnimate = "Success";

                        this.nIcon.Icon = global::PingServer.Properties.Resources.Computers;
                        this.nIcon.ShowBalloonTip(3000, "Ping Server", "Internet Connected", ToolTipIcon.Info);

                        stbSuccess.Begin();
                    } break;
            }
        }

        private async Task TicPing()
        {
            if (await PingRequest()) // Success
            { Success(); }
            else // Fail
            { Fail(); }
        }
    }
}
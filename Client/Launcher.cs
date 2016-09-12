using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Sean.WorldClient.Hosts.World;
using Sean.WorldClient.Utilities;

namespace Sean.WorldClient
{
    public partial class Launcher : Form
    {
        public Launcher()
        {
            InitializeComponent();
            Settings.Launcher = this;
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            Config.Load();
			ReadFromConfig();

            //other settings
            cbSoundEnabled.Checked = Config.SoundEnabled;
            cbMusic.Checked = Config.MusicEnabled;
        }
			
		private IPAddress _serverIp =  new IPAddress(new byte[]{127,0,0,1});
        private ushort _serverPort = 8084;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (txtUserName.Enabled && txtUserName.Text.Trim().Length == 0) { Misc.MessageError("UserName is required."); return; }

            FormLoading();

            try
            {
                var addressList = Dns.GetHostAddresses(ddlServerIp.Text.Split(' ')[0]); //if an ip was entered then no lookup is performed, otherwise a dns lookup is attempted
                foreach (var ipAddress in addressList.Where(ipAddress => ipAddress.GetAddressBytes().Length == 4)) //look for the ipv4 address
                {
                    _serverIp = ipAddress;
                    break;
                }
                if (_serverIp == null) throw new Exception("Valid IPv4 address not found.");
            }
            catch (Exception ex)
            {
                Misc.MessageError("Invalid Server IP address or hostname: " + ex.Message);
                FormReset();
                return;
            }

            if (!UInt16.TryParse(txtPort.Text, out _serverPort))
            {
                Misc.MessageError("Invalid Server Port.");
                FormReset();
                return;
            }

            SaveConfig();

			GameActions.NetworkClient.Connect();
			GameActions.NetworkClient.SendPing();

			InitGame ();
        }

		private void InitGame()//object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //if (e.Error != null) throw e.Error;

                UpdateProgress("Initializing Game Window", 0, 0);
                using (var game = new Game())
                {
                    Hide(); //need to hide the launcher rather then closing so that any errors can still display in a message box
                    Diagnostics.OutputDebugInfo(); //this only works after the game window has been initialized
                    game.Icon = Icon; //use this forms icon directly rather then from a resource file so the icon isnt in the output exe twice (reduces exe by 22k)
                    if (!Config.Windowed) game.WindowState = OpenTK.WindowState.Fullscreen; else if (Config.Maximized) game.WindowState = OpenTK.WindowState.Maximized;
                    game.Run(Constants.UPDATES_PER_SECOND);
                }
            }
            catch (ServerConnectException ex)
            {
                Misc.MessageError(ex.Message);
                FormReset();
                return; //no need to restart for a connect exception
            }
            catch (ServerDisconnectException ex)
            {
#if DEBUG
                Misc.MessageError(string.Format("{0}: {1}", ex.Message, ex.InnerException.StackTrace));
#else
                Misc.MessageError(ex.Message);
#endif
                Application.Restart(); //just restart the app so theres no need to worry about lingering forms, settings, state issues, etc.                
            }
            Application.Exit(); //close the application in case the launcher is still running
        }

        /// <summary>Change the form to a loading state.</summary>
        private void FormLoading()
        {
            foreach (Control control in Controls) control.Enabled = false; //disable the entire form
            btnStart.Text = "Loading...";
        }

        /// <summary>Reset the form if we hit any validation errors.</summary>
        private void FormReset()
        {
            foreach (Control control in Controls) control.Enabled = true;
            btnStart.Text = "Start Game";
            txtProgress.Visible = false;
            pbProgress.Visible = false;
        }

        private void ReadFromConfig()
        {
            txtUserName.Text = Config.UserName;
            ddlServerIp.Text = Config.Server;
            txtPort.Text = Config.Port.ToString();
            cbSoundEnabled.Checked = Config.SoundEnabled;
            cbMusic.Checked = Config.MusicEnabled;
        }
        private void SaveConfig()
        {
            Config.UserName = txtUserName.Text.Trim();
            Config.Server = ddlServerIp.Text;
            Config.Port = ushort.Parse(txtPort.Text);
            Config.SoundEnabled = cbSoundEnabled.Checked;
            Config.MusicEnabled = cbMusic.Checked;
            Config.Save();
        }

        internal void UpdateProgressInvokable(string message, int currentProgress, int maxProgress)
        {
            if (!InvokeRequired) UpdateProgress(message, currentProgress, maxProgress); else Invoke((MethodInvoker)(() => UpdateProgress(message, currentProgress, maxProgress))); //allows other threads to update this form
        }

        internal void UpdateProgress(string message, int currentProgress, int maxProgress)
        {
            if (String.IsNullOrEmpty(message))
            {
                txtProgress.Visible = false;
                pbProgress.Visible = false;
            }
            else
            {
                txtProgress.Visible = true;
                txtProgress.Text = message;
                pbProgress.Visible = maxProgress > 0;
                pbProgress.Maximum = maxProgress;
                pbProgress.Value = currentProgress;
            }
            Update();
        }

        #region Menu
        private void mnuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mnuVisitWebSite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Constants.URL);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Format("{0}\n{1}\n{2}\n{3}\n\nSome sounds from:\nwww.soundjay.com\nwww.freesound.org\nwww.soundbible.com\nwww.nosoapradio.us", Application.ProductName, Application.ProductVersion, Constants.URL, Application.CompanyName), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}
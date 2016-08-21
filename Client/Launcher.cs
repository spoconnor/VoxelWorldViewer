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

            //video settings
            cbVSync.Checked = Config.VSync;
            cbMipmapping.Checked = Config.Mipmapping;
            cbFog.Checked = Config.Fog;
            cbLinearMagnificationFilter.Checked = Config.LinearMagnificationFilter;
            cbSmoothLighting.Checked = Config.SmoothLighting;
            cbWindowed.Checked = Config.Windowed;

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

            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.RunWorkerCompleted += InitGame;
            backgroundWorker.DoWork += GameActions.NetworkClient.Connect;
            backgroundWorker.RunWorkerAsync(new object[] { _serverIp, _serverPort });
        }

        private void InitGame(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null) throw e.Error;

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
#if DEBUG
#else //comment this line to use this error handling while in debug mode
            catch (Exception ex)
            {
                try
                {
                    Misc.MessageError(string.Format("{0}\n\nApplication Version: {1}\nServer: {2}\nPosition: {3}\nPerformance: {4}\n\nOpenGL: {5} {6}\nGLSL: {7}\nVideo Card: {8}\nOS: {9}\nCulture: {10}\n\n{11}",
                                                    ex.Message,
                                                    ProductVersion,
                                                    _serverIp != null ? string.Format("{0}:{1}", _serverIp, _serverPort) : "n/a",
                                                    Game.Player != null ? Game.Player.Coords.ToString() : "unknown",
                                                    Game.PerformanceHost != null ? string.Format("{0} mb, {1} fps", Game.PerformanceHost.Memory, Game.PerformanceHost.Fps) : "unknown",
                                                    Diagnostics.OpenGlVersion,
                                                    Diagnostics.OpenGlVendor,
                                                    Diagnostics.OpenGlGlsl,
                                                    Diagnostics.OpenGlRenderer,
                                                    Diagnostics.OperatingSystem,
                                                    System.Globalization.CultureInfo.CurrentCulture.Name,
                                                    ex.StackTrace));
                }
                catch (Exception exInner)
                {
                    //should never happen, but we end up here if while trying to display the nice message some of the info is missing/null
                    //and ive already caught myself making several dumb mistakes by having this. its easy to introduce problems with the
                    //error handler above because its only getting compiled in release mode, so leave this here as well.
                    Misc.MessageError(string.Format("Error: {0}\n\n{1}", exInner.Message, exInner.StackTrace));
                }
            }
#endif
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

        private void SaveConfig()
        {
            Config.UserName = txtUserName.Text.Trim();
            Config.Server = ddlServerIp.Text;
            Config.Port = ushort.Parse(txtPort.Text);
            Config.VSync = cbVSync.Checked;
            Config.Mipmapping = cbMipmapping.Checked;
            Config.Fog = cbFog.Checked;
            Config.LinearMagnificationFilter = cbLinearMagnificationFilter.Checked;
            Config.SmoothLighting = cbSmoothLighting.Checked;
            Config.Windowed = cbWindowed.Checked;
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
using AmbilightLib;
using HidSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AmbilightGUI
{
    class MainWindowVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private IAmbilight model = new Ambilight();
        /// <summary>
        /// Default constructor. subscribes as listener to model.DataReceived
        /// </summary>
        public MainWindowVM()
        {
            model.DataReceived += Model_DataReceived;
        }
        /// <summary>
        /// updates ui when model changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_DataReceived(object sender, EventArgs e)
        {
            FPS = model.fps;
            DeviceFPS = model.deviceFps;
            if (model.state == true)
                State = "Running";
            else State = "Stopped";
        }

        public int Left
        {
            get
            {
                return model.vertical;
            }
            set
            {
                model.vertical = value;
            }
        }
        public int Right
        {
            get
            {
                return model.vertical;
            }
            set
            {
                model.vertical = value;
            }
        }
        public int Top
        {
            get
            {
                return model.horizontal;
            }
            set
            {
                model.horizontal = value;
            }
        }
        public int Bottom
        {
            get
            {
                return model.horizontal;
            }
            set
            {
                model.horizontal = value;
            }
        }

        public int VerticalOffset
        {
            get
            {
                return model.verticalOffset;
            }
            set
            {
                model.verticalOffset = value;
            }
        }

        public int HorizontalOffset
        {
            get
            {
                return model.horizontalOffset;
            }
            set
            {
                model.horizontalOffset = value;
            }
        }
        public int Depth
        {
            get
            {
                return model.depth;
            }
            set
            {
                model.depth = value;
            }
        }
        private int fps;
        public int FPS {
            get
            {
                return fps;
            }
            set
            {
                fps = value; NotifyPropertyChanged();
            }
        }
        private int deviceFps;
        public int DeviceFPS
        {
            get
            {
                return deviceFps;
            }
            set
            {
                deviceFps = value; NotifyPropertyChanged();
            }
        }
        private string state;
        public string State
        {
            get
            {
                return state;
            }
            set
            {
                state = value; NotifyPropertyChanged();
            }
        }
        /// <summary>
        /// bool used to determine if parameters are editable 
        /// </summary>
        private bool readOnly = false;
        public bool ReadOnly { get => readOnly; set { readOnly = value; NotifyPropertyChanged(); } }

        /// <summary>
        /// gets list of currently connected devices
        /// </summary>
        public List<string> Devices { get {
                HidDevice[] list = (HidDevice[])model.GetDevice().GetDeviceList();
                List<string> devices = new List<string>();
                foreach (var device in list)
                {
                    devices.Add(device.ProductName);
                }
                return devices; } }

        private string selectedPort;
        public string SelectedPort
        {
            get { return selectedPort; }
            set { selectedPort = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// command for the start button
        /// </summary>
        public RelayCommand StartCommand { get { return new RelayCommand((x) => Start(x)); } }
        private void Start(object x)
        {
            ReadOnly = true;
            model.Start(SelectedPort);
        }
        /// <summary>
        /// command for stop button
        /// </summary>
        public RelayCommand StopCommand { get { return new RelayCommand((x) => Stop(x)); } }

        private void Stop(object x)
        {
            ReadOnly = false;
            model.Stop();
        }
    }
}

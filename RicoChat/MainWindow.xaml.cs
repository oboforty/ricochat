using NAudio.Wave;
using RicoChat.api;
using RicoChat.test;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RicoChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VoiceIO app;
        private IVoiceHandler hand_out;
        private IVoiceHandler hand_udp;

        public MainWindow()
        {
            InitializeComponent();

            // Init GUI:
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                InputComboBox.Items.Add(WaveIn.GetCapabilities(i).ProductName);
            }
            if (WaveIn.DeviceCount > 0)
                InputComboBox.SelectedIndex = 0;

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                OutputComboBox.Items.Add(WaveOut.GetCapabilities(i).ProductName);
            }
            if (WaveOut.DeviceCount > 0)
                OutputComboBox.SelectedIndex = 0;


            // Init Voice IO + Connections
            int InputAudioDevice = InputComboBox.SelectedIndex;
            int OutputAudioDevice = OutputComboBox.SelectedIndex;

            //string host = "127.0.0.1:9001";
            string host = "80.217.114.215:9001";

            //var hand_save = new TestSoundSave();
            //var pp = new TestSoundIO(hand_udp);

            // Right now the chat plays back what you say
            hand_out = new VoicePlayback(OutputAudioDevice);
            hand_udp = new VoiceClient(host, hand_out);
            app = new VoiceIO(hand_udp, InputAudioDevice);
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            hand_udp.Dispose();
            hand_out.Dispose();
            app.Dispose();
        }

    }
}

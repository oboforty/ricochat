﻿using NAudio.Wave;
using RicoChat.api;
using RicoChat.test;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        RicoChatClient client;

        public BitmapImage im_online = new BitmapImage(new Uri(@"pack://application:,,,/Resources/online.png"));
        public BitmapImage im_offline = new BitmapImage(new Uri(@"pack://application:,,,/Resources/offline.png"));
        public BitmapImage im_talking = new BitmapImage(new Uri(@"pack://application:,,,/Resources/talking.png"));
        public BitmapImage im_admin = new BitmapImage(new Uri(@"pack://application:,,,/Resources/admin.png"));

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



            client = new RicoChatClient();
            //client.SetDevices(InputComboBox.SelectedIndex, OutputComboBox.SelectedIndex);

            //string addr = "80.217.114.215";
            string[] lines = File.ReadAllText("connection.txt").Split('\n');

            string addr = lines[0].Split(':')[0];
            int port = 9001;

            string username = lines[1];
            string uid = lines[1];

            if (lines.Length >= 3 && !string.IsNullOrWhiteSpace(lines[2]))
                // client-provided uuid (otherwise it's username)
                uid = lines[2];

            var _users = client.ConnectToServer(uid, username, addr, port);

            if (_users != null)
            {

                foreach (KeyValuePair<string, string> user in _users)
                {
                    Label label = new Label();
                    label.Content = user.Value;
                    label.Name = "Username_" + user.Key;
                    label.Height = 35;
                    label.Foreground = new SolidColorBrush(Colors.White);
                    Usernames.Children.Add(label);

                    Image img = new Image();
                    img.Source = im_offline;
                    img.Width = 35;
                    img.Height = 35;
                    img.Name = "Profile_" + user.Key;
                    Images.Children.Add(img);
                }

                client.StartVoiceIO(InputComboBox.SelectedIndex, OutputComboBox.SelectedIndex);
            }
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            client.Dispose();
        }

    }
}

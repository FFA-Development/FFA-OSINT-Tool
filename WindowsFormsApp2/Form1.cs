using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public RichTextBox Logs;
        public bool Scanning;
        public bool pinging;
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Load("https://images-ext-1.discordapp.net/external/TbeYEhrQJfDaCJM8oIHWaYYCxNwCNKiwNz5WgEf11lo/%3Fsize%3D1024/https/cdn.discordapp.com/icons/1370013148983201792/d26c2fddc3bdaf3a2fbd047c4fe4ec87.png");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            this.Logs = richTextBox1;
            new Thread(multi_threaded_scan).Start();
        }

        public async void multi_threaded_scan()
        {
            this.Logs.Text += $"Started scanning {textBox1.Text}....\n";

            /* 
            * Old slow method ( DO NOT REMOVE YET )
            * 
            */ 

            //for (int i = 0; i < 65535; i++)
            //{
            //    label8.Text = $"{i}";
            //    object[] args = new object[] { textBox1.Text, i };
            //    new Thread(port_scan).Start(args);
            //}
            int maxConcurrency = 500; // adjustable throttle
            int timeout = 200; // connection timeout (ms)

            using (SemaphoreSlim throttler = new SemaphoreSlim(maxConcurrency))
            {
                List<Task> tasks = new List<Task>();

                for (int port = 1; port <= 65535; port++)
                {
                    await throttler.WaitAsync();
                    int currentPort = port;
                    

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            label8.Invoke(new Action(() =>
                            {
                                label8.Text = $"{currentPort}";
                            }));
                            using (var client = new TcpClient())
                            {
                                var connectTask = client.ConnectAsync(textBox1.Text, currentPort);
                                if (await Task.WhenAny(connectTask, Task.Delay(timeout)) == connectTask && client.Connected)
                                {
                                    
                                    richTextBox1.Invoke(new Action(() =>
                                    {
                                        richTextBox1.AppendText($"[{textBox1.Text}]: Port {currentPort} is open!\n");
                                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                        richTextBox1.ScrollToCaret();
                                    }));
                                }
                            }
                        }
                        catch { }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }
            this.Logs.Text += $"Scan for {textBox1.Text} has finished....!\n";
        }

        private void ping_tcp(object n)
        {
            string ip = (string)((object[])n)[0];
            int port = (int)((object[])n)[1];
            this.pinging = true;
            while (this.pinging != false)
            {
                try
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    sock.ReceiveTimeout = 0;
                    sock.SendTimeout = 0;
                    Stopwatch sw = Stopwatch.StartNew();
                    sock.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                    sw.Stop();
                    sock.Close();
                    richTextBox2.Invoke(new Action(() =>
                    {
                        richTextBox2.AppendText($"[{ip}]: Port {port} is open -> {sw.ElapsedMilliseconds}ms!\n");
                        richTextBox2.SelectionStart = richTextBox2.Text.Length;
                        richTextBox2.ScrollToCaret();
                    }));
                }
                catch
                {
                    richTextBox2.Invoke(new Action(() =>
                    {
                        richTextBox2.AppendText($"Port: {port} has timed out!\n");
                        richTextBox2.SelectionStart = this.Logs.Text.Length;
                        richTextBox2.ScrollToCaret();
                    }));
                }
                Thread.Sleep(1000);
            }
            this.pinging = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty)
            {
                MessageBox.Show("Must provide a valid IP to continue....!\n", "Scan Error");
                return;
            }

            string resp = new WebClient().DownloadString($"https://free.freeipapi.com/api/json/{textBox1.Text}");
            object[,] keys = new object[,]
            {
                {"Status", "Status: ", label5},
                {"ipVersion", "IP Version: ", label6},
                {"ipAddress", "IP Address: ", label7},
                {"continent", "Continent: ", label9},
                {"continentCode", "Continent Code: ", label10},
                {"countryName", "Country Name: ", label11},
                {"countryCode", "Country Code: ", label12},
                {"capital", "Capital: ", label13},
                {"zipCode", "Zip Code: ", label14},
                {"cityName", "City Name: ", label15},
                {"regionName", "Region Name: ", label16},
                {"phoneCodes", "Phone Code: ", label17},
                {"latitude", "Latitude: ", label18},
                {"longitude", "Longitude: ", label20 },
                {"asn", "ASN: ", label21},
                {"asnOrganization", "ASN Org: ", label22},
                {"isProxy", "Is Proxy: ", label23}
            };
            foreach (string arg in resp.Replace("{", "").Replace("}", "").Replace("'", "").Replace("\"", "").Split(','))
            {
                string[] keyv = arg.Split(':');
                if (keyv.Length != 2)
                    continue;

                for (int i = 0; i < keys.GetLength(0); i++)
                    if (arg.StartsWith((string)keys[i, 0]))
                        ((System.Windows.Forms.Label)keys[i, 2]).Text = $"{keys[i, 1]}: {keyv[1]}";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(this.pinging == false)
            {
                // start pinging
                object[] gg = new object[] { textBox1.Text, Convert.ToInt32(numericUpDown1.Value) };
                new Thread(ping_tcp).Start((object[])gg);
                button3.Text = "Stop Ping";
                
            } else
            {
                this.pinging = false;
                button3.Text = "TCP Ping";
            }
        }
    }
}

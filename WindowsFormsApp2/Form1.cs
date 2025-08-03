using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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
            new Thread(port_scan).Start(textBox1.Text);
        }

        private void port_scan(object n)
        {
            string ip = (string)n;
            richTextBox1.Invoke(new Action(() =>
            {
                richTextBox1.Text = "";
            }));
            int[] ports = { 22, 80, 1194, 1337 };
            //for (int i = 1; i < 65535; i++) // slow (needs adjustments)
            for(int i = 0; i < 4; i++)
            {
                try
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //sock.Blocking = false;

                    sock.ReceiveTimeout = 0;
                    sock.SendTimeout = 0;
                    sock.Connect(new IPEndPoint(IPAddress.Parse(textBox1.Text), ports[i]));
                    sock.Close();
                    richTextBox1.Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText($"[{ip}]: Port {ports[i]} is open!\n");
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                    }));
                }
                catch
                {
                    richTextBox1.Invoke(new Action(() =>
                    {
                        richTextBox1.AppendText($"[{ip}]: Port {ports[i]} is closed!\n");
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                        richTextBox1.ScrollToCaret();
                    }));
                    if (i == 65535 || i == 65534)
                        break;

                    continue;
                }
            }
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
    }
}

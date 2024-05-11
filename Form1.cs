using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

namespace TurboBoost
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        public Form1()
        {
            InitializeComponent();
            serialPort = new SerialPort();
            serialPort.BaudRate = 38400;
            serialPort.DataReceived += SerialPort_DataReceived;
            comboBox1.Items.AddRange(new string[] { "9600", "10400", "38400", "250000", "500000" });
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine();

            // Tarkista, että vastaanotettu data ei ole tyhjä
            if (!string.IsNullOrEmpty(data))
            {
                // Erotetaan merkkijono pilkkujen kohdalta ja valitaan ensimmäinen kenttä
                string turboBoostValue = data.Split(',')[0];

                // Päivitetään labelin arvo uudella Turbo Boost -arvolla
                UpdateTurboBoostValue(turboBoostValue);
            }
        }
        private void UpdateTurboBoostValue(string value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateTurboBoostValue), value);
                return;
            }

            double turboBoostVolts = double.Parse(value);
            double minVolts = 0.5;
            double maxVolts = 4.5;
            double minPidValue = 0;
            double maxPidValue = 100;

            // Muunna volttiarvo pid-arvoksi
            double pidValue = ((turboBoostVolts - minVolts) / (maxVolts - minVolts)) * (maxPidValue - minPidValue) + minPidValue;

            // Aseta ProgressBarin arvo
            progressBar1.Value = (int)pidValue;

            boostLB.Text = "Turbo Boost: " + value + " V"; // Päivitä labelin teksti
        }


        private void connectBT_Click(object sender, EventArgs e)
        {
            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.PortName = "COM3"; // Määritä oikea COM-portti 
                    serialPort.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening serial port: " + ex.Message);
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        private void comPortBTN_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                //  ComboBox, joka sisältää COM-portit
                System.Windows.Forms.ComboBox comboBox = new System.Windows.Forms.ComboBox();
                comboBox.Items.AddRange(ports);
                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

              
                DialogResult result = MessageBox.Show(comboBox, "Valitse COM-portti:", "COM-portin valinta", MessageBoxButtons.OKCancel);

                if (result == DialogResult.OK)
                {
                    // Tarkistaa että käyttäjä on valinnut COM-portin
                    if (comboBox.SelectedItem != null)
                    {
                        // Aseta valittu COM-portti sarjaportin nimeksi
                        serialPort.PortName = comboBox.SelectedItem.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Et valinnut COM-porttia.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Ei löydetty käytettävissä olevia COM-portteja.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort.BaudRate = int.Parse(comboBox1.SelectedItem.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SelectBluetoothDeviceDialog dialog = new SelectBluetoothDeviceDialog();
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    BluetoothDeviceInfo device = dialog.SelectedDevice;
                    MessageBox.Show("Valittu laite: " + device.DeviceName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Virhe yhdistettäessä Bluetoothiin: " + ex.Message);
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using Automatak.DNP3.Interface;
using System.IO;

namespace Automatak.Simulator.DNP3.Components
{
    public partial class ChannelDialog : Form
    {
        public ChannelDialog()
        {
            InitializeComponent();            
            this.comboBoxSerialDeviceName.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            this.comboBoxParity.DataSource = Enum.GetValues(typeof(Parity));
            this.comboBoxStopBits.DataSource = Enum.GetValues(typeof(StopBits));
            this.comboBoxFlowControl.DataSource = Enum.GetValues(typeof(FlowControl));

            this.comboBoxParity.SelectedItem = Parity.NONE;
            this.comboBoxStopBits.SelectedItem = StopBits.ONE;
            this.comboBoxFlowControl.SelectedItem = FlowControl.NONE;           
        }

        private void buttonADD_Click(object sender, EventArgs e)
        {
            try
            {
                create = GetCreateFunctorMaybeNull();
                if (create == null)
                {
                    toolStripStatusLabel1.Text = "Unable to create channel";
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating channel");
            }
        }

        private Func<IDNP3Manager, IChannel> GetCreateFunctorMaybeNull()
        {
            var min = TimeSpan.FromMilliseconds(Decimal.ToDouble(numericUpDownMinRetryMS.Value));
            var max = TimeSpan.FromMilliseconds(Decimal.ToDouble(numericUpDownMaxRetryMS.Value));

            switch (tabControlChannelType.SelectedIndex)
            { 
                case(0):                                        
                    return GetTCPClientFunctor(min, max, clientEnableTls.Checked);
                case(1):                    
                    return GetTCPServerFunctor(min, max, serverEnableTls.Checked);
                case(2):
                    return GetSerialFunctor(min, max);
                default:
                    return null;
            }
        }        

        private Func<IDNP3Manager, IChannel> GetSerialFunctor(TimeSpan min, TimeSpan max)
        {
            var name = this.comboBoxSerialDeviceName.Text;
            var baud = Decimal.ToInt32(this.numericUpDownBaud.Value);
            
            var dataBits = Decimal.ToInt32(this.numericUpDownDataBits.Value);
            var parity = (Parity)comboBoxParity.SelectedValue;
            var flow = (FlowControl) comboBoxFlowControl.SelectedValue;
            var stopBits = (StopBits) comboBoxStopBits.SelectedValue;

            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max);
            var ss = new SerialSettings(name, baud, dataBits, stopBits, parity, flow);
            return (IDNP3Manager manager) => manager.AddSerial(this.textBoxID.Text, flags, retry, ss);
        }

        private Func<IDNP3Manager, IChannel> GetTCPClientFunctor(TimeSpan min, TimeSpan max, bool useTLS)
        {
            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max);
            if (useTLS)
            {
                var config = this.clientTLSOptionsControl.Configuration;
                return (IDNP3Manager manager) =>
                    manager.AddTLSClient(this.textBoxID.Text, flags, retry, textBoxHost.Text, Decimal.ToUInt16(numericUpDownPort.Value), config);
            }
            else
            {
                return (IDNP3Manager manager) => manager.AddTCPClient(this.textBoxID.Text, flags, retry, textBoxHost.Text, Decimal.ToUInt16(numericUpDownPort.Value));
            }
        }

        private Func<IDNP3Manager, IChannel> GetTCPServerFunctor(TimeSpan min, TimeSpan max, bool useTLS)
        {
            var flags = logLevelControl1.Filters.Flags;
            var retry = new ChannelRetry(min, max);
            if (useTLS)
            {
                var config = this.serverTLSOptionsControl.Configuration;
                return (IDNP3Manager manager) =>
                    manager.AddTLSServer(this.textBoxID.Text, flags, retry, textBoxServerHost.Text, Decimal.ToUInt16(numericUpDownServerPort.Value), config);
            }
            else
            {
                return (IDNP3Manager manager) =>
                    manager.AddTCPServer(this.textBoxID.Text, flags, retry, textBoxServerHost.Text, Decimal.ToUInt16(numericUpDownServerPort.Value));
            }
        } 

        public Func<IDNP3Manager, IChannel> ChannelAction
        {
            get
            {
                return create;
            }
        }

        public String SelectedAlias
        {
            get
            {
                return textBoxID.Text;
            }
        }

        private Func<IDNP3Manager, IChannel> create = null;

        /*
        private void clientTlsBrowseCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                clientTlsCert.Text = openFileDialog.FileName;
            }
        }

        private void clientTlsBrowsePrivateKey_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                clientTlsPrivateKey.Text = openFileDialog.FileName;
            }
        }

        private void clientTlsBrowseTrustedCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                clientTlsTrustedCert.Text = openFileDialog.FileName;
            }
        }

        private void serverTlsBrowseCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                serverTlsCert.Text = openFileDialog.FileName;
            }
        }

        private void serverTlsBrowsePrivateKey_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                serverTlsPrivateKey.Text = openFileDialog.FileName;
            }
        }

        private void serverTlsBrowseTrustedCert_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                serverTlsTrustedCert.Text = openFileDialog.FileName;
            }
        }
        */
    }
}

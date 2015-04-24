using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Server
{
    public partial class Server : Form
    {
        private delegate void UpdateStatusCallback(string strMessage);
        
        public Server()
        {
            InitializeComponent();
        }
                
        private void btnListen_Click(object sender, EventArgs e)
        {
            int portNum = Convert.ToInt32(txtPort.Text);   
            // Parse the server's IP address out of the TextBox
            IPAddress ipAddr = IPAddress.Parse(txtIp.Text);
            // Create a new instance of the ChatServer object
            ChatServer mainServer = new ChatServer(ipAddr);
            // Hook the StatusChanged event handler to mainServer_StatusChanged
            ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
            // Start listening for connections
            mainServer.StartListening(portNum);
            // Show that we started to listen for connections
            txtLog.AppendText("Monitoring for connections...\r\n");
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Call the method that updates the form
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
            
        }
        
        private void UpdateStatus(string strMessage)
        {            
            // Updates the log with the message
            txtLog.AppendText(strMessage + "\r\n");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (txtIp.Text.Length == 0 || txtPort.Text.Length == 0)
            {
                Application.Exit();
            }
            else
            {
                
                TcpListener closeConn;
                int portNum = Convert.ToInt32(txtPort.Text);
                IPAddress ipAddr = IPAddress.Parse(txtIp.Text);
                closeConn = new TcpListener(ipAddr, portNum);
                closeConn.Stop(); 
                //Close();
                Application.Exit();                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AboutBox AB = new AboutBox();
            AB.Show();
        }
    }
}

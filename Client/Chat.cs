using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Client
{
    public partial class Chat : Form
    {
        // Will hold the user name
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        // Needed to update the form with messages from another thread
        private delegate void UpdateLogCallback(string strMessage);
        // Needed to set the form to a "disconnected" state from another thread
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;
        public int portNum;

        public Chat()
        {
            // On application exit, don't forget to disconnect first
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        // The event handler for application exit
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // Closes the connections, streams, etc.
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        private void bConnect_Click(object sender, EventArgs e)
        {
            if (txtIp.Text.Length == 0 && txtPort.Text.Length == 0 && txtUser.Text.Length == 0)
            {
                MessageBox.Show("Одно из полей не заполнено", "Заполни поля",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                // If we are not currently connected but awaiting to connect
                if (Connected == false)
                {
                    // Initialize the connection
                    InitializeConnection();
                }
                else // We are connected, thus disconnect
                {
                    CloseConnection("Disconnected at user's request.");
                }
            }
        }

        private void InitializeConnection()
        {
            // Parse the IP address from the TextBox into an IPAddress object
            ipAddr = IPAddress.Parse(txtIp.Text);
            portNum = System.Convert.ToInt32(txtPort.Text);
            // Start a new TCP connections to the chat server
            tcpServer = new TcpClient();
            tcpServer.Connect(ipAddr, portNum);

            // Helps us track whether we're connected or not
            Connected = true;
            // Prepare the form
            UserName = txtUser.Text;

            // Disable and enable the appropriate fields
            txtIp.Enabled = false;
            txtPort.Enabled = false;
            txtUser.Enabled = false;
            txtMessage.Enabled = true;
            bSend.Enabled = true;
            bConnect.Text = "Disconnect";

            // Send the desired username to the server
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(txtUser.Text);
            swSender.Flush();

            // Start the thread for receiving messages and further communication
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            // Receive the response from the server
            srReceiver = new StreamReader(tcpServer.GetStream());
            // If the first character of the response is 1, connection was successful
            string ConResponse = srReceiver.ReadLine();
            // If the first character is a 1, connection was successful
            if (ConResponse[0] == '1')
            {
                // Update the form to tell it we are now connected
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
            }
            else // If the first character is not a 1 (probably a 0), the connection was unsuccessful
            {
                string Reason = "Not Connected: ";
                // Extract the reason out of the response message. The reason starts at the 3rd character
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                // Update the form with the reason why we couldn't connect
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                // Exit the method
                return;
            }
            // While we are successfully connected, read incoming lines from the server
            while (Connected)
            {
                // Show the messages in the log TextBox
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
            }
        }
        // This method is called from a different thread in order to update the log TextBox
        private void UpdateLog(string strMessage)
        {
            // Append text also scrolls the TextBox to the bottom each time
            txtLog.AppendText(strMessage + "\r\n");
        }

        // Closes a current connection
        private void CloseConnection(string Reason)
        {
            // Show the reason why the connection is ending
            txtLog.AppendText(Reason + "\r\n");
            // Enable and disable the appropriate controls on the form
            txtIp.Enabled = true;
            txtPort.Enabled = true;
            txtUser.Enabled = true;
            txtMessage.Enabled = false;
            bSend.Enabled = false;
            bConnect.Text = "Connect";

            // Close the objects
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpServer.Close();
        }

        // Sends the message typed in to the server
        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                swSender.WriteLine(txtMessage.Text);
                swSender.Flush();
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If the key is Enter
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }
    }
}

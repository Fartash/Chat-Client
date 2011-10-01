using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ChatClient
{
    public delegate void receptionHandler(object sender,
    receptionEventArgs e);
    public delegate void connectionAnswer(object sender,
    connectionEventArgs e);
    public delegate void catchusepassHandler(object sender,
    catchusepassEventArgs e);
    public delegate void SetTextCallback(object sender, idUpdateEventArgs e);
    public delegate void SetTextCallback1(object sender, receptionEventArgs e);

    public partial class Form1 : Form
    {
        private string ip;
        private Thread connThread1;
        private string username;
        private Connection conn;
        private string password;
        private IPEndPoint ipep;
        private IPEndPoint clientEP;
        private Socket sock;
        public static Socket clientSock;
        

        public Form1()
        {
            InitializeComponent();

            //Variables initializations

            /*ip = "127.0.0.1";
            ipep1 = new IPEndPoint(IPAddress.Parse(ip), 9050);
            Form1.clientSock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Form1.clientSock.Connect(ipep1);
            }
            catch (SocketException d)
            {
                MessageBox.Show(d.Message);
            }*/

            this.username = "";
            this.password = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool isInTheList = false;
            if (clientSock != null)
            {
                int a = 0, index = 0, ind = 0, b = 0;
                byte[] idMessage = new byte[1024];
                string selectedItem = (string)checkedListBox1.SelectedItem;
                /*if (selectedItem == null)
                {
                    MessageBox.Show("Select a User to send the message to");
                    return;
                }*/
                selectedItem = selectedItem.TrimEnd(null);

                for (int x = 0; x < checkedListBox1.CheckedItems.Count; x++)
                {
                    if (selectedItem == checkedListBox1.CheckedItems[x].ToString())
                        isInTheList = true;
                }

                if (selectedItem == null)
                {
                    MessageBox.Show("Select a User to send the message to");
                    return;
                }
                else if (isInTheList == false)
                {
                    MessageBox.Show("Selected user is NOT online!");
                    return;
                }
                else
                {
                    Encoding.ASCII.GetBytes(selectedItem, 0,
                        selectedItem.Length, idMessage, 0);
                    index = selectedItem.Length;
                    a = 10 - selectedItem.Length;
                    for (int i = 0; i < a; ++i)
                        Encoding.ASCII.GetBytes("\0", 0, 1, idMessage, index);

                    Encoding.ASCII.GetBytes(this.username, 0, this.username.Length,
                        idMessage, 10);
                    ind = 10 + this.username.Length;
                    b = 10 - this.username.Length;
                    for (int i = 0; i < b; ++i)
                        Encoding.ASCII.GetBytes("\0", 0, 1, idMessage, ind);

                    Encoding.ASCII.GetBytes(richTextBox2.Text, 0,
                        richTextBox2.Text.Length, idMessage, 20);

                    clientSock.Send(idMessage, SocketFlags.None);   //Sending the Message to Server

                    richTextBox1.AppendText(this.username);
                    richTextBox1.AppendText(": ");
                    richTextBox1.AppendText(richTextBox2.Text);
                    richTextBox1.AppendText("\n");
                    richTextBox2.Text = "";
                
                }
            }
            else if (clientSock == null)
            {
                MessageBox.Show("The Socket is null");
            }
        }
        private void Form1_Load(object sender, System.EventArgs e)
        {
            richTextBox2.Focus();
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (connThread1 != null)
                connThread1.Abort();
            if (clientSock != null)
            {
                try
                {
                    clientSock.Shutdown(SocketShutdown.Both);
                    clientSock.Close();
                }
                catch (ObjectDisposedException k)
                {
                    //this Exception was Handled in order that
                    //it might raise exception if the Server
                    //was Shut Down during the closing event
                    //of the Client
                }
                catch (SocketException g)
                {
                }
                finally
                {
                    this.Close();
                    Environment.Exit(0);
                }
            }
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loginForm u = new loginForm();
            u.catchusepass += new catchusepassHandler(Catchusepass);
            u.ShowDialog();
            //
            if (this.username != "" && this.password != "")
            {
                conn = new Connection();
                //ip = ConfigurationSettings.AppSettings["serverIP"];
                ip = "127.0.0.1";
                label3.Text = ip;
                conn.ip = ip;
                conn.username = this.username;
                conn.password = this.password;
                conn.result += new connectionAnswer(conn_result);
                conn.received += new receptionHandler(receptionEventHandler);
                conn.idUpdate += new idUpdateHandler(conn_idUpdate);
                conn.Login();
                
            }
        }
        private void conn_idUpdate(object sender, idUpdateEventArgs e)
        {
            int position = 0;
            string temp;
            if (this.checkedListBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(conn_idUpdate);
                this.Invoke(d, this, e);
            }
            else
            {
                if (e.theIDs != null)
                {
                    checkedListBox1.Items.Clear();
                    foreach (users item in e.theIDs)
                    {
                        if (item.id != "\0\0\0\0\0\0\0\0\0\0")
                        {
                            position = item.id.IndexOf('\0');
                            if (position != -1)
                                temp = item.id.Substring(0, position);
                            else
                                temp = item.id;
                            if (temp != this.username)
                                if (item.online)
                                    checkedListBox1.Items.Add(temp, true);
                                else
                                    checkedListBox1.Items.Add(temp, false);
                        }
                        else
                            break;
                    }
                }
            }
        }
        private void conn_result(object sender, connectionEventArgs e)
        {
            int position = 0;
            if (e.Agree == false)
                MessageBox.Show("Invalid Username/Password");
            else if (e.Agree == true)
            {
                this.label4.Text = this.username;
                //filling the list box
                if (e.IDs != null)
                {
                    string temp;
                    checkedListBox1.Items.Clear();
                    foreach (users item in e.IDs)
                    {
                        if (item.id != "\0\0\0\0\0\0\0\0\0\0")
                        {
                            position = item.id.IndexOf('\0');
                            if (position != -1)
                                temp = item.id.Substring(0, position);
                            else
                                temp = item.id;
                            if (temp != this.username)
                                if (item.online)
                                    checkedListBox1.Items.Add(temp, true);
                                else
                                    checkedListBox1.Items.Add(temp, false);
                        }
                        else
                            break;
                    }
                }
                //starting the thread
                connThread1 = new Thread(new ThreadStart(conn.Reception));
                connThread1.Start();
                label1.Visible = true;
            }
        }
        private void receptionEventHandler(object sender, receptionEventArgs e)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                SetTextCallback1 b = new SetTextCallback1(receptionEventHandler);
                this.Invoke(b, this, e);
            }
            else
            {
                richTextBox1.AppendText(e.IDsender);
                richTextBox1.AppendText(" : ");
                richTextBox1.AppendText(e.Message);
                richTextBox1.AppendText("\n");
            }
        }
        private void Catchusepass(object sender, catchusepassEventArgs e)
        {
            this.password = e.Password;
            this.username = e.Username;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connThread1 != null)
                connThread1.Abort();
            if (clientSock != null)
            {
                try
                {
                    clientSock.Shutdown(SocketShutdown.Both);
                    clientSock.Close();
                }
                catch (ObjectDisposedException k)
                {
                    //this Exception was Handled in order that
                    //it might raise exception if the Server
                    //was Shut Down during the closing event
                    //of the Client
                }
                catch (SocketException g)
                {
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
        }
    }
}

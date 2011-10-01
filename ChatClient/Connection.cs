using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
    public delegate void idUpdateHandler(object sender, idUpdateEventArgs e);
    public class Connection
    {
        private byte[] data;
        public string ip;
        private IPEndPoint ipep1;
        public string username;
        public string password;
        public void Reception()
        {
            int a = 1;
            try
            {
                while (true)
                {
                    data = new byte[1024];
                    a = Form1.clientSock.Receive(data);

                    if (a != 0)
                    {
                        if (Encoding.ASCII.GetString(data, 0, 10)
                            == "!!!!!!!!!!")
                        {
                            idUpdateEventArgs h = new idUpdateEventArgs(data);
                            idUpdate(this, h);
                        }
                        else
                        {
                            receptionEventArgs k = new receptionEventArgs(data);
                            received(this, k);
                        }
                    }
                    else
                    {
                        Form1.clientSock.Shutdown(SocketShutdown.Both);
                        Form1.clientSock.Close();
                        //yek event neveshte shavad ke elam konad
                        //connection close shode
                    }
                }
            }
            catch (ArgumentNullException q)
            {
                MessageBox.Show(q.Message);
            }
            catch (SocketException w)
            {
                MessageBox.Show(w.Message);
            }
            catch (ObjectDisposedException k)
            {
                MessageBox.Show("Disconnected!!");
            }
        }
        public void Login()
        {
            int a = 0;
            byte[] answer = new byte[1100];
            byte[] arr = new byte[20];

            
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
            }
            
    
            //change to byte array
            try
            {
                Encoding.ASCII.GetBytes(username, 0,
                    username.Length, arr, 0);
                int s = 10 - username.Length;
                int b = username.Length;
                for (int i = 0; i < s; ++i, ++b)
                    Encoding.ASCII.GetBytes("\0", 0, 1, arr, b);
                //
                Encoding.ASCII.GetBytes(password, 0,
                    password.Length, arr, 10);
                int e = 10 - password.Length;
                int t = password.Length + 10;
                for (int j = 0; j < e; ++j, ++t)
                    Encoding.ASCII.GetBytes("\0", 0, 1, arr, t);
            }
            catch (Exception h)
            {
                MessageBox.Show(h.Message);
            }
            try
            {
                a = Form1.clientSock.Send(arr); //sending username & password
                if (a == 0)
                {
                    MessageBox.Show("Could not send username and password");
                }
                Form1.clientSock.Receive(answer, 0, 1100, SocketFlags.None);
                if (Encoding.ASCII.GetString(answer, 0, 14) == "wrong password")
                {
                    connectionEventArgs j = new connectionEventArgs(false, answer);
                    result(this, j);
                }
                else //Connected , the array "answer" contains all the on line
                {	 //users
                    connectionEventArgs g = new connectionEventArgs(true, answer);
                    result(this, g);
                }
            }
            catch (SocketException o)
            {
                MessageBox.Show("Could not send the Username and Password");
            }
        }
        public event receptionHandler received;
        public event connectionAnswer result;
        public event idUpdateHandler idUpdate;
    }

    public class connectionEventArgs : EventArgs
    {
        private bool m_agreed;
        private users[] m_IDs;
        public connectionEventArgs(bool k, byte[] answer)
        {
            m_agreed = k;
            //
            if (Encoding.ASCII.GetString(answer, 0, 14) != "wrong password"
                && Encoding.ASCII.GetString(answer, 0, 7) != "welcome")
            {
                m_IDs = new users[100];
                int i, n, z;
                for (i = 0, n = 0, z = 10; i < 1100; i += 11, ++n, z += 11)
                {
                    m_IDs[n].id = Encoding.ASCII.GetString(answer, i, 10);
                    if (Encoding.ASCII.GetString(answer, z, 1) == "$")
                        m_IDs[n].online = true;
                    else if (Encoding.ASCII.GetString(answer, z, 1) == "%")
                        m_IDs[n].online = false;
                }
            }
            else
                m_IDs = null;
        }
        public users[] IDs
        {
            get
            {
                return m_IDs;
            }
        }
        public bool Agree
        {
            get
            {
                return m_agreed;
            }
        }
    }
    public struct users
    {
        public string id;
        public bool online;
    }
    public class receptionEventArgs : EventArgs
    {
        private byte[] messageB;
        private string m_idSender;
        private string m_id;
        private string m_message;
        public receptionEventArgs(byte[] h)
        {
            messageB = new byte[1024];
            messageB = h;
            changeToStr();
        }
        private void changeToStr()
        {
            byte[] idB = new byte[10];
            byte[] idBsender = new byte[10];
            byte[] msgB = new byte[1004];
            for (int i = 0; i < 10; ++i)
                idB[i] = messageB[i];

            for (int q = 10, v = 0; q < 20; ++q, ++v)
                idBsender[v] = messageB[q];

            for (int j = 20, k = 0; j < 1004; ++j, ++k)
                msgB[k] = messageB[j];
            //
            m_idSender = Encoding.ASCII.GetString(idBsender, 0, idBsender.Length);
            m_id = Encoding.ASCII.GetString(idB, 0, idB.Length);
            m_message = Encoding.ASCII.GetString(msgB, 0, msgB.Length);
        }
        public string Message
        {
            get
            {
                return m_message;
            }
        }
        public string ID
        {
            get
            {
                return m_id;
            }
        }
        public string IDsender
        {
            get
            {
                return m_idSender;
            }
        }
    }
    public class idUpdateEventArgs : EventArgs
    {
        private byte[] message;
        private users[] m_IDs;
        public idUpdateEventArgs(byte[] IDs)
        {
            message = IDs;
            m_IDs = new users[100];
            int i, n, z;
            for (i = 10, n = 0, z = 20; i < 1014; i += 11, ++n, z += 11)
            {
                m_IDs[n].id = Encoding.ASCII.GetString(message, i, 10);
                if (Encoding.ASCII.GetString(message, z, 1) == "$")
                    m_IDs[n].online = true;
                else if (Encoding.ASCII.GetString(message, z, 1) == "%")
                    m_IDs[n].online = false;
            }
        }
        public users[] theIDs
        {
            get
            {
                return m_IDs;
            }
        }
    }
}

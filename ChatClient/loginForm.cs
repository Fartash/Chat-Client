using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class loginForm : Form
    {
        public event catchusepassHandler catchusepass;
        public loginForm()
        {
            InitializeComponent();
            
        }
        private void linkLabel1_Click(object sender, EventArgs e)
        {
            catchusepassEventArgs r = new catchusepassEventArgs(textBox1.Text,
                textBox2.Text);
            catchusepass(this, r);
            this.Close();
        }
    }
    public class catchusepassEventArgs : EventArgs
    {
        private string m_username;
        private string m_password;
        public catchusepassEventArgs(string use, string pass)
        {
            m_username = use;
            m_password = pass;
        }
        public string Username
        {
            get
            {
                return m_username;
            }
        }
        public string Password
        {
            get
            {
                return m_password;
            }
        }
    }
}

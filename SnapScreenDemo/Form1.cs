using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Simple.Video;

namespace SnapScreenDemo
{
    public partial class Form1 : Form
    {
        VideoUtil videoUtil;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartRecord_Click(object sender, EventArgs e)
        {
             videoUtil= new VideoUtil(@"d:\aaasss.avi");
             videoUtil.MessageEvent += videoUtil_MessageEvent;
             videoUtil.StartRecord();
        }

        void videoUtil_MessageEvent(object sender, MessageEventArgs e)
        {
            textBox1.Text = e.Message;
        }

        private void btnStopRecord_Click(object sender, EventArgs e)
        {
            videoUtil.StopRecord();
        }
    }
}

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

namespace SnapImageDemo
{
    public partial class Form1 : Form
    {
        bool isClosingForm = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if(ofd.FileName.Length >0)
            {
                string path = ofd.FileName ;

                VideoUtil myMpeg = new VideoUtil(path);
                TimeSpan currTime = new TimeSpan(10000000);
                TimeSpan step = new TimeSpan(10000000);
                Image img = null;
                for (int i = 0; i < 1000; i++)
                {
                    if (isClosingForm) break;
                    Application.DoEvents();

                    img = myMpeg.CatchImage(currTime);
                    if (null != img)
                    {
                        label1.Text = currTime.ToString("t");
                        pictureBox1.Image = img;
                        pictureBox1.Refresh();
                    }
                    currTime += step;
                }
                
            }
 
        }
    }
}

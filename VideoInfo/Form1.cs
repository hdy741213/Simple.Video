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
using System.IO;


namespace VideoInfoTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.ShowDialog();
            //string path = fbd.SelectedPath;
            string path = @"D:\娱乐\movie";
            if (path.Length > 0)
            {
                VideoUtil  myMpeg;
                VideoInfo vi;

                string[] files = Directory.GetFiles(path);
                listBox1.Items.Clear();
                foreach (string file in files)
                {
                    myMpeg = new VideoUtil(file);
                    vi = myMpeg.GetVideoInfo();

                    listBox1.Items.Add(null == vi ? file+" 无法识别！": vi.ToString());
                }

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

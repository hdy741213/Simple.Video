using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Drawing ;
using System.Threading; // 测试用

namespace Simple.Video
{
    // 报告状态的类，参数传递用
    public class MessageEventArgs
    {
        public MessageEventArgs(string msg, double progress) 
        { 
            Message = msg;
            Progress = progress;
        }
        public String Message {get; private set;} // readonly
        public double Progress { get; private set; } // readonly
    }

    // 视频类，可获取视频信息、截屏、视频转换、录屏等
    public class VideoUtil
    {
        private Process pFFmpeg;    // 进程
        private string avFile;      // 视频文件

        public delegate void MessageEventHandler(object sender, MessageEventArgs e);
        public event MessageEventHandler MessageEvent;
                
        private SynchronizationContext Context=SynchronizationContext.Current;

        // 构造函数
        public VideoUtil(string AVFile)
        {
            avFile = AVFile;
        }

        // 获取视频的信息
        public VideoInfo   GetVideoInfo()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");
            startInfo.CreateNoWindow = true;//不显示dos命令行窗口
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;//是否指定操作系统外壳进程启动程序

            pFFmpeg = Process.Start(startInfo);

            pFFmpeg.StandardInput.WriteLine("ffmpeg -i " + avFile);
            pFFmpeg.StandardInput.WriteLine("exit");

            string errStr = pFFmpeg.StandardError.ReadToEnd();    // 包含需要的信息
            pFFmpeg.Close();//关闭进程

            if (errStr.Contains("不是内部或外部命令"))
            {
                throw new Exception("没找到ffmpeg，请将 ffmpeg.exe 文件拷贝到当前执行目录。");
            }
            

            VideoInfo info = VideoInfo.FromFFmpeg(errStr);
            
            if(null == info)
            {
                return null;
            }
            info.FileName = avFile;
            return info;
        }

        // 获取图像，图像文件名由imgFile指定
        public int CatchImage(string imgFile,TimeSpan time)
        {
            if (!File.Exists("ffmpeg.exe"))
                throw new Exception("没有找到 ffmpeg.exe");
            if (!imgFile.ToLower().EndsWith(".jpg"))
                imgFile += ".jpg";
            if (File.Exists(imgFile))
                File.Delete(imgFile);
            string timeStr = time.ToString("t");

            pFFmpeg = new Process();
            pFFmpeg.StartInfo.FileName = "cmd.exe";             //要执行的程序名称
            pFFmpeg.StartInfo.UseShellExecute = false;
            pFFmpeg.StartInfo.RedirectStandardInput = true;     //可能接受来自调用程序的输入信息
            //pFFmpeg.StartInfo.RedirectStandardOutput = true;    //由调用程序获取输出信息 
            pFFmpeg.StartInfo.RedirectStandardError = true;
            pFFmpeg.StartInfo.CreateNoWindow = true;            //不显示程序窗口 
            pFFmpeg.Start();    //启动程序 

            // 向进程发送输入信息
            pFFmpeg.StandardInput.WriteLine("ffmpeg -ss "+timeStr +" -i " + avFile + " -f image2 -y " + imgFile);
            // -ss表示搜索到指定的时间 -i表示输入的文件 -y表示覆盖输出 -f表示强制使用的格式
            pFFmpeg.StandardInput.WriteLine("exit");

            int iTime = Utils.WaitUtil(File.Exists, imgFile, 5000);

            //pFFmpeg.WaitForExit();
            //string errStr = pFFmpeg.StandardError.ReadToEnd();    // 包含需要的信息

            
            //
            pFFmpeg.Close();//关闭进程
            pFFmpeg.Dispose();

            //if (errStr.Contains("不是内部或外部命令"))
            //{
            //    throw new Exception("没找到ffmpeg，请将 ffmpeg.exe 文件拷贝到当前执行目录。");
            //}
            

            return iTime;
        }

        // 截取图像，并形成 Image 对象
        public Image CatchImage(TimeSpan time)
        {
            string tmpFile=Path.GetTempFileName() +".jpg";
            int iTime = CatchImage(tmpFile, time);
            
            if (iTime < 0)    return null ;   // 没读出来

            Image img = null;

            try
            {
                using (FileStream fs = new FileStream(tmpFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    img = Image.FromStream(fs);
                }
                File.Delete(tmpFile);
            }
            catch (Exception e1)    
            {  }
            return img;
        }

        // 录像,屏幕录像
        public string StartRecord()
        {
            // ffmpeg -f gdigrab -framerate 5 -i title="Video_ffmpeg - Microsoft Visual Studio" out.avi
            // ffmpeg -f gdigrab -framerate 5 -i desktop out.avi
            // ffmpeg -f gdigrab -framerate 5 -offset_x 10 -offset_y 20 -video_size 640x480 -i title="窗口名称" out.mpg 

            if (File.Exists(avFile)) File.Delete(avFile);

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe");//设置运行的命令行文件问ping.exe文件，这个文件系统会自己找到
            startInfo.CreateNoWindow = true;//不显示dos命令行窗口
            startInfo.RedirectStandardOutput = true;//
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;//
            startInfo.UseShellExecute = false;//是否指定操作系统外壳进程启动程序

            pFFmpeg = new Process();
            pFFmpeg.StartInfo = startInfo;

            pFFmpeg.OutputDataReceived += pFFmpeg_OutputDataReceived;
            pFFmpeg.ErrorDataReceived += pFFmpeg_ErrorDataReceived;

            pFFmpeg.Start();
            

            pFFmpeg.StandardInput.WriteLine("ffmpeg -f gdigrab -framerate 5 -i desktop " + avFile );

            // 开始异步传送消息
            pFFmpeg.BeginOutputReadLine();
            pFFmpeg.BeginErrorReadLine();

            return null;
        }

        // 结束录像
        public void StopRecord()
        {
            pFFmpeg.StandardInput.Write('q');
            pFFmpeg.Close();//关闭进程
        }

        // 引发事件，给线程调用，在调用线程运行
        void MyEventSender(object  e)
        {
            if (MessageEvent != null)
                MessageEvent(this, new MessageEventArgs(((DataReceivedEventArgs)e).Data, 0));
        }

        // 错误信息转发
        void pFFmpeg_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Context.Post(MyEventSender, e);
        }

        // 输出信息转发
        void pFFmpeg_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Context.Post(MyEventSender, e);
        }
    }
}

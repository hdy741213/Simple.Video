using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Simple.Video
{
    // 音频信息，记录各个音轨的信息
    public class SoundInfo
    {
        public string AudioFormat { get;  set; } // 音频格式
        public string AudioRate { get; set; }   // 速率
        public string AudioChannel { get; set; }   // 声道,如5.1
    }

    // 视频信息
    public class VideoInfo
    {
        public string FileName { get; set; }    // 文件名
        public DateTime CreteTime { get;private set; } // 生成时间
        public long Duration { get;private set; }      // 时长,毫秒
        public int BitRate { get; private set; }        // 码率？kb/s

        public string VideoFormat { get;private set; } // 视频编码
        public string VideoColorFormat { get; private set; }    // 色彩编码方式，如YUV
        public string VideoRate { get; private set; }   // 视频的码率？有的有，有的没有
        public int VideoWidth { get;private set; }
        public int VideoHeight { get;private set; }
        public string VideoDAR { get; private set; }
        public string VideoSAR { get; private set; }
        public double VideoFrameRate  { get; private set; } // 帧率
        public string VideoTBR { get; private set; }
        public string VideoTBN { get; private set; }
        public string VideoTBC { get; private set; }
        public SoundInfo[] SoundChannel { get; private set; }   // 音轨

        private static char[] sepChars=new char[] {','};   // 分割字符串

        // 从ffmpeg -i 生成的字符串，解析视频信息
        public static VideoInfo FromFFmpeg(string ffmpedStr)
        {
            VideoInfo currInfo = new VideoInfo();

            // 生成时间
            string createTimeStr = ffmpedStr.GetBetweenStr(0, "creation_time   :", "Z");
            currInfo.CreteTime = (null ==createTimeStr) ? default(DateTime): DateTime.Parse(createTimeStr );


            
            

            // 码率？kb/s
            string bitRateStr = ffmpedStr.GetBetweenStr(0, "bitrate: ", "kb/s");
            if (null == bitRateStr)  return null;
            currInfo.BitRate = int.Parse(bitRateStr);

            // 时长,毫秒
            string durationStr = ffmpedStr.GetBetweenStr(0, "Duration:", ",");
            currInfo.Duration = (long)TimeSpan.Parse(durationStr).TotalMilliseconds;
            

            // 视频信息
            string videoStr = ffmpedStr.GetBetweenStr(0, "Video: ", "\r");
            string[] videoArray = videoStr.SpliteButParentheses(',');
            
            currInfo.VideoFormat = videoArray[0];
            currInfo.VideoColorFormat  = videoArray[1];

            // 长宽
            string sSize = Regex.Matches(videoStr, "[1-9][0-9]+x[1-9][0-9]+")[0].Value ;
            currInfo.VideoWidth = int.Parse(sSize.GetBeforeStr('x'));
            currInfo.VideoHeight =int.Parse( sSize.GetBackStr('x'));
            
            var darRegex = Regex.Matches(videoStr, "DAR [0-9]+:[0-9]+");
            currInfo.VideoDAR=darRegex.Count > 0 ? darRegex[0].Value :"";
            

            var sarRegex = Regex.Matches(videoStr, "SAR [0-9]+:[0-9]+");
            currInfo.VideoSAR = sarRegex.Count > 0 ? sarRegex[0].Value : "";

            currInfo.VideoFrameRate = double.Parse(Regex.Matches(videoStr, "[0-9]+ fps")[0].Value.GetBeforeStr("fps"));
            currInfo.VideoTBR = Regex.Matches(videoStr, "[0-9]+ tbr")[0].Value;
            currInfo.VideoTBN = Regex.Matches(videoStr, "[0-9]+k* tbn")[0].Value;
            currInfo.VideoTBC = Regex.Matches(videoStr, "[0-9]+k* tbc")[0].Value;

            

            // 音频信息
            List<SoundInfo> listSound = new List<SoundInfo>();
            string audioStr;
            SoundInfo currSound;
            for(int i=1;i<100;i++)  //音轨数不会大于100吧？
            {
                audioStr = ffmpedStr.GetBetweenStr(0, "Stream #0:"+i.ToString(), "\r");
                if (null == audioStr)
                    break;  // 没有了，后面就不可能有

                audioStr = audioStr.GetBackStr("Audio:");
                if (null == audioStr)
                    break;  // 没有了，后面就不可能有

                string[] audioArray = audioStr.Split(sepChars);

                currSound = new SoundInfo();
                currSound.AudioFormat = audioArray[0];
                currSound.AudioRate = audioArray[1];
                currSound.AudioChannel = audioArray[2];

                listSound.Add(currSound);

            }
            currInfo.SoundChannel = listSound.ToArray();
            


            return currInfo;
        }

        // 把信息转化为格式字符串
        public override  string  ToString()
        {
            StringBuilder sb = new StringBuilder();
            int iLen = 60;
            if(FileName.Length >iLen )
            {
                sb.Append(FileName.Substring(FileName.Length -iLen ));
            }
            else
            {
                sb.Append(string.Format("{0,-"+iLen.ToString() +"}", FileName));
            }
            
            sb.Append("\t时长：");
            TimeSpan tspan = new TimeSpan(Duration*10000);
            sb.Append(tspan.ToString("hh\\:mm\\:ss"));

            sb.Append("\t尺寸：");
            sb.Append(VideoWidth);
            sb.Append('x');
            sb.Append(VideoHeight);

            sb.Append("\t编码：");
            sb.Append(VideoFormat);
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Simple.Video
{
    static class Utils
    {
        // 循环等待条件满足或超时
        public static int WaitUtil(Func<string, bool> condition,string args, int timeOut)
        {
            Stopwatch tWatch = new Stopwatch();
            tWatch.Start();
            while(!condition.Invoke(args))
            {
                if (tWatch.Elapsed.TotalMilliseconds > timeOut)
                    return -1;  // -1表示超时
            }
            return (int)tWatch.Elapsed.TotalMilliseconds;    // 返回执行时间
        }

        // 获取两个字符串中间的字符串
        public static string GetBetweenStr(this string srcStr, int startIndex, string StartStr, string EndStr)
        {
            int iStart = srcStr.IndexOf(StartStr,startIndex );
            if (-1 == iStart) return null;
            iStart +=StartStr.Length ;
            int iEnd = srcStr.IndexOf(EndStr, iStart);
            if (-1 == iEnd) return null;
            string sRet = srcStr.Substring(iStart, iEnd - iStart);
            return sRet ;
        }

        // 获取一个特定字符前的所有字符串
        public static string GetBeforeStr(this string srcStr,char sepChar)
        {
            int iPos = srcStr.IndexOf(sepChar);
            if (-1 == iPos) return null ;   // 没找到
            return srcStr.Substring(0, iPos);
        }

        // 获取一个特定字符前的所有字符串
        public static string GetBeforeStr(this string srcStr, string  sepStr)
        {
            int iPos = srcStr.IndexOf(sepStr);
            if (-1 == iPos) return null;   // 没找到
            return srcStr.Substring(0, iPos);
        }

        // 获取一个特定字符后的所有字符串
        public static string GetBackStr(this string srcStr, string  sepChar)
        {
            int iPos = srcStr.IndexOf(sepChar);
            if (-1 == iPos) return null;   // 没找到
            iPos += sepChar.Length;
            return srcStr.Substring(iPos , srcStr.Length -iPos );
        }

        // 获取一个特定字符后的所有字符串
        public static string GetBackStr(this string srcStr, char  sepChar)
        {
            int iPos = srcStr.IndexOf(sepChar);
            if (-1 == iPos) return null;   // 没找到
            iPos++;
            return srcStr.Substring(iPos, srcStr.Length - iPos);
        }

        // 特殊的splite，括弧中的字符串不splite
        public static string[] SpliteButParentheses(this string srcStr,char sepChar)
        {
            List<string> allStrings = new List<string>();
            StringBuilder sb=new StringBuilder();
            int iParenthNum = 0;   // 表示括号的层级，经过一个（加1，经过一个）减1
            foreach(char c in srcStr )
            {
                if(c==sepChar && iParenthNum==0)
                {
                    // 表示是一个分割开的字符串
                    allStrings.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                else if('('==c)
                {
                    iParenthNum++;
                }
                else if (')' == c)
                {
                    iParenthNum--;
                }
                sb.Append(c);
            }
            allStrings.Add(sb.ToString());

            return allStrings.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboReplicate
{
    class ErrorLogger
    {
        System.IO.StreamWriter file;
        string f;
        public ErrorLogger(string logpath, string filename)
        {
            f = logpath + filename;
            System.IO.Directory.CreateDirectory(logpath);
        }

        public void WriteLog(string text)
        {
            
            file = new System.IO.StreamWriter(f, true);
            file.WriteLine(text);
            file.Close();
        }
        
        public void WriteLogTime(string text)
        {
            file = new System.IO.StreamWriter(f, true);
            file.WriteLine(DateTime.Now.ToString());
            file.WriteLine(text);
            file.Close();
        }

    }
}

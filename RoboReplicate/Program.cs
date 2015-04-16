using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RoboReplicate
{
    class Program
    {
        struct Config
        {
            public string destinationUNC;
            public string sourceDir;
            public string destinationDir;
            public string destinationDriveLetter;
            public string username;
            public string password;
            public string robocopySwitches;
            public string smtpHost;
            public string smtpPort;
            public string smtpSSL;
            public string smtpUsername;
            public string smtpPassword;
            public string smtpDomain;
            public string smtpFrom;
            public string smtpTo;
            public string smtpSubjectSrvName;
            public string maxRuntime;
        }

        static string startTime;
        static string logpath = AppDomain.CurrentDomain.BaseDirectory + "log\\";
        static string robocopyLogPath = logpath + "RoboCopylog.txt";
        static Config cfg = new Config();
        static Process p = new Process();


        static void prog()
        {
            ErrorLogger logger = new ErrorLogger(logpath, "ReplicationLog.txt");
            startTime = DateTime.Now.ToString();
            logger.WriteLogTime("Replication Initialize");
            
            try
            {
                p.StartInfo.Arguments = "/c net use " + cfg.destinationDriveLetter + " /delete";
                p.StartInfo.FileName = "CMD.EXE";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                while (!p.HasExited) { }
                logger.WriteLogTime("Unmapped " + cfg.destinationDriveLetter + " drive successfully");
            }
            catch(Exception ex)
            {
                logger.WriteLogTime("Failed to delete map drive.");
                logger.WriteLog(ex.Message);
                logger.WriteLog(ex.StackTrace);
            }

            try
            {
                p.StartInfo.Arguments = "/c net use " + cfg.destinationDriveLetter + " " + cfg.destinationUNC + " /user:" + cfg.username + " " + cfg.password;
                p.StartInfo.FileName = "CMD.EXE";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                while (!p.HasExited) { }
                logger.WriteLogTime("Mapped " + cfg.destinationDriveLetter + " to " + cfg.destinationUNC + " successfully");
            }
            catch (Exception ex)
            {
                logger.WriteLogTime("Failed to map drive.");
                logger.WriteLog(ex.Message);
                logger.WriteLog(ex.StackTrace);
            }

            int exitCode;
            try
            {
                p.StartInfo.Arguments = string.Format("/c ROBOCOPY {0} {1} {2} /LOG:{3}", cfg.sourceDir, cfg.destinationDir, cfg.robocopySwitches, robocopyLogPath);
                p.StartInfo.FileName = "CMD.EXE";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                logger.WriteLogTime("Start Robocopy");
                int timer = 0;
                int max = int.Parse(cfg.maxRuntime);
                while (!p.HasExited) {
                    if(timer == max)
                    {
                        exitCode = 18;
                        goto failed;
                    }
                    ++timer;
                    System.Threading.Thread.Sleep(3600000);
                    
                }
                logger.WriteLogTime("Robocopy Complete");
                exitCode = p.ExitCode;
            }
            catch (Exception ex)
            {
                logger.WriteLogTime("Failed to run robocopy.");
                logger.WriteLog(ex.Message);
                logger.WriteLog(ex.StackTrace);
                exitCode = 17;
            }
            failed:
            
            string msg;
            string subject = "[" + cfg.smtpSubjectSrvName + "]";

            if (exitCode <= 1)
            {
                subject += " Replication Successful";
                msg = string.Format("Replication Successful \n\n Start Time: {0} \n End Time: {1} \n\nDescription:\n", startTime, DateTime.Now.ToString());
            }
            else if (exitCode >= 7)
            {
                subject += " Replication Failed";
                msg = string.Format("Replication Failed \n\n Start Time: {0} \n End Time: {1} \n\nDescription:\n", startTime, DateTime.Now.ToString());
            }
            else
            {
                subject += " Replication Warning";
                msg = string.Format("Replication Warning \n\n Start Time: {0} \n End Time: {1} \n\nDescription:\n", startTime, DateTime.Now.ToString());
            }

            switch (exitCode)
            {
                case 0:
                    msg += "No errors occurred, and no copying was done. The source and destination directory trees are completely synchronized.";
                    break;
                case 1:
                    msg += "One or more files were copied successfully (that is, new files have arrived).";
                    break;
                case 2:
                    msg += "Some Extra files or directories were detected. No files were copied Examine the output log for details.";
                    break;
                case 3:
                    msg += "Some files were copied. Additional files were present. No failure was encountered.";
                    break;
                case 4:
                    msg += "Some Mismatched files or directories were detected. Examine the output log. Some housekeeping may be needed.";
                    break;
                case 5:
                    msg += "Some files were copied. Some files were mismatched. No failure was encountered.";
                    break;
                case 6:
                    msg += "Additional files and mismatched files exist. No files were copied and no failures were encountered. This means that the files already exist in the destination directory";
                    break;
                case 7:
                    msg += "Files were copied, a file mismatch was present, and additional files were present.";
                    break;
                case 8:
                    msg += "Some files or directories could not be copied(copy errors occurred and the retry limit was exceeded). Check these errors further.";
                    break;
                case 16:
                    msg += "Serious error. Robocopy did not copy any files. Either a usage error or an error due to insufficient access privileges on the source or destination directories.";
                    break;
                case 17:
                    msg += "Robocopy failed to run.";
                    break;
                case 18:
                    msg += "Robocopy took too long to complete.";
                    break;
                default:
                    msg = "No exit code found. Robo Copy failed to run.";
                    break;
            }
            logger.WriteLogTime(msg);
            try
            {
                SMTP.SendWithAttachment(cfg.smtpHost, int.Parse(cfg.smtpPort), bool.Parse(cfg.smtpSSL), cfg.smtpUsername, cfg.smtpPassword, cfg.smtpDomain, cfg.smtpFrom, cfg.smtpTo, subject, msg, robocopyLogPath);
            }
            catch(Exception ex)
            {
                logger.WriteLogTime("Failed to send email notification!");
                logger.WriteLog(ex.Message);
                logger.WriteLog(ex.StackTrace);
            }
            Environment.Exit(0);
        }

        static void OnExit(object sender, EventArgs e)
        {
            p.Kill();
            p.Close();
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);
            cfg = ConfigManager.LoadConfig(cfg);
            prog();
        }
    }
}

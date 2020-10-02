using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flower_Space
{
    public class WriteLog
    {
        private string m_exePath = string.Empty;

        public void OpenLog()
        {
            m_exePath = Helper.ApplicationDataPath();
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    LogHeader(w);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public void LogWriter(string logMessage)
        {
            LogWrite(logMessage);
        }

        public void LogGroupStart(string logMessage)
        {
            m_exePath = Helper.ApplicationDataPath();

            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    logGroupStart(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }


        public void LogGroupLine(string logMessage)
        {
            m_exePath = Helper.ApplicationDataPath();

            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    logGroupLine(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void LogWriter(string[] logMessage)
        {
            LogWrite(logMessage);
        }

        public void CloseLog()
        {
            m_exePath = Helper.ApplicationDataPath();
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    LogFooter(w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LogWrite(string[] logMessage)
        {
//            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            m_exePath = Helper.ApplicationDataPath();
            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                   // LogHeader(w);
                    foreach ( string msg in logMessage)
                    {
                        LogLine(msg, w);
                    }

                    //LogFooter(w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LogWrite(string logMessage)
        {
            m_exePath = Helper.ApplicationDataPath();

            try
            {
                using (StreamWriter w = File.AppendText(m_exePath + "\\" + "log.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LogHeader(TextWriter txtWriter)
        {
            try
            {
                txtWriter.WriteLine("\r\n-------------------------------");
                txtWriter.WriteLine("Starting at {0} {1} ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
            }
            catch (Exception ex)
            {
            }
        }

        private void LogLine(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.WriteLine("  :{0}", logMessage);
            }
            catch (Exception ex)
            {
            }
        }

        private void LogFooter(TextWriter txtWriter)
        {
            try
            {
                txtWriter.WriteLine("Closing at {0} {1} ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("-------------------------------\r\n");
            }
            catch (Exception ex)
            {
            }
        }

        private void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1} ", DateTime.Now.ToLongTimeString(),   DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :{0}", logMessage);
            }
            catch (Exception ex)
            {
            }
        }

        private void logGroupStart(string logMessage, TextWriter txtWriter)
        {
            try
            {
                try
                {
                    txtWriter.Write("\r\nLog Entry : ");
                    txtWriter.WriteLine("{0} {1} ", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    txtWriter.WriteLine("  :{0}", logMessage);
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void logGroupLine(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.WriteLine("    :{0}", logMessage);
            }
            catch (Exception ex)
            {
            }
        }
    }
}

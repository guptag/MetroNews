using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace NewsExtractor
{
    public class Logger
    {
        static Logger _logger = new Logger();
        StringBuilder logBuffer = new StringBuilder();

        string evSource = "NewsExtractorService";
        string evLog = "Application";

        static Logger()
        {
            
        }

        private Logger() 
        {
            try
            {
                if (!EventLog.SourceExists(evSource))
                    EventLog.CreateEventSource(evSource, evLog);
            }
            catch { }
        }

        public static Logger GetInstance()
        {
            return _logger;
        }

        public void AddLogEntry(string message)
        {
            logBuffer.Append(DateTime.Now.ToString());
            logBuffer.Append(":");
            logBuffer.Append(message);
            logBuffer.Append(Environment.NewLine);
        }

        public void AddLogEntry(Exception ex)
        {
            logBuffer.Append(DateTime.Now.ToString());
            logBuffer.Append(":");
            
            logBuffer.Append(ex.Message);
            logBuffer.Append(ex.InnerException);
            logBuffer.Append(ex.StackTrace);
            
            if (ex.InnerException != null)
            {
                logBuffer.Append(ex.InnerException.Message);
                logBuffer.Append(ex.InnerException.InnerException);
                logBuffer.Append(ex.InnerException.StackTrace);
            }

            logBuffer.Append(Environment.NewLine);           
        }

        public void WriteEventlogEntry(string message, EventLogEntryType eventLogEntryType)
        {
            try
            {
                EventLog.WriteEntry(evSource, message, eventLogEntryType);
            }catch{}
        }

        public void WriteEventlogEntry(Exception ex, EventLogEntryType eventLogEntryType)
        {
            try
            {
                StringBuilder message = new StringBuilder();

                message.Append(DateTime.Now.ToString());
                message.Append(":");
                message.Append(ex.Message);
                message.Append(ex.InnerException);
                message.Append(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    message.Append(ex.InnerException.Message);
                    message.Append(ex.InnerException.InnerException);
                    message.Append(ex.InnerException.StackTrace);
                }
                message.Append(Environment.NewLine);
                EventLog.WriteEntry(evSource, message.ToString(), eventLogEntryType);
            }catch{}
        }

        public void Flush()
        {
            try
            {
                if (logBuffer.Length > 0)
                {
                    logBuffer.Append("-----------------------------------------------------------------");
                    logBuffer.Append(Environment.NewLine);
                    string filePath = ConfigurationManager.AppSettings["LogFilePath"].Replace("~", Utility.GetCurrentDirectory());
                    string fileName = "log" + DateTime.Now.ToShortDateString().Replace("/", "_") + ".txt";
                    File.AppendAllText(filePath + fileName, logBuffer.ToString());
                    logBuffer.Clear();
                }
            }
            catch{}
        }
    }
}

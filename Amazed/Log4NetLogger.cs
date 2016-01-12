using System;
using System.IO;
using DreamAmazon.Interfaces;
using log4net;

namespace DreamAmazon
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Log4NetLogger));

        public Log4NetLogger()
        {
            if (!Directory.Exists(@".\Logs"))
                Directory.CreateDirectory(@".\Logs");
        }

        public void Error(Exception exception)
        {
            _logger.Error(exception.Message, exception);
        }

        public void Debug(string text)
        {
            _logger.Debug(text);
        }

        public void Info(string text)
        {
            _logger.Info(text);
        }
    }
}
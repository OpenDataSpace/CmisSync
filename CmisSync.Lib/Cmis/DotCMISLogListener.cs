using System;
using log4net.Core;

namespace CmisSync.Lib.Cmis
{
    public class DotCMISLogListener : System.Diagnostics.TraceListener
    {
        private readonly log4net.ILog _log;

        public DotCMISLogListener() : this(log4net.LogManager.GetLogger(typeof(DotCMISLogListener)))
        { }

        public DotCMISLogListener(log4net.ILog log)
        {
            if(log == null) {
                throw new ArgumentNullException("Given logger is null");
            }
            _log = log;
            SetLog4NetLevelToTraceLevel();
            _log.Logger.Repository.ConfigurationChanged += delegate(object sender, EventArgs e) {
                SetLog4NetLevelToTraceLevel();
            };
        }

        private void SetLog4NetLevelToTraceLevel()
        {
            if(_log.IsDebugEnabled) 
            {
                DotCMIS.Util.DotCMISDebug.DotCMISTraceLevel = System.Diagnostics.TraceLevel.Info;
            }
            /*else if(_log.IsErrorEnabled)
            {
                DotCMIS.Util.DotCMISDebug.DotCMISSwitch.Level = System.Diagnostics.TraceLevel.Error;
            }else if(_log.IsFatalEnabled)
            {
                DotCMIS.Util.DotCMISDebug.DotCMISSwitch.Level = System.Diagnostics.TraceLevel.Error;
            }else if(_log.IsInfoEnabled)
            {
                DotCMIS.Util.DotCMISDebug.DotCMISSwitch.Level = System.Diagnostics.TraceLevel.Info;
            }else if(_log.IsWarnEnabled)
            {
                DotCMIS.Util.DotCMISDebug.DotCMISSwitch.Level = System.Diagnostics.TraceLevel.Warning;
            }*/
            else{
                DotCMIS.Util.DotCMISDebug.DotCMISTraceLevel = System.Diagnostics.TraceLevel.Off;
            }
        }

        public override void Write(string message)
        {
            _log.Debug(message);
        }

        public override void WriteLine(string message)
        {
            _log.Debug(message);
        }
    }
}

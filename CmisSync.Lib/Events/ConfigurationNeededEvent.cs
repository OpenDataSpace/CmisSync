
namespace CmisSync.Lib.Events
{
    using System;

    using CmisSync.Lib.Config;

    public class ConfigurationNeededEvent : ExceptionEvent
    {
        public ConfigurationNeededEvent(Exception e) : base(e)
        {
        }
    }
}
using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Diagnostics.EventHubListener
{
    public class EventHubListener : TraceListener
    {
        private EventHubClient _client;

        public EventHubListener()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EventHub.ConnectionString"])) { ConnectionString = ConfigurationManager.AppSettings["EventHub.ConnectionString"]; }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EventHub.UseJson"])) { UseJson = bool.Parse(ConfigurationManager.AppSettings["EventHub.UseJson"]); }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EventHub.LogPattern"])) { LogPattern = ConfigurationManager.AppSettings["EventHub.LogPattern"]; }

            _client = EventHubClient.CreateFromConnectionString(ConnectionString);

        }

        private string _logPattern = "%a% %e% %u% %t% %m%";
        public string LogPattern { get { return _logPattern; } set { _logPattern = value; } }

        public string ConnectionString { get; set; }


        public override void Write(string message)
        {
            WriteLine(message);
        }

        public bool UseJson { get; set; }

        private string FormatMessage(string message)
        {
            string formattedMessage = LogPattern.Replace("%e%", Environment.MachineName);
            formattedMessage = formattedMessage.Replace("%a%", Assembly.GetEntryAssembly().FullName);
            formattedMessage = formattedMessage.Replace("%u%", Environment.UserName);
            formattedMessage = formattedMessage.Replace("%t%", string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            formattedMessage = formattedMessage.Replace("%m%", message);
            return formattedMessage;
        }

        private string JsonMessage(string message)
        {
            string json = string.Format("{{ \"dateTime\": \"{0} {1}\"", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            if (LogPattern.Contains("%a%")) { json = string.Format("{0},\"assemblyName\": \"{1}\"", json, Assembly.GetEntryAssembly().FullName); }
            if (LogPattern.Contains("%e%")) { json = string.Format("{0},\"machineName\": \"{1}\"", json, Environment.MachineName); }
            if (LogPattern.Contains("%u%")) { json = string.Format("{0},\"userName\": \"{1}\"", json, Environment.UserName); }
            json = string.Format("{0},\"message\": \"{1}\"", json, message);
            json = string.Format("{0}}}", json);
            return json;
        }

        public override void WriteLine(string message)
        {
            string logMessage = string.Empty;

            if (UseJson) { logMessage = JsonMessage(message); }
            else { logMessage = FormatMessage(message); }

            _client.Send(new EventData(Encoding.UTF8.GetBytes(logMessage)));
        }

        public override void Write(object o)
        {
            base.Write(o);
        }

        public override void Write(object o, string category)
        {
            base.Write(o, category);
        }

        public override void Write(string message, string category)
        {
            base.Write(message, category);
        }

        public override void WriteLine(object o)
        {
            base.WriteLine(o);
        }

        public override void WriteLine(object o, string category)
        {
            base.WriteLine(o, category);
        }

        public override void WriteLine(string message, string category)
        {
            base.WriteLine(message, category);
        }

        public static void ShutdownLogging()
        {
        }
    }
}

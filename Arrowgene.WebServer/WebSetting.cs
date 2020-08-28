using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Arrowgene.WebServer
{
    [DataContract]
    public class WebSetting
    {
        public WebSetting()
        {
            ServerHeader = null;
            WebFolder = "";
            HttpPorts = new List<ushort>();
            HttpPorts.Add(80);
            HttpsEnabled = false;
            HttpsPort = 443;
            HttpsCertPath = "";
            HttpsCertPw = "";
        }

        public WebSetting(WebSetting webSetting)
        {
            ServerHeader = webSetting.ServerHeader;
            WebFolder = webSetting.WebFolder;
            HttpPorts = new List<ushort>(webSetting.HttpPorts);
            HttpsEnabled = webSetting.HttpsEnabled;
            HttpsPort = webSetting.HttpsPort;
            HttpsCertPath = webSetting.HttpsCertPath;
            HttpsCertPw = webSetting.HttpsCertPw;
        }

        [DataMember(Order = 1)] public string ServerHeader { get; set; }
        [DataMember(Order = 2)] public List<ushort> HttpPorts { get; set; }
        [DataMember(Order = 3)] public bool HttpsEnabled { get; set; }
        [DataMember(Order = 4)] public ushort HttpsPort { get; set; }
        [DataMember(Order = 5)] public string HttpsCertPath { get; set; }
        [DataMember(Order = 6)] public string HttpsCertPw { get; set; }
        [DataMember(Order = 7)] public string WebFolder { get; set; }
    }
}
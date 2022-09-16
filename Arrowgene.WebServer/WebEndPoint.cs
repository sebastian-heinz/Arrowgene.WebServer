using System.Net;
using System.Runtime.Serialization;
using System.Security.Authentication;

namespace Arrowgene.WebServer
{
    [DataContract]
    public class WebEndPoint
    {
        public WebEndPoint()
        {
            IpAddress = IPAddress.Any;
            Port = 80;
            IsHttps = false;
            HttpsCertPath = "";
            HttpsCertPw = "";
            SslProtocols = SslProtocols.None;
        }

        public WebEndPoint(WebEndPoint webEndPoint)
        {
            IpAddress = webEndPoint.IpAddress;
            Port = webEndPoint.Port;
            IsHttps = webEndPoint.IsHttps;
            HttpsCertPath = webEndPoint.HttpsCertPath;
            HttpsCertPw = webEndPoint.HttpsCertPw;
            SslProtocols = webEndPoint.SslProtocols;
        }

        [IgnoreDataMember] public IPAddress IpAddress { get; set; }

        [DataMember(Name = "IpAddress", Order = 0)]
        public string DataListenIpAddress
        {
            get => IpAddress.ToString();
            set => IpAddress = string.IsNullOrEmpty(value) ? null : IPAddress.Parse(value);
        }

        [DataMember(Order = 1)] public ushort Port { get; set; }

        [DataMember(Order = 2)] public bool IsHttps { get; set; }
        [DataMember(Order = 3)] public string HttpsCertPath { get; set; }
        [DataMember(Order = 4)] public string HttpsCertPw { get; set; }
        [DataMember(Order = 5)] public SslProtocols SslProtocols { get; set; }
    }
}
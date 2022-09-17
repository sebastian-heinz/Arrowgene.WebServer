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
            IpAddress = IPAddress.Loopback;
            Port = 80;
            IsHttps = false;
            HttpsCertPath = "";
            HttpsCertPw = "";
            SslProtocols = SslProtocols.None;
            DomainName = "";
        }

        public WebEndPoint(WebEndPoint webEndPoint)
        {
            IpAddress = webEndPoint.IpAddress;
            Port = webEndPoint.Port;
            IsHttps = webEndPoint.IsHttps;
            HttpsCertPath = webEndPoint.HttpsCertPath;
            HttpsCertPw = webEndPoint.HttpsCertPw;
            SslProtocols = webEndPoint.SslProtocols;
            DomainName = webEndPoint.DomainName;
        }

        public string GetUrl()
        {
            return
                $"{(IsHttps ? "https" : "http")}://{(string.IsNullOrWhiteSpace(DomainName) ? IpAddress : DomainName)}:{Port}";
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
        [DataMember(Order = 6)] public string DomainName { get; set; }
    }
}
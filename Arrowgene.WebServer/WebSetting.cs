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
            WebEndpoints = new List<WebEndPoint>();
            WebEndpoints.Add(new WebEndPoint());
        }

        public WebSetting(WebSetting webSetting)
        {
            ServerHeader = webSetting.ServerHeader;
            WebFolder = webSetting.WebFolder;
            WebEndpoints = new List<WebEndPoint>(webSetting.WebEndpoints);
        }

        [DataMember(Order = 0)] public string ServerHeader { get; set; }
        [DataMember(Order = 1)] public List<WebEndPoint> WebEndpoints { get; set; }
        [DataMember(Order = 2)] public string WebFolder { get; set; }
    }
}
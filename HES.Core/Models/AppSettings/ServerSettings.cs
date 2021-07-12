namespace HES.Core.Models.AppSettings
{
    public class ServerSettings
    {
        public string ServerUrl { get; set; }
        public string ServerFullName { get; set; }
        public string ServerShortName { get; set; }
        public string CompanyName { get; set; }
        public bool ReverseProxyHandleSSL { get; set; }
    }
}
using System.Net;

namespace Net.Ntp
{
    public class ReferenceIdentifier
    {
        public PrimaryReferenceIdentifier PrimaryServerType { get; set; }
        public IPAddress SecondaryServerSourceIpAddress { get; set; }
        public string SecondaryServerMD5HashFirst32BitsOfIPv6 { get; set; }
    }
}

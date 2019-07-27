
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace Net.Ntp
{
    public class NtpResponse : NtpRequest
    {
        /// <summary>
        /// The total round-trip delay from the server to the primary reference sourced.
        /// The value is a 32-bit signed fixed-point number in units of seconds, with the fraction point between bits 15 and 16.
        /// This field is significant only in server messages
        /// </summary>
        public int RootDelay { get; set; }
        /// <summary>
        /// The maximum error due to clock frequency tolerance.
        /// The value is a 32-bit signed fixed-point number in units of seconds, with the fraction point between bits 15 and 16.
        /// This field is significant only in server messages
        /// </summary>
        public int RootDispersion { get; set; }
        /// <summary>
        /// For stratum 1 servers this value is a four-character ASCII code that describes the external reference source (refer to Figure 2).
        /// For secondary servers this value is the 32-bit IPv4 address of the synchronization source, or the first 32 bits of the Message Digest Algorithm 5 (MD5) hash of the IPv6 address of the synchronization source
        /// </summary>
        public ReferenceIdentifier ReferenceIdentifier { get; set; }
        /// <summary>
        /// The precision of the system clock, in log2 seconds
        /// </summary>
        [Range(-128, 127)]
        public int PrecisionLog2Seconds { get; set; }
        /// <summary>
        /// This value is an unsigned 32-bit seconds value, and a 32-bit fractional part.
        /// In this notation the value 2.5 would be represented by the 64-bit string: 0000|0000|0000|0000|0000|0000|0000|0010.|1000|0000|0000|0000|0000|0000|0000|0000
        /// The smallest time fraction that can be represented in this format is 232 picoseconds
        ///
        /// This field is the time the system clock was last set or corrected, in 64-bit time-stamp format
        /// </summary>
        public DateTime ReferenceTimestampUtc { get; set; }
        /// <summary>
        /// This value is an unsigned 32-bit seconds value, and a 32-bit fractional part.
        /// In this notation the value 2.5 would be represented by the 64-bit string: 0000|0000|0000|0000|0000|0000|0000|0010.|1000|0000|0000|0000|0000|0000|0000|0000
        /// The smallest time fraction that can be represented in this format is 232 picoseconds
        ///
        /// This value is the time at which the client request arrived at the server in 64-bit time-stamp format
        /// </summary>
        public DateTime ReceiveTimestampUtc { get; set; }
        /// <summary>
        /// This value is an unsigned 32-bit seconds value, and a 32-bit fractional part.
        /// In this notation the value 2.5 would be represented by the 64-bit string: 0000|0000|0000|0000|0000|0000|0000|0010.|1000|0000|0000|0000|0000|0000|0000|0000
        /// The smallest time fraction that can be represented in this format is 232 picoseconds
        ///
        /// This value is the time at which the server reply departed the server, in 64-bit time-stamp format
        /// </summary>
        public DateTime TransmitTimestampUtc { get; set; }

        private const int TransmitTimestampOffset = 40;
        private const int ReceiveTimestampOffset = 32;
        private const int OriginateTimestampOffset = 24;
        private const int ReferenceTimestampOffset = 16;

        private LeapIndicator GetLeapIndictator(byte input)
        {
            var flag = (byte)192;
            var filteredResult = (byte)(input & flag);
            if (filteredResult == 128) return LeapIndicator.LastMinuteHas59Seconds;
            if (filteredResult == 64) return LeapIndicator.LastMinuteHas61Seconds;
            if (filteredResult == 192) return LeapIndicator.ClockUnsynchronized;
            return LeapIndicator.NoLeapSecondAdjustment;
        }

        private int GetVersionNumber(byte input)
        {
            var flag = (byte)56;
            var filteredResult = (byte)(input & flag);
            if (filteredResult == 0) return 0;
            if (filteredResult == 8) return 1;
            if (filteredResult == 16) return 2;
            if (filteredResult == 24) return 3;
            if (filteredResult == 32) return 4;
            if (filteredResult == 40) return 5;
            if (filteredResult == 48) return 6;
            if (filteredResult == 56) return 7;
            return 0;
        }

        private Mode GetMode(byte input)
        {
            var flag = (byte)7;
            var filteredResult = (byte)(input & flag);
            if (filteredResult == 1) return Mode.SymmetricActive;
            if (filteredResult == 2) return Mode.SymmetricPassive;
            if (filteredResult == 3) return Mode.Client;
            if (filteredResult == 4) return Mode.Server;
            if (filteredResult == 5) return Mode.Broadcast;
            if (filteredResult == 6) return Mode.NtpControlMessage;
            if (filteredResult == 7) return Mode.ReservedPrivateUse;
            return Mode.Reserved;
        }

        private Stratum GetStratum(byte stratum)
        {
            var input = (UInt32)stratum;//BitConverter.ToUInt32(new byte[] { stratum });
            if (input == 0) return Stratum.UnspecifiedOrInvalid;
            if (input == 1) return Stratum.PrimaryServer;
            if (input >= 2 && input <= 15) return Stratum.SecondaryServer;
            if (input == 16) return Stratum.Unsynchronized;
            if (input >= 17 && input <= 255) return Stratum.Reserved;
            return Stratum.UnspecifiedOrInvalid;
        }

        private ReferenceIdentifier GetReferenceIdentifier(Stratum stratum, byte[] input)
        {
            var result = new ReferenceIdentifier();
            if (stratum == Stratum.PrimaryServer)
            {
                var parseSuccess = Enum.TryParse(Encoding.ASCII.GetString(input), true, out PrimaryReferenceIdentifier yolo);
                if (parseSuccess)
                {
                    result.PrimaryServerType = yolo;
                }
            }
            if (stratum == Stratum.SecondaryServer)
            {
                result.SecondaryServerSourceIpAddress = new IPAddress(input);
                result.SecondaryServerMD5HashFirst32BitsOfIPv6 = Convert.ToBase64String(input);
            }
            return result;
        }

        public DateTime GetDateTime(byte[] bytes, int index)
        {
            ulong seconds = BitConverter.ToUInt32(bytes, index);
            ulong subSeconds = BitConverter.ToUInt32(bytes, index + 4);

            //Convert From big-endian to little-endian
            seconds = SwapEndianness(seconds);
            subSeconds = SwapEndianness(subSeconds);
            var milliseconds = (seconds * 1000) + ((subSeconds * 1000) / 0x100000000L);

            //**UTC** time
            var result = Base.AddMilliseconds((long)milliseconds);
            return result;
        }

        public static NtpResponse ParseBytes(byte[] bytes)
        {
            if (bytes.Length != 48) return null;

            var response = new NtpResponse();
            response.TransmitTimestampUtc = response.GetDateTime(bytes, TransmitTimestampOffset);
            response.ReceiveTimestampUtc = response.GetDateTime(bytes, ReceiveTimestampOffset);
            response.OriginateTimestampUtc = response.GetDateTime(bytes, OriginateTimestampOffset);
            response.ReferenceTimestampUtc = response.GetDateTime(bytes, ReferenceTimestampOffset);
            response.RootDispersion = BitConverter.ToInt32(new byte[] { bytes[8], bytes[9], bytes[10], bytes[11] }, 0);
            response.RootDelay = BitConverter.ToInt32(new byte[] { bytes[4], bytes[5], bytes[6], bytes[7] }, 0);
            response.PrecisionLog2Seconds = Convert.ToInt32(bytes[3]);
            response.PrecisionLog2Seconds = Convert.ToInt32(bytes[2]);
            response.Stratum = response.GetStratum(bytes[1]);
            response.ReferenceIdentifier = response.GetReferenceIdentifier(response.Stratum, new byte[] { bytes[12], bytes[13], bytes[14], bytes[15] });
            response.LeapIndicator = response.GetLeapIndictator(bytes[0]);
            response.VersionNumber = response.GetVersionNumber(bytes[0]);
            response.Mode = response.GetMode(bytes[0]);

            return response;
        }

        public static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}

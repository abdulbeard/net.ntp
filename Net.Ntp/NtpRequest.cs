using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Net.Ntp
{
    public class NtpRequest
    {
        protected static readonly DateTime Base = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// This field indicates whether the last minute of the current day is to have a leap second applied
        /// </summary>
        public LeapIndicator LeapIndicator { get; set; }
        /// <summary>
        /// NTP Version Number (3 bits) (current version is 4)
        /// </summary>
        [Range(0, 7)]
        public int VersionNumber { get; set; }
        /// <summary>
        /// Packet mode
        /// </summary>
        public Mode Mode { get; set; }
        /// <summary>
        /// The stratum level
        /// </summary>
        public Stratum Stratum { get; set; }
        /// <summary>
        /// The log2 value of the maximum interval between successive NTP messages, in seconds
        /// </summary>
        [Range(-128, 127)]
        public int PollInterval { get; set; }
        /// <summary>
        /// This value is an unsigned 32-bit seconds value, and a 32-bit fractional part.
        /// In this notation the value 2.5 would be represented by the 64-bit string: 0000|0000|0000|0000|0000|0000|0000|0010.|1000|0000|0000|0000|0000|0000|0000|0000
        /// The smallest time fraction that can be represented in this format is 232 picoseconds
        ///
        /// This value is the time at which the request departed the client for the server, in 64-bit time-stamp format
        /// </summary>
        public DateTime OriginateTimestampUtc { get; set; }


        public byte[] GetBytes()
        {
            var result = new byte[48];

            result[0] = GetLiVnModeByte(LeapIndicator, VersionNumber, Mode);
            result[1] = GetStratum(Stratum);
            result[2] = GetPollInterval(PollInterval);

            var originateTimestamp = GetOriginateTimestamp();
            //var convertedBack = new NtpResponse().GetDateTime(originateTimestamp, 0);
            result[24] = originateTimestamp[0];
            result[25] = originateTimestamp[1];
            result[26] = originateTimestamp[2];
            result[27] = originateTimestamp[3];
            result[28] = originateTimestamp[4];
            result[29] = originateTimestamp[5];
            result[30] = originateTimestamp[6];
            result[31] = originateTimestamp[7];

            return result;
        }

        public byte[] GetOriginateTimestamp()
        {
            var time = OriginateTimestampUtc;
            var totalTicks = (time - Base).Ticks;
            var totalSeconds = (uint)(totalTicks / 10000000);
            var leftOverPicoseconds = (uint)(((totalTicks % 10000000) * 100000) / 232);
            totalSeconds = NtpResponse.SwapEndianness(totalSeconds);
            leftOverPicoseconds = NtpResponse.SwapEndianness(leftOverPicoseconds);
            var result = new byte[8];
            BitConverter.GetBytes(totalSeconds).CopyTo(result, 0);
            BitConverter.GetBytes(leftOverPicoseconds).CopyTo(result, 4);
            return result;
        }

        public byte GetPollInterval(int pollInterval)
        {
            return 16;
        }

        public byte GetStratum(Stratum stratum)
        {
            switch (stratum)
            {
                case Stratum.PrimaryServer: { return 1; }
                case Stratum.SecondaryServer: { return 15; }
                case Stratum.Unsynchronized: { return 16; }
                case Stratum.Reserved: { return 255; }
                case Stratum.UnspecifiedOrInvalid:
                default: { return 0; }
            }
        }

        public byte GetLiVnModeByte(LeapIndicator li, int vn, Mode mode)
        {
            var intermediateResult = 0;
            switch (li)
            {
                case LeapIndicator.LastMinuteHas61Seconds: { intermediateResult += 64; break; }
                case LeapIndicator.LastMinuteHas59Seconds: { intermediateResult += 128; break; }
                case LeapIndicator.ClockUnsynchronized: { intermediateResult += 192; break; }
                case LeapIndicator.NoLeapSecondAdjustment:
                default: { break; }
            }
            switch (vn)
            {
                case 1: { intermediateResult += 8; break; }
                case 2: { intermediateResult += 16; break; }
                case 3: { intermediateResult += 24; break; }
                case 4: { intermediateResult += 32; break; }
                case 5: { intermediateResult += 40; break; }
                case 6: { intermediateResult += 48; break; }
                case 7: { intermediateResult += 56; break; }
                default: { break; }
            }
            switch (mode)
            {
                case Mode.SymmetricActive: { intermediateResult += 1; break; }
                case Mode.SymmetricPassive: { intermediateResult += 2; break; }
                case Mode.Client: { intermediateResult += 3; break; }
                case Mode.Server: { intermediateResult += 4; break; }
                case Mode.Broadcast: { intermediateResult += 5; break; }
                case Mode.NtpControlMessage: { intermediateResult += 6; break; }
                case Mode.ReservedPrivateUse: { intermediateResult += 7; break; }
                case Mode.Reserved:
                default: { break; }
            }
            byte result = (byte)intermediateResult;
            return result;
        }
    }
}

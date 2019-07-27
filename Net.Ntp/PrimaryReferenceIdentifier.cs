namespace Net.Ntp
{
    public enum PrimaryReferenceIdentifier
    {
        LOCL, //uncalibrated local clock
        CESM, //calibrated Cesium clock
        RBDM, //calibrated Rubidium clock
        PPS, //calibrated quartz clock or other pulse-per-second source
        IRIG, //Inter-Range instrumentation group
        ACTS, //NIST telephone modem service
        USNO, //USNO telephone modem service
        PTB, //PTB (Germany) telephone modem service
        TDF, //Allouis (France) telephone modem service
        DCF, //Mainflingen (Germany) Radio 77.5 kHz
        MSF, //Rugby (UK) Radio 60 kHz
        WWV, //Ft. Collins (US) Radio 2.5, 5, 10, 15, 20 MHz
        WWVB, //Boulder (US) Radio 60 kHz
        WWVH, //Kauai Hawaii (US) Radio 2.5, 5, 10, 15 MHz
        CHU, //Ottawa (Canada) Radio 3330, 7335, 14670 kHz
        LORC, //LORAN-C radionavigation system
        OMEG, //OMEGA radionavigation system
        GPS, //Global Positioning Service
    }
}

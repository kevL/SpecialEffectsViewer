using System;


namespace SpecialEffectsViewer
{
	static class BwResourceTypes
	{
		internal static string GetResourceTypeString(ushort rt)
		{
			switch (rt)
			{
				case    0: return "RES";
				case    1: return "BMP";
				case    2: return "MVE";
				case    3: return "TGA";
				case    4: return "WAV";
				case    5: return "WFX";
				case    6: return "PLT";
				case    7: return "INI";
				case    8: return "MP3";
				case    9: return "MPG";
				case   10: return "TXT";
				case 2000: return "PLH";
				case 2001: return "TEX";
				case 2002: return "MDL";
				case 2003: return "THG";
				case 2005: return "FNT";
				case 2007: return "LUA";
				case 2008: return "SLT";
				case 2009: return "NSS";
				case 2010: return "NCS";
				case 2011: return "MOD";
				case 2012: return "ARE";
				case 2013: return "SET";
				case 2014: return "IFO";
				case 2015: return "BIC";
				case 2016: return "WOK";
				case 2017: return "2DA";
				case 2018: return "TLK";
				case 2022: return "TXI";
				case 2023: return "GIT";
				case 2024: return "BTI";
				case 2025: return "UTI";
				case 2026: return "BTC";
				case 2027: return "UTC";
				case 2029: return "DLG";
				case 2030: return "ITP";
				case 2031: return "BTT";
				case 2032: return "UTT";
				case 2033: return "DDS";
				case 2034: return "BTS";
				case 2035: return "UTS";
				case 2036: return "LTR";
				case 2037: return "GFF";
				case 2038: return "FAC";
				case 2039: return "BTE";
				case 2040: return "UTE";
				case 2041: return "BTD";
				case 2042: return "UTD";
				case 2043: return "BTP";
				case 2044: return "UTP";
				case 2045: return "DFT";
				case 2046: return "GIC";
				case 2047: return "GUI";
				case 2048: return "CSS";
				case 2049: return "CCS";
				case 2050: return "BTM";
				case 2051: return "UTM";
				case 2052: return "DWK";
				case 2053: return "PWK";
				case 2054: return "BTG";
				case 2055: return "UTG";
				case 2056: return "JRL";
				case 2057: return "SAV";
				case 2058: return "UTW";
				case 2059: return "4PC";
				case 2060: return "SSF";
				case 2061: return "HAK";
				case 2062: return "NWM";
				case 2063: return "BIK";
				case 2064: return "NDB";
				case 2065: return "PTM";
				case 2066: return "PTT";
				case 2067: return "BAK";
				case 3000: return "OSC";
				case 3001: return "USC";
				case 3002: return "TRN";
				case 3003: return "UTR";
				case 3004: return "UEN";
				case 3005: return "ULT";
				case 3006: return "SEF";
				case 3007: return "PFX";
				case 3008: return "CAM";
				case 3009: return "LFX";
				case 3010: return "BFX";
				case 3011: return "UPE";
				case 3012: return "ROS";
				case 3013: return "RST";
				case 3014: return "IFX";
				case 3015: return "PFB";
				case 3016: return "ZIP";
				case 3017: return "WMP";
				case 3018: return "BBX";
				case 3019: return "TFX";
				case 3020: return "WLK";
				case 3021: return "XML";
				case 3022: return "SCC";
				case 3033: return "PTX";
				case 3034: return "LTX";
				case 3035: return "TRX";
				case 4007: return "JPG";
				case 4008: return "PWC";
				case 4000: return "MDB";
				case 4001: return "MDA";
				case 4002: return "SPT";
				case 4003: return "GR2";
				case 4004: return "FXA";
				case 4005: return "FXE";
				case 9999: return "KEY";
				case 9998: return "BIF";
				case 9997: return "ERF";
				case 9996: return "IDS";

				case UInt16.MaxValue:
					return "non";
			}
			return "ResourceType UNKNOWN";
		}
	}
}

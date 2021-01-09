using System;


namespace SpecialEffectsViewer
{
	static class BwResourceTypes
	{
		internal static string GetResourceTypeString(int rt)
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

				case 65535: return "non";
			}
			return "ResourceType UNKNOWN";
		}

/*		internal static void PrintResourceTypes()
		{
			string info = String.Empty;

			info += "OEIShared.Utils.BWResourceTypes" + Environment.NewLine + Environment.NewLine;

			info +=    "0= " + StringDecryptor.Decrypt("ᒻᒮᒼ") + Environment.NewLine;
			info +=    "1= " + StringDecryptor.Decrypt("ᒫᒶᒹ") + Environment.NewLine;
			info +=    "2= " + StringDecryptor.Decrypt("ᒶᒿᒮ") + Environment.NewLine;
			info +=    "3= " + StringDecryptor.Decrypt("ᒽᒰᒪ") + Environment.NewLine;
			info +=    "4= " + StringDecryptor.Decrypt("ᓀᒪᒿ") + Environment.NewLine;
			info +=    "5= " + StringDecryptor.Decrypt("ᓀᒯᓁ") + Environment.NewLine;
			info +=    "6= " + StringDecryptor.Decrypt("ᒹᒵᒽ") + Environment.NewLine;
			info +=    "7= " + StringDecryptor.Decrypt("ᓒᓗᓒ") + Environment.NewLine;
			info +=    "8= " + StringDecryptor.Decrypt("ᒶᒹᒜ") + Environment.NewLine;
			info +=    "9= " + StringDecryptor.Decrypt("ᒶᒹᒰ") + Environment.NewLine;
			info +=   "10= " + StringDecryptor.Decrypt("ᒽᓁᒽ") + Environment.NewLine;
			info += "2000= " + StringDecryptor.Decrypt("ᒹᒵᒱ") + Environment.NewLine;
			info += "2001= " + StringDecryptor.Decrypt("ᒽᒮᓁ") + Environment.NewLine;
			info += "2002= " + StringDecryptor.Decrypt("ᒶᒭᒵ") + Environment.NewLine;
			info += "2003= " + StringDecryptor.Decrypt("ᒽᒱᒰ") + Environment.NewLine;
			info += "2005= " + StringDecryptor.Decrypt("ᒯᒷᒽ") + Environment.NewLine;
			info += "2007= " + StringDecryptor.Decrypt("ᒵᒾᒪ") + Environment.NewLine;
			info += "2008= " + StringDecryptor.Decrypt("ᒼᒵᒽ") + Environment.NewLine;
			info += "2009= " + StringDecryptor.Decrypt("ᒷᒼᒼ") + Environment.NewLine;
			info += "2010= " + StringDecryptor.Decrypt("ᒷᒬᒼ") + Environment.NewLine;
			info += "2011= " + StringDecryptor.Decrypt("ᒶᒸᒭ") + Environment.NewLine;
			info += "2012= " + StringDecryptor.Decrypt("ᒪᒻᒮ") + Environment.NewLine;
			info += "2013= " + StringDecryptor.Decrypt("ᒼᒮᒽ") + Environment.NewLine;
			info += "2014= " + StringDecryptor.Decrypt("ᒲᒯᒸ") + Environment.NewLine;
			info += "2015= " + StringDecryptor.Decrypt("ᒫᒲᒬ") + Environment.NewLine;
			info += "2016= " + StringDecryptor.Decrypt("ᓀᒸᒴ") + Environment.NewLine;
			info += "2017= " + StringDecryptor.Decrypt("ᒛᒭᒪ") + Environment.NewLine;
			info += "2018= " + StringDecryptor.Decrypt("ᒽᒵᒴ") + Environment.NewLine;
			info += "2022= " + StringDecryptor.Decrypt("ᒽᓁᒲ") + Environment.NewLine;
			info += "2023= " + StringDecryptor.Decrypt("ᒰᒲᒽ") + Environment.NewLine;
			info += "2024= " + StringDecryptor.Decrypt("ᒫᒽᒲ") + Environment.NewLine;
			info += "2025= " + StringDecryptor.Decrypt("ᒾᒽᒲ") + Environment.NewLine;
			info += "2026= " + StringDecryptor.Decrypt("ᒫᒽᒬ") + Environment.NewLine;
			info += "2027= " + StringDecryptor.Decrypt("ᒾᒽᒬ") + Environment.NewLine;
			info += "2029= " + StringDecryptor.Decrypt("ᒭᒵᒰ") + Environment.NewLine;
			info += "2030= " + StringDecryptor.Decrypt("ᒲᒽᒹ") + Environment.NewLine;
			info += "2031= " + StringDecryptor.Decrypt("ᒫᒽᒽ") + Environment.NewLine;
			info += "2032= " + StringDecryptor.Decrypt("ᒾᒽᒽ") + Environment.NewLine;
			info += "2033= " + StringDecryptor.Decrypt("ᒭᒭᒼ") + Environment.NewLine;
			info += "2034= " + StringDecryptor.Decrypt("ᒫᒽᒼ") + Environment.NewLine;
			info += "2035= " + StringDecryptor.Decrypt("ᒾᒽᒼ") + Environment.NewLine;
			info += "2036= " + StringDecryptor.Decrypt("ᒵᒽᒻ") + Environment.NewLine;
			info += "2037= " + StringDecryptor.Decrypt("ᒰᒯᒯ") + Environment.NewLine;
			info += "2038= " + StringDecryptor.Decrypt("ᒯᒪᒬ") + Environment.NewLine;
			info += "2039= " + StringDecryptor.Decrypt("ᒫᒽᒮ") + Environment.NewLine;
			info += "2040= " + StringDecryptor.Decrypt("ᒾᒽᒮ") + Environment.NewLine;
			info += "2041= " + StringDecryptor.Decrypt("ᒫᒽᒭ") + Environment.NewLine;
			info += "2042= " + StringDecryptor.Decrypt("ᒾᒽᒭ") + Environment.NewLine;
			info += "2043= " + StringDecryptor.Decrypt("ᒫᒽᒹ") + Environment.NewLine;
			info += "2044= " + StringDecryptor.Decrypt("ᒾᒽᒹ") + Environment.NewLine;
			info += "2045= " + StringDecryptor.Decrypt("ᒭᒯᒽ") + Environment.NewLine;
			info += "2046= " + StringDecryptor.Decrypt("ᒰᒲᒬ") + Environment.NewLine;
			info += "2047= " + StringDecryptor.Decrypt("ᒰᒾᒲ") + Environment.NewLine;
			info += "2048= " + StringDecryptor.Decrypt("ᒬᒼᒼ") + Environment.NewLine;
			info += "2049= " + StringDecryptor.Decrypt("ᒬᒬᒼ") + Environment.NewLine;
			info += "2050= " + StringDecryptor.Decrypt("ᒫᒽᒶ") + Environment.NewLine;
			info += "2051= " + StringDecryptor.Decrypt("ᒾᒽᒶ") + Environment.NewLine;
			info += "2052= " + StringDecryptor.Decrypt("ᒭᓀᒴ") + Environment.NewLine;
			info += "2053= " + StringDecryptor.Decrypt("ᒹᓀᒴ") + Environment.NewLine;
			info += "2054= " + StringDecryptor.Decrypt("ᒫᒽᒰ") + Environment.NewLine;
			info += "2055= " + StringDecryptor.Decrypt("ᒾᒽᒰ") + Environment.NewLine;
			info += "2056= " + StringDecryptor.Decrypt("ᒳᒻᒵ") + Environment.NewLine;
			info += "2057= " + StringDecryptor.Decrypt("ᒼᒪᒿ") + Environment.NewLine;
			info += "2058= " + StringDecryptor.Decrypt("ᒾᒽᓀ") + Environment.NewLine;
			info += "2059= " + StringDecryptor.Decrypt("ᒝᒹᒬ") + Environment.NewLine;
			info += "2060= " + StringDecryptor.Decrypt("ᒼᒼᒯ") + Environment.NewLine;
			info += "2061= " + StringDecryptor.Decrypt("ᒱᒪᒴ") + Environment.NewLine;
			info += "2062= " + StringDecryptor.Decrypt("ᒷᓀᒶ") + Environment.NewLine;
			info += "2063= " + StringDecryptor.Decrypt("ᒫᒲᒴ") + Environment.NewLine;
			info += "2064= " + StringDecryptor.Decrypt("ᒷᒭᒫ") + Environment.NewLine;
			info += "2065= " + StringDecryptor.Decrypt("ᒹᒽᒶ") + Environment.NewLine;
			info += "2066= " + StringDecryptor.Decrypt("ᒹᒽᒽ") + Environment.NewLine;
			info += "2067= " + StringDecryptor.Decrypt("ᒫᒪᒴ") + Environment.NewLine;
			info += "3000= " + StringDecryptor.Decrypt("ᒸᒼᒬ") + Environment.NewLine;
			info += "3001= " + StringDecryptor.Decrypt("ᒾᒼᒬ") + Environment.NewLine;
			info += "3002= " + StringDecryptor.Decrypt("ᒽᒻᒷ") + Environment.NewLine;
			info += "3003= " + StringDecryptor.Decrypt("ᒾᒽᒻ") + Environment.NewLine;
			info += "3004= " + StringDecryptor.Decrypt("ᒾᒮᒷ") + Environment.NewLine;
			info += "3005= " + StringDecryptor.Decrypt("ᒾᒵᒽ") + Environment.NewLine;
			info += "3006= " + StringDecryptor.Decrypt("ᒼᒮᒯ") + Environment.NewLine;
			info += "3007= " + StringDecryptor.Decrypt("ᒹᒯᓁ") + Environment.NewLine;
			info += "3008= " + StringDecryptor.Decrypt("ᒬᒪᒶ") + Environment.NewLine;
			info += "3009= " + StringDecryptor.Decrypt("ᒵᒯᓁ") + Environment.NewLine;
			info += "3010= " + StringDecryptor.Decrypt("ᒫᒯᓁ") + Environment.NewLine;
			info += "3011= " + StringDecryptor.Decrypt("ᒾᒹᒮ") + Environment.NewLine;
			info += "3012= " + StringDecryptor.Decrypt("ᒻᒸᒼ") + Environment.NewLine;
			info += "3013= " + StringDecryptor.Decrypt("ᒻᒼᒽ") + Environment.NewLine;
			info += "3014= " + StringDecryptor.Decrypt("ᒲᒯᓁ") + Environment.NewLine;
			info += "3015= " + StringDecryptor.Decrypt("ᒹᒯᒫ") + Environment.NewLine;
			info += "3016= " + StringDecryptor.Decrypt("ᓃᒲᒹ") + Environment.NewLine;
			info += "3017= " + StringDecryptor.Decrypt("ᓀᒶᒹ") + Environment.NewLine;
			info += "3018= " + StringDecryptor.Decrypt("ᒫᒫᓁ") + Environment.NewLine;
			info += "3019= " + StringDecryptor.Decrypt("ᒽᒯᓁ") + Environment.NewLine;
			info += "3020= " + StringDecryptor.Decrypt("ᓀᒵᒴ") + Environment.NewLine;
			info += "3021= " + StringDecryptor.Decrypt("ᓁᒶᒵ") + Environment.NewLine;
			info += "3022= " + StringDecryptor.Decrypt("ᒼᒬᒬ") + Environment.NewLine;
			info += "3033= " + StringDecryptor.Decrypt("ᒹᒽᓁ") + Environment.NewLine;
			info += "3034= " + StringDecryptor.Decrypt("ᒵᒽᓁ") + Environment.NewLine;
			info += "3035= " + StringDecryptor.Decrypt("ᒽᒻᓁ") + Environment.NewLine;
			info += "4007= " + StringDecryptor.Decrypt("ᒳᒹᒰ") + Environment.NewLine;
			info += "4008= " + StringDecryptor.Decrypt("ᒹᓀᒬ") + Environment.NewLine;
			info += "4000= " + StringDecryptor.Decrypt("ᒶᒭᒫ") + Environment.NewLine;
			info += "4001= " + StringDecryptor.Decrypt("ᒶᒭᒪ") + Environment.NewLine;
			info += "4002= " + StringDecryptor.Decrypt("ᒼᒹᒽ") + Environment.NewLine;
			info += "4003= " + StringDecryptor.Decrypt("ᒰᒻᒛ") + Environment.NewLine;
			info += "4004= " + StringDecryptor.Decrypt("ᒯᓁᒪ") + Environment.NewLine;
			info += "4005= " + StringDecryptor.Decrypt("ᒯᓁᒮ") + Environment.NewLine;
			info += "9999= " + StringDecryptor.Decrypt("ᒴᒮᓂ") + Environment.NewLine;
			info += "9998= " + StringDecryptor.Decrypt("ᒫᒲᒯ") + Environment.NewLine;
			info += "9997= " + StringDecryptor.Decrypt("ᒮᒻᒯ") + Environment.NewLine;
			info += "9996= " + StringDecryptor.Decrypt("ᒲᒭᒼ") + Environment.NewLine;
			info += ushort.MaxValue + "= \"\"" + Environment.NewLine;

			System.IO.File.WriteAllText(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "BWResourceTypes.txt"), info);
			// ps. Fuck you. If you're going to release a toolset and expect it
			// to be user-friendly for the creation of plugins don't obfuscate
			// the shit that's needed to write those plugins.
		} */
	}
}

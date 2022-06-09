using MySql.Data.MySqlClient;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;

namespace PlayerInfoLibrary
{
    public static class Extensions
    {
        public static DateTime FromTimeStamp(this long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
        }

        public static long ToTimeStamp(this DateTime datetime)
        {
            return (long)(datetime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }

        public static bool IsDBNull(this MySqlDataReader reader, string fieldname)
        {
            return reader.IsDBNull(reader.GetOrdinal(fieldname));
        }

        public static string GetIP(this CSteamID cSteamID)
        {
            var player = UnturnedPlayer.FromCSteamID(cSteamID);
            var IP = player.Player.channel.owner.getIPv4AddressOrZero();
            return Parser.getIPFromUInt32(IP);
        }

        // Returns a Steamworks.CSteamID on out from a string, and returns true if it is a CSteamID.
        public static bool IsCSteamID(this string sCSteamID, out CSteamID cSteamID)
        {
            cSteamID = CSteamID.Nil;
            if (ulong.TryParse(sCSteamID, out var ulCSteamID))
            {
                if ((ulCSteamID >= 0x0110000100000000 && ulCSteamID <= 0x0170000000000000) || ulCSteamID == 0)
                {
                    cSteamID = new CSteamID(ulCSteamID);
                    return true;
                }
            }
            return false;
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        // Returns formatted string with how long they've played on the server in d, h, m, s.
        public static string FormatTotalTime(this int totalTime)
        {
            string totalTimeFormated = "";
            if (totalTime >= (60 * 60 * 24))
            {
                totalTimeFormated = ((int)(totalTime / (60 * 60 * 24))).ToString() + "d ";
            }
            if (totalTime >= (60 * 60))
            {
                totalTimeFormated += ((int)((totalTime / (60 * 60)) % 24)).ToString() + "h ";
            }
            if (totalTime >= 60)
            {
                totalTimeFormated += ((int)((totalTime / 60) % 60)).ToString() + "m ";
            }
            totalTimeFormated += ((int)(totalTime % 60)).ToString() + "s";
            return totalTimeFormated;
        }

        public static int TotalPlayTime(this CSteamID PlayerID)
        {
            var RegistedTime = 0;
            var Player = UnturnedPlayer.FromCSteamID(PlayerID);
            if (Player != null)
            {
                var Component = Player.GetComponent<PlayerInfoLibPComponent>();
                if (Component != null && Component.pData.IsValid() && Component.pData.IsLocal())
                    RegistedTime = Component.pData.TotalPlayTime;
            }

            if (RegistedTime == 0)
            {
                var pData = PlayerInfoLib.Database.QueryById(PlayerID, false);
                if (pData.IsValid() && pData.IsLocal())
                    RegistedTime = pData.TotalPlayTime;
            }

            if (PlayerInfoLib.LoginTime.TryGetValue(PlayerID, out var Date))
                RegistedTime += (int)(DateTime.Now - Date).TotalSeconds;
            
            return RegistedTime;
        }
    }
}

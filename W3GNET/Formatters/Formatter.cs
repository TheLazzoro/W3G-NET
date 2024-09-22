using System;
using System.Linq;
using System.Text;
using W3GNET.Types;

namespace W3GNET.Formatters
{
    internal static class Formatter
    {
        public static ObjectId ObjectIdFormatter(int[] arr)
        {
            if (arr[3] >= 0x41 && arr[3] <= 0x7a)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in arr)
                {
                    sb.Append((char)item);
                }
                var reverse = sb.ToString().Reverse();
                return new ObjectId
                {
                    value_str = string.Join("", reverse)
                };
            }

            return new ObjectId
            {
                value_num = arr
            };
        }

        public static Race RaceFlagFormatter(int flag)
        {
            switch (flag)
            {
                case 0x01:
                case 0x41:
                    return Race.Human;
                case 0x02:
                case 0x42:
                    return Race.Orc;
                case 0x04:
                case 0x44:
                    return Race.NightElf;
                case 0x08:
                case 0x48:
                    return Race.Undead;
                case 0x20:
                case 0x60:
                    return Race.Random;
                default:
                    break;
            }
            return Race.Random;
        }

        public static string ChatModeFormatter(int flag)
        {
            switch (flag)
            {
                case 0x00:
                    return "ALL";
                case 0x01:
                    return "ALLY";
                case 0x02:
                    return "OBS";
            }

            if (flag >= 3 && flag <= 27)
            {
                return $"PRIVATE{flag}";
            }

            return $"UNKNOWN";
        }
    }
}

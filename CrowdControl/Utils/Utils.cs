using System;

namespace WarpWorld.CrowdControl {
    public static class Utils {
        private static Random random = new Random();

        internal static string GenerateRandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[length];

            for (int i = 0; i < length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            return new String(stringChars);
        }
    }
}

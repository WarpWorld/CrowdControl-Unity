using System;

namespace WarpWorld.CrowdControl {
    public static class Utils {
        public static bool RandomBool() {
            return UnityEngine.Random.Range(0, 2) >= 1;
        }

        public static ulong Randomulong() {
            ulong value = Convert.ToUInt64(UnityEngine.Random.Range(0, int.MaxValue));
            value += Convert.ToUInt64(UnityEngine.Random.Range(0, int.MaxValue)) * 0x100000000;

            int final = UnityEngine.Random.Range(0, 4);

            if (final % 1 == 0)
                value += 0x80000000;

            if (final % 2 == 0)
                value += 0x8000000000000000;

            return value;
        }

        private static Random random = new Random();

        public static string GenerateRandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[length];

            for (int i = 0; i < length; i++)
                stringChars[i] = chars[random.Next(chars.Length)];

            return new String(stringChars);
        }
    }
}

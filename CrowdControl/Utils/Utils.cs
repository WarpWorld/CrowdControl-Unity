using System;
using System.Security.Cryptography;
using System.Text;

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
    }
}

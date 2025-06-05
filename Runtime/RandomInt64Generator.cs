using System;

namespace UnityTools.Runtime
{
    public static class RandomInt64Generator
    {
        private static Random random = new Random();
        private static byte[] buffer = new byte[8];

        public static long Next()
        {
            random.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}

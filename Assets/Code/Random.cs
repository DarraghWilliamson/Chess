public static class Random {
    private static uint seed = 1804289383;

    public static uint GetRandomNumbler() {
        uint num = seed;
        num ^= num << 13;
        num ^= num >> 17;
        num ^= num << 5;

        seed = num;

        return num;
    }

    public static ulong GetRandom64() {
        ulong n0 = (ulong)(GetRandomNumbler() & 0xFFFF);
        ulong n1 = (ulong)(GetRandomNumbler() & 0xFFFF);
        ulong n2 = (ulong)(GetRandomNumbler() & 0xFFFF);
        ulong n3 = (ulong)(GetRandomNumbler() & 0xFFFF);
        return n0 | (n1 << 16) | (n2 << 32) | (n3 << 48);
    }
}
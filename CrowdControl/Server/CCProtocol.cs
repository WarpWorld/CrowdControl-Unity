namespace WarpWorld.CrowdControl {
    public static class Protocol {
        public const byte VERSION = 4;
        public const int PING_INTERVAL = 300;

        // $00 - $3F Client to Server
        // $40 - $7F Server to Client
        // $80 - $FF Protocol

        public enum ResultType : byte {
            Success = 0,
            Failure = 1,
            Unavailable = 2,
            Retry = 3,
            Queue = 4,
            Running = 5
        }
    }
}

namespace WarpWorld.CrowdControl
{
    public class PendingMessage
    {
        public byte[] Bytes { get; private set; }
        public byte MsgType { get; private set; }
        public int Size { get; private set; }

        public PendingMessage(byte[] bytes, byte msgType, int size)
        {
            Bytes = bytes;
            MsgType = msgType;
            Size = size;
        }
    }
}

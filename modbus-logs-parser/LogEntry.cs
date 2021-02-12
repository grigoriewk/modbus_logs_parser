using System;

namespace test_app_for_techart
{
    [Serializable]
    public class LogEntry
    {
        public enum LogCommand
        {
            Request,
            Response
        };

        public enum LogResult
        {
            Success,
            Timeout,
            FAIL
        };

        public int Id { get => id; set => id = value; }
        public DateTime Time { get => time; set => time = value; }
        public string App { get => app; set => app = value; }
        public LogCommand Direction { get => direction; set => direction = value; }
        public string Serial { get => source; set => source = value; }
        public LogResult Result { get => result; set => result = value; }
        public int DataLength { get => dataLength; set => dataLength = value; }
        public byte[] Data { get => data; set => data = value; }

        int id;
        DateTime time;
        string app;
        LogCommand direction;
        string source;
        LogResult result;
        int dataLength;
        byte[] data;
    }
}

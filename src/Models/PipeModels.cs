namespace WpfClient.Models
{
    public class PipeModels
    {
        public class Packet
        {
            public Type PacketType { get; set; }
        }
    
        public class Packet<T>
        {
            public Type PacketType
            {
                get
                {
                    if (typeof(T) == typeof(ShutdownData))
                        return Type.Shutdown;
                    if (typeof(T) == typeof(LogOffData))
                        return Type.Logoff;
                    return Type.None;
                }
            }

            public T Data { get; set; }
        }
    
        public class ShutdownData
        {
            public Reasons Reason { get; set; }
            public string NextLesson { get; set; }
        }

        public class LogOffData {}

        public enum Reasons
        {
            LessonsOver,
            NoUser
        }
    
        public enum Type
        {
            Shutdown,
            Logoff,
            None
        }
    }
}
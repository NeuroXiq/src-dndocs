using DNDocs.Domain.Enums;

namespace DNDocs.Domain.Entity.App
{
    public class SystemMessage : Entity
    {
        public SystemMessageType Type { get; set; }
        public SystemMessageLevel Level { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public int? TraceBgJobId { get; set; }
        public int? UserId { get; set; }
        public int? ProjectId { get; set; }
        public int? ProjectVersioningId { get; set; }
        // public int? ProjectVersioningId { get; set; }

        public User User { get; set; }
        public Project Project { get; set; }
        public ProjectVersioning ProjectVersioning { get; set; }
        
        protected SystemMessage()
        {
        }

        public SystemMessage(
            SystemMessageType type,
            SystemMessageLevel level,
            string title,
            string message,
            DateTime datetime) : this(type, level, title, message, datetime, null)
        { }

        public SystemMessage(
            SystemMessageType type,
            SystemMessageLevel level,
            string title,
            string message,
            DateTime datetime, int? traceBgJobId)
        {
            Type = type;
            Level = level;
            Title = title;
            Message = message;
            DateTime = datetime;
            TraceBgJobId = traceBgJobId;
        }
    }
}

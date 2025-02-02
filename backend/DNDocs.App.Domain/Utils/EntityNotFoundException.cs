namespace DNDocs.Domain.Utils
{
    public class EntityNotFoundException : RobiniaException
    {
        public EntityNotFoundException(string message) : base(message) { }
    }

    public class EntityNotFoundException<T> : EntityNotFoundException
    {
        public EntityNotFoundException(string msg) : base($"Entity '{typeof(T).Name}' was not found. {msg}") { }
        public EntityNotFoundException(int id) : base($"Entity '{typeof(T).Name}' with id '{id}' was not found") { }
    }
}

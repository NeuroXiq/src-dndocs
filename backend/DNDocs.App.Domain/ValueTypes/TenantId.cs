using System.Diagnostics;

namespace DNDocs.Domain.ValueTypes
{
    [DebuggerDisplay("{ToString()}")]
    public struct ProjectId
    {
        public Guid Id { get; set; }

        public ProjectId(Guid id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"ProjectId: {Id.ToString().ToUpper()}";
        }
    }
}

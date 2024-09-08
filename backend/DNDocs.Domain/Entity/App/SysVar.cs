using DNDocs.Domain.Utils;

namespace DNDocs.Domain.Entity.App
{
    public class SysVar : Entity
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public SysVar() { }

        public SysVar(string key, string value)
        {
            Validation.AppArgStringNotEmpty(key, nameof(key));

            Key = key;
            Value = value;
        }
    }
}

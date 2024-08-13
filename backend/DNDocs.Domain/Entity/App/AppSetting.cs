using DNDocs.Domain.Utils;

namespace DNDocs.Domain.Entity.App
{
    public class AppSetting : Entity
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public AppSetting() { }

        public AppSetting(string key, string value)
        {
            Validation.AppArgStringNotEmpty(key, nameof(key));

            Key = key;
            Value = value;
        }
    }
}

using Microsoft.Extensions.Localization;

namespace DNDocs.Web.Application
{
    public class RobiniaResources : IRobiniaResources
    {
        private IStringLocalizer<DefaultResources> localizer;

        public RobiniaResources(IStringLocalizer<DefaultResources> stringLocalizer)
        {
            this.localizer = stringLocalizer;
        }

        public string this[string stringName] => localizer[stringName];
    }
}

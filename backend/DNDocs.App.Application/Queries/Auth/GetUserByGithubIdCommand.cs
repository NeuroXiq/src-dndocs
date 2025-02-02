using DNDocs.Application.Shared;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Application.Queries.Auth
{
    public class GetUserByGithubIdCommand : Query<UserDto>
    {
        public string Id { get; set; }

        public GetUserByGithubIdCommand()
        {
        }
    }
}

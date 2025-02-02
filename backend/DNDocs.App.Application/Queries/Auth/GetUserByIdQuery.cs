using DNDocs.Application.Shared;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Application.Queries.Auth
{
    public class GetUserByIdQuery : Query<UserDto>
    {
        public int Id { get; set; }

        public GetUserByIdQuery() { }
    }
}

using DNDocs.Application.Queries.Auth;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Application.QueryHandlers.Auth
{
    internal class GetUserByGithubIdHandler : QueryHandlerA<GetUserByGithubIdCommand, UserDto>
    {
        private IAppUnitOfWork uow;

        public GetUserByGithubIdHandler(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        protected override async Task<UserDto> Handle(GetUserByGithubIdCommand query)
        {
            var user = this.uow.GetSimpleRepository<User>()
                .Query()
                .Where(t => t.GithubId == query.Id)
                .FirstOrDefault();

            UserDto result = UserDto.Map(user);

            return result;
        }
    }
}

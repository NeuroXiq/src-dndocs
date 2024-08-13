using DNDocs.Application.Queries;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Application.QueryHandlers
{
    internal class GetMyUserHandler : QueryHandler<GetMyUserQuery, UserDto>
    {
        private int userid;
        private IAppUnitOfWork appUow;

        public GetMyUserHandler(IAppUnitOfWork appUow,
            ICurrentUser currentUser)
        {
            this.userid = currentUser.UserIdAuthorized;
            this.appUow = appUow;
        }

        protected override UserDto Handle(GetMyUserQuery query)
        {
            var user = this.appUow.GetSimpleRepository<User>()
                .GetById(userid);

            return UserDto.Map(user);
        }
    }
}

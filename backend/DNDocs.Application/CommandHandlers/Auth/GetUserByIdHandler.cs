using DNDocs.Application.Queries.Auth;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.ValueTypes;

namespace DNDocs.Application.CommandHandlers.Auth
{
    internal class GetUserByIdHandler : QueryHandler<GetUserByIdQuery, UserDto>
    {
        private IAppUnitOfWork uow;

        public GetUserByIdHandler(IAppUnitOfWork appuow)
        {
            this.uow = appuow;
        }

        protected override UserDto Handle(GetUserByIdQuery query)
        {
            return Mapper.Map(uow.GetSimpleRepository<User>().GetById(query.Id));
        }
    }
}

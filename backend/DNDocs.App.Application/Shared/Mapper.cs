using DNDocs.Domain.ValueTypes;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.Home;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.DTO.Shared;
using System.Reflection.Metadata.Ecma335;
using DNDocs.Domain.Entity;

namespace DNDocs.Application.Shared
{
    public class Mapper
    {
        public static DNDocs.Api.DTO.Shared.BgJobDto Map(BgJob j)
        {
            return new DNDocs.Api.DTO.Shared.BgJobDto
            {
                CompletedDateTime = j.CompletedDateTime,
                CreateByUserId = j.ExecuteAsUserId,
                DoWorkCommandData = j.DoWorkCommandData,
                DoWorkCommandType = j.DoWorkCommandType,
                Id = j.Id,
                QueuedDateTime = j.QueuedDateTime,
                StartedDateTime = j.StartedDateTime,
                Status = (DNDocs.Api.DTO.Enum.BgJobStatus)j.Status
            };
        }

        internal static UserDto Map(User user)
        {
            return UserDto.Map(user);
        }

        static TDest SimpleAutoMap<TSrc, TDest>(TSrc src) where TSrc: class where TDest: class
        {
            if (src == null) return null;

            var srcProps = src.GetType().GetProperties();
            var destProps = typeof(TDest).GetProperties();
            var dest = Activator.CreateInstance(typeof(TDest)) as TDest;

            foreach (var sp in srcProps)
            {
                var dp = destProps.FirstOrDefault(t => t.Name == sp.Name);

                if (dp == null ||
                    (!sp.PropertyType.IsValueType && 
                    typeof(string) != sp.PropertyType)) continue;

                var sptype = sp.PropertyType;
                object setDest = sp.GetValue(src);
                var spn = Nullable.GetUnderlyingType(sptype);
                var dpn = Nullable.GetUnderlyingType(dp.PropertyType);

                if (spn != null && setDest != null && spn.IsEnum)
                {
                    setDest = sptype.GetProperty("Value").GetValue(setDest);
                    setDest = Enum.Parse(dpn, setDest.ToString());
                }
                //if (sptype.IsEnum)
                //{
                //    var srcEnumVal = sp.GetValue(src);
                //    var asdf = srcEnumVal.ToString();
                //    if (srcEnumVal == null) dp.SetValue(dest, null);
                //    else dp.SetValue(dest, Enum.Parse(dp.PropertyType, srcEnumVal.ToString()));
                //}
                else
                {
                    
                }

                dp.SetValue(dest, setDest);
            }

            return dest;
        }

        public static IList<DNDocs.Api.DTO.ProjectManage.NugetPackageDto> Map(IEnumerable<NugetPackage> nugetPackages)
        {
            if (nugetPackages == null) return null;
            return nugetPackages.Select(Map).ToList();
        }

        static DNDocs.Api.DTO.ProjectManage.NugetPackageDto Map(NugetPackage package)
        {
            return new DNDocs.Api.DTO.ProjectManage.NugetPackageDto
            {
                Id = package.Id,
                PackageDetailsUrl = package.PackageDetailsUrl,
                IdentityId = package.IdentityId,
                IdentityVersion = package.IdentityVersion,
                IsListed = package.IsListed,
                ProjectUrl = package.ProjectUrl,
                PublishedDate = package.PublishedDate,
                Title = package.Title
            };
        }

        public static CommandResultDto Map(CommandResult commandResult)
        {
            return new CommandResultDto();
        }

        public static CommandResultDto<TResult> MapCR<TResult>(CommandResult<TResult> commandResult)
        {
            return new CommandResultDto<TResult>(commandResult.Result);
        }

        public static QueryResultDto<TResult> MapQR<TResult>(QueryResult<TResult> qr)
        {
            return new QueryResultDto<TResult>() { Result = qr.Result };
        }
    }
}

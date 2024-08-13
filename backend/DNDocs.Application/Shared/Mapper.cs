using DNDocs.Domain.Entity.App;
using DNDocs.Domain.ValueTypes;
using DNDocs.Api.DTO;
using DNDocs.Api.DTO.Home;
using DNDocs.Api.DTO.ProjectManage;
using DNDocs.Api.DTO.Shared;
using System.Reflection.Metadata.Ecma335;

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

        public static ProjectDto Map(Project project)
        {
            if (project == null) return null;

            ProjectDto result = null;
            try
            {
                result = SimpleAutoMap<Project, ProjectDto>(project);
            }
            catch (Exception e)
            {

                throw;
            }
            

            result.ProjectNugetPackages = project.ProjectNugetPackages?.Select(Map).ToList() ?? new List<DNDocs.Api.DTO.ProjectManage.NugetPackageDto>();

            return result;
        }

        public static IList<SystemMessageDto> Map(IEnumerable<SystemMessage> messages)
        {
            return messages.Select(Map).ToArray();
        }

        public static SystemMessageDto Map(SystemMessage m)
        {
            return new SystemMessageDto
            {
                Id = m.Id,
                DateTime = m.DateTime,
                Level = (DNDocs.Api.DTO.Enum.SystemMessageLevel)m.Level,
                Message = m.Message,
                ProjectId = m.ProjectId,
                Title = m.Title,
                Type = (DNDocs.Api.DTO.Enum.SystemMessageType)m.Type,
                UserId = m.UserId
            };
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
            return new CommandResultDto(commandResult.Success,
                commandResult.ErrorMessage,
                commandResult.FieldErrors?.Select(t => new FieldErrorDto(t.FieldName, t.ErrorMessage)));
        }

        internal static TableDataDto<ProjectDto> Map(TableDataResponse<Project> projects)
        {
            return new TableDataDto<ProjectDto>(
                projects.RowsCount,
                projects.CurrentPage,
                projects.RowsPerPage,
                projects.Result.Select(Mapper.Map).ToArray());
        }

        public static CommandResultDto<TResult> MapCR<TResult>(CommandResult<TResult> commandResult)
        {
            return new CommandResultDto<TResult>(
                commandResult.Result,
                commandResult.Success, 
                commandResult.ErrorMessage,
                commandResult.FieldErrors?.Select(t => new FieldErrorDto(t.FieldName, t.ErrorMessage)));
        }

        public static QueryResultDto<TResult> MapQR<TResult>(QueryResult<TResult> qr)
        {
            return new QueryResultDto<TResult>() { Result = qr.Result };
        }

        internal static List<ProjectVersioningDto> Map(List<ProjectVersioning> v)
        {
            return v?.Select(Map).ToList();
        }

        internal static ProjectVersioningDto Map(ProjectVersioning v)
        {
            return new ProjectVersioningDto(
                v.Id,
                v.ProjectName,
                v.ProjectWebsiteUrl,
                v.UrlPrefix,
                v.GitDocsRepoUrl,
                v.GitDocsBranchName,
                v.GitDocsRelativePath,
                v.GitHomepageRelativePath,
                v.Autoupgrage,
                v.NugetPackages?.Select(Map)?.ToList());
        }
    }
}

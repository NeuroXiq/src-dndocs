//namespace DNDocs.Api.DTO
//{
//    public struct TenantId
//    {
//        public Guid UUID { get; set; }

//        public int ProjectId { get; private set;  }
//        public string ProjectName { get; private set; }

//        public TenantId() { }

//        public TenantId(
//            int tenantProjectId,
//            string stringPrefix)
//        {

//            ProjectId = tenantProjectId;
//            ProjectName = stringPrefix;

//            // if (stringPrefix != null)
//            //     Parse(ProjectName);
//        }

//        public string ConvertToString()
//        {
//            return string.Format("{0}", ProjectName);
//        }

//        //public static bool TryParse(string tenantContextString, out TenantId parsedInfo)
//        //{
//        //    try
//        //    {
//        //        parsedInfo = Parse(tenantContextString);
//        //        return true;
//        //    }
//        //    catch
//        //    {
//        //        parsedInfo = default(TenantId);
//        //        return false;
//        //    }
//        //}

//        public static bool VaildateRobiniaUrlPrefix(string tenantContextString)
//        {
//            bool invalid =
//                    string.IsNullOrWhiteSpace(tenantContextString) ||
//                    tenantContextString.Length <= 3 ||
//                    !tenantContextString.All(c => (c >= 'a' && c <= 'z') || c == '-' || (c >= '0' && c <= '9')) ||
//                    tenantContextString.All(c => c == '-') ||
//                    tenantContextString.Length > 64;

//            return !invalid;
//        }

//        //static TenantId Parse(string tenantContextString)
//        //{
//        //    bool invalid = string.IsNullOrWhiteSpace(tenantContextString) ||
//        //        tenantContextString.Length < 3 ||
//        //        tenantContextString.Any(charr => !char.IsAscii(charr));

//        //    invalid |= tenantContextString.Count(c => c == '-') != 1;

//        //    if (invalid) throw new ArgumentException(nameof(tenantContextString));

//        //    string[] items = tenantContextString.Split('-');

//        //    invalid |= items.Length != 2 || items.Any(s => string.IsNullOrWhiteSpace(s));
//        //    invalid |= items[1].Length < 1 || items[1].Any(c => c < (int)'0' || c > '9');

//        //    ThrowIfPrefixInvalid(items[0]);

//        //    if (invalid) throw new ArgumentException(tenantContextString);

//        //    string strPrefix = items[0];
//        //    int projId = int.Parse(items[1]);

//        //    return new TenantId(projId, strPrefix);
//        //}
//    }
//}

namespace DNDocs.Infrastructure.Utils
{
    internal class SqlQueryFilters
    {
        public static string ObjPropertyInfo_PublicProjectMember(string objPropInfoAlias)
        {
            //string a = objPropInfoAlias;
            //string isspecialname = ObjPropertyInfoCol.IsSpecialName;
            //string obj_type = DbMetadata.ObjType.Table_Name;
            //string objtype_id = DbMetadata.ObjType.Columns.Id;
            //string objtype_alias = "INTERNAL_pi_objtype";
            //string propertyinfo_reflectedobjtypeid = ObjPropertyInfoCol.ReflectedObjTypeId;


            //// belongs to 'public class'
            //string filter = $"{a}.{isspecialname} = 0 AND EXISTS " + 
            //    $"(" +
            //    $"SELECT * FROM {obj_type} {objtype_alias} WHERE {objtype_alias}.{objtype_id} = {a}.{propertyinfo_reflectedobjtypeid} AND " +
            //    $"{ObjType_PublicProjectMember(objtype_alias)}" +
            //    ")";

            //return filter;

            return null;
        }

        public static string ObjFieldInfo_PublicProjectMember(string objFieldInfoAlias)
        {
            //string a = objFieldInfoAlias;
            //string isspecialname = ObjFieldInfoCol.IsSpecialName;
            //string isfamily = ObjFieldInfoCol.IsFamily;
            //string ispublic = ObjFieldInfoCol.IsPublic;

            //string sql = $"{a}.{isspecialname} = 0 AND ({a}.{isfamily} = 1 OR {a}.{ispublic} = 1)";

            //return sql;

            return null;
        }


        public static string ObjMethodInfo_NormalObjTypeMethod(string methodInfoAlias)
        {
            //string a = methodInfoAlias;
            //string isspecialname = ObjMethodInfoCol.IsSpecialName;
            //string isfamily = ObjMethodInfoCol.IsFamily;
            //string ispublic = ObjMethodInfoCol.IsPublic;

            //string sql = $"({a}.{isspecialname} = 0 AND ({a}.{isfamily} = 1 OR {a}.{ispublic} = 1))";

            //return sql;

            return null;
        }

        public static string ObjNamespace_PublicProjectMember(string namespaceAlias)
        {
            //string a = namespaceAlias;
            //string objtype = DbMetadata.ObjType.Table_Name;
            //string objtype_namespaceid = ObjTypeColumns.NamespaceId;
            //string objnamespace_id = DbMetadata.ObjNamespace.Column_Id;

            //string sql =
            //    $"EXISTS (SELECT * FROM {objtype} ot WHERE \r\n" + 
            //    $"({ObjType_PublicProjectMember("ot")}) AND " +
            //    $"ot.{objtype_namespaceid} = {a}.{objnamespace_id}) ";

            //return sql;

            return null;
        }

        public static string ObjType_PublicProjectMember(string objTypeAlias)
        {
            //string isclass = ObjTypeColumns.IsClass;
            //string isvaluetype = ObjTypeColumns.IsValueType;
            //string isinterface = ObjTypeColumns.IsInterface;
            //string isenum = ObjTypeColumns.IsEnum;
            //string ispublic = ObjTypeColumns.IsPublic;
            //string isnestedpublic = ObjTypeColumns.IsNestedPublic;
            //string isvisible = ObjTypeColumns.IsVisible;
            //string isgenericparameter = ObjTypeColumns.IsGenericParameter;
            //string istypedefinition = ObjTypeColumns.IsTypeDefinition;

            //string a = objTypeAlias;

            //string sql = $"({a}.{isclass} = 1 OR {a}.{isvaluetype} OR {a}.{isinterface} = 1 OR {a}.{isenum} = 1) AND " +
            //    $"({a}.{ispublic} = 1 OR {a}.{isnestedpublic} = 1) AND " +
            //    $"{a}.{isvisible} = 1 AND {a}.{isgenericparameter} = 0 AND {a}.{istypedefinition} = 1";

            //return sql;

            return null;
        }
    }
}

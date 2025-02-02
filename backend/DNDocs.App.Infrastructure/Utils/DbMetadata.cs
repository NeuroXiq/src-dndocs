namespace DNDocs.Infrastructure.Utils
{
    internal class DbMetadata
    {
        // todo use this in ef mapping configurations

        public static class ObjPropertyInfo
        {
            public const string Table_Name = "obj_propertyinfo";

            public static class Columns
            {
                public const string Id = "id";
                public const string IsSpecialName = "IsSpecialName";
                public const string ReflectedObjTypeId = "ReflectedObjTypeId";
            }
        }

        public static class ObjFieldInfo
        {
            public const string Table_Name = "obj_fieldinfo";

            public static class Columns
            {
                public const string Id = "id";
                public const string IsSpecialName = "IsSpecialName";
                public const string IsFamily = "IsFamily";
                public const string IsPublic = "IsPublic";
                public const string ReflectedObjTypeId = "ReflectedObjTypeId";
            }
        }

        public static class ObjMethodInfo
        {
            public static class Columns
            {
                public const string Id = "id";
                public const string IsSpecialName = "IsSpecialName";
                public const string IsFamily = "IsFamily";
                public const string IsPublic = "IsPublic";
                public const string ReflectedObjTypeId = "ReflectedObjTypeId";
            }

            public const string Table_Name = "obj_methodinfo";
        }

        public static class ObjConstructorInfo
        {
            public const string Table_Name = "obj_constructorinfo";
            public const string Column_Id = "id";
            public const string Column_ReflectedObjTypeId = "ReflectedObjTypeId";
        }

        public static class ObjParameterInfo
        {
            public const string Table_Name = "obj_parameterinfo";
            public const string Column_Id = "id";
        }

        public class ObjAssembly
        {
            public const string Table_Name = "obj_assembly";
            public const string Column_Id = "id";

        }

        public class ObjNamespace
        {
            public const string Table_Name = "obj_namespace";
            public const string Column_AssemblyId = "obj_assembly_id";
            public const string Column_NamespaceId = "obj_assembly_id";

            public const string Column_Id = "id";
        }

        public class ObjType
        {
            public static class Columns
            {
                public const string Id = "id";
                public const string AssemblyId = "obj_assembly_id";
                internal static readonly string IsInterface = "IsInterface";
                internal static readonly string IsEnum = "IsEnum";
                internal static readonly string IsPublic = "IsPublic";
                internal static readonly string IsNestedPublic = "IsNestedPublic";
                internal static readonly string IsVisible = "IsVisible";
                internal static readonly string IsGenericParameter = "IsGenericParameter";
                internal static readonly string BaseTypeId = "BaseTypeId";
                internal static readonly string IsTypeDefinition = "IsTypeDefinition";
                public static string IsClass = "IsClass";
                public static string IsValueType = "IsValueType";

                public static string NamespaceId = "NamespaceId";
                public static string Name = "Name";
            }

            public const string Table_Name = "obj_type";
        }
    }
}

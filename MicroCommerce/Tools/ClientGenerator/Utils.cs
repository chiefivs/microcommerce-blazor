using System.Runtime.CompilerServices;

namespace ClientGenerator
{
    internal static class Extensions
    {
        private static string ApiControllerAttributeType = "Microsoft.AspNetCore.Mvc.ApiControllerAttribute";
        private static string RouteAttributeType = "Microsoft.AspNetCore.Mvc.RouteAttribute";
        private static string HttpDeleteAttributeType = "Microsoft.AspNetCore.Mvc.HttpDeleteAttribute";
        private static string HttpGetAttributeType = "Microsoft.AspNetCore.Mvc.HttpGetAttribute";
        private static string HttpPatchAttributeType = "Microsoft.AspNetCore.Mvc.HttpPatchAttribute";
        private static string HttpPostAttributeType = "Microsoft.AspNetCore.Mvc.HttpPostAttribute";
        private static string HttpPutAttributeType = "Microsoft.AspNetCore.Mvc.HttpPutAttribute";
        private static string FromBodyAttributeType = "Microsoft.AspNetCore.Mvc.FromBodyAttribute";

        public static bool IsApiControllerAttribute(this Type type)
        {
            return type.FullName == ApiControllerAttributeType;
        }

        public static bool IsRouteAttribute(this Type type)
        {
            return type.FullName == RouteAttributeType;
        }

        public static bool IsHttpDeleteAttribute(this Type type)
        {
            return type.FullName == HttpDeleteAttributeType;
        }

        public static bool IsHttpGetAttribute(this Type type)
        {
            return type.FullName == HttpGetAttributeType;
        }

        public static bool IsHttpPatchAttribute(this Type type)
        {
            return type.FullName == HttpPatchAttributeType;
        }

        public static bool IsHttpPostAttribute(this Type type)
        {
            return type.FullName == HttpPostAttributeType;
        }

        public static bool IsHttpPutAttribute(this Type type)
        {
            return type.FullName == HttpPutAttributeType;
        }

        public static bool IsHttpMethodAttribute(this Type type)
        {
            return type.IsHttpGetAttribute() || type.IsHttpPatchAttribute() || type.IsHttpPostAttribute() || type.IsHttpPutAttribute() || type.IsHttpDeleteAttribute();
        }

        public static bool IsFromBodyAttribute(this Type type)
        {
            return type.FullName == FromBodyAttributeType;
        }

        public static T GetProperty<T>(this Attribute attribute, string propertyName, T defaultValue) where T: class
        {
            var type = attribute.GetType();
            var propInfo = type.GetProperty(propertyName);
            if (propInfo == null)
                return defaultValue;

            return propInfo.GetValue(attribute) as T ?? defaultValue;
        }
    }

    public static class HttpMethods
    {
        public const string GET = "GET";
        public const string POST = "POST";
        public const string PUT = "PUT";
        public const string DELETE = "DELETE";
        public const string PATCH = "PATCH";
    }
}

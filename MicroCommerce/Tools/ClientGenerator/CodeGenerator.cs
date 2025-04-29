using System.Reflection;
using System.Text;

namespace ClientGenerator
{
    internal class CodeGenerator
    {
        private readonly Type _controllerType;
        private readonly string _ns;
        private List<string> _entryPoints = new List<string>();
        private List<string> _usings = new List<string>();
        private static Dictionary<string, string> _typeReplaces = new Dictionary<string, string>()
        {
            { typeof(Int16).Name, "short" },
            { typeof(Int32).Name, "int" },
            { typeof(Int64).Name, "long" },
            { typeof(UInt16).Name, "ushort" },
            { typeof(UInt32).Name, "uint" },
            { typeof(UInt64).Name, "ulong" },
            { typeof(String).Name, "string" },
            { typeof(Boolean).Name, "bool" },
        };

        public CodeGenerator(Type controllerType, string ns)
        {
            _controllerType = controllerType;
            _ns = ns;
        }

        public string ClassName
        {
            get { return _controllerType.Name.Replace("Controller", "") + "Client"; }
        }

        public string CreateClient()
        {
            var descriptors = MethodDescriptor.GetAll(_controllerType);
            foreach (var descriptor in descriptors)
            {
                CreateRequestMethod(descriptor);
            }

            var builder = new StringBuilder();
            builder.AppendLine($"//  Generated from {_controllerType.FullName} Don't change it!!!");
            builder.AppendLine("using System.Net.Http.Json;");
            foreach (var us in _usings)
                builder.AppendLine($"using {us};");
            builder.AppendLine("");
            builder.AppendLine($"namespace {_ns}");
            builder.AppendLine("{");
            builder.AppendLine($"\tpublic class {ClassName}");
            builder.AppendLine("\t{");

            builder.AppendLine("\t\tprivate readonly HttpClient _httpClient;");
            builder.AppendLine("");

            builder.AppendLine($"\t\tpublic {ClassName}(HttpClient httpClient)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\t_httpClient = httpClient;");
            builder.AppendLine("\t\t}");
            builder.AppendLine("");

            builder.AppendLine(string.Join("\r\n", _entryPoints));
            builder.AppendLine("\t}");
            builder.AppendLine("}");

            return builder.ToString().Replace("\t", "    ");
        }

        private void CreateRequestMethod(MethodDescriptor descriptor)
        {
            var parInfos = descriptor.MethodInfo.GetParameters();
            var parBodyInfo = parInfos.FirstOrDefault(pi => pi.GetCustomAttributes().Any(a => a.GetType().IsFromBodyAttribute()));
            var parNames = parInfos.Where(pi => !pi.GetCustomAttributes().Any(a => a.GetType().IsFromBodyAttribute())).Select(pi => pi.Name).ToList();
            var parDefs = new List<string>();
            foreach (var pi in parInfos)
            {
                parDefs.Add($"{GenerateType(pi.ParameterType)} {pi.Name}");
            }

            var returnType = GenerateType(descriptor.MethodInfo.ReturnType);
            var segments = descriptor.Segments.Select(s =>
            {
                if (s.StartsWith("{"))
                {
                    var segmPar = s.Trim('{', '}');
                    parNames.Remove(segmPar);
                }

                return s;
            });
            var urlParams = parNames.Select(pn => pn + "={" + pn + "}");

            var builder = new StringBuilder();
            var url = $"{string.Join('/', segments)}";
            if (urlParams.Any())
                url += $"?{string.Join('&', urlParams)}";

            builder.AppendLine($"\t\tpublic async {(returnType != null ? $"Task<{returnType}>" : "Task")} {descriptor.MethodName}({string.Join(", ", parDefs)})");
            builder.AppendLine("\t\t{");
            switch (descriptor.HttpMethod)
            {
                case HttpMethods.GET:
                    builder.AppendLine($"\t\t\tvar response = await _httpClient.GetAsync($\"{url}\");");
                    break;
                case HttpMethods.DELETE:
                    builder.AppendLine($"\t\t\tvar response = await _httpClient.DeleteAsync($\"{url}\");");
                    break;
                case HttpMethods.POST:
                    builder.AppendLine($"\t\t\tvar response = await _httpClient.PostAsJsonAsync($\"{url}\", {parBodyInfo?.Name ?? "new object()"});");
                    break;
                case HttpMethods.PUT:
                    builder.AppendLine($"\t\t\tvar response = await _httpClient.PutAsJsonAsync($\"{url}\", {parBodyInfo?.Name ?? "new object()"});");
                    break;
                case HttpMethods.PATCH:
                    builder.AppendLine($"\t\t\tvar response = await _httpClient.PatchAsJsonAsync($\"{url}\", {parBodyInfo?.Name ?? "new object()"});");
                    break;
                default: return;
            }

            builder.AppendLine($"\t\t\tresponse.EnsureSuccessStatusCode();");
            if(returnType != null)
            {
                    builder.AppendLine($"\t\t\treturn await response.Content.ReadFromJsonAsync<{returnType}>();");
            }

            builder.AppendLine("\t\t}");
            _entryPoints.Add(builder.ToString());
        }

        private string? GenerateType(Type type)
        {
            if (type.Name.StartsWith("Task"))
                return type.IsGenericType ? GenerateType(type.GenericTypeArguments.First()) : null;

            if(!type.FullName!.StartsWith("System.") && !_usings.Contains(type.Namespace!))
                _usings.Add(type.Namespace!);
            
            if (type.IsGenericType)
                return type.Name.StartsWith("Nullable")
                    ? $"{GenerateType(type.GenericTypeArguments.First())}?"
                    : $"{type.Name.Substring(0, type.Name.Length - 2)}<{string.Join(",", type.GenericTypeArguments.Select(GenerateType))}>";

            return _typeReplaces.ContainsKey(type.Name) ? _typeReplaces[type.Name] : type.Name;
        }
    }

    internal class MethodDescriptor
    {
        public string[] Segments;
        public string HttpMethod;
        public MethodInfo MethodInfo;
        public string MethodName => MethodInfo.Name;

        public static IEnumerable<MethodDescriptor> GetAll(Type type)
        {
            var rootSegments = GetInterfaceSegments(type);
            var descriptors = type.GetMethods()
                .Where(mtdInfo => mtdInfo.DeclaringType == type && mtdInfo.IsPublic)
                .Select(mtdInfo => new MethodDescriptor(rootSegments, type, mtdInfo));

            return descriptors;
        }

        private MethodDescriptor(string[] interfaceSegments, Type type, MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes(false);
            HttpMethod = GetHttpMethodFromAttributes(attributes) ?? GetHttpMethodFromName(methodInfo);

            var routeAttr = (attributes.FirstOrDefault(a => a.GetType().IsHttpMethodAttribute())
                ?? attributes.FirstOrDefault(a => a.GetType().IsRouteAttribute())) as Attribute;

            var segments = new List<string>();

            if (interfaceSegments != null)
                segments.AddRange(interfaceSegments);

            if (routeAttr != null)
                segments.AddRange(GetRouteSegments(routeAttr, type));
            else
                segments.Add(methodInfo.Name.ToLower());

            Segments = segments.ToArray();
            MethodInfo = methodInfo;
        }

        private static string[] GetInterfaceSegments(Type type)
        {
            var routeAttr = type.GetCustomAttributes().FirstOrDefault(a => a.GetType().IsRouteAttribute());
            return GetRouteSegments(routeAttr, type);
        }

        private static string[] GetRouteSegments(Attribute routeAttr, Type type)
        {
            var controllerName = type.Name;
            if (controllerName.EndsWith("Controller"))
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

            var template = routeAttr.GetProperty("Template", "").Replace("[controller]", controllerName);
            return template.Trim('/').Split('/').Where(s => s != string.Empty).ToArray();
        }

        private static string GetHttpMethodFromAttributes(object[] attributes)
        {
            if (attributes.Any(a => a.GetType().IsHttpDeleteAttribute()))
                return HttpMethods.DELETE;

            if (attributes.Any(a => a.GetType().IsHttpGetAttribute()))
                return HttpMethods.GET;

            if (attributes.Any(a => a.GetType().IsHttpPatchAttribute()))
                return HttpMethods.PATCH;

            if (attributes.Any(a => a.GetType().IsHttpPostAttribute()))
                return HttpMethods.POST;

            if (attributes.Any(a => a.GetType().IsHttpPutAttribute()))
                return HttpMethods.PUT;

            return null;
        }

        private static string GetHttpMethodFromName(MethodInfo mtd)
        {
            var name = mtd.Name.ToUpper();

            if (name.StartsWith(HttpMethods.DELETE))
                return HttpMethods.DELETE;

            if (name.StartsWith(HttpMethods.GET))
                return HttpMethods.GET;

            if (name.StartsWith(HttpMethods.PATCH))
                return HttpMethods.PATCH;

            if (name.StartsWith(HttpMethods.POST))
                return HttpMethods.POST;

            if (name.StartsWith(HttpMethods.PUT))
                return HttpMethods.PUT;

            return HttpMethods.GET;
        }

    }
}

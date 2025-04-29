using System.Reflection;

namespace ClientGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var keys = args.Select(x => x.Split('=')).ToDictionary(x => x[0], x => x.Length > 1 ? x[1] : string.Empty);
            foreach (var entry in keys)
            {
                Console.WriteLine($"{entry.Key}->{entry.Value}");
            }

            var tempPath = Path.Combine(Path.GetTempPath(), "ClientGenerator");
            if (keys.ContainsKey("-clean"))
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
                Console.WriteLine($"Папка {tempPath} удалена");
                return;
            }

            var projectPath = Path.GetFullPath(keys["-project"]);
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var destinationPath = Path.GetFullPath(keys["-destination"]);

            var publishDir = Path.Combine(tempPath, projectName + "_publish");
            if (Directory.Exists(publishDir))
                Directory.Delete(publishDir, true);

            Console.WriteLine($"current dir: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"project src: {projectPath}");
            Console.WriteLine($"destination: {publishDir}");

            Directory.CreateDirectory(publishDir);
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = $"publish {projectPath} -c Release -o {publishDir} --self-contained true --runtime win-x64";
                process.Start();
                process.WaitForExit();

                var assembly = Assembly.LoadFrom(Path.Combine(publishDir, $"{projectName}.dll"));
                var controllers = assembly.GetTypes().Where(t => t.GetCustomAttributes().Any(a => a.GetType().IsApiControllerAttribute()));
                foreach (var controller in controllers)
                {
                    var generator = new CodeGenerator(controller, Path.GetFileName(destinationPath));
                    var code = generator.CreateClient();
                    File.WriteAllText(Path.Combine(destinationPath, generator.ClassName + ".cs"), code);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

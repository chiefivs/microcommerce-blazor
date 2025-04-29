namespace WebUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.Services.AddRazorPages();

            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                //  when root calls, the start page will be returned
                if (string.IsNullOrEmpty(context.Request.Path.Value.Trim('/')))
                {
                    context.Request.Path = "/index.html";
                }

                await next();
            });
            app.UseStaticFiles();

            //app.UseRouting();
            //app.UseAuthorization();
            //app.MapRazorPages();

            app.Run();
        }
    }
}

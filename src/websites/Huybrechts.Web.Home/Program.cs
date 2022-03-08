using System.IO.Enumeration;
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add MVC to builder : when debugging add razor runtime debugging
#if DEBUG
    var mvcBuilder = builder.Services.AddControllersWithViews();
    mvcBuilder.AddRazorRuntimeCompilation();
#endif
    builder.Services.AddRazorPages();
    builder.WebHost.UseWebRoot("wwwroot");
    builder.WebHost.UseStaticWebAssets();

    var app = builder.Build();

    app.UseStaticFiles();
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapRazorPages();
    });

    app.Run();
}
catch (System.Exception ex)
{
    
    throw;
}
finally
{

}

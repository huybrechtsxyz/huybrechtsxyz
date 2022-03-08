# Creating a home page
Welcome back. In the previous post [clicky](001_Starting_a_new_application.md) we made sure our environment and machine were ready to start building an application. In this post we will create a basic home page using ASP.Net Core MVC and Bootstrap.

## Way of Thinking
We are going to build our home page. To put it plainly. The first impression is important. That is true for people and for applications / websites. Most of your visitors will see and interact with your website for the first time through this page. Make it count, give your application the showcase it deserves. Introduce the world to our new application with this static home page. Make sure the page capture what you are going to implement in the future, what can the potential customer expect?
By starting out with a static home page we try to avoid the ‘but this feature is not in it yet’ paralysis. We want to let the world know about our application. We will add functionality to the application along the way. You want feedback on what we have, even if it is only a home page. Each time we add a new feature we want it in the hands of the customer as soon as possible.

## Way of Working
We start by creating an empty ASP.Net core application and will add the required components along the way. We want a secure and fast home page, that is easy to create and maintain. Let us start with something small. Either you already know what your application will do, then go ahead and create the page that show cases your desired functionality. If you have no clue, you can create a personal home page. Use it to give your CV / resume a boost with a personal website.

We use Bootstrap (and jQuery) for our home page. Bootstrap and MVC are popular frameworks to building web applications. Bootstrap contains web-components, visual elements on the page, that you can use to build content and layout to your webpage. At this moment in time, we are building a fixed webpage. We will add dynamic content in the future.

At the end we commit our code to our GitHub repository and update our project status.

## Way of Writing
### GitHub Project and Issue
Open GitHub and go to your projects. If you not already have done so, create a project for this application. 
1. Go to your avatar and open the menu, select your projects
2. Create a new project, give the project a meaningful name and save it

Then in GitHub go to application's repository.
1. Go to the issues tab
2. Create a new issue
3. Issue name: Create a home page
4. Select the appropriate tags and project for the issue
5. Save the issue

### Root folder structure
You will notice that the issue is automatically added to the project. Let’s start working on the application. Open VSCode and open the folder you cloned from GitHub. You should see the root directory with 3 files: gitignore, license, and readme. Create a folder 'src' and subfolder 'websites', with this result:

```ps1
> {root}
> ---- src
> -------- websites
```

### ASP.Net Solution and Project
In the websites directory create an empty home application using the command line. Try building the application using, make sure your working directory points to your new project. And you can run the project. If you open your web browser on the port displayed on the terminal, you should see 'Hello World'.

```ps1
> dotnet new web --name {namespace}.Home
> dotnet build
> dotnet run
```

As this is not the only project of our solution, we are going to create a blank solution in the root directory and add this project.

```ps1
> cd {go to root}
> dotnet new sln --name {solution name}
> dotnet sln add {path to new csproj}
```

### Adding Bootstrap
Great, we now have a working web application. Granted. Showing 'Hello World' was probably not the feature set of your dreams. Let us start by updating the project to get a more visual result. We start by adding bootstrap to our project. Bootstrap can be found at:
> https://getbootstrap.com/ 

Update the home project folder structure.
```ps1
> {root}
> ---- src
> -------- websites
> ------------ {home project}
> ---------------- wwwroot
> -------------------- lib
> ------------------------ bootstrap
> --------------------------- css
> --------------------------- js
```
Unzip the download and move the contents of the downloaded folder to the new bootstrap folder in your project. CSS and JS folder should be overwritten with the content of the download.

### Adding MVC (Model View Controller)
MVC is a part of the ASP.Net core SDK and can easily be added to our application. You will need to update Program.cs to add the services and configuration. To debug the pages using hot reload we will need to add specific NuGet package to our project.
```ps1
cd {path to home page folder}
dotnet add package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation --version 6.0.2
```

*Program.cs*
```C#
var builder = WebApplication.CreateBuilder(args);
// Start: between CreateBuilder and app = builder.Build add
#if DEBUG
var mvcBuilder = builder.Services.AddControllersWithViews();
mvcBuilder.AddRazorRuntimeCompilation();
#endif
builder.Services.AddRazorPages();
builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();
// End: between CreateBuilder and app = builder.Build add
var app = builder.Build();
// Start: between app = builder.Build and app.Run add
app.UseStaticFiles();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});
// app.MapGet("/", () => "Hello World!"); -> you can delete this line
// End: between app = builder.Build and app.Run add
app.Run();
```

### Add index page
Situation. ASP.Net understands we will be using MVC and Bootstrap (with jQuery). This is the moment we will be building our home page.
Update the home project folder structure.
```ps1
> {root}
> ---- src
> -------- websites
> ------------ {home project}
> ---------------- Pages
```

Add the following files to the Pages directory:
*_ViewStart.cshtml*
```c#
@{
}
```

*_ViewImports.cshtml*
```c#
@using {project namespace}
@namespace {project namespace}.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

*Index.cshtml.cs*
```c#
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace {project namespace}.Pages
{
    public class IndexModel : PageModel
    {
		public void OnGet()
        {

        }
    }
}
```

*Index.cshtml* (Check the path to the bootstrap file!)
```html
@page
@model Huybrechts.Website.Pages.IndexModel
@{
	Layout = null;
}
<!DOCTYPE html>
<html lang="en">
<head>
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge">
	<meta name="viewport" content="width=device-width, initial-scale=1.0" />
	<meta name="description" content="My website: add a description for search engines.">
	<title>Welcome</title>
	<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" asp-append-version="true" media="screen"/>
</head>
<body>
    <script src="~/lib/bootstrap/dist/js/bootstrap.min.js"></script>
</body>
</html>
```

### Build and run
Build and run your new solution. It should automatically open your Index page.
```ps1
> dotnet new web --name {namespace}/home
> dotnet build
> dotnet run
```
Once you checked your new home page. Stop the service.

### Commit
Congratulations. You now have an application. We need to save our progress to this project. Save the changes to our source code and upload them to GitHub.
1. Make sure all files are saved and closed in the VSCode windows.
2. Go to the source control in VSCode
3. At the top in the Message textbox type: #{number of the isse on github} Creation of home website
4. Press the checkmark (Commit) - Select Save all and commit if needed
5. Press the refresh (next to Commit) to synchronize your repository

If this is the first time you commit, you will have to enter your credentials for Github.

End of post.

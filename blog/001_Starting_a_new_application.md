# Starting a new application
Welcome to this blog. In this series we are going to build a web application with ASP.Net Core. It is not important to us at this moment what this application will do. We are more interested in the journey then the functionality at this moment. Feel free to create your own functionality. As there is no revenue model for this blog series or the application, cost will be a factor in our design decisions. We will try to choose technologies that have a free tier where possible.
Ready? Let’s go!

## Way of Thinking
Building an enterprise application requires more then just adding functionality. Although important for your users, we as developers of the system have our own requirements we want to fulfill. Users need to login before viewing or changing information. We want the application to be manageable and secure, know what is running and what is down. Updates to the application should be easily deployed. Metrics will tell us what part of the application is visited the most. We will need to think about these and other non-functional requirements while creating our application.

## Way of Working
We will start by getting our system ready for developing the application. Then we will build a skeleton application. A skeleton provides the basic building blocks, without focusing on the functionality of the application. The goal of this approach is to create a a structure which we can use to build the functionality on.

Let’s get our machine ready to develop. The easiest way is to have all tools available on your machine. In this series we will take the viewpoint that we have a low-end machine, so low end even docker will not run on it. It would of course be a lot easier if you had docker desktop (or equivalent) installed, so do so if you can.

A first building block is your development software (or Integrated Development Environment - IDE). We are going to use Visual Studio Code (or VSCode). Visual Studio Code is a code editor in layman’s terms. It is “a free-editor that helps the programmer write code, helps in debugging and corrects the code using the intelli-sense method” - Microsoft. In normal terms, it facilitates users to write the code in an easy manner. It has multiple extensions that can be used to enhance your experience and the functionality of the editor. It is cross-platform, so it works on Windows, Linux, or Mac systems.

The next building block is a source code repository with version control. Git has become the worldwide standard for version control. Git is a distributed version control system. This means that a local copy of the project is the complete repository. These fully functional local copies or repositories make it is easy to work offline. Developers commit their work locally, and then sync their copy of the repository with the copy on the server. This paradigm differs from centralized version control where clients must synchronize code with a server before creating new versions of code.

The last building block before we can get started is a place to save our code. Just saving the repository on your local machine is not save. Disks crash. Most of the time when you forgot to make that backup. The last piece of the puzzle is GitHub. GitHub is a code hosting platform for collaboration and version control and provides a place to store our Git repository. It is a website and cloud-based service that helps developers store and manage their code, as well as track and control changes to their code.

The last item on the list for this post is to clone a new repository from repository from GitHub.com to your local computer to make it easier to fix merge conflicts, add or remove files, and push larger commits. When you clone a repository, you copy the repository from GitHub.com to your local machine. Cloning a repository pulls down a full copy of all the repository data that GitHub.com has at that point in time, including all versions of every file and folder for the project. You can push your changes back to the remote repository on GitHub.com. Now you have a secure backup in case something goes wrong on your machine, or for other people to pull your changes from GitHub.com.

## Way of Writing
### Git
First we install Git, download and install:
* For Windows, go to the website and download and install the software.
  Once installed, Git will be available from the command prompt or PowerShell.
  It is recommended that the defaults are selected during installation.
  The software will not automatically update!
    > https://git-scm.com/download/win

* For macOS, it is recommended that Git be installed through Homebrew
  Download and install Homebrew, then install/update git using the command line
    > http://brew.sh/ \
    > brew install git
    > brew upgrade git

* For Linux, use the Linux distribution's native package management system
  For example, on Ubuntu:
    > sudo apt-get install git

### .Net Core
To install .net core, visit the site, download and install the SDK (Software Development Kit). At the time of writing the LTS (long time supported) version of .Net core is version 6.
> https://dotnet.microsoft.com/en-us/download

### VS Code
To install VSCode, select the appropriate version, download it, and complete the installation
> https://code.visualstudio.com/

## GitHub
Open your web browser and got to the website and sign-in or create your account.
> https://github.com

Create a new repository, don't forget to add (don't worry, you can change those options later):
* Read Me
* License
* Git ignore

No we need to clone the repository. Select a place on your harddrive (below c:\users\{your username}\).
1. On GitHub.com, navigate to the main page of the repository.
2. Above the list of files, click 'Code'.
3. "Clone with HTTPS", click Copy icon.
4. Open VSCode, on the Explorer tab select Clone Repository (no folders should be opened)
5. Choose clone from GitHub and follow the procedure, select your chosen location on your harddrive.

You do not have to create the subfolder: the system will automatically create a folder with the name of the repository in the chosen folder.

### Docker Desktop
As we comply to the license agreement to use Docker for free (check before use!), we could install Docker on our local machine. Again on the internet go to the website, download the appropriate version and install it.
> https://www.docker.com/get-started

Choose Linux when asked about the operating system for your containers.

## Summary
Great work, we now have software to develop our software in. A system to keep track of the changes and a place to store our result. In the next post, we will get started.

End of post.
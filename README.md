# gh-users-tat

A simple ASP.NET MVC WebApp that searches for a user on GitHub and returns some basic info.

This project uses .NET Framework 4.8/MVC 5, but introduces the modern .NET Core dependency injection pattern.

# Quick Start - Visual Studio 2022

To run the project locally after cloning the repo:

1. Before opening the project in Visual Studio, create a copy of the `GhUsersTat\Web.example.config` file in the same folder, named `Web.config`.
2. Open the new `Web.config` file and edit the value of `githubApiKey` to one of your own tokens. You can generate a new token [here](https://github.com/settings/tokens/new).
3. You should now be able to open and run the project (F5).

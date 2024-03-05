using Huybrechts.App.Common;
using Huybrechts.App.Data;
using System.CommandLine;

namespace Huybrechts.App.Config;

public class CommandLineOptions
{
	public DatabaseProviderType DatabaseProviderType { get; private set; }

	public bool IsError => Exception is null;

	public Exception? Exception { get; private set; }

	public int Interpret(string[] args)
	{
		DatabaseProviderType = DatabaseProviderType.None;

		if (args.Length == 0)
			return 0;

		if (args[0].Equals("--provider", StringComparison.CurrentCultureIgnoreCase))
		{
			if (Enum.TryParse(typeof(DatabaseProviderType), args[1], true, out var result))
			{
				DatabaseProviderType = (DatabaseProviderType)result;
			}
		}

		return 0;

		//var databaseProviderOption = new Option<DatabaseProviderType?>(
		//   name: "--provider",
		//   description: "Entity Framework Core can access many different databases through plug-in libraries called database providers.",
		//   getDefaultValue: () => DatabaseProviderType.None);

		//var applicationNameOption = new Option<string?>(
		//	name: "--applicationName",
		//	description: "Full assembly name",
		//	getDefaultValue: () => String.Empty)
		//{
		//	//AllowMultipleArgumentsPerToken = true,
		//};

		//var rootCommand = new RootCommand("Huybrechts.xyz");
		//rootCommand.AddOption(databaseProviderOption);
		//rootCommand.AddOption(applicationNameOption);

		//rootCommand.SetHandler((databaseProviderType) =>
		//{
		//	this.DatabaseProviderType = databaseProviderType ?? DatabaseProviderType.None;
		//},
		//databaseProviderOption);

		//return await rootCommand.InvokeAsync(args);
	}
}

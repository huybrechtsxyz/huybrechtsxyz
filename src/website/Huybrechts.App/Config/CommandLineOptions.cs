using Huybrechts.App.Common;
using Huybrechts.App.Data;
using System.CommandLine;

namespace Huybrechts.App.Config;

public class CommandLineOptions
{
    public DatabaseProviderType DatabaseProviderType { get; private set; }

	public string ConnectionString { get; private set; } = string.Empty;

	public bool IsError => Exception is null;

	public Exception? Exception { get; private set; }

	public int Interpret(string[] args)
	{
		DatabaseProviderType = DatabaseProviderType.None;
		ConnectionString = string.Empty;
		string provider = "--provider";
		string connection = "--connection";

		if (args.Length == 0)
			return 0;

		var values = args.Select(s => s.Trim().ToLower()).ToList();

		int pos = values.IndexOf(provider);
		if (pos >= 0 && values.Count >= pos + 1)
		{
			if (Enum.TryParse(typeof(DatabaseProviderType), args[pos + 1], true, out var result))
			{
				DatabaseProviderType = (DatabaseProviderType)result;
			}
		}

		pos = values.IndexOf(connection);
		if (pos >= 0 && values.Count >= pos + 1)
		{
			ConnectionString = args[pos + 1];
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

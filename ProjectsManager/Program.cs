//imports
using Microsoft.Extensions.Configuration;
using ProjectsManager.Models;
using Serilog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProjectsManager
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Initialize an instance of the Methods class, presumably for utility functions
            Methods methods = new Methods();
            // Configure application settings using a ConfigurationBuilder
            var builder = new ConfigurationBuilder()
                              .SetBasePath(AppContext.BaseDirectory) // Use AppContext.BaseDirectory for consistent path resolution
                              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Check for an environment-specific configuration file (e.g., appsettings.Development.json)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (!string.IsNullOrEmpty(environment))
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
            }
            // Build the final configuration
            IConfiguration configuration = builder.Build();

            // Set up logging using Serilog
            // Create a "logs" directory if it doesn't exist
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Verbose()
                  .WriteTo.File(Path.Combine(logsDirectory, "Log_.txt"), rollingInterval: RollingInterval.Day)
                  .CreateLogger();


            //OS information
            var CurrentOS = Environment.OSVersion;
            string CurrenteOSUser = Environment.UserName;
            string CurrentOSDomain = Environment.UserDomainName;
            string CurrentMachineName = Environment.MachineName;

            //OS Path
            string projectRootPath = AppContext.BaseDirectory;  // Get the root directory of your project           
            string documentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            //Lang
            CultureInfo CurrentCulture = CultureInfo.CurrentCulture;
            string OSCultureName = CurrentCulture.DisplayName;
            string OSCultureCode = CurrentCulture.TwoLetterISOLanguageName;

            Root rootPathAndFolders = new();
            List<string> AccountOrganization = new();
            string OrgName = string.Empty;

            string configFilePath = Path.Combine(projectRootPath, "appsettings.json"); // Construct the full path to appsettings.json

            // Deserialize configuration data from appsettings\.json
            using (StreamReader reader = new StreamReader(configFilePath))
            {
                string json = reader.ReadToEnd();
                rootPathAndFolders = JsonSerializer.Deserialize<Root>(json) ?? new();

                Console.WriteLine($"GitHub Name: {rootPathAndFolders.GitConfig.Name}");
                Console.WriteLine($"GitHub Account: {rootPathAndFolders.GitConfig.Account}");
                Console.WriteLine($"GitHub Email: {rootPathAndFolders.GitConfig.Email}");
            }
            // Initialize lists for repositories
            List<Repos> RepoList = new();
            List<Repos> OrganizationRepoList = new();
            // Set command executable paths
            string CmdExecutable = "cmd.exe";
            string GitHubExecutable = "gh.exe";

            string GitHubExecutableDirectory = @"C:\Program Files\GitHub CLI\";
            string CmdExecutableDirectory = @"C:\Windows\System32\";
            // Construct GitHub command lines
            string GitHubRepoCommandLine = $"repo list {rootPathAndFolders.GitConfig.Account} --json name,description,visibility --limit 100";
            string GitHubOrganizationList = "org list";
            
            //string GitHubStatusCommandLine = " auth status";
            //string GitHubSCommandLogin = " auth login";
            //string GitCheckCommandLine = "/c git --version";
            //string GitHubCheckCommandLine = "/c gh --version";
            //string GitInstallerCommandLine = "/c winget install --id Git.Git -e --source winget";
            //string GitHubInstallerCommandLine = "/c winget install --id GitHub.cli -e --source winget";
            //string GitConfigUserName = "/c git config --global user.name ";
            //string GitConfigUserEmail = "/c git config --global user.email ";
            //string GitCheckConfig = "/c git config --global --list";
         
            try
            {
                // Obtain repository information using the Methods class
                RepoList = methods.ObtainRepositoriesNames(GitHubExecutable, GitHubRepoCommandLine, GitHubExecutableDirectory);

                // Get organization information and clean up the result
                string Result = Regex.Replace(methods.ExcecuteProcces(GitHubExecutable, GitHubOrganizationList, GitHubExecutableDirectory), @"\r\n?|\n", "");
                // If organizations exist, get their repository information
                AccountOrganization.Add(Result);
                if (AccountOrganization.Count() != 0) 
                {
                    OrgName = AccountOrganization.First();
                    string GitHubOrganizationRepoList = $"repo list {OrgName} --json name,description,visibility --limit 100";

                    OrganizationRepoList = methods.ObtainRepositoriesNames(GitHubExecutable, GitHubOrganizationRepoList, GitHubExecutableDirectory);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log.Error(ex, "Error:");
            }
            //FolderStructureCreation
            try
            {
                Console.WriteLine("Evaluating existing structure and creating new folders...");
                // Find the "Documents" folder path 

                int i = 0;

                foreach (var Folders in rootPathAndFolders.Folders)
                {
                    // Construct the full folder path within the "Documents" folder
                    string fullFolderPath = Path.Combine(documentsFolderPath, Folders.Path);

                    if (!Directory.Exists(fullFolderPath))
                    {
                        Console.WriteLine($"Creating folder: {fullFolderPath}"); // Added for clarity

                        Directory.CreateDirectory(fullFolderPath);

                        i++;
                    }
                }
                foreach (var OrgFolder in AccountOrganization) 
                {
                    // Construct the full folder path within the "Documents" folder
                    string fullFolderPath = Path.Combine(documentsFolderPath, $"Develop\\CustomerProjects\\{OrgFolder}");

                    if (!Directory.Exists(fullFolderPath))
                    {
                        Console.WriteLine($"Creating folder: {fullFolderPath}"); // Added for clarity
                        Directory.CreateDirectory(fullFolderPath);
                        i++;
                    }
                }

                if (i > 0)
                    Console.WriteLine($"{i} folders were created.");
                else
                    Console.WriteLine("The bas structure folders already exist.");                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log.Error(ex, "Error:");
            }
            // Clone or sync personal repositories
            try
            {
                Console.WriteLine("Cloning or syncing Personal GitHub repositories...");
                RepoList.ForEach(x =>
                {
                    var Folder = rootPathAndFolders.Folders.Where(y => y.Name == (!string.IsNullOrWhiteSpace(x.description) ? x.description : y.Name)).First();

                    // Use the "Documents" folder path in the repo cloning path
                    string FolderPath = Path.Combine(documentsFolderPath, Folder.Path, x.name);

                    if (!Directory.Exists(FolderPath))
                    {
                        string CloneScript = $@"repo clone {x.name} {FolderPath}";
                        try
                        {
                            var result = methods.ExcecuteProcces(GitHubExecutable, CloneScript, GitHubExecutableDirectory);
                            Console.WriteLine(result);
                            Console.WriteLine("Waiting....");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Log.Error(ex, "Error:");
                        }
                    }
                    else 
                    {                        
                        string SyncScript = $"/c cd \"{FolderPath}\" && gh repo sync"; // Fixed command
                        try
                        {
                            var SyncRepo = methods.ExcecuteProcces(CmdExecutable, SyncScript, CmdExecutableDirectory);
                            Console.WriteLine($"Repo: {x.name}, Sync Status: {SyncRepo}");
                            Console.WriteLine("Waiting....");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Log.Error(ex, "Error:");
                        }
                    }

                });
                Console.WriteLine("Ending...");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
                Log.Error(ex, "Error:");
            }
            // Clone or sync organization repositories
            try
            {
                Console.WriteLine("Cloning or syncing Customer or Teams GitHub repositories...");
                OrganizationRepoList.ForEach(x =>
                {
                    var Folder = rootPathAndFolders.Folders.Where(y => y.Name == OrgName).First();

                    // Use the "Documents" folder path in the repo cloning path
                    string FolderPath = Path.Combine(documentsFolderPath, Folder.Path, x.name);
                    Console.WriteLine(FolderPath);

                    if (!Directory.Exists(FolderPath))
                    {
                        string CloneScript = $"repo clone {OrgName}/{x.name} {FolderPath}";
                        Console.WriteLine($"script : {CloneScript}");

                        Console.WriteLine(CloneScript);
                        try
                        {
                            var result = methods.ExcecuteProcces(GitHubExecutable, CloneScript, GitHubExecutableDirectory);
                            Console.WriteLine(result);
                            Console.WriteLine("Waiting....");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Log.Error(ex, "Error:");
                        }
                    }
                    else 
                    {
                        string SyncScript = $"/c cd \"{FolderPath}\" && gh repo sync"; // Fixed command
                        try
                        {
                            var SyncRepo = methods.ExcecuteProcces(CmdExecutable, SyncScript, CmdExecutableDirectory);
                            Console.WriteLine($"Repo: {x.name}, Sync Status: {SyncRepo}");
                            Console.WriteLine("Waiting....");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Log.Error(ex, "Error:");
                        }
                    }
                });
                Console.WriteLine("Ending...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log.Error(ex, "Error:");
            }
        }
    }
}
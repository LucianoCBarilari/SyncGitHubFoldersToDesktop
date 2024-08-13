using ProjectsManager.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectsManager
{
    public class Methods
    {
        string PatternVersion = @"(?<name>\b\w+\b) version (?<version>\d+\.\d+\.\d+)";

        /// <summary>
        /// Executes a process with the specified executable and command-line arguments in the given directory.
        /// </summary>
        /// <param name="Executable">The name or path of the executable file to run.</param>
        /// <param name="CommandLine">The command-line arguments to pass to the executable.</param>
        /// <param name="LaunchDirectory">The working directory in which to start the process.</param>
        /// <returns>The standard output of the process if successful, the error output if there was an error, or "Error" if something went wrong.</returns>
        public string ExcecuteProcces(string Executable, string CommandLine, string LaunchDirectory)
        {
            string Output = string.Empty;  // Stores the standard output of the executed process
            string ErrorOutput = string.Empty; // Stores any error output from the process

            // Configure the Process to be executed
            Process CurrentProcess = new();
            CurrentProcess.StartInfo.FileName = Executable;          // The executable file to run (e.g., "cmd.exe")
            CurrentProcess.StartInfo.Arguments = CommandLine;       // Command-line arguments for the executable
            CurrentProcess.StartInfo.WorkingDirectory = LaunchDirectory; // The directory to start the process in

            // Set process options for better control and output handling
            CurrentProcess.StartInfo.UseShellExecute = false;        // Don't use the shell to start the process
            CurrentProcess.StartInfo.RedirectStandardOutput = true;  // Capture standard output
            CurrentProcess.StartInfo.RedirectStandardError = true;   // Capture standard error
            CurrentProcess.StartInfo.CreateNoWindow = true;          // Don't show a console window

            try
            {
                CurrentProcess.Start();                             // Start the process

                // Read the standard output of the process
                Output = CurrentProcess.StandardOutput.ReadToEnd();

                // Read the error output (optional, but useful for debugging)
                ErrorOutput = CurrentProcess.StandardError.ReadToEnd();

                // Wait for the process to finish before returning
                CurrentProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during execution
                Console.WriteLine(ex.ToString()); // Print the exception details
                Log.Error(ex, "Error:");
            }

            // Return the appropriate output
            // If there was standard output, return it
            // If there was only error output, return that
            // Otherwise, return "Error" to indicate something went wrong
            return !string.IsNullOrWhiteSpace(Output) ? Output : !string.IsNullOrWhiteSpace(ErrorOutput) ? ErrorOutput : "Error";
        }

        /// <summary>
        /// Checks if Git is installed and configures global user settings if necessary.
        /// </summary>
        /// <summary>
        /// Checks for Git installation, installs it if missing, and configures global user settings (name and email) if not already set.
        /// </summary>
        /// <param name="CmdExe">The executable for the command prompt or terminal.</param>
        /// <param name="GitCheckCommand">The command to check the Git version.</param>
        /// <param name="GitInstallerCommand">The command to install Git if it's not found.</param>
        /// <param name="GitCheckC">The command to check the Git configuration.</param>
        /// <param name="CmdExeDir">The directory where the command prompt executable is located.</param>
        /// <param name="RootPathAndFolder">An object containing information about the root path and folder structure relevant to Git configuration.</param>
        /// <param name="SetGitConfigUserName">The command to set the global Git username.</param>
        /// <param name="SetGitConfigUserEmail">The command to set the global Git user email.</param>
        public void GitChecker(string CmdExe, string GitCheckCommand, string GitInstallerCommand, string GitCheckC, string CmdExeDir, Root rootPathAndFolders, string SetGitConfigUserName, string SetGitConfigUserEmail)
        {
            try
            {
                string Result = ExcecuteProcces(CmdExe, GitCheckCommand, CmdExeDir);

                Match RegexMatch = Regex.Match(Result, PatternVersion);
                bool isGitInstalled = RegexMatch.Success;

                if (!isGitInstalled)
                {
                    Console.WriteLine("Installing Git...");
                    Console.WriteLine("Downloading Git installer, wait please...");

                    try
                    {
                        string GitInstaller = ExcecuteProcces(CmdExe, GitInstallerCommand, CmdExeDir);
                        Console.WriteLine(GitInstaller);

                        // Wait for the installation to complete (adjust time if needed)
                        Thread.Sleep(5000);

                        // Continue with configuration after installation (no restart needed)
                        Console.WriteLine("Configuring Git...");
                        string GitConfigUser = ExcecuteProcces(CmdExe, $"{SetGitConfigUserName}\"{rootPathAndFolders.GitConfig.Name}\"", CmdExeDir);
                        string GitConfigemail = ExcecuteProcces(CmdExe, $"{SetGitConfigUserEmail}\"{rootPathAndFolders.GitConfig.Email}\"", CmdExeDir);

                        Console.WriteLine($"GitConfigUser: {GitConfigUser}");
                        Console.WriteLine($"GitConfigemail: {GitConfigemail}");

                        Console.WriteLine("Git successfully installed and configured.");
                    }
                    catch (Exception installEx)
                    {
                        Console.WriteLine($"Error during Git installation: {installEx.Message}");
                        Log.Error(installEx, "Error during Git installation: {ErrorMessage}", installEx.Message);
                    }
                }
                else
                {
                    string CheckGitConfig = ExcecuteProcces(CmdExe, GitCheckC, CmdExeDir);

                    bool ConfigExist = CheckGitConfig.Contains(rootPathAndFolders.GitConfig.Name) && CheckGitConfig.Contains(rootPathAndFolders.GitConfig.Email);

                    if (!ConfigExist)
                    {
                        Console.WriteLine($"Setting Git global config to : {rootPathAndFolders.GitConfig.Name}");

                        string GitConfigUser = ExcecuteProcces(CmdExe, $"{SetGitConfigUserName}\"{rootPathAndFolders.GitConfig.Name}\"", CmdExeDir);
                        string GitConfigemail = ExcecuteProcces(CmdExe, $"{SetGitConfigUserEmail}\"{rootPathAndFolders.GitConfig.Email}\"", CmdExeDir);

                        Console.WriteLine($"GitConfigUser: {GitConfigUser}");
                        Console.WriteLine($"GitConfigemail: {GitConfigemail}");
                    }
                }

                // More specific output message
                if (isGitInstalled)
                {
                    Console.WriteLine("Git is already installed.");
                }
                else
                {
                    Console.WriteLine("Git was successfully installed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred in GitChecker: {ex.Message}");
                Log.Error(ex, "An unexpected error occurred in GitChecker");
            }
        }


        /// <summary>
        /// Checks for GitHub CLI installation, installs if missing, handles authentication, and retrieves a list of repositories.
        /// </summary>
        /// <param name="CmdExe">The command prompt executable (e.g., "cmd.exe").</param>
        /// <param name="CmdExeDir">The directory where the command prompt executable is located.</param>
        /// <param name="GitHubCheckCommand">The command to check the GitHub CLI version.</param>
        /// <param name="GitInstallerCommand">The command to install the GitHub CLI if it's missing (this seems to be a typo, it should probably reference the GitHub CLI installer).</param>
        /// <param name="GitHubAuth">The command to check GitHub authentication status.</param>
        /// <param name="GitHubAuthLogin">The command to log in to GitHub.</param>
        /// <param name="GHExe">The GitHub CLI executable (e.g., "gh.exe").</param>
        /// <param name="GHExeDir">The directory where the GitHub CLI executable is located.</param>
        /// <param name="GitHubRepoCommand">The command to list GitHub repositories.</param>   
        public void GitHubChecker(string CmdExe, string CmdExeDir, string GitHubCheckCommand, string GitInstallerCommand, string GitHubAuth, string GitHubAuthLogin, string GHExe, string GHExeDir, string GitHubRepoCommand)
        {
            try
            {
                string Result = ExcecuteProcces(CmdExe, GitHubCheckCommand, CmdExeDir);

                Match RegexMatch = Regex.Match(Result, PatternVersion);
                bool isGitHubInstalled = RegexMatch.Success;

                if (!isGitHubInstalled)
                {
                    Console.WriteLine("Installing GitHub CLI...");
                    Console.WriteLine("Downloading GitHub CLI installer, wait please......"); // More specific message

                    try
                    {
                        string GitHubInstaller = ExcecuteProcces(CmdExe, GitInstallerCommand, CmdExeDir);
                        Console.WriteLine(GitHubInstaller);

                        // Wait for the installation to complete (adjust time if needed)
                        Thread.Sleep(5000);

                        // Restart the application in a new console window
                        string PathExe = Process.GetCurrentProcess().MainModule.FileName;
                        ProcessStartInfo startInfo = new ProcessStartInfo(PathExe);
                        startInfo.CreateNoWindow = false;
                        startInfo.UseShellExecute = true;
                        Process.Start(startInfo);

                        // Exit the current process
                        Environment.Exit(0);
                    }
                    catch (Exception installEx)
                    {
                        Console.WriteLine($"Error during GitHub CLI installation: {installEx.Message}");
                        Log.Error(installEx, "Error during GitHub CLI installation: {ErrorMessage}", installEx.Message);
                    }
                }
                else
                {
                    string CheckGitHubAuth = ExcecuteProcces(GHExe, GitHubAuth, GHExeDir);

                    if (CheckGitHubAuth != null && CheckGitHubAuth.Contains("Active account: true"))
                    {
                        Console.WriteLine("Credentials verified. Account status: Active.");
                        Console.WriteLine("Now consulting GitHub for the list of repositories.");

                    }
                    else
                    {
                        Console.WriteLine("Account not active. Attempting to log in...");

                        // Ejecutar el comando de inicio de sesión
                        string loginResult = ExcecuteProcces(GHExe, GitHubAuthLogin, GHExeDir); // Reemplaza con tu comando real
                        Console.WriteLine(loginResult);

                        // Obtiene la ruta completa del ejecutable actual
                        string PathExe = Process.GetCurrentProcess().MainModule.FileName;

                        // Inicia un nuevo proceso de la misma aplicación en una nueva ventana de consola
                        ProcessStartInfo startInfo = new ProcessStartInfo(PathExe);
                        startInfo.CreateNoWindow = false; // Asegúrate de mostrar la ventana de la consola
                        startInfo.UseShellExecute = true;  // Usa el shell para iniciar el proceso

                        Process.Start(startInfo);

                        // Cierra el proceso actual
                        Environment.Exit(0);
                    }
                }
                Console.WriteLine($"GitHub CLI version already installed or successfully installed. {Result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log.Error(ex, "An unexpected error occurred in GitHubChecker");
            }
        }


        public List<Repos> ObtainRepositoriesNames(string Exe, string Command, string ExeDir)
        {
            List<Repos> TemporalList = new();
            try
            {
                var Temp = ExcecuteProcces(Exe, Command, ExeDir);
                TemporalList = JsonSerializer.Deserialize<List<Repos>>(Temp) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log.Error(ex, "An unexpected error occurred obtaining the repositories.");
            }
            return TemporalList;
        }
    }
}
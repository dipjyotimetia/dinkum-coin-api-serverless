using System.IO;
using System.IO.Compression;
using Amazon.SecurityToken.Model;
using Build.Settings;
using CrownBet.Build;
using CrownBet.Build.Aws;
using Nuke.Core;
using Nuke.Core.IO;
using Nuke.Core.Tooling;
using Nuke.Common.Tools.DotNet;

           

using static Nuke.Common.Tools.DotNet.DotNetTasks;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Build.Targets
{
    public partial class Build : NukeBuild
    {
        [Parameter("The file to write the build version to (" + nameof(Export_Build_Version) + " target only)")] public string BuildVersionFilePath;

        private string _buildVersion;
        private GlobalSettings _globalSettings;

        public Target Clean => _ => _
              .Description("Remove previous build output")
              .Executes(() => FileSystemTasks.DeleteDirectory(Settings.BuildOutputDirectory));

        public Target Compile => _ => _
             .DependsOn(Clean)
             .Description("Build all projects in the solution")
             .Executes(() => DotNetBuild(SolutionDirectory));

        public Target Export_Build_Version => _ => _
             .Description("Outputs the build version to a file")
             .Requires(() => BuildVersionFilePath)
             .Executes(() => File.WriteAllText(BuildVersionFilePath, GetBuildVersion()));

        public Target Package => _ => _
             .Description("Package the application")
             .DependsOn(Test)
             .Executes(() =>
             {
                 ProcessTasks.StartProcess(
                     DotnetPath, $"publish src/DinkumCoin.Wallet.Lambda/DinkumCoin.Wallet.Lambda.csproj -c Release /p:Version=\"{GetBuildVersion()}\" -o \"{Settings.PublishDirectory}\"", RootDirectory).AssertZeroExitCode();
                 Directory.CreateDirectory(Settings.PackageDirectory);

                 if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                 {
                     ZipFile.CreateFromDirectory(Settings.PublishDirectory, Settings.PackageDirectory / $"DinkumCoin.Api.Wallet.Lambda_{GetBuildVersion()}.zip");
                 }
                 else
                 {
                    BundleWithZipCLI("zip", Settings.PackageDirectory / $"DinkumCoin.Api.Wallet.Lambda_{GetBuildVersion()}.zip", Settings.PublishDirectory, false);

                 }

             });

        public Target Test => _ => _
             .Description("Perform all unit tests")
             .DependsOn(Compile)
             .Executes(() =>
             {
                 //DotNetTest(settings => settings
                 //      .SetProjectFile(Settings.TestDirectory / "DinkumCoin.Data.Tests")
                 //.SetNoBuild(true));
                 DotNetTest(settings => settings
                            .SetProjectFile(Settings.TestDirectory / "DinkumCoin.Wallet.Lambda.Tests")
                            .SetLogger("xunit;LogFilePath=TestResults.xml")
                         .SetNoBuild(true));
                 

             });

        public Target Upload => _ => _
             .Description("Upload application package to S3")
             .DependsOn(Package)
             .Executes(() =>
             {
                 Credentials credentials = Sts.AssumeRole(GlobalSettings.BucketWriteRoleArn, "ci-build").Result;
                 S3.UploadDirectory(Settings.PackageDirectory, GlobalSettings.BucketName, GetBuildVersion(), credentials: credentials).Wait();
             });


        private string DotnetPath { get; } = new DotNetSettings().ToolPath;

        private GlobalSettings Settings => _globalSettings = _globalSettings ?? new GlobalSettings(RootDirectory);

        public static int Main() => Execute<Build>(x => x.Package);

        private string GetBuildVersion()
        {
            if (_buildVersion != null) { return _buildVersion; }

            string branch = Git.GetBranchName(RootDirectory);

            branch = branch == "master" ? "" : "-" + branch.Replace("/", "-");

            return _buildVersion = GetSemanticBuildVersion() + branch;
        }

        private string GetSemanticBuildVersion()
        {
            return $"1.0.{Git.GetCommitCount(RootDirectory)}";
        }



        private static void BundleWithZipCLI(string zipCLI, string zipArchivePath, string publishLocation, bool flattenRuntime)
        {
            var args = new StringBuilder("\"" + zipArchivePath + "\"");

            // so that we can archive content in subfolders, take the length of the
            // path to the root publish location and we'll just substring the
            // found files so the subpaths are retained
            var publishRootLength = publishLocation.Length;
            if (publishLocation[publishRootLength - 1] != Path.DirectorySeparatorChar)
                publishRootLength++;

            var allFiles = GetFilesToIncludeInArchive(publishLocation, flattenRuntime);
            foreach (var kvp in allFiles)
            {
                args.AppendFormat(" \"{0}\"", kvp.Key);
            }

            var psiZip = new ProcessStartInfo
            {
                FileName = zipCLI,
                Arguments = args.ToString(),
                WorkingDirectory = publishLocation,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var handler = (DataReceivedEventHandler)((o, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                    return;
            });

            using (var proc = new Process())
            {
                proc.StartInfo = psiZip;
                proc.Start();

                proc.ErrorDataReceived += handler;
                proc.OutputDataReceived += handler;
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.EnableRaisingEvents = true;
                proc.WaitForExit();

            }
        }


        private static IDictionary<string, string> GetFilesToIncludeInArchive(string publishLocation, bool flattenRuntime)
        {
            string RUNTIME_FOLDER_PREFIX = "runtimes" + Path.DirectorySeparatorChar;

            var includedFiles = new Dictionary<string, string>();
            var allFiles = Directory.GetFiles(publishLocation, "*.*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var relativePath = file.Substring(publishLocation.Length);
                if (relativePath[0] == Path.DirectorySeparatorChar)
                    relativePath = relativePath.Substring(1);

                if (flattenRuntime && relativePath.StartsWith(RUNTIME_FOLDER_PREFIX))
                    continue;

                includedFiles[relativePath] = file;
            }

            return includedFiles;
        }
    }
}
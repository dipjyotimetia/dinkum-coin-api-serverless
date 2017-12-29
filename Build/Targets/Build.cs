using System;
using System.IO;
using System.IO.Compression;
using Build.Settings;
using Nuke.Core;
using Nuke.Core.IO;
using Nuke.Core.Tooling;

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
                     DotnetPath, $"publish -c Release /p:Version=\"{GetBuildVersion()}\" -o \"{Settings.PublishDirectory}\"", RootDirectory).AssertZeroExitCode();
                 File.Copy(Settings.PublishDirectory / "runtimes" / "linux" / "lib" / "netstandard1.3" / "System.Net.NetworkInformation.dll", Settings.PublishDirectory / "System.Net.NetworkInformation.dll");
                 Directory.CreateDirectory(Settings.PackageDirectory);
                 ZipFile.CreateFromDirectory(Settings.PublishDirectory, Settings.PackageDirectory / $"communication-service_{GetBuildVersion()}.zip");
             });

        public Target Test => _ => _
             .Description("Perform all unit tests")
             .DependsOn(Compile)
             .Executes(() =>
             {
                 DotNetTest(settings => settings
                      .SetProjectFile(Settings.TestDirectory / "CommService.DeferredMessage.Lambda.Test")
                      .SetNoBuild(true));
                 DotNetTest(settings => settings
                     .SetProjectFile(Settings.TestDirectory / "CommunicationEndpointLambda.Test")
                     .SetNoBuild(true));
                 DotNetTest(settings => settings
                    .SetProjectFile(Settings.TestDirectory / "EmailAdapaterLambda.Test")
                    .SetNoBuild(true));
                 DotNetTest(settings => settings
                     .SetProjectFile(Settings.TestDirectory / "MessageProcessingLambda.Test")
                     .SetNoBuild(true));
                 DotNetTest(settings => settings
                     .SetProjectFile(Settings.TestDirectory / "MobilePushAdapaterLambda.Test")
                     .SetNoBuild(true));
                 DotNetTest(settings => settings
                    .SetProjectFile(Settings.TestDirectory / "SmsAdapterLambda.Test")
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
    }
}
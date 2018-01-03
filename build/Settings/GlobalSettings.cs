using static Nuke.Core.IO.PathConstruction;


namespace Build.Settings
{
    public class GlobalSettings
    {

        public const string ApplicationName = "DinkumCoin-API";
        public const string BucketName = "dinkum-coin-api-packages";
        public const string BucketWriteRoleArn = "arn:aws:iam::" + DevAccountId + ":role/DinkumCoinApi-WriteBucketRole";
        public const string JenkinsRoleArn = "arn:aws:iam::" + DevAccountId + ":user/JenkinsUser";
        public const string DevAccountId = "303010477609";


        public GlobalSettings(AbsolutePath rootDirectory)
        {
            SourceDirectory = rootDirectory / "src";
            TestDirectory = rootDirectory / "test";
            TemplateDirectory = rootDirectory / "deploy";
            BuildOutputDirectory = rootDirectory / "buildOutput";

            LogsDirectory = BuildOutputDirectory / "logs";
            PackageDirectory = BuildOutputDirectory / "package";
            PublishDirectory = BuildOutputDirectory / "publish";
        }

        public AbsolutePath BuildOutputDirectory { get; }

        public AbsolutePath LogsDirectory { get; }

        public AbsolutePath PackageDirectory { get; }

        public AbsolutePath PublishDirectory { get; }

        public AbsolutePath SourceDirectory { get; }

        public AbsolutePath SqlDirectory { get; }

        public AbsolutePath TemplateDirectory { get; }

        public AbsolutePath TestDirectory { get; }
    }
}
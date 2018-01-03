using Amazon.SecurityToken.Model;
using Build.Settings;
using CrownBet.Build;
using CrownBet.Build.Aws;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nuke.Common.Tools.DotNet;
using Nuke.Core;
using Nuke.Core.Tooling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Parameter = Amazon.CloudFormation.Model.Parameter;
using Tag = Amazon.CloudFormation.Model.Tag;
using Target = Nuke.Core.Target;

namespace Build.Targets
{
    public partial class Build
    {
        [Parameter("Name of AWS account (Deploy targets only)", Name = "Account")] public string AccountName;
        [Parameter("Name of environment (Deploy targets only)", Name = "Environment")] public string EnvironmentName;
        [Parameter("Version of application to deploy (Deploy targets only)")] public string VersionToDeploy;

        private Credentials _credentials;
        private EnvironmentSettings _environmentSettings;
        private StackName _stackName;
        private List<Tag> _stackTags;
        private string _sourceCodeZip;

        public Target Deploy => _ => _
            .Description("Provision AWS resources for Dinkum Coin API")
            .DependsOn(Deploy_DynamoDb)
            .Executes(() =>
            { });


        public Target Deploy_DynamoDb => _ => _
            .Description("Provision DynamoDB for Dinkum Coin Api")
                .DependsOn(Deploy_ApiGateway)
            .Requires(() => AccountName)
            .Requires(() => EnvironmentName)
            .Executes(() => UpsertStack(StackName.DynamoDb, Settings.TemplateDirectory / "dynamoDb.yaml",
            new List<Parameter>
            {
            new Parameter { ParameterKey = "IamRoleLambdaExecution", ParameterValue = GetStackOutputValue(StackName.WalletLambdas,"DinkumCoinLambdaRoleName") }

            }, StackTags, false));

        public Target Deploy_ApiGateway => _ => _
            .Description("Provision API Gateway for Dinkum Coin")
            .DependsOn(Deploy_Lambdas)
            .Requires(() => AccountName)
            .Requires(() => EnvironmentName)
            .Executes(() =>
            {
                UpsertStack(StackName.ApiGateway, Settings.TemplateDirectory / "apiGateway.yaml",
                new List<Parameter>
                {
                    new Parameter { ParameterKey = "EnvironmentName", ParameterValue = EnvironmentName },
                new Parameter { ParameterKey = "GetAllWalletsLambdaArn", ParameterValue = GetStackOutputValue(StackName.WalletLambdas,"GetAllWalletsLambdaArn") },
                new Parameter { ParameterKey = "GetWalletByIdLambdaArn", ParameterValue =GetStackOutputValue(StackName.WalletLambdas,"GetWalletByIdLambdaArn")  },
                new Parameter { ParameterKey = "MineCoinLambdaArn", ParameterValue = GetStackOutputValue(StackName.WalletLambdas,"MineCoinLambdaArn")},
                }, StackTags, false);
            });

    
        public Target Deploy_Lambdas => _ => _
        .Description("Provision lambdas for Dinkum Coin API")
        .Requires(() => AccountName)
        .Requires(() => EnvironmentName)
        .Executes(() => UpsertStack(StackName.WalletLambdas, Settings.TemplateDirectory / "lambda.yaml",
        new List<Parameter>
        {
            new Parameter { ParameterKey = "SourceCodeBucketName", ParameterValue = GlobalSettings.BucketName },
            new Parameter { ParameterKey = "SourceCodeZip", ParameterValue = SourceCodeZip}

        }, StackTags, false));


        public Target Deploy_S3 => _ => _
         .Description("Provision S3 bucket for deployment packages")
         .Requires(() => AccountName)
         .Requires(() => EnvironmentName)
         .Executes(() => UpsertStack(StackName.S3, Settings.TemplateDirectory / "s3.yaml",
          new List<Parameter>
          {
                        new Parameter { ParameterKey = "AllowedRoleArns", ParameterValue = GlobalSettings.JenkinsRoleArn },
                        new Parameter { ParameterKey = "S3BucketName", ParameterValue = GlobalSettings.BucketName }

         }, StackTags, false));


        //public Target Deploy_S3 => _ => _
        //.Description("Provision S3 resources")
        //.Requires(() => MfaCode)
        //.Executes(() =>
        //{
        //    if (Sts.GetCallerAccount().Result != GlobalSettings.WgtdevAccountId)
        //    {
        //        throw new InvalidOperationException("S3 resources can only be deployed to the wgtdev account. Security credentials for the wgtdev account are required. Refer to readme.md for more details.");
        //    }

        //    CloudFormation.UpsertStack(
        //        StackName.S3, Settings.TemplateDirectory / "S3.yaml",
        //        new List<Parameter>
        //        {
        //            new Parameter { ParameterKey = "S3BucketName", ParameterValue = GlobalSettings.BucketName },
        //            new Parameter { ParameterKey = "AllowedRoleArns", ParameterValue = GlobalSettings.JenkinsRoleArn }
        //        }, StackTags, Credentials).Wait();
        //});


        private Credentials Credentials => _credentials = _credentials ?? Sts.AssumeRole(EnvironmentSettings.DeployRoleArn, "deploy").Result;

        private EnvironmentSettings EnvironmentSettings => _environmentSettings = _environmentSettings ?? EnvironmentSettings.CreateSettings(AccountName, EnvironmentName);

        private StackName StackName => _stackName = _stackName ?? new StackName(EnvironmentName);

        private List<Tag> StackTags => _stackTags = _stackTags ?? EnvironmentSettings.CreateStackTags(EnvironmentName);

        private string SourceCodeZip => _sourceCodeZip = _sourceCodeZip ?? $"{VersionToDeploy}/DinkumCoin.Api.Wallet.Lambda_{VersionToDeploy}.zip";

        private IDictionary<string, string> GetEnvironmentVariables()
        {
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();

            return environmentVariables
                .Keys
                .Cast<string>()
                .ToDictionary(key => key, key => (string)environmentVariables[key]);
        }

        private string GetStackOutputValue(string stackName, string key) => CloudFormation.GetStackOutputValue(stackName, key, Credentials).Result;

        private void UpsertStack(string stackName, string templatePath, List<Parameter> parameters, List<Tag> tags, bool notification = false)
        {
            var settings = new StackSettings
            {
                NotificationArns = notification ? new List<string> {  } : new List<string>()
            };

            CloudFormation.UpsertStack(stackName, templatePath, parameters, tags, Credentials, settings).Wait();
        }
    }
}
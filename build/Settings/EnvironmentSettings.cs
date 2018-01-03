using Amazon.CloudFormation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Build.Settings
{
    public abstract class EnvironmentSettings
    {
        protected readonly string EnvironmentName;
        private readonly string _accountNumber;

        protected EnvironmentSettings(string accountNumber, string environmentName)
        {
            _accountNumber = accountNumber;
            EnvironmentName = environmentName;

            GetWalletByIdLambdaName = $"GetWalletByIdLambda-{environmentName}";
            GetWalletsLambdaName = $"GetAllWalletsLambda-{environmentName}";
            MineCoinLambdaName= $"MineCoinLambda-{environmentName}";
            LambdaExecutionRoleName = "DinkumCoinLambdaRole";
        }

        public string BetDomainCloudFormationTopic => $"arn:aws:sns:ap-southeast-2:{_accountNumber}:BetDomainCloudFormation";

        public string DeployRoleArn => $"arn:aws:iam::{_accountNumber}:role/DinkumCoin-dev-DeployRole";

        public bool OnlyCallerCanAssumeDeployRole { get; set; }

        public bool PerformIntegrationTests { get; set; }

        public string GetWalletsLambdaName { get; set; }
        public string GetWalletByIdLambdaName { get; set; }
        public string MineCoinLambdaName { get; set; }

        public string GetWalletsLambdaArn { get; set; }
        public string GetWalletByIdLambdaArn { get; set; }
        public string MineCoinLambdaArn { get; set; }

        public string LambdaExecutionRoleName { get; set; }


        public static EnvironmentSettings CreateSettings(string accountName, string environmentName)
        {
            Type[] types = typeof(EnvironmentSettings)
                .GetTypeInfo()
                .Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(EnvironmentSettings)))
                .ToArray();

            Type settingType =
                types.FirstOrDefault(type => string.Equals(type.Name, accountName + environmentName, StringComparison.OrdinalIgnoreCase)) ??
                types.FirstOrDefault(type => string.Equals(type.Name, accountName, StringComparison.OrdinalIgnoreCase));

            if (settingType == null)
            {
                throw new ArgumentOutOfRangeException($"Unable to find environment settings for {environmentName} environment or {accountName} account");
            }

            return (EnvironmentSettings)Activator.CreateInstance(settingType, environmentName);
        }

        public static List<Tag> CreateStackTags(string environmentName)
        {
            var tags = new List<Tag>
            {
                new Tag { Key = "App", Value = GlobalSettings.ApplicationName },
                new Tag { Key = "Department", Value = "QA" }
            };

            if (environmentName != null)
            {
                tags.Add(new Tag { Key = "Environment", Value = environmentName.ToUpper() });
            }

            return tags;
        }

    }
}
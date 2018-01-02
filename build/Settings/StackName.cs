namespace Build.Settings
{
    public class StackName
    {
        public const string S3 = "DinkumCoin-Deploy-S3";
        private readonly string _environment;

        public StackName(string environment)
        {
            _environment = environment;
        }

        public string WalletLambdas => $"{Prefix}-Wallet-Lambdas";
        public string ApiGateway => $"{Prefix}-Wallet-ApiGateway";
        public string DynamoDb => $"{Prefix}-Wallet-Db";


        private string Prefix => $"DinkumCoin-{_environment}";
    }
}
namespace Build.Settings
{
    public class UatEnv : EnvironmentSettings
    {
        public UatEnv(string environmentName) : base(GlobalSettings.DevAccountId, environmentName)
        {

        }
    }
}
namespace Build.Settings
{
    public class DevEnv : EnvironmentSettings
    {
        public DevEnv(string environmentName) : base(GlobalSettings.DevAccountId, environmentName)
        {

        }
    }
}
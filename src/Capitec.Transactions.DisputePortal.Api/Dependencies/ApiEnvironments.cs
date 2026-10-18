namespace Capitec.Transactions.DisputePortal.Api.Dependencies
{
    public static class ApiEnvironments
    {
        public const string Local = "Local";

        public static bool IsLocalOrDevelopment(IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(environment);

            return environment.IsDevelopment() || environment.IsEnvironment(Local);
        }
    }
}

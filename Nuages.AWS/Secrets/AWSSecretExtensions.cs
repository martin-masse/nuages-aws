namespace Nuages.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public static class AWSSecretExtensions
{
    public static void AddSecretsProvider(this IServiceCollection services)
    {
        services.AddScoped<IAWSSecretProvider, AWSSecretProvider>();
    }
}
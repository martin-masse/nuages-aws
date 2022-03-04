namespace Nuage.AWS.Secrets;

public static class AWSSecretExtensions
{
    public static void AddSecretsProvider(this IServiceCollection services)
    {
        services.AddScoped<IAWSSecretProvider, AWSSecretProvider>();
    }
}
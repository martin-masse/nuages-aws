namespace Nuages.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public static class AWSSecretExtensions
{
    public static void AddSecretsProvider(this IServiceCollection services)
    {
        services.AddScoped<IAWSSecretProvider, AWSSecretProvider>();
    }

    public static void TransformSecrets(this ConfigurationManager configuration)
    {
         AWSSecretProvider.Instance.TransformSecrets(configuration);
    }
    
    public static void TransformSecrets(this ConfigurationManager configuration, string name)
    {
        AWSSecretProvider.Instance.TransformSecret(configuration, name);
    }
}
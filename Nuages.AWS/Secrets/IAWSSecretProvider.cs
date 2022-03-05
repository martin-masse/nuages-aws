namespace Nuages.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public interface IAWSSecretProvider 
{
    Task<T?> GetSecretAsync<T>(string secretArn) where T : class; 
    Task<string?> GetSecretStringAsync(string secretArn);

    void TransformSecret<T>(IConfigurationBuilder nuilder, IConfiguration configuration, string key) where T : class, ISecret;
}

// ReSharper disable once InconsistentNaming
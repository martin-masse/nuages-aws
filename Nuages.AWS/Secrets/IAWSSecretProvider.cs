namespace Nuages.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public interface IAWSSecretProvider 
{
    Task<T?> GetSecretAsync<T>(string secretArn) where T : class; 
    Task<string?> GetSecretStringAsync(string secretArn);

    void TransformSecret(IConfigurationBuilder nuilder, IConfiguration configuration, string key);
    //void TransformSecret<T>(IConfigurationBuilder nuilder, IConfiguration configuration, string key) where T : class, ISecret;
    void TransformSecret(ConfigurationManager configurationManager, string key);
    void TransformSecrets(ConfigurationManager configurationManager);
    IEnumerable<KeyValuePair<string, string>> GetSecretsValues(IConfiguration configuration);
}

// ReSharper disable once InconsistentNaming
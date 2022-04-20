using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Nuages.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public class AWSSecretProvider : IDisposable, IAWSSecretProvider
{
    private readonly IAmazonSecretsManager _secretsManager;

    public AWSSecretProvider(IAmazonSecretsManager? secretsManager = null)
    {
        _secretsManager = secretsManager ?? new AmazonSecretsManagerClient();
    }

    private static AWSSecretProvider _instance = null;
    
    public static AWSSecretProvider Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AWSSecretProvider();

            return _instance;
        }
    }
    public async Task<T?> GetSecretAsync<T>(string secretArn) where T : class
    {
        var value = await GetSecretStringAsync(secretArn);

        return string.IsNullOrEmpty(value) ? null : JsonSerializer.Deserialize<T>(value);
    }
    
    public async Task<string?> GetSecretStringAsync(string secretArn)
    {
        if (string.IsNullOrEmpty(secretArn))
            return null;
        
        if (!secretArn.StartsWith("arn:aws:secretsmanager"))
            return null;
        
        var request = new GetSecretValueRequest
        {
            SecretId = secretArn
        };

        var response = _secretsManager.GetSecretValueAsync(request).Result;

        if (response.SecretString != null)
        {
            return response.SecretString;
        }
        
        await using var memoryStream = response.SecretBinary;
        var reader = new StreamReader(memoryStream);
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(await reader.ReadToEndAsync()));
    }

    public void TransformSecret(IConfigurationBuilder builder, IConfiguration configuration, string name) 
    {
        var value = configuration[name];
        if (!string.IsNullOrEmpty(value))
        {
            if (value.StartsWith("arn:aws:secretsmanager"))
            {
                var secret = GetSecretStringAsync(value).Result;
                if (secret != null)
                {
                    builder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new (name,  secret)
                    });
                }
            }
        }
    }
    
    public void TransformSecret(ConfigurationManager configurationManager, string name) 
    {
        var value = configurationManager[name];
        if (!string.IsNullOrEmpty(value))
        {
            if (value.StartsWith("arn:aws:secretsmanager"))
            {
                var secret = GetSecretStringAsync(value).Result;
                if (secret != null)
                {
                    configurationManager.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new (name,  secret)
                    });
                }
            }
        }
    }
    
    public void TransformSecrets(ConfigurationManager configurationManager) 
    {
        foreach (var item in configurationManager.AsEnumerable())
        {
            if (!string.IsNullOrEmpty(item.Value) && item.Value.StartsWith("arn:aws:secretsmanager"))
            {
                var secret = GetSecretStringAsync(item.Value).Result;
                if (secret != null)
                {
                    configurationManager.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new (item.Key,  secret)
                    });
                }
            }
        }
    }
    
    public IEnumerable<KeyValuePair<string, string>> GetSecretsValues(IConfiguration configuration) 
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var (key, value) in configuration.AsEnumerable())
        {
            if (!string.IsNullOrEmpty(value) && value.StartsWith("arn:aws:secretsmanager"))
            {
                var secret = GetSecretStringAsync(value).Result;
                if (secret != null)
                {
                    yield return new KeyValuePair<string, string>(key, secret);
                }
            }
        }
    }


    public void Dispose()
    {
        _secretsManager.Dispose();
    }
}
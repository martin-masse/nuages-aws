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

    // public void TransformSecret<T>(IConfigurationBuilder builder, IConfiguration configuration, string name) where T : class, ISecret
    // {
    //     var value = configuration[name];
    //     if (!string.IsNullOrEmpty(value))
    //     {
    //         if (value.StartsWith("arn:aws:secretsmanager"))
    //         {
    //             var secret = GetSecretAsync<T>(value).Result;
    //             if (secret != null)
    //             {
    //                 builder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
    //                 {
    //                     new (name,  secret.Value)
    //                 });
    //             }
    //         }
    //     }
    // }
    
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

    public void Dispose()
    {
        _secretsManager.Dispose();
    }
}
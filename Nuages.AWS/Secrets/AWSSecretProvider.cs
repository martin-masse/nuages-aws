using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Nuages.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public class AWSSecretProvider : IAWSSecretProvider
{
    private readonly IAmazonSecretsManager _secretsManager;

    public AWSSecretProvider(IAmazonSecretsManager secretsManager)
    {
        _secretsManager = secretsManager;
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
}
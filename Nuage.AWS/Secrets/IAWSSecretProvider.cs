using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Nuage.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public interface IAWSSecretProvider
{
    Task<string>? GetValue(string secretArn);
}

// ReSharper disable once InconsistentNaming
public class AWSSecretProvider : IAWSSecretProvider
{
    private readonly IAmazonSecretsManager _secretsManager;

    public AWSSecretProvider(IAmazonSecretsManager secretsManager)
    {
        _secretsManager = secretsManager;
    }
    
    public async Task<string>? GetValue(string secretArn)
    {
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
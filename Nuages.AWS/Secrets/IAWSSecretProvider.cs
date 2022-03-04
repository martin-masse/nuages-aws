namespace Nuages.AWS.Secrets;

// ReSharper disable once InconsistentNaming
public interface IAWSSecretProvider
{
    Task<string> GetValueAsync(string secretArn);
}

// ReSharper disable once InconsistentNaming
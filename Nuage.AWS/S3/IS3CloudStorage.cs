// ReSharper disable UnusedMember.Global
namespace Nuages.Web.S3;

#region



// ReSharper disable UnusedMemberInSuper.Global

#endregion


public interface IS3CloudStorage
{
    Task<byte[]?> ReadFileAsync(string credential, string region, string bucket, string key);

    string? GetDownloadUrl(string credential, string region, string bucket, string key);

    Task<bool> DeleteFileAsync(string credential, string region, string bucket, string key);
    Task<bool> WriteFileAsync(string credential, string region, string bucket, string key, byte[] data);

    // ReSharper disable once UnusedMember.Global
    Task<bool> IsFileExistAsync(string credential, string region, string bucket, string key);

    Task<List<S3CloudFile>> GetObjectsAsync(string credential, string region, string bucket, string prefix,
        bool withData = false);

    Task<byte[]?> ReadFileAsync(string bucket, string key);

    string? GetDownloadUrl(string bucket, string key);

    Task<bool> DeleteFileAsync( string bucket, string key);
    Task<bool> WriteFileAsync(string bucket, string key, byte[] data);

    Task<bool> IsFileExistAsync(string bucket, string key);

    Task<List<S3CloudFile>> GetObjectsAsync(string bucket, string prefix,
        bool withData = false);
}
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global

namespace Nuages.AWS.S3;

    #region



#endregion

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class S3CloudStorage : IS3CloudStorage
{
    private readonly IAmazonS3 _s3;
    private IAmazonS3? _lastClient;


    private string _lastCredentials = string.Empty;

    public S3CloudStorage(IAmazonS3 s3)
    {
        _s3 = s3;
    }

    public async Task<byte[]?> ReadFileAsync(string bucket, string key)
    {
        return await ReadFileAsync(_s3, bucket, key);
    }

    public string? GetDownloadUrl(string bucket, string key)
    {
        return GetDownloadUrl(_s3, bucket, key);
    }

    public async Task<bool> IsFileExistAsync(string bucket, string key)
    {
        return await IsFileExistAsync(_s3, bucket, key);
    }

    public async Task<bool> DeleteFileAsync(string bucket, string key)
    {
        return await DeleteFileAsync(_s3, bucket, key);
    }

    public async Task<bool> WriteFileAsync(string bucket, string key, byte[] data)
    {
        return await WriteFileAsync(_s3, bucket, key, data);
    }

    public async Task<List<S3CloudFile>> GetObjectsAsync(string bucket, string prefix,
        bool withData = false)
    {
        return await GetObjectsAsync(_s3, bucket, prefix, withData);
    }

    public string? GetDownloadUrl(string credentials, string region, string bucket, string key)
    {
        return GetDownloadUrl(GetClient(credentials, region), bucket, key);
    }

    public async Task<byte[]?> ReadFileAsync(string credentials, string region, string bucket, string key)
    {
        return await ReadFileAsync(GetClient(credentials, region), bucket, key);
    }

    public async Task<bool> IsFileExistAsync(string credentials, string region, string bucket, string key)
    {
        return await IsFileExistAsync(GetClient(credentials, region), bucket, key);
    }

    public async Task<bool> DeleteFileAsync(string credentials, string region, string bucket, string key)
    {
        return await DeleteFileAsync(GetClient(credentials, region), bucket, key);
    }

    public async Task<bool> WriteFileAsync(string credentials, string region, string bucket, string key,
        byte[] data)
    {
        return await WriteFileAsync(GetClient(credentials, region), bucket, key, data);
    }

    public async Task<List<S3CloudFile>> GetObjectsAsync(string credentials, string region, string bucket,
        string prefix,
        bool withData = false)
    {
        return await GetObjectsAsync(GetClient(credentials, region), bucket, prefix, withData);
    }

    public string? GetDownloadUrl(IAmazonS3 s3, string bucket, string key)
    {
        var response = s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Expires = DateTime.Now.AddMinutes(2)
        });

        return response;
    }

    public async Task<byte[]?> ReadFileAsync(IAmazonS3 s3, string bucket, string key)
    {
        var response = await s3.GetObjectAsync(new GetObjectRequest
        {
            BucketName = bucket,
            Key = key
        });

        await using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms);

        return ms.ToArray();
    }


    // ReSharper disable once SuggestBaseTypeForParameter
    public async Task<bool> IsFileExistAsync(IAmazonS3 s3, string bucket, string key)
    {
        var response = await s3.GetAllObjectKeysAsync(bucket, key, null);
        return response.Count > 0;
    }

    public async Task<bool> DeleteFileAsync(IAmazonS3 s3, string bucket, string key)
    {
        var res = await s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = bucket,
            Key = key
        });

        return res.HttpStatusCode == HttpStatusCode.NoContent;
    }

    public async Task<bool> WriteFileAsync(IAmazonS3 s3, string bucket, string key, byte[] data)
    {
        var ms = new MemoryStream();
        ms.Write(data, 0, data.Length);
        ms.Position = 0;

        var res = await s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = ms
        });

        return res.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<List<S3CloudFile>> GetObjectsAsync(IAmazonS3 s3, string bucket, string prefix,
        bool withData = false)
    {
        var list = new List<S3CloudFile>();

        // if (!prefix.EndsWith("/"))
        //     prefix += "/";

        var response = await s3.ListObjectsAsync(new ListObjectsRequest
        {
            BucketName = bucket,
            Prefix = prefix
        });

        foreach (var obj in response.S3Objects)
        {
            var file = new S3CloudFile
            {
                Key = obj.Key,
                BucketName = obj.BucketName,
                Name = Path.GetFileName(obj.Key)
            };

            if (withData) file.Data = await ReadFileAsync(bucket, obj.Key);

            list.Add(file);
        }

        return list;
    }

    private IAmazonS3 GetClient(string credentials, string region)
    {
        if (credentials == _lastCredentials)
            if (_lastClient != null)
                return _lastClient;

        _lastCredentials = credentials;

        var parts = credentials.Split('|');

        _lastClient = new AmazonS3Client(new BasicAWSCredentials(parts.First(), parts.Last()), GetRegion(region));

        return _lastClient;
    }

    [ExcludeFromCodeCoverage]
    public static RegionEndpoint GetRegion(string name)
    {
        return name switch
        {
            "ca-central-1" => RegionEndpoint.CACentral1,
            _ => RegionEndpoint.USEast1
        };
    }
}
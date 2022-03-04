using System.Diagnostics.CodeAnalysis;

namespace Nuages.Web.S3;

[ExcludeFromCodeCoverage]
// ReSharper disable once UnusedType.Global
public static class S3CloudStorageConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static IServiceCollection AddS3CloudStorage(this IServiceCollection services)
    {
        services.AddScoped<IS3CloudStorage, S3CloudStorage>();

        return services;
    }
}
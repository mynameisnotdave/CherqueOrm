using Microsoft.Extensions.DependencyInjection;

namespace CherqueOrm;

public static class CherqueServiceCollection
{
    public static IServiceCollection AddCherque(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }
        
        services.AddScoped<CherqueQuery>();
        
        return services;
    }
}
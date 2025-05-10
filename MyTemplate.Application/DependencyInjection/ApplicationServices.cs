using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

namespace MyTemplate.Application.DependencyInjection;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyTemplate.Application.Behaviors;
using System.Reflection;

namespace MyTemplate.Application.DependencyInjection;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);
        return services;
    }
}

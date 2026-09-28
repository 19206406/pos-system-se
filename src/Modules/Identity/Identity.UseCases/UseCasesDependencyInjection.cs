using Identity.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.UseCases
{
    public static class UseCasesDependencyInjection
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services)
            => services.AddUsersUseCases();

        public static IServiceCollection AddUsersUseCases(this IServiceCollection services)
            => services.AddScoped<UsersUseCases>()
                       .AddScoped<RegisterUser>()
                       .AddScoped<LoginUser>(); 
    }   
}

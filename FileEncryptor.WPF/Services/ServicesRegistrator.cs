using FileEncryptor.WPF.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FileEncryptor.WPF.Services
{
    internal static class ServicesRegistrator
    {
        public static IServiceCollection AddService(this IServiceCollection services) => services
            .AddTransient<IUserDialog, UserDialogService>();

    }
}

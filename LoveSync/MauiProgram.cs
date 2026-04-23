using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using LoveSync.Services;
using LoveSync.ViewModels;

// Csak mobilon (Android/iOS) töltjük be az értesítés kezelőt
#if ANDROID || IOS
using Plugin.LocalNotification;
#endif

namespace LoveSync;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // CommunityToolkit inicializálása:
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. Értesítések bekapcsolása:
#if ANDROID || IOS
        builder.UseLocalNotification();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // 2. Szolgáltatások regisztrálása:
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<NoteService>();
        builder.Services.AddSingleton<CalendarService>();
        builder.Services.AddSingleton<MovieService>();
        builder.Services.AddSingleton<IdeaService>();
        builder.Services.AddSingleton<BucketService>();


        // 3. Viewmodelek és oldalak regisztrálása:

        // Login & Regisztráció
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();

        // Párosítás
        builder.Services.AddTransient<PairingViewModel>();
        builder.Services.AddTransient<PairingPage>();

        // Főoldal
        builder.Services.AddTransient<MainPage>();

        // Profil
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePage>();

        // Üzenőfal
        builder.Services.AddSingleton<NotesViewModel>();
        builder.Services.AddSingleton<NotesPage>();

        // Naptár
        builder.Services.AddSingleton<CalendarViewModel>();
        builder.Services.AddSingleton<CalendarPage>();

        // Swipe
        builder.Services.AddTransient<SwipeViewModel>();
        builder.Services.AddTransient<SwipePage>();
        builder.Services.AddTransient<CategoryPage>();

        // Közös találatok
        builder.Services.AddSingleton<MatchesViewModel>();
        builder.Services.AddTransient<MatchesPage>();

        // Bakancslista
        builder.Services.AddSingleton<BucketViewModel>();
        builder.Services.AddTransient<BucketListPage>();

        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
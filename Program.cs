using demo_webapplication;
using Npgsql;
using System.Security.Cryptography;

// Här skapar vi en builder som skapar en webapplikation och en app som bygger webapplikationen med hjälp av builder.
// Detta är inbyggt i .NET 6 och uppåt. Vad vi skapar är en webserver som hanterar HTTP-requests och responses. För vår applikation.
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


// Här skapar vi en middleware som genererar en unik client ID för varje användare som besöker webbplatsen. Istället för att använda sessions i http så använder vi cookies för att spara client ID:t.
app.Use(async (context, next) =>
{   
    // Här sätter vi en cookie som heter "ClientId" som innehåller en unik client ID för varje användare som besöker webbplatsen.
    const string clientIdCookieName = "ClientId";

    // Här kollar vi om det finns en cookie som heter "ClientId" i requesten. Om det inte finns så genererar vi en unik client ID för användaren och sätter den i en cookie.
    if (!context.Request.Cookies.TryGetValue(clientIdCookieName, out var clientId))
    {
        // Generera en unik client ID för användaren och sätt den i en cookie
        clientId = GenerateUniqueClientId();
        // Sätt en cookie som heter "ClientId" som innehåller en unik client ID för användaren
        context.Response.Cookies.Append(clientIdCookieName, clientId, new CookieOptions
        {
            HttpOnly = true, // Enbart tillgänglig för HTTP-requests
            Secure = false,   // Används endast för HTTPS-requests
            SameSite = SameSiteMode.Strict, // Vad som är tillåtet för att skicka cookies till andra webbplatser
            MaxAge = TimeSpan.FromDays(365) // Cookie livstid
        });
        Console.WriteLine($"New client ID generated and set: {clientId}");
    }
    else
    {
        Console.WriteLine($"Existing client ID found: {clientId}");
    }

    // Kör vidare till nästa middleware i pipelinen (om det finns någon) eller till slutanvändaren om det inte finns någon mer middleware i pipelinen.
    await next();
});

// Här använder vi UseDefaultFiles och UseStaticFiles för att servera statiska filer som index.html från wwwroot mappen.
// UseDefaultFiles används för att servera default filer som index.html direkt på url - "/" i vår webbapplikation.
// UseStaticFiles används för att servera statiska filer som bilder, css, js, etc. från wwwroot mappen. Så att vår styling och bilder med mera är åtkomligt för HTML och så att HTML kan använda sig av "wordgame.js" filen.
app.UseDefaultFiles(); // Serve default files like index.html
app.UseStaticFiles(); // Serve static files from wwwroot directory


// Här skapar vi en metod som genererar en unik client ID för varje användare som besöker webbplatsen. Vi använder en RandomNumberGenerator för att generera en unik client ID.
// Vi skapar en byte array som innehåller 16 bytes och genererar en unik client ID genom att konvertera bytes till Base64String.
// Vi returnerar sedan den unika client ID:t.
// Detta är en hjälp metod som används i "app.Use" middleware för att generera en unik client ID för varje användare som besöker webbplatsen.
static string GenerateUniqueClientId()
{
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[16];
    rng.GetBytes(bytes);
    return Convert.ToBase64String(bytes);
}

// Här skapar vi en instans av klassen Queries som innehåller metoder för att hämta data från databasen. Och vi skickar med appen som argument till klassen Queries.
// Detta gör att vi kan använda oss av metoder för att hämta data från databasen i vår webbapplikation.
Queries queries = new(app);

// Här startar vi webbapplikationen och kör den.
await app.RunAsync();

// Här importeras de nödvändiga biblioteken för att kunna använda sig av Npgsql och Microsoft.AspNetCore.Http
using Npgsql;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography.X509Certificates;


// Här anger vi vilken namespace som används (namespace demo_webapplication är själva scope:en för projektet av vilka klasser och metoder som finns tillgängliga)
namespace demo_webapplication;

// ************************************************************************************************************************************************************************************
// Här skapar vi en klass som heter Queries - 
// Denna klass innehåller en konstruktor som tar emot en WebApplication som parameter och sedan initierar en databasanslutning och mappar routes - 
// - till de olika metoder som finns i klassen.
// Klassen innehåller även metoder för att hämta ord från databasen, kolla om ett ord redan finns i databasen, lägga till ett nytt ord i databasen,
// hämta spelare från databasen, kolla om en spelare redan finns i databasen och lägga till en ny spelare i databasen.
// Klassen innehåller även två klasser som används för att hantera inkommande requests - PlayerRequest och WordRequest.
// Dessa klasser innehåller properties som används för att binda inkommande JSON-data till objekt.
public class Queries
{
    // Här skapar vi en privat variabel som heter _db och är av typen NpgsqlDataSource
    private readonly NpgsqlDataSource _db;

    // Här skapar vi en konstruktor som tar emot en WebApplication som parameter
    public Queries(WebApplication app)
    {
        // Här skapar vi en ny instans av klassen Database och kallar på metoden Connection() för att initiera en databasanslutning. Denna klassen finns i filen Database.cs
        var database = new Database();
        _db = database.Connection(); // Här initieras databasanslutningen

        // Här kallar vi på metoden MapRoutes() som tar emot en WebApplication som parameter - denna metod mappar routes till de olika metoderna i klassen
        // Vi har skapat en metod för alla routes som finns i klassen för att kunna samla alla routes på ett ställe.
        // Vi skapar 'MapRoutes' nedanför på rad 37.
        MapRoutes(app);
    }

// ************************************************************************************************************************************************************************************

// ************************************************************************************************************************************************************************************
    // Här skapar vi en privat metod som heter MapRoutes som tar emot en WebApplication som parameter och mappar routes till de olika metoderna i klassen.
    private void MapRoutes(WebApplication app)
    {   
        // Här mappar vi en GET-request till /api/status till en anonym metod (lambda) som returnerar en sträng "Hello from API!"
        app.MapGet("/api/status", () => "Hello from API!");

        // Här mappar vi en GET-request till /api/words till metoden GetWordsAsync som hämtar alla ord från databasen och returnerar dem som en sträng med radbrytningar. Rad 53
        app.MapGet("/api/words", GetWordsAsync);

        // Här mappar vi en POST-request till /api/word/new till metoden AddWordAsync som tar emot en HttpContext som parameter och lägger till ett nytt ord i databasen. Rad 80
        app.MapPost("/api/word/new", AddWordAsync);

        // Här mappar vi en GET-request till /api/players till metoden GetPlayersAsync som hämtar alla spelare från databasen och returnerar dem som en sträng med radbrytningar. Rad 115
        app.MapGet("/api/players", GetPlayersAsync);

        // Här mappar vi en POST-request till /api/player/new till metoden AddPlayerAsync som tar emot en HttpContext som parameter och lägger till en ny spelare i databasen. Rad 143
        app.MapPost("/api/player/new", AddPlayerAsync);
    }

// ************************************************************************************************************************************************************************************

    // Här skapar vi en privat metod som heter GetWordsAsync som returnerar en Task<string> - denna metod hämtar alla ord från databasen och returnerar dem som en sträng med radbrytningar.
    private async Task<string> GetWordsAsync()
    {
        // Här skapar vi en ny lista som heter result
        // Listan kommer att innehålla alla ord som hämtas från databasen
        var result = new List<string>();

        // Här skapar vi en ny instans av klassen NpgsqlCommand som heter cmd och initierar den med en SQL-query som hämtar alla ord från tabellen 'words'
        await using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT word FROM words"; // SQL-query

        // Här skapar vi en ny instans av klassen NpgsqlDataReader som heter reader och kallar på metoden ExecuteReaderAsync() för att exekvera SQL-queryn
        await using var reader = await cmd.ExecuteReaderAsync();

        // Här loopar vi igenom alla rader som finns i tabellen 'words' och lägger till dem i listan 'result'
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0)); // Lägg till ord i listan 'result', Vi hämtar orden och ber bara om att få värdet på kolumn word vilket resulterar -
            // - i att vi endast en kolumn med ord i tabellen words. Där av används index 0.
        }

        // Här returnerar vi listan 'result' som en sträng med radbrytningar
        return string.Join("\n", result);
    }

// ************************************************************************************************************************************************************************************

    // Här skapar vi en privat metod som heter WordExistsAsync som returnerar en Task<bool> och tar emot en sträng som parameter - denna metod kollar om ett ord redan finns i databasen -
    // - genom att exekvera en SQL-query som kollar om ordet finns i tabellen 'words'
    // Denna metod används för att kolla om ett ord redan finns i databasen innan det läggs till och är en hjälp metod till 'AddWordAsync'
    private async Task<bool> WordExistsAsync(string word)
    {
        await using var cmd = _db.CreateCommand(); // Här skapar vi en ny instans av klassen NpgsqlCommand som heter cmd
        cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM words WHERE word = $1)"; // Här initierar vi cmd med en SQL-query som kollar om ordet finns i tabellen 'words'
        cmd.Parameters.AddWithValue(word); // Här lägger vi till ordet som parameter i SQL-queryn ($1)

        // Här returnerar vi resultatet av SQL-queryn som en bool - true om ordet finns i tabellen 'words' och false om ordet inte finns
        // ?? false betyder att om resultatet är null så returneras false
        return (bool)(await cmd.ExecuteScalarAsync() ?? false);
    }

// ************************************************************************************************************************************************************************************

    // Här skapar vi en privat metod som heter AddWordAsync som returnerar en Task<IResult> och tar emot en HttpContext som parameter - denna metod lägger till ett nytt ord i databasen
    // genom att exekvera en SQL-query som lägger till ordet i tabellen 'words'
    // Denna metod används för att lägga till ett nytt ord i databasen och returnerar en IResult som indikerar om ordet har lagts till eller inte
    // Metoden tar emot en HttpContext som parameter för att kunna läsa inkommande JSON-data. Och då behöver vi använda ReadFromJsonAsync<WordRequest>() för att binda JSON-data till objekt -
    // - av typen 'WordRequest' som innehåller en property 'Word'
    private async Task<IResult> AddWordAsync(HttpContext context)
    {
        // Skriv ut inkommande request till konsolen (för applikationsloggen / inte i webbläsaren)
        Console.WriteLine($"Received a request: {context.Request}");

        // Här läser vi inkommande JSON-data från requesten och binder den till ett objekt av typen 'WordRequest'
        var requestBody = await context.Request.ReadFromJsonAsync<WordRequest>();

        // Om requestBody är null eller om ordet är tomt så returneras en BadRequest
        if (requestBody == null || string.IsNullOrEmpty(requestBody.Word))
        {
            return Results.BadRequest("Invalid request");
        }

        // Här hämtar vi ordet från requestBody
        var word = requestBody.Word;

        // Här kollar vi om ordet redan finns i databasen genom att kalla på metoden WordExistsAsync
        bool exist = await WordExistsAsync(word);

        // Om ordet redan finns i databasen så returneras en BadRequest
        if (exist)
        {
            return Results.BadRequest("The word already exists");
        }

        // Om ordet inte finns i databasen så läggs det till i databasen genom att exekvera en SQL-query som lägger till ordet i tabellen 'words'
        else
        {

            // Insert new word
            await using var cmd = _db.CreateCommand(); // Här skapar vi en ny instans av klassen NpgsqlCommand som heter cmd
            cmd.CommandText = "INSERT INTO words (word) VALUES ($1)"; // Här initierar vi cmd med en SQL-query som lägger till ordet i tabellen 'words'
            cmd.Parameters.AddWithValue(word); // Här lägger vi till ordet som parameter i SQL-queryn ($1)
            await cmd.ExecuteNonQueryAsync(); // Här exekverar vi SQL-queryn

            return Results.Ok("Word added"); // Här returneras en Ok som indikerar att ordet har lagts till
        }
    }

// ************************************************************************************************************************************************************************************


// ************************************************************************************************************************************************************************************
    // Här skapar vi en privat metod som heter GetPlayersAsync som returnerar en Task<string> - denna metod hämtar alla spelare från databasen och returnerar dem som en sträng med radbrytningar.
    // Just nu använder vi inte denna metod i applikationen. Men för att göra det behöver vi endast mappa en GET-request till /api/players som kallar på denna metod.
    private async Task<string> GetPlayersAsync()
    {   
        // Här skapar vi en ny lista som heter result
        var result = new List<string>();

        // Här skapar vi en ny instans av klassen NpgsqlCommand som heter cmd och initierar den med en SQL-query som hämtar alla spelare från tabellen 'player'
        await using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT name FROM player"; // SQL-query

        await using var reader = await cmd.ExecuteReaderAsync(); // Här exekverar vi SQL-queryn och skapar en ny instans av klassen NpgsqlDataReader som heter reader
        while (await reader.ReadAsync()) // Här loopar vi igenom alla rader som finns i tabellen 'player' och lägger till dem i listan 'result'
        {
            result.Add(reader.GetString(0));
        }

        // Här returnerar vi listan 'result' som en sträng med radbrytningar
        return string.Join("\n", result);
    }

// ************************************************************************************************************************************************************************************


    // Här skapar vi en privat metod som heter PlayerExistsAsync som returnerar en Task<bool> och tar emot en sträng som parameter - denna metod kollar om en spelare redan finns i databasen -
    // - genom att exekvera en SQL-query som kollar om spelaren finns i tabellen 'player'
    // Denna metod används för att kolla om en spelare redan finns i databasen innan den läggs till och är en hjälp metod till 'AddPlayerAsync'
    private async Task<bool> PlayerExistsAsync(string name)
    {
        await using var cmd = _db.CreateCommand(); // Här skapar vi en ny instans av klassen NpgsqlCommand som heter cmd
        cmd.CommandText = "SELECT EXISTS (SELECT 1 FROM player WHERE name = $1)"; // Här initierar vi cmd med en SQL-query som kollar om spelaren finns i tabellen 'player'
        cmd.Parameters.AddWithValue(name); // Här lägger vi till spelaren som parameter i SQL-queryn ($1)
        return (bool)(await cmd.ExecuteScalarAsync() ?? false); // Här returnerar vi resultatet av SQL-queryn som en bool - true om spelaren finns i tabellen 'player' och false om spelaren inte finns
    }

// ************************************************************************************************************************************************************************************


// ************************************************************************************************************************************************************************************
    // Här skapar vi en privat metod som heter AddPlayerAsync som returnerar en Task<IResult> och tar emot en HttpContext som parameter - denna metod lägger till en ny spelare i databasen
    // genom att exekvera en SQL-query som lägger till spelaren i tabellen 'player'
    // Metoden tar emot en HttpContext som parameter för att kunna läsa inkommande JSON-data. Och då behöver vi använda ReadFromJsonAsync<PlayerRequest>() för att binda JSON-data till objekt -
    // - av typen 'PlayerRequest' som innehåller en property 'Name'

    private async Task<IResult> AddPlayerAsync(HttpContext context) 
    {   
        // Här läser vi inkommande JSON-data från requesten och binder den till ett objekt av typen 'PlayerRequest'
        var requestBody = await context.Request.ReadFromJsonAsync<PlayerRequest>();

        // Om requestBody är null eller om namnet är tomt så returneras en BadRequest
        if (requestBody == null || string.IsNullOrEmpty(requestBody.Name))
        {
            return Results.BadRequest("Invalid request");
        }

        var name = requestBody.Name; // Här hämtar vi namnet från requestBody

        if (await PlayerExistsAsync(name))  // Här kollar vi om spelaren redan finns i databasen genom att kalla på metoden PlayerExistsAsync på rad 179 - om spelaren redan finns så returneras en BadRequest
        {
            return Results.BadRequest("The player already exists");
        }

        // Om spelaren inte finns i databasen så läggs den till i databasen genom att exekvera en SQL-query som lägger till spelaren i tabellen 'player'
        await using var cmd = _db.CreateCommand(); // Här skapar vi en ny instans av klassen NpgsqlCommand som heter cmd
        cmd.CommandText = "INSERT INTO player (name) VALUES ($1)"; // Här initierar vi cmd med en SQL-query som lägger till spelaren i tabellen 'player'
        cmd.Parameters.AddWithValue(name); // Här lägger vi till namnet som parameter i SQL-queryn ($1)
        await cmd.ExecuteNonQueryAsync(); // Här exekverar vi SQL-queryn
        return Results.Ok("Player added"); // Här returneras en Ok som indikerar att spelaren har lagts till
    }

// ************************************************************************************************************************************************************************************


    // Här skapar vi en klass/model/record som heter PlayerRequest - denna klass används för att binda inkommande JSON-data till objekt och innehåller en property 'Name' som används för att binda namnet på en spelare till objektet
    public class PlayerRequest
    {
        public string Name { get; set; }
    }

    // Här skapar vi en klass/model/record som heter WordRequest - denna klass används för att binda inkommande JSON-data till objekt och innehåller en property 'Word' som används för att binda ordet till objektet som används när vi använder oss av ReadFromJsonAsync<WordRequest>()
    public class WordRequest
    {
        public string Word { get; set; }
    }
}

// ************************************************************************************************************************************************************************************
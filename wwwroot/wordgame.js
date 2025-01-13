// Skapar en variabel 'words' som är en tom array som kommer att innehålla ord som hämtas från servern
let words = [];

// Skapar en variabel 'oneRandomWord' som är en tom sträng som kommer att innehålla det slumpmässiga ordet
let oneRandomWord = '';

// Skapar en variabel 'letters' som är en tom array som kommer att innehålla bokstäver från det slumpmässiga ordet
let letters = [];

// ******** HÄR KÖRS KODEN SOM SPARAR SPELARENS NAMN I DATABASEN OCH UPPDATERAR TEXT PÅ HTML TILL SPELARENS NAMN ********
// ************************************************************
$('#save-player').on('click', addPlayer);
// Den här funktionen körs när sidan laddas men den lyssnar på knappen med (id='save-player') efter ett "click" event
async function addPlayer(event) {
    // event.preventDefault() förhindrar att sidan laddas om när knappen trycks
    event.preventDefault();
    // Här hämtas spelarens namn från input-fältet med id='playername'
    let playerName = await $('#playername').val();
    console.log(playerName)
    // Här skickas spelarens namn till servern för att sparas i databasen
    // fetch() är en inbyggd funktion i JavaScript som skickar en förfrågan till servern
    // I det här fallet skickas en POST-förfrågan till /api/player/new - en route som vi har skapat via app.Map*** vid våra queries/actions (rad 55 i Queries.cs)
    // I fetch() skickas även spelarens namn som ett JSON-objekt
    await fetch("/api/player/new", {
        method: "POST",
        headers: {
            'Content-type': 'application/json'
        },
        body: JSON.stringify({ name: playerName })
    });
    // Här uppdateras texten på sidan till spelarens namn
    // $('#player-label') väljer elementet med id='player-label' och .text() ändrar texten till spelarens namn
    // $('#playername') väljer elementet med id='playername' och .val('') tömmer input-fältet och .css('display', 'none') gömmer input-fältet
    // $('#save-player') väljer elementet med id='save-player' och .css('display', 'none') gömmer knappen
    $('#player-label').text(playerName)
    $('#playername').val('').css('display', 'none');
    $('#save-player').css('display', 'none');

}

// ************************************************************


// ******** HÄR KÖRS KODEN SOM HÄMTAR ORD FRÅN API ********
// ************************************************************
// Här lyssnar vi på knappen med id='get-word' för ett "click" event och kör funktionen getOneWord() som i sin tur kör funktionen getWords()
// getWords() hämtar ord från servern och returnerar en array med ord
// getOneWord() väljer ett slumpmässigt ord från arrayen och skapar input-fält för varje bokstav i ordet
// ************************************************************
// $('#get-word') väljer elementet med id='get-word' och .on('click', getOneWord) lyssnar på ett "click" event och kör funktionen getOneWord()
$('#get-word').on('click', getOneWord);
async function getWords() {
    // Här skickas en GET-förfrågan till /api/words - en route som vi har skapat via app.Map*** vid våra queries/actions (rad 27 i Queries.cs)
    const response = await fetch('/api/words');

    // Här hämtas texten från svaret på förfrågan
    // Just nu så är texten en lång sträng med ord separerade av radbrytningar -
    // - detta bestäms av routen i Queries.cs (rad 27) vad som returneras när man skickar en GET-förfrågan till /api/words
    const text = await response.text();


    // Här delas texten upp i en array med ord. Och att varje ord i texten ska vara ett element i arrayen. - 
    // Jag definierar att 'words' är en array på rad 1 i denna filen. och att varje ord i texten ska vara ett element i arrayen
    words = text.split('\n').filter(word => word.trim() !== ""); // Remove empty lines

    // Här returneras arrayen med ord
    return words;
}

// ************************************************************


// ******** HÄR KÖRS KODEN SOM SHUFFLAR BOKSTÄVERNA I ETT ORD ********
// ************************************************************
// Här skapas en funktion som tar en array med bokstäver som argument och blandar om bokstäverna
// Funktionen returnerar en sträng med bokstäverna blandade i en slumpmässig ordning. Jag väljer att göra om till en sträng så att jag kan skriva ut bokstäverna på sidan som en mening och inte som en array med bokstäver separerade av kommatecken. Och min jämförelse blir enklare.
// ************************************************************
function shuffleArray(lettersArray) {
    for (let i = lettersArray.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1)); // Random index
        [lettersArray[i], lettersArray[j]] = [lettersArray[j], lettersArray[i]]; // skiftar plats på bokstäverna
    }
    return lettersArray.join(" ");
}

// ************************************************************


// ******** HÄR KÖRS KODEN SOM SKAPAR INPUT-FÄLT FÖR VARJE BOKSTAV I ETT ORD ********
// ************************************************************
// Här skapas en funktion som tar ett ord som argument och skapar ett input-fält för varje bokstav i ordet
// Funktionen väljer en container med id='input-container' och tömmer den (* på rad 33 i index.html *) för att sedan skapa nya input-fält för varje bokstav i ordet
// ************************************************************
function createInputFieldsForWord(word) {
    console.log('Creating input fields for word: ', word);
    const inputContainer = $('#input-container'); // Välj container för input-fält

    inputContainer.empty(); // Töm container

    // Skapa ett input-fält för varje bokstav i ordet
    // Loopa igenom varje bokstav i ordet och skapa ett input-fält för varje bokstav
    for (let i = 0; i < word.length; i++) {
        const input = $('<input>')
            .attr('type', 'text')       // Enkel text-input
            .attr('maxlength', 1)       // Max 1 tecken
            .attr('name', `letter${i}`)
            .attr('id', `letter-input-${i}`) // Unikt id för varje input-fält
            .addClass('letter-input');  // Lägg till klass för styling

        inputContainer.append(input); // Lägg till input-fält i container
    }

    // Skapa en knapp för att skicka in ordet
    // Om det inte redan finns en knapp i container så ska en knapp skapas
    if (!$('#guessword button').length) {
        const submitButton = $('<button>') // Skapa en knapp
            .attr('type', 'submit') // Submit-knapp
            .text('Check Word'); // Text på knappen
        $('#guessword').append(submitButton); // Lägg till knappen i container
    }
}

// ************************************************************


// ******** HÄR KÖRS KODEN SOM HÄMTAR ETT ORD SLUMPAT ORD UT UR LISTAN MED ORD ********
// ************************************************************
// Här skapas en funktion som hämtar en slumpmässig bokstav från listan med ord
// Funktionen använder sig av funktionen getWords() (rad 47 i denna filen) för att hämta en lista med ord
// Funktionen väljer ett slumpmässigt ord från listan och sparar det i en variabel
// Funktionen skapar input-fält för varje bokstav i ordet och blandar om bokstäverna i ordet innan det skrivs ut på sidan - 
// - med hjälp av funktionerna createInputFieldsForWord() (rad 63 i denna filen) och shuffleArray() (rad 41 i denna filen)
// ************************************************************

async function getOneWord(event) {
    // Förhindra att sidan laddas om
    event.preventDefault();

    // Hämta lista med ord
    // Skapa en variabel 'wordList' som innehåller en array med ord som hämtas från funktionen getWords() (rad 47 i denna filen)
    let wordList = await getWords();
    console.log('List of words: ', wordList)

    // Välj ett slumpmässigt ord från listan
    // Skapa en variabel 'randomIndex' som innehåller ett slumpmässigt index från listan med ord
    let randomIndex = Math.floor(Math.random() * wordList.length);
    console.log(randomIndex)

    // Skapa en variabel 'oneRandomWord' som innehåller ett slumpmässigt ord från listan med ord
    oneRandomWord = wordList[randomIndex];
    // Skapa input-fält för varje bokstav i ordet med hjälp av funktionen createInputFieldsForWord() (rad 63 i denna filen)
    createInputFieldsForWord(oneRandomWord);
    console.log(oneRandomWord);
    // Skapa en variabel 'letters' som innehåller en array med bokstäver från det slumpmässiga ordet
    letters = oneRandomWord.split("");
    console.log('Letters before shuffle: ', letters);

    // Blanda om bokstäverna i ordet med hjälp av funktionen shuffleArray() (rad 41 i denna filen)
    let shuffledLetters = shuffleArray(letters);
    console.log('Letters after shuffle: ', shuffledLetters);

    // Ta tag i elementet med id='message' och skriv ut bokstäverna i ordet
    // $('#message') väljer elementet med id='message' och .text() ändrar texten till bokstäverna i ordet
    $('#message').text(letters.join(' '));

}

// ************************************************************


// ******** HÄR KÖRS KODEN SOM JÄMFÖR SPELARENS ORD MED DET SLUMPAT ORD ********
// ************************************************************
// Här skapas en funktion som jämför spelarens ord med det slumpmässiga ordet
// Funktionen förhindrar att sidan laddas om när knappen trycks
// Funktionen skapar en variabel 'word' som innehåller spelarens ord
// Funktionen jämför spelarens ord med det slumpmässiga ordet och skriver ut om ordet är rätt eller fel
// *************************************************************
// $('#guessword') väljer elementet med id='guessword' och .on('submit', checkWord) lyssnar på ett "submit" event och kör funktionen checkWord()
$('#guessword').on('submit', checkWord);
// Den här funktionen körs när spelaren trycker på knappen "Check Word"
async function checkWord(event) {
    event.preventDefault();
    // Hämta ordet som spelaren skrivit in i input-fälten och sätt ihop det till en sträng med hjälp av .join('') -
    // - använder .map() för att loopa igenom varje input-fält och hämta värdet i fältet
    let word = $('#input-container input').map((i, input) => $(input).val()).get().join('');
    console.log('Word: ', word);
    console.log('Random word: ', oneRandomWord);

    // Jämför spelarens ord med det slumpmässiga ordet
    // Om spelarens ord är lika med det slumpmässiga ordet så skrivs "Correct!" ut på sidan
    // Om spelarens ord inte är lika med det slumpmässiga ordet så skrivs "Incorrect!" ut på sidan
    if (word.toLowerCase() === oneRandomWord.toLowerCase()) {
        $('#message').text('Correct!');
    } else {
        $('#message').text('Incorrect!');
    }
}

// ************************************************************




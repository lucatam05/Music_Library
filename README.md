# Music Library Service

Microservizio .NET 9 responsabile della gestione delle librerie musicali degli utenti. Permette di aggiungere, rimuovere e visualizzare le canzoni salvate, comunicando in modo sincrono con il CatalogueService e in modo asincrono con il UserService tramite Kafka.

---

## Composizione del progetto

Il servizio è strutturato in 5 progetti secondo l'architettura a layer:

| Progetto | Responsabilità |
|---|---|
| `Music.Library.WebApi` | Controller REST, configurazione Swagger, middleware, consumer/producer Kafka |
| `Music.Library.Business` | Logica applicativa, orchestrazione delle operazioni |
| `Music.Library.Repository` | Accesso al database tramite Entity Framework Core |
| `Music.Library.ClientHttp` | Client HTTP pubblicato come pacchetto NuGet per gli altri microservizi |
| `Music.Library.Shared` | DTO, eventi Kafka e modelli condivisi |

---

## Cosa fa

Il LibraryService gestisce la libreria personale di ogni utente. Ogni utente ha una sola libreria, creata automaticamente alla registrazione.

### Endpoint esposti

- `GET /Library/GetLibraryPerIdAsync` — visualizza le canzoni della libreria dell'utente autenticato
- `POST /Library/AddSongToLibrary?songId=` — aggiunge una canzone alla libreria tramite SpotifyId
- `DELETE /Library/RemoveSongFromLibrary?songId=` — rimuove una canzone dalla libreria
- `PUT /Library/UpdateLibraryName?nome=` — aggiorna il nome della libreria

> L'endpoint `CreateLibrary` è nascosto da Swagger ed è riservato esclusivamente al UserService.

### Flusso di aggiunta canzone

1. L'utente invia lo SpotifyId della canzone
2. Il Business recupera la libreria dell'utente dal DB
3. Chiama il CatalogueService via HTTP per verificare che la canzone esista e ottenerne i dettagli
4. Salva la canzone nel database locale
5. Pubblica un evento Kafka `song-added-to-library` per notificare il UserService

---

## Comunicazioni

- **HTTP in uscita**: chiama il CatalogueService tramite il pacchetto `Music.Catalogue.ClientHttp`
- **Kafka (producer)**: pubblica eventi `song-added-to-library` e `song-removed-from-library`
- **In entrata (HTTP)**: riceve chiamate dal UserService tramite il pacchetto NuGet `Music.Library.ClientHttp`

---

## Autenticazione

Gli endpoint sono protetti tramite JWT. Il token viene emesso dal UserService al momento del login e deve essere incluso nell'header:

```
Authorization: Bearer <token>
```

---

## Tecnologie

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core con PostgreSQL
- Kafka (tramite `Utility.Kafka.2025`)
- JWT Bearer Authentication
- Swagger / OpenAPI

---

## Come eseguire in locale

Il modo consigliato è tramite Docker Compose dal repository [Music_Compose](https://github.com/lucatam05/Music_Compose).

---

## Pacchetto NuGet

Il progetto pubblica il pacchetto `Music.Library.ClientHttp` e `Music.Library.Shared` su GitHub Packages, usato dallo UserService per comunicare con questo servizio.


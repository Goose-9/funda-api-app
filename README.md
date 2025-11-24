# 📘 Funda API Assignment — Real Estate Stats

This project implements the assignment from the Funda Partner API exercise.
It retrieves real-estate listings for Amsterdam and determines which makelaars (real estate agents) have the most properties listed for sale.

Two rankings are produced:

1. **Top 10 makelaars that are selling houses (koop) in Amsterdam**

2. **Top 10 makelaars that are selling houses (koop) with a graden (tuin) in Amsterdam**

The project is written in C# (.NET 8).

## 📦 How to Run

**1. Clone the repository**
``` bash
git clone https://github.com/Goose-9/funda-api-app
cd funda-api-app
```

**2. Add the API key**

Create a `.env` file inside `FundaStats.App/`:
``` ini
FUNDA_API_KEY=your-funda-key-here
```

**3. Run the app**
``` bash
dotnet run --project FundaStats.App
```
The console will display two formatted Top-10 tables.

## 🧱 Architecture Overview
I tried to split the solution into three layers (or four if you include tests), each with a clear responsibility: 

``` lua
FundaStats.Core/
 ├── Funda/          <-- HTTP client + DTOs + search query
 └── Stats/          <-- Pure logic: grouping & ranking makelaars

FundaStats.App/      <-- Console app: orchestration + output
FundaStats.Tests/    <-- Unit tests for stats
```

### Funda Layer (Data Access)
- `FundaClient` fetches all the pages of listings using the API. 
- The API response is then deserialized into small DTOs:
    - `FundaResponseDTO`
    - `FundaObjectTO`
    - `PagingInfo`
- The client does not contain presentation logic or stats logic
- The client also runs a simple inline rate limiting delay (`Task.Delay`) between page requests. The reason why I did not create a dedicated rate limiter component is explained in [Rate-Limiting](#rate-limiting)

### Stats Layer (Logic)
- `MakelaarStatsService` calculates the Top N makelaars, which in this case is 10. 
- I group the objects based on the unique `MakelaarId` and the names come from the first item in each group. 

### App Layer (Console Output)
- Loads the API key from `.env`
- Creates and calls the Funda client
- Calls the stats service for each query.
- Prints the final tables

### Extra Test Layer
- I wrote some simple sanity tests for myself. I did this to verify that my logic in the stats service was correct. 

## 🌐 Funda API Behaviour

### Page Size
Through testing, I noticed that the page size parameter did not return more than 25 objects from the API, thus I couldn't increase the page size to reduce the number of requests. The page sizes accepted (returned the same number of objects) by the API is 0-25 with a default of 15 when the parameter is omitted. Therefore I have decided to leave it at 25 for my implementation. 

## 🧭 Rate-limiting: 
After exceeding the usage limit of 100 requests per minute, the API appears to start returning 401 Unauthorized. In this case, the client stops with a clear error instead of retrying, because retrying will not succeed until the limit resets. 

So to avoid this error response, I created a dedicated rate limiting layer. This layer is fully separate from the client and is built around one central abstraction: 

``` C#
public interface IRateLimiter
{
    ValueTask WaitAsync(CancellationToken cancellationToken = default);
}
```
To ensure that all outgoing requests respect the active rate limiting algorithm, every request to the Funda API starts by calling: 
``` C#
await _rateLimiter.WaitAsync(token);
```

### Implementations
*To switch between implementations, comment out the lines below from ```Program.cs``` and uncomment the one you would like to use.*
1. FixedDelayRateLimiter (Simple Example)

    The simplest implementation, which uses a constant delay between requests: 
    ``` C#
    new FixedDelayRateLimiter(TimeSpan.FromMilliseconds(600));
    ```
    This guarantees at most 100 requests/minute by spacing requests 600 ms apart. It is easy to understand and it is useful to demonstrate how easy it is to extend the base interface with more algorithms. 

2. TokenBucketRateLimiter (Primary Algorithm)

    My main implementation uses a token bucket that allows up to N tokens per minute, where each token allows one request to be sent.

    The bucket starts empty and slowly fill with tokens over time (e.g., 100 tokens/minute ≈ 1.667 tokens/second). If a token is available, a request is sent immediately, and if not, the caller waits until a token has been accumulated, before consuming the token and sending. 

    ``` C#
    new TokenBucketRateLimiter(maxTokensPerMinute: 100);
    ```
    This allows a smoother request pattern while achieving [higher throughput](#performance) than fixed delays. 

Thanks to the ```IRateLimiter``` interface, any new implementation is easy to add and use. 

### Performance
Using the same Funda dataset (214 pages + 42 pages = 256 pages):

| Rate Limiter | Configuration      | Duration       |
| ------------ | ------------------ | -------------- |
| Fixed Delay  | 600 ms per request | **3 min 20 s** |
| Token Bucket | 100 tokens/minute  | **2 min 33 s** |

Token bucket is ~23% faster whhile still staying safely withing the API's limits. 

## 🧪 Testing
Unit tests are located in: 
``` 
FundaStats.Tests
    - Stats
    - RateLimiting
```

I wrote simple tests to verify that the Stats Service logic and Token Bucket logic was correct, but this can obviously be extended to be more conclusive. 

## 📊 Output Example
The console prints two labels such as: 
```
Loading Funda API client...
Fetching Funda data... this may take a while.

Amsterdam: fetched 5308 objects.

Top 10 makelaars in Amsterdam (koop)
------------------------------------
#   ID         Makelaar                                  Objects
1   24648      Heeren Makelaars                              164
2   24607      KRK Makelaars Amsterdam                       107
3   24067      Broersma Wonen                                104
4   24592      Ramón Mossel Makelaardij o.g. B.V.             99
5   24705      Eefje Voogd Makelaardij                        92
6   24605      Hallie & Van Klooster Makelaardij              86
8   24131      De Graaf & Groot Makelaars                     76
9   60557      Linger OG Makelaars en Taxateurs               74
10  24079      Makelaardij Van der Linden Amsterdam           74

Amsterdam + tuin: fetched 1048 objects.
Top 10 makelaars in Amsterdam (koop + tuin)
-------------------------------------------
#   ID         Makelaar                                  Objects
1   24067      Broersma Wonen                                 28
2   24648      Heeren Makelaars                               27
4   24599      DSTRCT Amsterdam                               20
5   24131      De Graaf & Groot Makelaars                     19
6   24848      KIJCK. makelaars Amsterdam                     18
7   24065      Carla van den Brink B.V.                       17
8   24607      KRK Makelaars Amsterdam                        16
9   24630      Keizerskroon Makelaars - Certified Ex...       14
10  12285      Makelaarsland                                  14
```

## 🤖AI Usage: 

1. I used ChatGPT to generate a BuildUrl function for my FundaClient class. I did this because it was a simple string building function, which is trivial to make once I knew the url string I wanted and the variables that I had. 

```C#    
private string BuildUrl(FundaSearchQuery query, int page)
    {
        // Example: 
        // http://partnerapi.funda.nl/feeds/Aanbod.svc/json/{key}/?type=koop&zo=/amsterdam/&page=1&pagesize=25
        var uri =
            $"feeds/Aanbod.svc/json/{_apiKey}/" +
            $"?type={Uri.EscapeDataString(query.Type)}" +
            $"&zo={Uri.EscapeDataString(query.Path)}" +
            $"&page={page}" +
            $"&pagesize={query.PageSize}";

        return uri;
    }
```

2. I used ChatGPT to generate console messages and to make the output table pretty. This is documented in ```Program.cs```, but it is the extra helper functions below the try-catch. I did this to make the output more readable, without having to waste a lot of my time to do so.

3. I used ChatGPT to generate some of the visuals seen in the README file, namely the icons, and the visual layer separation under the Architecture Overview header. My reasoning behind the visual is that I believe it helps to reinforce my explanation underneath, and my reasoning for the icons is that it is fun. 😀

All generated code was reviewed, modified, or rewritten where appropriate. 





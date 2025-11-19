# funda-api-app


Through testing, I noticed that the page size parameter did not return more than 25 objects from the API, thus I couldn't increase the page size to reduce the number of requests. The page sizes accepted (returned the same number of objects) by the API is 0-25 with a default of 15 when the parameter is omitted. Therefore I have decided to leave it at 25 for my implementation. 


Rate-limiting: 

As the API limited me to only 100 requests per minute, my first idea to limit the request rate was to simply use a Task delay for 600-700ms to prevent the requests from hitting the cap. 

After exceeding the usage limits, the API appears to start returning 401 Unauthorized. In this case, the client stops with a clear error instead of retrying, because retrying will not succeed until the limit resets. 

So to mitigate this, I used the small delay between page requests as mentioned before. 

AI: 

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





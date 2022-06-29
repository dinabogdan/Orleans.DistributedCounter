using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SessionStateWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    private const string SessionKey = "FirstVisitTimestamp";
    public DateTimeOffset DateFirstSeen;
    public DateTimeOffset Now = DateTimeOffset.Now;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGet()
    {
        await HttpContext.Session.LoadAsync();

        if (HttpContext.Session.TryGetValue(SessionKey, out var sessionDate))
        {
            DateFirstSeen = JsonSerializer.Deserialize<DateTimeOffset>(sessionDate);
        }
        else
        {
            DateFirstSeen = Now;
            HttpContext.Session.Set(SessionKey, JsonSerializer.SerializeToUtf8Bytes(DateFirstSeen));
        }
    }
}
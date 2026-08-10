using Microsoft.AspNetCore.Components;
using Taurus.Application.Tickets;

namespace Taurus.Components.Features.Tickets;

public partial class Tickets
{
    private const int DefaultPageSize = 20;

    // TODO TAU: Replace this temporary hard-coded status rule when
    // "Support status filtering" establishes the Taurus ticket status representation.
    private const int CompletedStatusId = 3;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private ITicketService TicketService { get; set; } = default!;

    private IReadOnlyList<Ticket> TicketItems { get; set; } = [];

    private int PageSize { get; set; }

    private int CurrentPage { get; set; } = 1;

    private int PageCount => Math.Max(1, (int)Math.Ceiling(TicketItems.Count / (double)PageSize));

    private IEnumerable<Ticket> PagedTicketItems =>
        TicketItems
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize);

    protected override async Task OnInitializedAsync()
    {
        PageSize = Configuration.GetValue("Tickets:PageSize", DefaultPageSize);

        if (PageSize <= 0)
        {
            PageSize = DefaultPageSize;
        }

        await LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        TicketItems = await TicketService.GetTicketsAsync();

        if (CurrentPage > PageCount)
        {
            CurrentPage = PageCount;
        }
    }

    private static string GetTicketClass(Ticket ticket)
    {
        return ticket.StatusId == CompletedStatusId
            ? "ticket-row ticket-completed"
            : "ticket-row";
    }

    private static string GetMobileTicketClass(Ticket ticket)
    {
        return ticket.StatusId == CompletedStatusId
            ? "mobile-ticket ticket-completed"
            : "mobile-ticket";
    }

    private static string FormatLastUpdated(DateTimeOffset lastModified)
    {
        var elapsed = DateTimeOffset.Now - lastModified.ToLocalTime();

        if (elapsed < TimeSpan.Zero)
        {
            return "just now";
        }

        if (elapsed.TotalMinutes < 1)
        {
            return "just now";
        }

        if (elapsed.TotalHours < 1)
        {
            var minutes = Math.Max(1, (int)elapsed.TotalMinutes);
            return $"{minutes} min{(minutes == 1 ? string.Empty : "s")} ago";
        }

        if (elapsed.TotalDays < 1)
        {
            var hours = Math.Max(1, (int)elapsed.TotalHours);
            return $"{hours} hr{(hours == 1 ? string.Empty : "s")} ago";
        }

        if (elapsed.TotalDays < 7)
        {
            var days = Math.Max(1, (int)elapsed.TotalDays);
            return $"{days} day{(days == 1 ? string.Empty : "s")} ago";
        }

        return lastModified.ToLocalTime().ToString("dd MMM yyyy");
    }
}
namespace Hipsy.Net.Models;

/// <summary>One page of results from a paginated endpoint.</summary>
public sealed record PagedResponse<T>
{
    /// <summary>The items on this page.</summary>
    public IReadOnlyList<T> Data { get; init; } = [];

    /// <summary>Links to the first, last, previous and next page.</summary>
    public PageLinks Links { get; init; } = new();

    /// <summary>Paging totals.</summary>
    public PageMeta Meta { get; init; } = new();

    /// <summary>True if there is a page after this one.</summary>
    public bool HasNextPage => Meta.CurrentPage < Meta.LastPage;
}

/// <summary>Page links. On the reviews endpoint these only contain a query string such as <c>?page=2</c>.</summary>
public sealed record PageLinks
{
    /// <summary>The first page.</summary>
    public string? First { get; init; }
    /// <summary>The last page.</summary>
    public string? Last { get; init; }
    /// <summary>The previous page, or null.</summary>
    public string? Prev { get; init; }
    /// <summary>The next page, or null.</summary>
    public string? Next { get; init; }
}

/// <summary>Paging totals.</summary>
public sealed record PageMeta
{
    /// <summary>The current page number (1-based).</summary>
    public int CurrentPage { get; init; }
    /// <summary>The position of the first item on this page, or null if the page is empty.</summary>
    public int? From { get; init; }
    /// <summary>The last page number.</summary>
    public int LastPage { get; init; }
    /// <summary>The endpoint URL without query string.</summary>
    public string? Path { get; init; }
    /// <summary>The number of items per page.</summary>
    public int PerPage { get; init; }
    /// <summary>The position of the last item on this page, or null if the page is empty.</summary>
    public int? To { get; init; }
    /// <summary>The total number of items.</summary>
    public int Total { get; init; }
}

/// <summary>Filters for the list events endpoint.</summary>
public sealed record EventQuery
{
    /// <summary>Which events to return. The API defaults to <see cref="EventPeriod.Upcoming"/>.</summary>
    public EventPeriod? Period { get; init; }

    /// <summary>Include concept (draft) events.</summary>
    public bool IncludeConcepts { get; init; }

    /// <summary>Events per page, 1 to 100. The API defaults to 25.</summary>
    public int? Limit { get; init; }

    /// <summary>The page number, starting at 1.</summary>
    public int? Page { get; init; }
}

/// <summary>Filters for the list orders endpoint.</summary>
public sealed record OrderQuery
{
    /// <summary>Only return orders of these events.</summary>
    public IReadOnlyCollection<int>? EventIds { get; init; }

    /// <summary>Orders per page, 1 to 100. The API defaults to 25.</summary>
    public int? Limit { get; init; }

    /// <summary>The page number, starting at 1.</summary>
    public int? Page { get; init; }
}

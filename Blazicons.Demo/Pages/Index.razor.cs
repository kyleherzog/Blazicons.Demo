using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using Blazicons.Demo.Components;
using Blazicons.Demo.Models;
using Blazor.Analytics;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Blazicons.Demo.Pages;

public partial class Index : IDisposable
{
    private readonly List<IconEntry> filteredIcons = [];
    private string? activeQuery;
    private bool areaFiltersExpanded;
    private bool hasDisposed;
    private string libraryFilter = string.Empty;
    private RenderFragment? libraryFilterContent;
    private IDisposable? queryChangedSubscription;

    public Index()
    {
        Search = new IconSearchModel();
        SubscribeToChanges();
    }

    ~Index()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public IconEntry ActiveIcon { get; set; } = new IconEntry();

    public string? ActiveQuery
    {
        get
        {
            return activeQuery;
        }

        set
        {
            if (activeQuery != value)
            {
                activeQuery = value;
                LoadFilteredIcons();
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    [Inject]
    public IAnalytics? Analytics { get; set; }

    public bool AreaFiltersExpanded
    {
        get
        {
            return areaFiltersExpanded;
        }

        set
        {
            if (areaFiltersExpanded != value)
            {
                areaFiltersExpanded = value;
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    public string FilterAreaClass => AreaFiltersExpanded ? "mt-1 mt-md-3" : "d-none d-md-block mt-1 mt-md-3";

    public string FilterAreaTogglerClass => AreaFiltersExpanded ? "d-none" : "d-md-none";

    public IList<IconEntry> FilteredIcons
    {
        get
        {
            return filteredIcons;
        }
    }

    public IList<FontLibrarySelection> Filters { get; } = new List<FontLibrarySelection>();

    public IList<IconEntry> Icons { get; } = new List<IconEntry>();

    public string? IconsFilteredCount => filteredIcons.Count.ToString("N0");

    public string IconsTotalCount => Icons.Count.ToString("N0");

    public bool IsAdminMode { get; set; }

    public bool IsSelectingMultiples { get; set; }

    public bool IsShowingAddKeywordModal { get; set; }

    public bool IsShowingModal { get; set; }

    public KeywordAddModel KeywordsToAdd { get; set; } = new();

    public string LibraryFilter
    {
        get
        {
            return libraryFilter;
        }

        set
        {
            if (libraryFilter != value)
            {
                libraryFilter = value;
                foreach (var filter in Filters)
                {
                    filter.ParentFilterChanged(value);
                }

                LoadFilteredIcons();
                StateHasChanged();
            }
        }
    }

    public RenderFragment? LibraryFilterContent
    {
        get
        {
            return libraryFilterContent;
        }

        set
        {
            if (libraryFilterContent != value)
            {
                libraryFilterContent = value;
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    public IconSearchModel Search { get; }

    public IEnumerable<IconEntry> SelectedIcons => Icons.Where(x => x.IsSelected);

    public Virtualize<IconEntry>? VirtualizedIcons { get; set; }

    [Inject]
    private IBlazorDownloadFileService FileDownloader { get; set; } = default!;

    [Inject]
    private KeywordsManager KeywordsManager { get; set; } = default!;

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void HandleAddKeywordsClick()
    {
        KeywordsToAdd = new();
        IsShowingAddKeywordModal = true;
    }

    public void HandleDeselectAllClick()
    {
        foreach (var item in FilteredIcons)
        {
            item.IsSelected = false;
        }
    }

    public void HandleMultipleSelectClick()
    {
        IsSelectingMultiples = !IsSelectingMultiples;
        if (!IsSelectingMultiples)
        {
            foreach (var item in SelectedIcons)
            {
                item.IsSelected = false;
            }
        }
    }

    public void HandleSelectAllClick()
    {
        foreach (var item in FilteredIcons)
        {
            item.IsSelected = true;
        }
    }

    public void HideAddKeywordsModal()
    {
        IsShowingAddKeywordModal = false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!hasDisposed)
        {
            if (disposing)
            {
                UnsubsribeFromChanges();
            }

            hasDisposed = true;
        }
    }

    protected override Task OnInitializedAsync()
    {
        var uri = new Uri(Navigation.Uri);
        IsAdminMode = uri.AbsolutePath.EndsWith("admin", StringComparison.OrdinalIgnoreCase);

        AddLibraryIcons(typeof(MdiIcon));
        AddLibraryIcons(typeof(FontAwesomeRegularIcon));
        AddLibraryIcons(typeof(FontAwesomeSolidIcon));
        AddLibraryIcons(typeof(BootstrapIcon));
        AddLibraryIcons(typeof(GoogleMaterialOutlinedIcon));
        AddLibraryIcons(typeof(GoogleMaterialFilledIcon));
        AddLibraryIcons(typeof(GoogleMaterialRoundIcon));
        AddLibraryIcons(typeof(GoogleMaterialSharpIcon));
        AddLibraryIcons(typeof(GoogleMaterialTwoToneIcon));
        AddLibraryIcons(typeof(Ionicon));
        AddLibraryIcons(typeof(FluentUiIcon));
        AddLibraryIcons(typeof(FluentUiFilledIcon));

        LoadFilteredIcons();

        return base.OnInitializedAsync();
    }

    private void AddLibraryIcons(Type type)
    {
        var properties = type.GetProperties();

        properties = properties.OrderBy(x => x.Name).ToArray();

        foreach (var property in properties)
        {
            var icon = (SvgIcon?)property.GetValue(null);
            if (icon is not null)
            {
                var entry = new IconEntry
                {
                    Name = property.Name,
                    Icon = icon,
                    Library = type.Name,
                    Assembly = type.Assembly?.GetName().Name ?? string.Empty,
                };

                var key = entry.Code;
                if (KeywordsManager.Keywords.ContainsKey(key))
                {
                    entry.Keywords = KeywordsManager.Keywords[key];
                }

                Icons.Add(entry);
            }
        }
    }

    private void HandleAddKeywordsSubmit()
    {
        if (!string.IsNullOrEmpty(KeywordsToAdd.Keywords))
        {
            var lowered = KeywordsToAdd.Keywords.ToLowerInvariant();
            var keywords = lowered.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var icon in SelectedIcons)
            {
                foreach (var keyword in keywords)
                {
                    KeywordsManager.AddKeyword(icon.Code, keyword);
                }

                icon.Keywords = KeywordsManager.Keywords[icon.Code];
            }
        }

        HideAddKeywordsModal();

        foreach (var item in SelectedIcons)
        {
            item.IsSelected = false;
        }
    }

    private async Task HandleExportClick()
    {
        var serialized = JsonSerializer.Serialize(KeywordsManager.Keywords, new JsonSerializerOptions { WriteIndented = true });
        await FileDownloader.DownloadFileFromText("SearchMeta.json", serialized, Encoding.Unicode, "text/json", true).ConfigureAwait(true);
    }

    private void HandleFilterExpandToggle()
    {
        AreaFiltersExpanded = !AreaFiltersExpanded;
    }

    private void HandleSubmit()
    {
        ActiveIcon.Keywords = ActiveIcon.KeywordsPending ?? string.Empty;
        KeywordsManager.Keywords[ActiveIcon.Code] = ActiveIcon.Keywords.ToLowerInvariant();
        HideModal();
    }

    private void HideModal()
    {
        ActiveIcon.KeywordsPending = null;
        IsShowingModal = false;
    }

    private void LoadFilteredIcons()
    {
        var result = Icons.AsEnumerable();
        if (!string.IsNullOrEmpty(ActiveQuery))
        {
            var queryWords = ActiveQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            result = Icons.Where(x => queryWords.TrueForAll(w => x.SearchTerms.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrEmpty(LibraryFilter))
        {
            result = result.Where(x => x.Library == LibraryFilter);
        }

        filteredIcons.Clear();
        filteredIcons.AddRange(result);
    }

    private void SelectIcon(IconEntry entry)
    {
        if (IsSelectingMultiples)
        {
            entry.IsSelected = !entry.IsSelected;
        }
        else
        {
            ActiveIcon = entry;
            ShowModal();

            if (Analytics is not null)
            {
                _ = Analytics.TrackEvent("select_content", new { content_type = "icon", item_id = entry.Code }).ConfigureAwait(true);
            }
        }
    }

    private void ShowModal()
    {
        IsShowingModal = true;
    }

    private void SubscribeToChanges()
    {
        queryChangedSubscription = Search.WhenPropertyChanged.Throttle(TimeSpan.FromMilliseconds(400)).Subscribe(x => ActiveQuery = Search.Query);
    }

    private void UnsubsribeFromChanges()
    {
        queryChangedSubscription?.Dispose();
    }
}
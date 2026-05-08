using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Blazicons.Demo.Components;
using Blazicons.Demo.Models;
using Blazor.Analytics;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;

namespace Blazicons.Demo.Pages;

public partial class Index : IDisposable
{
    private const string CheckeredStyle =
        "background-color: #ffffff; background-image: linear-gradient(45deg, #cccccc 25%, transparent 25%), linear-gradient(-45deg, #cccccc 25%, transparent 25%), linear-gradient(45deg, transparent 75%, #cccccc 75%), linear-gradient(-45deg, transparent 75%, #cccccc 75%); background-size: 16px 16px; background-position: 0 0, 0 8px, 8px -8px, -8px 0px;";

    private static readonly JsonSerializerOptions defaultExportOptions = new() { WriteIndented = true };

    [GeneratedRegex(@"\s+(?:width|height)=(?:'[^']*'|""[^""]*"")", RegexOptions.IgnoreCase)]
    private static partial Regex SvgDimensionAttributeRegex();
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

    public ImageDownloadOptionsModel DownloadOptions { get; set; } = new();

    public string FilterAreaClass => AreaFiltersExpanded ? "mt-1 mt-md-3" : "d-none d-md-block mt-1 mt-md-3";

    public string FilterAreaToggleClass => AreaFiltersExpanded ? "d-none" : "d-md-none";

    public IList<IconEntry> FilteredIcons
    {
        get
        {
            return filteredIcons;
        }
    }

    public IList<FontLibrarySelection> Filters { get; } = [];

    public IList<IconEntry> Icons { get; } = [];

    public string? IconsFilteredCount => filteredIcons.Count.ToString("N0");

    public string IconsTotalCount => Icons.Count.ToString("N0");

    public bool IsAdminMode { get; set; }

    public bool IsSelectingMultiples { get; set; }

    public bool IsShowingAddKeywordModal { get; set; }

    public bool IsShowingAdvancedDownloadModal { get; set; }

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

    private string AdvancedPreviewBgStyle
    {
        get
        {
            if (DownloadOptions.TransparentBackground)
                return string.Empty;
            var radius = DownloadOptions.CornerRadius > 0
                ? $"border-radius: {DownloadOptions.CornerRadius}px;"
                : string.Empty;
            return $"position: absolute; inset: 0; background-color: {DownloadOptions.BackgroundColor}; {radius}";
        }
    }

    private string AdvancedPreviewStyle =>
        $"width: 128px; height: 128px; position: relative; display: inline-block; {CheckeredStyle}";

    private string PreviewIconInsetStyle
    {
        get
        {
            const int previewSize = 128;
            var pad = DownloadOptions.Size > 0
                ? (int)Math.Round(previewSize * (double)DownloadOptions.Padding / DownloadOptions.Size)
                : 0;
            var iconSize = previewSize - (2 * pad);
            var flipX = DownloadOptions.FlipHorizontal ? -1 : 1;
            var flipY = DownloadOptions.FlipVertical ? -1 : 1;
            var transform = $"rotate({DownloadOptions.Rotation}deg) scaleX({flipX}) scaleY({flipY})";
            return $"position: absolute; top: {pad}px; left: {pad}px; width: {iconSize}px; height: {iconSize}px; z-index: 1; transform: {transform}; transform-origin: center;";
        }
    }

    [Inject]
    private IBlazorDownloadFileService FileDownloader { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private KeywordsManager KeywordsManager { get; set; } = default!;

    private string PreviewSvgContent
    {
        get
        {
            var markup = ActiveIcon.Icon.Markup
                .Replace("currentColor", DownloadOptions.ForegroundColor, StringComparison.OrdinalIgnoreCase);
            return NormalizeSvgMarkup(markup, "width: 100%; height: 100%; display: block;");
        }
    }

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

    public void HideAdvancedDownloadModal()
    {
        IsShowingAdvancedDownloadModal = false;
    }

    public void ShowAdvancedDownloadModal()
    {
        DownloadOptions = new ImageDownloadOptionsModel();
        IsShowingAdvancedDownloadModal = true;
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await JSRuntime.InvokeVoidAsync("blaziconsDemo.initTooltips").ConfigureAwait(true);
        await JSRuntime.InvokeVoidAsync("blaziconsDemo.initPopovers").ConfigureAwait(true);
        await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);
    }

    protected override Task OnInitializedAsync()
    {
        var uri = new Uri(Navigation.Uri);
        IsAdminMode = uri.AbsolutePath.EndsWith("admin", StringComparison.OrdinalIgnoreCase);

        AddLibraryIcons(typeof(BootstrapIcon));
        AddLibraryIcons(typeof(DeviconLine));
        AddLibraryIcons(typeof(DeviconOriginal));
        AddLibraryIcons(typeof(DeviconPlain));
        AddLibraryIcons(typeof(FlagIcon4x3));
        AddLibraryIcons(typeof(FlagIcon1x1));
        AddLibraryIcons(typeof(FluentUiIcon));
        AddLibraryIcons(typeof(FluentUiFilledIcon));
        AddLibraryIcons(typeof(FontAwesomeRegularIcon));
        AddLibraryIcons(typeof(FontAwesomeSolidIcon));
        AddLibraryIcons(typeof(GoogleMaterialFilledIcon));
        AddLibraryIcons(typeof(GoogleMaterialOutlinedIcon));
        AddLibraryIcons(typeof(GoogleMaterialRoundIcon));
        AddLibraryIcons(typeof(GoogleMaterialSharpIcon));
        AddLibraryIcons(typeof(GoogleMaterialTwoToneIcon));
        AddLibraryIcons(typeof(Ionicon));
        AddLibraryIcons(typeof(Lucide));
        AddLibraryIcons(typeof(MdiIcon));

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
                if (KeywordsManager.Keywords.TryGetValue(key, out var value))
                {
                    entry.Keywords = value;
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

    private async Task HandleAdvancedDownloadSubmit()
    {
        var foregroundColor = DownloadOptions.ForegroundColor;
        var backgroundColor = DownloadOptions.TransparentBackground ? string.Empty : DownloadOptions.BackgroundColor;
        var cornerRadius = DownloadOptions.TransparentBackground ? 0 : DownloadOptions.CornerRadius;
        var svgContent = NormalizeSvgMarkup(
            ActiveIcon.Icon.Markup.Replace("currentColor", foregroundColor, StringComparison.OrdinalIgnoreCase));
        var fileName = $"{ActiveIcon.Name}.png";
        await JSRuntime.InvokeVoidAsync("blaziconsDemo.downloadSvgAsPng", svgContent, fileName, DownloadOptions.Size, backgroundColor, cornerRadius, DownloadOptions.Padding, DownloadOptions.Rotation, DownloadOptions.FlipHorizontal, DownloadOptions.FlipVertical).ConfigureAwait(true);
        HideAdvancedDownloadModal();
    }

    private async Task HandleDownloadPngClick()
    {
        var svgContent = NormalizeSvgMarkup(
            ActiveIcon.Icon.Markup.Replace("currentColor", "#000000", StringComparison.OrdinalIgnoreCase));
        var fileName = $"{ActiveIcon.Name}.png";
        await JSRuntime.InvokeVoidAsync("blaziconsDemo.downloadSvgAsPng", svgContent, fileName, 256, string.Empty, 0, 0, 0, false, false).ConfigureAwait(true);
    }

    private async Task HandleDownloadSvgClick()
    {
        var svgContent = NormalizeSvgMarkup(
            ActiveIcon.Icon.Markup.Replace("currentColor", "#000000", StringComparison.OrdinalIgnoreCase));
        var fileName = $"{ActiveIcon.Name}.svg";
        await FileDownloader.DownloadFileFromText(fileName, svgContent, Encoding.UTF8, "image/svg+xml", true).ConfigureAwait(true);
    }

    private async Task HandleExportClick()
    {
        var serialized = JsonSerializer.Serialize(KeywordsManager.Keywords, defaultExportOptions);
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

    private static string NormalizeSvgMarkup(string markup, string? additionalStyle = null)
    {
        // Remove explicit width/height attributes so the SVG scales to its container/destination
        var result = SvgDimensionAttributeRegex().Replace(markup, string.Empty);

        // Add xmlns namespace declaration if not already present
        if (!result.Contains("xmlns=", StringComparison.OrdinalIgnoreCase))
        {
            result = result.Replace("<svg ", "<svg xmlns=\"http://www.w3.org/2000/svg\" ", StringComparison.OrdinalIgnoreCase);
        }

        // Inject an inline style to control rendered dimensions when needed (e.g., preview)
        if (!string.IsNullOrEmpty(additionalStyle))
        {
            result = result.Replace("<svg ", $"<svg style=\"{additionalStyle}\" ", StringComparison.OrdinalIgnoreCase);
        }

        return result;
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
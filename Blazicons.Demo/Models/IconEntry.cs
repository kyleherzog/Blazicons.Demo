using CodeCasing;

namespace Blazicons.Demo.Models;

public class IconEntry
{
    private string keywords = string.Empty;

    private string? keywordsPending;
    private string name = string.Empty;

    private string searchTerms = string.Empty;

    public string Assembly { get; set; } = string.Empty;

    public string BadgeLink => $"https://img.shields.io/nuget/v/{Assembly}?label={Assembly}&logo=nuget";

    public string Code => $"{Library}.{Name}";

    public string CopyExampleLink => $"javascript:navigator.clipboard.writeText('{Example}');";

    public string CopyNameLink => $"javascript:navigator.clipboard.writeText('{Code}');";

    public string CopyPackageLink => $"javascript:navigator.clipboard.writeText('{Assembly}');";

    public string DisplayName => Name.ExpandToTitleCase() ?? string.Empty;

    public string Example => $"<Blazicon Svg=\"{Code}\" />";

    public SvgIcon Icon { get; set; } = SvgIcon.FromContent(string.Empty);

    public bool IsSelected { get; set; }

    public string Keywords
    {
        get
        {
            return keywords;
        }

        set
        {
            if (keywords != value)
            {
                keywords = value;
                searchTerms = $"{Name} {keywords}";
            }
        }
    }

    public string? KeywordsPending
    {
        get
        {
            return keywordsPending ?? Keywords;
        }

        set
        {
            keywordsPending = value;
        }
    }

    public int KeywordsPendingCount
    {
        get
        {
            if (string.IsNullOrEmpty(KeywordsPending))
            {
                return 0;
            }

            var words = KeywordsPending.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }
    }

    public string Library { get; set; } = string.Empty;

    public string Name
    {
        get
        {
            return name;
        }

        set
        {
            if (name != value)
            {
                name = value;
                searchTerms = $"{Name} {keywords}";
            }
        }
    }

    public string NugetAddress => $"https://www.nuget.org/packages/{Assembly}";

    public string SearchTerms => searchTerms;
}
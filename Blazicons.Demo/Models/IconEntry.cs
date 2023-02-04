namespace Blazicons.Demo.Models;

public class IconEntry
{
    public string Assembly { get; set; } = string.Empty;

    public string BadgeLink => $"https://img.shields.io/nuget/v/{Assembly}?label={Assembly}&logo=nuget";

    public string Name { get; set; } = string.Empty;

    public SvgIcon Icon { get; set; } = SvgIcon.FromContent(string.Empty);

    public string Library { get; set; } = string.Empty;

    public string Code => $"{Library}.{Name}";
}
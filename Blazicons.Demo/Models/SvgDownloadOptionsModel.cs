namespace Blazicons.Demo.Models;

public class SvgDownloadOptionsModel
{
    public string BackgroundColor { get; set; } = "#ffffff";

    public int CornerRadius { get; set; } = 0;

    public int Padding { get; set; } = 0;

    public string ForegroundColor { get; set; } = "#000000";

    public int Size { get; set; } = 256;

    public bool TransparentBackground { get; set; } = true;

    public int Rotation { get; set; } = 0;

    public bool FlipHorizontal { get; set; } = false;

    public bool FlipVertical { get; set; } = false;
}
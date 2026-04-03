namespace SmrtDoodle.Models;

public class CanvasSettings
{
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
    public float Dpi { get; set; } = 72f;
    public Windows.UI.Color BackgroundColor { get; set; } = Windows.UI.Color.FromArgb(255, 255, 255, 255);
    public bool ShowGrid { get; set; }
    public bool ShowRuler { get; set; }
    public int GridSpacing { get; set; } = 20;
}

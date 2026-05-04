namespace ExpenseTracker.Models.DTOs.Analytics;

public class AchievementDto
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Icon { get; set; }
    public bool Earned { get; set; }
    public double Progress { get; set; }
    public double Total { get; set; }
    public DateTime? DateEarned { get; set; }
}

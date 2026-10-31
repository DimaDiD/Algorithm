namespace MMSA.DAL.Constants;

public static class ReviewStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Overridden = "Overridden";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Pending, Confirmed, Overridden };
}

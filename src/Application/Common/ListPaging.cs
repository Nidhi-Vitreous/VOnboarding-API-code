namespace Vitreous.Onboarding.Application.Common;

public static class ListPaging
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    public static int NormalizePage(int page) =>
        page < MinPage ? DefaultPage : page;

    public static int NormalizePageSize(int pageSize) =>
        Math.Clamp(pageSize, MinPageSize, MaxPageSize);

    public static int ComputeTotalPages(int totalCount, int pageSize) =>
        totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
}

namespace Vitreous.Onboarding.Application.Merchants;

public static class MerchantMessages
{
    public const string NotFound = "Merchant not found.";
    public const string CreatorNotFound = "The specified creator user was not found.";
    public const string ActingUserNotFound = "The specified user was not found.";
    public const string CannotUpdate = "Merchant cannot be updated.";    public const string DraftOnlyUpdate = "Only draft merchants can be updated by non-admin users.";
    public const string InvalidStatus = "The requested status is not valid.";
}

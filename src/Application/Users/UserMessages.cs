namespace Vitreous.Onboarding.Application.Users;

public static class UserMessages
{
    public const string InvalidRole = "The selected role is invalid.";
    public const string RoleNotFound = "The selected role was not found.";
    public const string RoleInactive = "The selected role is inactive.";
    public const string DuplicateUsername = "Unable to generate a unique username.";
    public const string CannotDeleteOwnAccount = "You cannot delete your own account.";
    public const string CannotDeleteSuperAdmin = "Super Admin accounts cannot be deleted.";
    public const string CannotDeleteAdmin = "Admin accounts cannot be deleted.";
    public const string CannotEditSuperAdmin = "Super Admin accounts can only be edited by the account owner.";
    public const string CannotDeactivateSuperAdmin = "Super Admin accounts cannot be deactivated.";
    public const string ActorNotFound = "The authenticated user was not found.";
}

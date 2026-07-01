namespace Vitreous.Onboarding.Application.Roles;

public static class RoleMessages
{
    public const string NotFound = "Role not found.";
    public const string NameMustBeUnique = "Role name must be unique.";
    public const string DuplicateNameDetail = "A role with this name already exists.";
    public const string CannotDelete = "Role cannot be deleted.";
    public const string CannotModify = "Role cannot be modified.";
    public const string SystemRoleProtected = "System roles are protected and cannot be removed.";
    public const string SystemRoleCannotBeModified = "System roles are protected and cannot be modified.";
    public const string RoleInUse = "One or more users are assigned to this role.";
}

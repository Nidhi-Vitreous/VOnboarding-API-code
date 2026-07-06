using Vitreous.Onboarding.Application.Authorization;
using Vitreous.Onboarding.Application.Common;
using Vitreous.Onboarding.Domain.Entities;

namespace Vitreous.Onboarding.Application.Users;

public static class UserProtectionRules
{
    public static void ValidateSuperAdminSelfServiceOnly(User actor, User target)
    {
        if (IsSuperAdminUser(target) && actor.Id != target.Id)
        {
            throw new BusinessRuleException(UserMessages.CannotEditSuperAdmin);
        }
    }

    public static void ValidateDeletion(User actor, User target)
    {
        if (actor.Id == target.Id)
        {
            throw new BusinessRuleException(UserMessages.CannotDeleteOwnAccount);
        }

        if (IsSuperAdminUser(target))
        {
            throw new BusinessRuleException(UserMessages.CannotDeleteSuperAdmin);
        }
    }

    public static bool IsSuperAdminUser(User user)
    {
        if (DepartmentPermissionRegistry.IsSuperAdminRoleName(user.Role))
        {
            return true;
        }

        return user.UserRoles.Any(userRole =>
            userRole.Role is not null
            && DepartmentPermissionRegistry.IsSuperAdminRoleName(userRole.Role.Name));
    }
}

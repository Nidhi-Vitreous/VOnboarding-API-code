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

    public static void ValidateSuperAdminSelfDeactivateOnly(User actor, User target, bool requestedIsActive)
    {
        if (IsSuperAdminUser(actor) && actor.Id == target.Id && !requestedIsActive)
        {
            throw new BusinessRuleException(UserMessages.CannotDeactivateSuperAdmin);
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

        if (!IsSuperAdminUser(actor) && IsAdminRoleUser(target))
        {
            throw new BusinessRuleException(UserMessages.CannotDeleteAdmin);
        }
    }

    public static bool IsAdminRoleUser(User user)
    {
        if (IsSuperAdminUser(user))
        {
            return false;
        }

        if (DepartmentPermissionRegistry.IsNonSuperAdminAdminRoleName(user.Role))
        {
            return true;
        }

        return user.UserRoles.Any(userRole =>
            userRole.Role is not null
            && DepartmentPermissionRegistry.IsNonSuperAdminAdminRoleName(userRole.Role.Name));
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

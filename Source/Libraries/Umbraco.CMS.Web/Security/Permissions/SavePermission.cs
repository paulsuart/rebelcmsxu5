using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security.Permissions
{
    [Permission(FixedPermissionIds.Save, "Save", FixedPermissionTypes.EntityAction, UserType.User)]
    public class SavePermission : Permission
    { }
}
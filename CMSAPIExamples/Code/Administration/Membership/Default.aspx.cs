using System;
using System.Data;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.LicenseProvider;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.UIControls;

[Title(Text = "Membership", ImageUrl = "Objects/CMS_User/object.png")]
public partial class CMSAPIExamples_Code_Administration_Membership_Default : CMSAPIExamplePage
{
    #region "Initialization"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Check license
        LicenseHelper.CheckFeatureAndRedirect(URLHelper.GetCurrentDomain(), FeatureEnum.Membership);

        // User
        apiCreateUser.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(CreateUser);
        apiGetAndUpdateUser.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(GetAndUpdateUser);
        apiGetAndBulkUpdateUsers.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(GetAndBulkUpdateUsers);
        apiDeleteUser.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(DeleteUser);
        apiAuthenticateUser.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(AuthenticateUser);

        // User on site
        apiAddUserToSite.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(AddUserToSite);
        apiRemoveUserFromSite.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(RemoveUserFromSite);

        // Role
        apiCreateRole.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(CreateRole);
        apiGetAndUpdateRole.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(GetAndUpdateRole);
        apiGetAndBulkUpdateRoles.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(GetAndBulkUpdateRoles);
        apiDeleteRole.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(DeleteRole);

        // User role
        apiCreateUserRole.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(CreateUserRole);
        apiDeleteUserRole.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(DeleteUserRole);

        // Membership
        apiCreateMembership.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(CreateMembership);
        apiGetAndUpdateMembership.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(GetAndUpdateMembership);
        apiGetAndBulkUpdateMemberships.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(GetAndBulkUpdateMemberships);
        apiDeleteMembership.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(DeleteMembership);

        // Membership role
        apiAddMembershipToRole.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(AddMembershipToRole);
        apiRemoveMembershipFromRole.RunExample +=new CMSAPIExamples_Controls_APIExample.OnRunExample(RemoveMembershipFromRole);

        apiAddMembershipToUser.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(AddMembershipToUser);
        apiRemoveMembershipFromUser.RunExample +=new CMSAPIExamples_Controls_APIExample.OnRunExample(RemoveMembershipFromUser);

        // Online users
        apiGetOnlineUsers.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(GetOnlineUsers);
        apiIsUserOnline.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(IsUserOnline);
        apiKickUser.RunExample += new CMSAPIExamples_Controls_APIExample.OnRunExample(KickUser);
    }

    #endregion


    #region "Mass actions"

    /// <summary>
    /// Runs all creating and managing examples.
    /// </summary>
    public override void RunAll()
    {
        base.RunAll();

        // User
        apiCreateUser.Run();
        apiGetAndUpdateUser.Run();
        apiGetAndBulkUpdateUsers.Run();

        // User on site
        apiAddUserToSite.Run();

        // Role
        apiCreateRole.Run();
        apiGetAndUpdateRole.Run();
        apiGetAndBulkUpdateRoles.Run();

        // User role
        apiCreateUserRole.Run();

        // Membership
        apiCreateMembership.Run();
        apiGetAndUpdateMembership.Run();
        apiGetAndBulkUpdateMemberships.Run();

        // Membership role
        apiAddMembershipToRole.Run();

        // Membership user
        apiAddMembershipToUser.Run();
        
        // Session management
        apiGetOnlineUsers.Run();
        apiIsUserOnline.Run();
        apiKickUser.Run();
    }


    /// <summary>
    /// Runs all cleanup examples.
    /// </summary>
    public override void CleanUpAll()
    {
        base.CleanUpAll();

        // User role
        apiDeleteUserRole.Run();

        // Membership role
        apiRemoveMembershipFromRole.Run();

        // Membership user
        apiRemoveMembershipFromUser.Run();

        // Membership
        apiDeleteMembership.Run();

        // User on site
        apiRemoveUserFromSite.Run();

        // User
        apiDeleteUser.Run();

        // Role
        apiDeleteRole.Run();
    }

    #endregion


    #region "API examples - User"

    /// <summary>
    /// Creates user. Called when the "Create user" button is pressed.
    /// </summary>
    private bool CreateUser()
    {
        // Create new user object
        UserInfo newUser = new UserInfo();

        // Set the properties
        newUser.FullName = "My new user";
        newUser.UserName = "MyNewUser";
        newUser.UserEnabled = true;
        newUser.UserIsGlobalAdministrator = true;

        // Save the user
        UserInfoProvider.SetUserInfo(newUser);

        return true;
    }


    /// <summary>
    /// Gets and updates user. Called when the "Get and update user" button is pressed.
    /// Expects the CreateUser method to be run first.
    /// </summary>
    private bool GetAndUpdateUser()
    {
        // Get the user
        UserInfo updateUser = UserInfoProvider.GetUserInfo("MyNewUser");
        if (updateUser != null)
        {
            // Update the properties
            updateUser.FullName = updateUser.FullName.ToLowerCSafe();

            // Save the changes
            UserInfoProvider.SetUserInfo(updateUser);

            return true;
        }

        return false;
    }


    /// <summary>
    /// Gets and bulk updates users. Called when the "Get and bulk update users" button is pressed.
    /// Expects the CreateUser method to be run first.
    /// </summary>
    private bool GetAndBulkUpdateUsers()
    {
        // Prepare the parameters
        string where = "UserName LIKE N'MyNewUser%'";

        // Get the data
        DataSet users = UserInfoProvider.GetUsers(where, null);
        if (!DataHelper.DataSourceIsEmpty(users))
        {
            // Loop through the individual items
            foreach (DataRow userDr in users.Tables[0].Rows)
            {
                // Create object from DataRow
                UserInfo modifyUser = new UserInfo(userDr);

                // Update the properties
                modifyUser.FullName = modifyUser.FullName.ToUpper();

                // Save the changes
                UserInfoProvider.SetUserInfo(modifyUser);
            }

            return true;
        }

        return false;
    }


    /// <summary>
    /// Deletes user. Called when the "Delete user" button is pressed.
    /// Expects the CreateUser method to be run first.
    /// </summary>
    private bool DeleteUser()
    {
        // Get the user
        UserInfo deleteUser = UserInfoProvider.GetUserInfo("MyNewUser");

        // Delete the user
        UserInfoProvider.DeleteUser(deleteUser);

        return (deleteUser != null);
    }


    private bool AuthenticateUser()
    {
        // Get the user
        UserInfo user = UserInfoProvider.GetUserInfo("MyNewUser");
        if (user != null)
        {
            if (AuthenticationHelper.AuthenticateUser("MyNewUser", "", CMSContext.CurrentSiteName) != null)
            {
                return true;
            }
        }
        return false;
    }

    #endregion


    #region "API examples - User on site"

    /// <summary>
    /// Adds user to site. Called when the "Add user to site" button is pressed.
    /// Expects the CreateUser method to be run first.
    /// </summary>
    private bool AddUserToSite()
    {
        // Get the user
        UserInfo user = UserInfoProvider.GetUserInfo("MyNewUser");
        if (user != null)
        {
            int userId = user.UserID;
            int siteId = CMSContext.CurrentSiteID;

            // Save the binding
            UserSiteInfoProvider.AddUserToSite(userId, siteId);

            return true;
        }

        return false;
    }


    /// <summary>
    /// Removes user from site. Called when the "Remove user from site" button is pressed.
    /// Expects the AddUserToSite method to be run first.
    /// </summary>
    private bool RemoveUserFromSite()
    {
        // Get the user
        UserInfo removeUser = UserInfoProvider.GetUserInfo("MyNewUser");
        if (removeUser != null)
        {
            int siteId = CMSContext.CurrentSiteID;

            // Get the binding
            UserSiteInfo userSite = UserSiteInfoProvider.GetUserSiteInfo(removeUser.UserID, siteId);

            // Delete the binding
            UserSiteInfoProvider.DeleteUserSiteInfo(userSite);

            return true;
        }

        return false;
    }

    #endregion


    #region "API examples - Role"

    /// <summary>
    /// Creates role. Called when the "Create role" button is pressed.
    /// </summary>
    private bool CreateRole()
    {
        // Create new role object
        RoleInfo newRole = new RoleInfo();

        // Set the properties
        newRole.DisplayName = "My new role";
        newRole.RoleName = "MyNewRole";
        newRole.SiteID = CMSContext.CurrentSiteID;

        // Save the role
        RoleInfoProvider.SetRoleInfo(newRole);

        return true;
    }


    /// <summary>
    /// Gets and updates role. Called when the "Get and update role" button is pressed.
    /// Expects the CreateRole method to be run first.
    /// </summary>
    private bool GetAndUpdateRole()
    {
        // Get the role
        RoleInfo updateRole = RoleInfoProvider.GetRoleInfo("MyNewRole", CMSContext.CurrentSiteID);
        if (updateRole != null)
        {
            // Update the properties
            updateRole.DisplayName = updateRole.DisplayName.ToLowerCSafe();

            // Save the changes
            RoleInfoProvider.SetRoleInfo(updateRole);

            return true;
        }

        return false;
    }


    /// <summary>
    /// Gets and bulk updates roles. Called when the "Get and bulk update roles" button is pressed.
    /// Expects the CreateRole method to be run first.
    /// </summary>
    private bool GetAndBulkUpdateRoles()
    {
        // Prepare the parameters
        string where = "RoleName LIKE N'MyNewRole%'";

        // Get the data
        DataSet roles = RoleInfoProvider.GetRoles(where, null);
        if (!DataHelper.DataSourceIsEmpty(roles))
        {
            // Loop through the individual items
            foreach (DataRow roleDr in roles.Tables[0].Rows)
            {
                // Create object from DataRow
                RoleInfo modifyRole = new RoleInfo(roleDr);

                // Update the properties
                modifyRole.DisplayName = modifyRole.DisplayName.ToUpper();

                // Save the changes
                RoleInfoProvider.SetRoleInfo(modifyRole);
            }

            return true;
        }

        return false;
    }


    /// <summary>
    /// Deletes role. Called when the "Delete role" button is pressed.
    /// Expects the CreateRole method to be run first.
    /// </summary>
    private bool DeleteRole()
    {
        // Get the role
        RoleInfo deleteRole = RoleInfoProvider.GetRoleInfo("MyNewRole", CMSContext.CurrentSiteID);

        // Delete the role
        RoleInfoProvider.DeleteRoleInfo(deleteRole);

        return (deleteRole != null);
    }

    #endregion


    #region "API examples - User role"

    /// <summary>
    /// Creates user role. Called when the "Create role" button is pressed.
    /// </summary>
    private bool CreateUserRole()
    {
        // Get role and user objects
        RoleInfo role = RoleInfoProvider.GetRoleInfo("MyNewRole", CMSContext.CurrentSiteID);
        UserInfo user = UserInfoProvider.GetUserInfo("MyNewUser");

        if ((role != null) && (user != null))
        {
            // Create new user role object
            UserRoleInfo userRole = new UserRoleInfo();

            // Set the properties
            userRole.UserID = user.UserID;
            userRole.RoleID = role.RoleID;

            // Save the user role
            UserRoleInfoProvider.SetUserRoleInfo(userRole);

            return true;
        }

        return false;
    }


    /// <summary>
    /// Deletes user role. Called when the "Delete role" button is pressed.
    /// Expects the CreateUserRole method to be run first.
    /// </summary>
    private bool DeleteUserRole()
    {
        // Get role and user objects
        RoleInfo role = RoleInfoProvider.GetRoleInfo("MyNewRole", CMSContext.CurrentSiteID);
        UserInfo user = UserInfoProvider.GetUserInfo("MyNewUser");

        if ((role != null) && (user != null))
        {
            // Get the user role
            UserRoleInfo deleteRole = UserRoleInfoProvider.GetUserRoleInfo(user.UserID, role.RoleID);

            // Delete the user role
            UserRoleInfoProvider.DeleteUserRoleInfo(deleteRole);

            return true;
        }

        return false;
    }

    #endregion


    #region "API examples - Membership"

    /// <summary>
    /// Creates membership. Called when the "Create membership" button is pressed.
    /// </summary>
    private bool CreateMembership()
    {
        // Create new membership object
        MembershipInfo newMembership = new MembershipInfo();

        // Set the properties
        newMembership.MembershipDisplayName = "My new membership";
        newMembership.MembershipName = "MyNewMembership";
        newMembership.MembershipSiteID = CMSContext.CurrentSiteID;

        // Save the membership
        MembershipInfoProvider.SetMembershipInfo(newMembership);

        return true;
    }


    /// <summary>
    /// Gets and updates membership. Called when the "Get and update membership" button is pressed.
    /// Expects the CreateMembership method to be run first.
    /// </summary>
    private bool GetAndUpdateMembership()
    {
        // Get the membership
        MembershipInfo updateMembership = MembershipInfoProvider.GetMembershipInfo("MyNewMembership", CMSContext.CurrentSiteName);
        if (updateMembership != null)
        {
            // Update the properties
            updateMembership.MembershipDisplayName = updateMembership.MembershipDisplayName.ToLower();

            // Save the changes
            MembershipInfoProvider.SetMembershipInfo(updateMembership);

            return true;
        }

        return false;
    }


    /// <summary>
    /// Gets and bulk updates memberships. Called when the "Get and bulk update memberships" button is pressed.
    /// Expects the CreateMembership method to be run first.
    /// </summary>
    private bool GetAndBulkUpdateMemberships()
    {
        // Prepare the parameters
        string where = "MembershipName LIKE N'MyNewMembership%'";

        // Get the data
        DataSet memberships = MembershipInfoProvider.GetMemberships(where, null);
        if (!DataHelper.DataSourceIsEmpty(memberships))
        {
            // Loop through the individual items
            foreach (DataRow membershipDr in memberships.Tables[0].Rows)
            {
                // Create object from DataRow
                MembershipInfo modifyMembership = new MembershipInfo(membershipDr);

                // Update the properties
                modifyMembership.MembershipDisplayName = modifyMembership.MembershipDisplayName.ToUpper();

                // Save the changes
                MembershipInfoProvider.SetMembershipInfo(modifyMembership);
            }

            return true;
        }

        return false;
    }


    /// <summary>
    /// Deletes membership. Called when the "Delete membership" button is pressed.
    /// Expects the CreateMembership method to be run first.
    /// </summary>
    private bool DeleteMembership()
    {
        // Get the membership
        MembershipInfo deleteMembership = MembershipInfoProvider.GetMembershipInfo("MyNewMembership", CMSContext.CurrentSiteName);

        // Delete the membership
        MembershipInfoProvider.DeleteMembershipInfo(deleteMembership);

        return (deleteMembership != null);
    }


    /// <summary>
    /// Adds membership to role. Called when the "Add membership to role" button is pressed.
    /// </summary>
    private bool AddMembershipToRole()
    {
        // Get role and user objects
        RoleInfo role = RoleInfoProvider.GetRoleInfo("MyNewRole", CMSContext.CurrentSiteID);
        MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("MyNewMembership", CMSContext.CurrentSiteName);

        if ((role != null) && (membership != null))
        {
            MembershipRoleInfoProvider.AddMembershipToRole(membership.MembershipID, role.RoleID);

            return true;
        }
        
        return false;
    }


    /// <summary>
    /// Removes membership from role. Called when the "Remove membership from role" button is pressed.
    /// </summary>
    private bool RemoveMembershipFromRole()
    {
        // Get role and user objects
        RoleInfo role = RoleInfoProvider.GetRoleInfo("MyNewRole", CMSContext.CurrentSiteID);
        MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("MyNewMembership", CMSContext.CurrentSiteName);

        if ((role != null) && (membership != null))
        {
            MembershipRoleInfoProvider.RemoveMembershipFromRole(membership.MembershipID, role.RoleID);

            return true;
        }

        return false;
    }


    /// <summary>
    /// Adds membership to user. Called when the "Add membership to user" button is pressed.
    /// </summary>
    private bool AddMembershipToUser()
    {
        // Get role and user objects
        UserInfo user = UserInfoProvider.GetUserInfo("MyNewUser");
        MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("MyNewMembership", CMSContext.CurrentSiteName);

        if ((user != null) && (membership != null))
        {
            MembershipUserInfoProvider.AddMembershipToUser(membership.MembershipID, user.UserID, DateTime.Now.AddMinutes(3));

            return true;
        }

        return false;
    }


    /// <summary>
    /// Removes membership from user. Called when the "Remove membership from user" button is pressed.
    /// </summary>
    private bool RemoveMembershipFromUser()
    {
        // Get role and user objects
        UserInfo user = UserInfoProvider.GetUserInfo("MyNewUser");
        MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("MyNewMembership", CMSContext.CurrentSiteName);

        if ((user != null) && (membership != null))
        {
            MembershipUserInfoProvider.RemoveMembershipFromUser(membership.MembershipID, user.UserID);

            return true;
        }

        return false;
    }

    #endregion



    #region "API examples - Session management"

    /// <summary>
    /// Gets and bulk updates on-line users. Called when the "Get and bulk update on-line users" button is pressed.
    /// </summary>
    private bool GetOnlineUsers()
    {
        string where = "";
        int topN = 10;
        string orderBy = "";
        string location = "";
        string siteName = CMSContext.CurrentSiteName;
        bool includeHidden = true;
        bool includeKicked = false;

        // Get DataSet of online users
        DataSet users = SessionManager.GetOnlineUsers(where, orderBy, topN, location, siteName, includeHidden, includeKicked);
        if (!DataHelper.DataSourceIsEmpty(users))
        {
            foreach (DataRow userDr in users.Tables[0].Rows)
            {
                // Create object from DataRow
                UserInfo modifyUser = new UserInfo(userDr);

                // Update the properties
                modifyUser.FullName = modifyUser.FullName.ToUpper();

                // Save the changes
                UserInfoProvider.SetUserInfo(modifyUser);
            }

            return true;
        }

        return false;
    }


    /// <summary>
    /// Checks if user is online. Called when the "Is user on-line?" button is pressed.
    /// </summary>
    private bool IsUserOnline()
    {
        bool includeHidden = true;

        // Get user and site objects
        UserInfo user = UserInfoProvider.GetUserInfo("MyNewUser");
        SiteInfo site = SiteInfoProvider.GetSiteInfo(CMSContext.CurrentSiteName);

        if ((user != null) && (site != null))
        {
            // Check if user is online
            return SessionManager.IsUserOnline(site.SiteName, user.UserID, includeHidden);
        }

        return false;
    }

    

    /// <summary>
    /// Kicks on-line user. Called when the "Kick user" button is pressed.
    /// </summary>
    private bool KickUser()
    {
        // Get the user 
        UserInfo kickedUser = UserInfoProvider.GetUserInfo(CMSContext.CurrentUser.UserID);

        if (kickedUser != null)
        {
            // Kick the user
            SessionManager.KickUser(kickedUser.UserID);

            return true;
        }

        return false;
    }

    #endregion
}
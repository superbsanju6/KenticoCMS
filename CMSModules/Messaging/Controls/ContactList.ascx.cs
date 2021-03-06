using System;
using System.Data;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.Messaging;
using CMS.SettingsProvider;
using CMS.UIControls;
using CMS.ExtendedControls;

public partial class CMSModules_Messaging_Controls_ContactList : CMSUserControl
{
    #region "Private variables"

    private string currentValues = String.Empty;

    #endregion


    #region "Public properties"

    /// <summary>
    /// Messages placeholder
    /// </summary>
    public override MessagesPlaceHolder MessagesPlaceHolder
    {
        get
        {
            return plcMess;
        }
    }


    /// <summary>
    /// Indicates if control is used on live site.
    /// </summary>
    public override bool IsLiveSite
    {
        get
        {
            return base.IsLiveSite;
        }
        set
        {
            plcMess.IsLiveSite = value;
            base.IsLiveSite = value;
        }
    }


    /// <summary>
    /// Zero rows text.
    /// </summary>
    public string ZeroRowsText
    {
        get
        {
            EnsureChildControls();
            return usUsers.ZeroRowsText;
        }
        set
        {
            EnsureChildControls();
            usUsers.ZeroRowsText = value;
        }
    }


    /// <summary>
    /// Page size value.
    /// </summary>
    public string PageSize
    {
        get
        {
            EnsureChildControls();
            return usUsers.UniSelector.ItemsPerPage.ToString();
        }
        set
        {
            EnsureChildControls();
            usUsers.UniSelector.ItemsPerPage = ValidationHelper.GetInteger(value, 0);
        }
    }

    #endregion


    #region "Page events"

    protected override void EnsureChildControls()
    {
        base.EnsureChildControls();
        if (usUsers == null)
        {
            pnlContactList.LoadContainer();
        }
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        if (StopProcessing)
        {
            // Stop processing
            usUsers.StopProcessing = true;
        }
        else
        {
            CurrentUserInfo ui = CMSContext.CurrentUser;

            // Content is visible only for authenticated users
            if (ui.IsAuthenticated())
            {
                usUsers.UniSelector.OnSelectionChanged += UniSelector_OnSelectionChanged;
                usUsers.IsLiveSite = IsLiveSite;
                usUsers.WhereCondition = "UserName NOT LIKE N'public'";

                // Global admin can see all users
                if (ui.UserSiteManagerAdmin)
                {
                    usUsers.DisplayUsersFromAllSites = true;
                }
                else
                {
                    usUsers.DisplayUsersFromAllSites = false;

                    // Show only users from current site for normal user
                    usUsers.SiteID = CMSContext.CurrentSiteID;

                    if (IsLiveSite && !CMSContext.CurrentUser.IsGlobalAdministrator)
                    {
                        usUsers.HideDisabledUsers = true;
                        usUsers.HideHiddenUsers = true;
                        usUsers.HideNonApprovedUsers = true;
                    }
                }

                // Hide site selector
                usUsers.ShowSiteFilter = false;

                currentValues = GetContactListValues();

                if (!RequestHelper.IsPostBack())
                {
                    usUsers.Value = currentValues;
                }

                if (string.IsNullOrEmpty(ZeroRowsText))
                {
                    // Use default text
                    ZeroRowsText = GetString("messaging.contactlist.nodatafound");
                }
            }
            else
            {
                Visible = false;
            }
        }
    }

    #endregion


    #region "Other events"

    protected void UniSelector_OnSelectionChanged(object sender, EventArgs e)
    {
        // Save selection
        SaveUsers();
    }

    #endregion


    #region "Other methods"

    private static string GetContactListValues()
    {
        DataSet contactList = ContactListInfoProvider.GetContactList(CMSContext.CurrentUser.UserID, null, null, 0, "UserName,UserNickname,ContactListContactUserID");
        if (!DataHelper.DataSourceIsEmpty(contactList))
        {
            return TextHelper.Join(";", SystemDataHelper.GetStringValues(contactList.Tables[0], "ContactListContactUserID"));
        }

        return String.Empty;
    }


    private void SaveUsers()
    {
        bool falseValues = false;

        // Remove old items
        string newValues = ValidationHelper.GetString(usUsers.Value, null);
        string items = DataHelper.GetNewItemsInList(newValues, currentValues);
        if (!String.IsNullOrEmpty(items))
        {
            string[] newItems = items.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Add all new items to user
            foreach (string item in newItems)
            {
                int userId = ValidationHelper.GetInteger(item, 0);
                ContactListInfoProvider.RemoveFromContactList(CMSContext.CurrentUser.UserID, userId);
            }
        }

        // Add new items
        items = DataHelper.GetNewItemsInList(currentValues, newValues);
        if (!String.IsNullOrEmpty(items))
        {
            string[] newItems = items.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Add all new items to user
            foreach (string item in newItems)
            {
                int userId = ValidationHelper.GetInteger(item, 0);
                ContactListInfoProvider.AddToContactList(CMSContext.CurrentUser.UserID, userId);
            }
        }

        if (falseValues)
        {
            currentValues = GetContactListValues();
            usUsers.Value = currentValues;
        }

        ShowChangesSaved();
    }

    #endregion
}
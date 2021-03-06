﻿using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.PortalControls;
using CMS.GlobalHelper;
using CMS.CMSHelper;

public partial class CMSWebParts_CommunityServices_Pinterest_PinterestFollowButton : CMSAbstractWebPart
{
    #region "Properties"

    /// <summary>
    /// Username.
    /// </summary>
    public string Username
    {
        get
        {
            return ValidationHelper.GetString(GetValue("Username"), string.Empty);
        }
        set
        {
            SetValue("Username", value);
        }
    }


    /// <summary>
    /// Display type.
    /// </summary>
    public string DisplayType
    {
        get
        {
            return ValidationHelper.GetString(GetValue("DisplayType"), string.Empty);
        }
        set
        {
            SetValue("DisplayType", value);
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Content loaded event handler
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties
    /// </summary>
    protected void SetupControl()
    {
        if (this.StopProcessing)
        {
            // Do not process
        }
        else
        {
            // Build plugin code
            string src = "http://pinterest.com/" + Username;

            // Output HTML code
            string output = "<a href=\"{0}\"><img src=\"http://passets-cdn.pinterest.com/images/{1}.png\" alt=\"Follow Me on Pinterest\" /></a>";
            ltlPluginCode.Text = String.Format(output, src, DisplayType);
        }
    }


    /// <summary>
    /// Reloads the control data
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();

        SetupControl();
    }

    #endregion
}
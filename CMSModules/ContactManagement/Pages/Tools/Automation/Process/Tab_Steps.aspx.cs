﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.UIControls;
using CMS.GlobalHelper;
using CMS.SettingsProvider;
using CMS.LicenseProvider;
using CMS.OnlineMarketing;
using CMS.WorkflowEngine;

// Set edited object
[EditedObject(WorkflowObjectType.AUTOMATIONPROCESS, "processid")]

public partial class CMSModules_ContactManagement_Pages_Tools_Automation_Process_Tab_Steps : CMSAutomationPage
{
    #region "Constants"

    private const string SERVICEURL = "~/CMSModules/Automation/Services/AutomationDesignerService.svc";

    #endregion


    #region "Variables"

    private int mProcessID = 0;

    #endregion


    #region "Properties"

    /// <summary>
    /// Current workflow ID
    /// </summary>
    public int ProcessID
    {
        get
        {
            if (mProcessID <= 0)
            {
                mProcessID = QueryHelper.GetInteger("processid", 0);
            }
            return mProcessID;
        }
    }

    #endregion


    #region "Event handlers"

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        designerElem.ServiceUrl = SERVICEURL;
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        if (ProcessID <= 0)
        {
            designerElem.StopProcessing = true;
            return;
        }

        designerElem.WorkflowID = ProcessID;
        designerElem.WorkflowType = WorkflowTypeEnum.Automation;

        bool licenseFail = !WorkflowInfoProvider.IsMarketingAutomationAllowed();
        if(licenseFail || !WorkflowStepInfoProvider.CanUserManageAutomationProcesses(CurrentUser, CurrentSiteName))
        {
            designerElem.ReadOnly = true;
            MessagesPlaceHolder.OffsetY = 10;
            MessagesPlaceHolder.UseRelativePlaceHolder = false;
            ShowInformation(GetString(licenseFail ? "wf.licenselimitation" : "general.modifynotallowed"));
        }
    }

    #endregion
}
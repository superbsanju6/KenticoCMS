﻿using System;
using System.Data;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.WorkflowEngine;
using CMS.FormControls;
using CMS.ExtendedControls;

public partial class CMSModules_Automation_Controls_Process_Edit : CMSUserControl
{
    #region "Private variables"

    private CMSAutomationManager mAutomationManager = null;

    #endregion


    #region "Properties"

    /// <summary>
    /// Object instance
    /// </summary>
    public BaseInfo InfoObject
    {
        get
        {
            return AutomationManager.InfoObject;
        }
    }


    /// <summary>
    /// State object
    /// </summary>
    public AutomationStateInfo StateObject
    {
        get
        {
            return AutomationManager.StateObject;
        }
    }


    /// <summary>
    /// Automation manager
    /// </summary>
    public AutomationManager Manager
    {
        get
        {
            return AutomationManager.Manager;
        }
    }


    /// <summary>
    /// Automation manager control
    /// </summary>
    public CMSAutomationManager AutomationManager
    {
        get
        {
            if (mAutomationManager == null)
            {
                mAutomationManager = ControlsHelper.GetChildControl(Page, typeof(CMSAutomationManager)) as CMSAutomationManager;
                if (mAutomationManager == null)
                {
                    throw new Exception("[AutomationMenu.AutomationManager]: Missing automation manager.");
                }
            }

            return mAutomationManager;
        }
    }

    #endregion


    #region "Page events"

    protected void Page_Load(object sender, EventArgs e)
    {
        ReloadData();
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (RequestHelper.IsPostBack())
        {
            ReloadData();
        }
    }

    #endregion


    #region "Grid events"

    /// <summary>
    /// External history binding.
    /// </summary>
    protected object gridHistory_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        DataRowView drv = null;
        switch (sourceName.ToLowerCSafe())
        {
            case "action":
                drv = (DataRowView)parameter;
                bool wasrejected = ValidationHelper.GetBoolean(drv["HistoryWasRejected"], false);

                // Get type of the steps
                WorkflowStepTypeEnum stepType = (WorkflowStepTypeEnum)ValidationHelper.GetInteger(DataHelper.GetDataRowViewValue(drv, "HistoryStepType"), 0);
                WorkflowStepTypeEnum targetStepType = (WorkflowStepTypeEnum)ValidationHelper.GetInteger(DataHelper.GetDataRowViewValue(drv, "HistoryTargetStepType"), 0);
                WorkflowTransitionTypeEnum transitionType = (WorkflowTransitionTypeEnum)ValidationHelper.GetInteger(DataHelper.GetDataRowViewValue(drv, "HistoryTransitionType"), 0);

                if (!wasrejected)
                {
                    bool isAutomatic = (transitionType == WorkflowTransitionTypeEnum.Automatic);
                    string actionString = isAutomatic ? GetString("WorfklowProperties.Automatic") + " ({0})" : "{0}";
                    // Return correct step title
                    switch (targetStepType)
                    {
                        case WorkflowStepTypeEnum.Finished:
                            actionString = string.Format(actionString, GetString("ma.finished"));
                            break;

                        default:
                            if (stepType == WorkflowStepTypeEnum.Start)
                            {
                                actionString = string.Format(actionString, GetString("ma.started"));
                            }
                            else
                            {
                                actionString = isAutomatic ? GetString("WorfklowProperties.Automatic") : GetString("ma.movedtonextstep");
                            }
                            break;
                    }

                    return actionString;
                }
                else
                {
                    return GetString("ma.movedtopreviousstep");
                }

            // Get approved time
            case "approvedwhen":
            case "approvedwhentooltip":
                if (string.IsNullOrEmpty(parameter.ToString()))
                {
                    return string.Empty;
                }
                else
                {
                    // Apply time zone information
                    bool displayGMT = (sourceName == "approvedwhentooltip");
                    DateTime time = ValidationHelper.GetDateTime(parameter, DateTimeHelper.ZERO_TIME);
                    return TimeZoneHelper.ConvertToUserTimeZone(time, displayGMT, CurrentUser, CurrentSite);
                }

            case "stepname":
                drv = (DataRowView)parameter;
                string step = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(drv, "HistoryStepDisplayName"), String.Empty);
                string targetStep = ValidationHelper.GetString(DataHelper.GetDataRowViewValue(drv, "HistoryTargetStepDisplayName"), String.Empty);
                if (!string.IsNullOrEmpty(targetStep))
                {
                    step += " -> " + targetStep;
                }
                return HTMLHelper.HTMLEncode(ResHelper.LocalizeString(step));
        }
        return parameter;
    }

    #endregion


    #region "Protected methods"

    /// <summary>
    /// Reloads the page data.
    /// </summary>
    protected void ReloadData()
    {
        if (InfoObject != null)
        {
            var workflow = AutomationManager.Process;
            if (workflow != null)
            {
                ucDesigner.WorkflowID = workflow.WorkflowID;
                ucDesigner.SelectedStepID = StateObject.StateStepID;
                ucDesigner.WorkflowType = WorkflowTypeEnum.Automation;

                // Initialize grids
                gridHistory.OnExternalDataBound += gridHistory_OnExternalDataBound;
                gridHistory.ZeroRowsText = string.Format(GetString("ma.nohistoryyet"), ResHelper.GetString(TypeHelper.GetObjectTypeResourceKey(InfoObject.ObjectType)).ToLowerCSafe());
            }
        }
        else
        {
            pnlWorkflow.Visible = false;
        }

        gridHistory.WhereCondition = "HistoryStateID=" + AutomationManager.StateObjectID;
        gridHistory.ReloadData();
    }

    #endregion
}

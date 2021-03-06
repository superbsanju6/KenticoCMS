using System;

using CMS.CMSHelper;
using CMS.FormEngine;
using CMS.GlobalHelper;
using CMS.UIControls;

public partial class CMSModules_BizForms_Tools_BizForm_Edit : CMSBizFormPage
{
    protected BizFormInfo bfi = null;
    protected int nodeID;

    protected void Page_Load(object sender, EventArgs e)
    {
        //CurrentMaster.DisplayControlsPanel = true;

        int formId = 0;
        int formRecordId = 0;

        // Check 'ReadData' permission
        if (!CMSContext.CurrentUser.IsAuthorizedPerResource("cms.form", "ReadData"))
        {
            RedirectToCMSDeskAccessDenied("cms.form", "ReadData");
        }
        // Check 'EditData' permission
        else if (!CMSContext.CurrentUser.IsAuthorizedPerResource("cms.form", "EditData"))
        {
            RedirectToCMSDeskAccessDenied("cms.form", "EditData");
        }

        // Get form id from url
        formId = QueryHelper.GetInteger("formid", 0);

        if (formId > 0)
        {
            // Get form record id
            formRecordId = QueryHelper.GetInteger("formrecordid", 0);

            string currentRecord = "";

            // Edit record
            if (formRecordId > 0)
            {
                currentRecord = GetString("BizForm_Edit_EditRecord.EditRecord");
                // if (!RequestHelper.IsPostBack())
                // {
                    // chkSendNotification.Checked = false;
                    // chkSendAutoresponder.Checked = false;
                // }
            }
            // New record
            else
            {
                currentRecord = GetString("BizForm_Edit_EditRecord.NewRecord");
                // if (!RequestHelper.IsPostBack())
                // {
                    // chkSendNotification.Checked = true;
                    // chkSendAutoresponder.Checked = true;
                // }
            }

            // Initializes page title
            //string[,] breadcrumbs = new string[2,3];
            // breadcrumbs[0, 0] = GetString("BizForm_Edit_EditRecord.Data");
            // breadcrumbs[0, 1] = "~/CMSModules/BizForms/Tools/BizForm_Edit_Data.aspx?formid=" + formId;
            // breadcrumbs[0, 2] = "";
            // breadcrumbs[1, 0] = currentRecord;
            // breadcrumbs[1, 1] = "";
            // breadcrumbs[1, 2] = "";

            //CurrentMaster.Title.Breadcrumbs = breadcrumbs;

            bfi = BizFormInfoProvider.GetBizFormInfo(formId);
            EditedObject = bfi;

            if (!RequestHelper.IsPostBack())
            {
                // Get form info
                if (bfi != null)
                {
                    // Set form
                    formElem.FormName = bfi.FormName;
                    formElem.ItemID = formRecordId;
                    formElem.ShowPrivateFields = true;
                    bizFormName.Value = bfi.FormName;
                }
            }

            formElem.FormRedirectToUrl = String.Empty;
            formElem.FormDisplayText = String.Empty;
            formElem.FormClearAfterSave = false;
            formElem.OnBeforeSave += formElem_OnBeforeSave;

            if (!String.IsNullOrEmpty(Request.QueryString["templateName"]))
            {
                Page.Title = Request.QueryString["templateName"];
            }

            if (!String.IsNullOrEmpty(Request.QueryString["clientID"]))
            {
                clientID.Value = Request.QueryString["clientID"];
            }

            nodeID = QueryHelper.GetInteger("mtssContentID", 0);
        }
    }


    /// <summary>
    /// OnBefore save bizform.
    /// </summary>
    private void formElem_OnBeforeSave(object sender, EventArgs e)
    {
        // formElem.EnableNotificationEmail = chkSendNotification.Checked;
        // formElem.EnableAutoresponder = chkSendAutoresponder.Checked;
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if ((formElem.BasicForm != null) && (bfi != null))
        {
            int textLength = bfi.FormSubmitButtonText.Length;
            if (textLength > 15)
            {
                formElem.BasicForm.SubmitButton.CssClass = "XLongSubmitButton";
            }
            else if (textLength > 8)
            {
                formElem.BasicForm.SubmitButton.CssClass = "LongSubmitButton";
            }
            else
            {
                formElem.BasicForm.SubmitButton.CssClass = "SubmitButton";
            }

            // Check document 'Modify' permission
            if (nodeID == 0 || CMSContext.CurrentUser.IsAuthorizedPerTreeNode(nodeID, CMS.DocumentEngine.NodePermissionsEnum.Modify) == CMS.DocumentEngine.AuthorizationResultEnum.Denied)
            {
                formElem.BasicForm.Enabled = false;
                CurrentMaster.HeaderActions.Visible = false;
            }
        }
    }
}
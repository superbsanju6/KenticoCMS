using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.SettingsProvider;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.ExtendedControls;

public partial class CMSModules_MediaLibrary_Controls_MediaLibrary_FolderActions_EditFolder : CMSAdminControl
{
    #region "Delegates & Events"

    /// <summary>
    /// Delegate of event fired when 'Cancel' button of control is clicked.
    /// </summary>
    public delegate void OnCancelClickEventHandler();

    /// <summary>
    /// Delegate of event fired when folder has been deleted.
    /// </summary>
    public delegate void OnFolderChangeEventHandler(string pathToSelect);

    /// <summary>
    /// Event raised when 'Click' button is clicked.
    /// </summary>
    public event OnCancelClickEventHandler CancelClick;

    /// <summary>
    /// Event raised when folder has been deleted.
    /// </summary>
    public event OnFolderChangeEventHandler OnFolderChange;

    #endregion


    #region "Private variables"

    private int mLibraryId = 0;
    private string mLibraryFolder = null;
    private string mAction = null;
    private string mFolderPath = null;
    private string mNewFolderPath = null;
    private string mRootFolderPath = null;
    private string mNewTreePath = null;
    private string mCustomScript = null;
    private bool mCheckAdvancedPermissions = false;
    private bool mErrorOccurred = false;

    #endregion


    #region "Private properties"

    /// <summary>
    /// Indicates whether the error occurred during internal folder action processing.
    /// </summary>
    private bool ErrorOccurred
    {
        get
        {
            return mErrorOccurred;
        }
        set
        {
            mErrorOccurred = value;
        }
    }

    #endregion


    #region "Public properties"

    /// <summary>
    /// Header actions control
    /// </summary>
    public override HeaderActions HeaderActions
    {
        get
        {
            return headerActions;
        }
    }


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
    /// Indicates whether the properties should be stored in ViewState.
    /// </summary>
    public bool UseViewStateProperties
    {
        get
        {
            return ValidationHelper.GetBoolean(ViewState["UseViewStateProperties"], false);
        }
        set
        {
            ViewState["UseViewStateProperties"] = value;
        }
    }


    /// <summary>
    /// Indicates whether the CANCEL button should be displayed.
    /// </summary>
    public bool DisplayCancel
    {
        get
        {
            if (UseViewStateProperties)
            {
                return ValidationHelper.GetBoolean(ViewState["DisplayCancel"], true);
            }
            return plcCancelArea.Visible;
        }
        set
        {
            plcCancelArea.Visible = value;
            btnOk.RegisterHeaderAction = !value;

            if (UseViewStateProperties)
            {
                ViewState["DisplayCancel"] = value;
            }
        }
    }


    /// <summary>
    /// Indicates whether the control and all the nested controls are enabled.
    /// </summary>
    public bool Enabled
    {
        get
        {
            if (UseViewStateProperties)
            {
                return ValidationHelper.GetBoolean(ViewState["Enabled"], true);
            }
            return txtFolderName.Enabled;
        }
        set
        {
            txtFolderName.Enabled = value;
            btnOk.Enabled = value;
            btnCancel.Enabled = value;

            if (UseViewStateProperties)
            {
                ViewState["Enabled"] = value;
            }
        }
    }


    /// <summary>
    /// Indicates whether the control is loaded for new folder creation.
    /// </summary>
    public string Action
    {
        get
        {
            if (UseViewStateProperties && (mAction == null))
            {
                mAction = ValidationHelper.GetString(ViewState["Action"], null);
            }
            return mAction;
        }
        set
        {
            mAction = value;

            if (UseViewStateProperties)
            {
                ViewState["Action"] = mAction;
            }
        }
    }


    /// <summary>
    /// JavaScript used for OnClick event of OK button.
    /// </summary>
    public string CustomScript
    {
        get
        {
            if (UseViewStateProperties && (mCustomScript == null))
            {
                mCustomScript = ValidationHelper.GetString(ViewState["CustomScript"], null);
            }
            return mCustomScript;
        }
        set
        {
            mCustomScript = value;

            if (UseViewStateProperties)
            {
                ViewState["CustomScript"] = mCustomScript;
            }
        }
    }


    /// <summary>
    /// Path of the media library folder.
    /// </summary>
    public string FolderPath
    {
        get
        {
            if (UseViewStateProperties && (mFolderPath == null))
            {
                mFolderPath = ValidationHelper.GetString(ViewState["FolderPath"], null);
            }
            return mFolderPath;
        }
        set
        {
            mFolderPath = value;

            if (UseViewStateProperties)
            {
                ViewState["FolderPath"] = mFolderPath;
            }
        }
    }


    /// <summary>
    /// ID of the currently processed media library.
    /// </summary>
    public int LibraryID
    {
        get
        {
            if (UseViewStateProperties && (mLibraryId == 0))
            {
                mLibraryId = ValidationHelper.GetInteger(ViewState["LibraryID"], 0);
            }
            return mLibraryId;
        }
        set
        {
            mLibraryId = value;

            if (UseViewStateProperties)
            {
                ViewState["LibraryID"] = mLibraryId;
            }
        }
    }


    /// <summary>
    /// Folder path of the currently processed library.
    /// </summary>
    public string LibraryFolder
    {
        get
        {
            if (UseViewStateProperties && (mLibraryFolder == null))
            {
                mLibraryFolder = ValidationHelper.GetString(ViewState["LibraryFolder"], null);
            }
            return mLibraryFolder;
        }
        set
        {
            mLibraryFolder = value;

            if (UseViewStateProperties)
            {
                ViewState["LibraryFolder"] = mLibraryFolder;
            }
        }
    }


    /// <summary>
    /// Gets or sets library root folder path.
    /// </summary>
    public string RootFolderPath
    {
        get
        {
            if (UseViewStateProperties && (mRootFolderPath == null))
            {
                mRootFolderPath = ValidationHelper.GetString(ViewState["RootFolderPath"], null);
            }
            return mRootFolderPath;
        }
        set
        {
            mRootFolderPath = value;

            if (UseViewStateProperties)
            {
                ViewState["RootFolderPath"] = mRootFolderPath;
            }
        }
    }


    /// <summary>
    /// Indicates whether the advanced permissions should be checked.
    /// </summary>
    public bool CheckAdvancedPermissions
    {
        get
        {
            return mCheckAdvancedPermissions;
        }
        set
        {
            mCheckAdvancedPermissions = value;
        }
    }

    #endregion


    protected override void OnLoad(EventArgs e)
    {
        RaiseOnCheckPermissions(PERMISSION_READ, this);

        plcMess.IsLiveSite = IsLiveSite;
        plcMess.WrapperControlClientID = pnlContent.ClientID;

        base.OnLoad(e);
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (!StopProcessing)
        {
            // Initialize control
            SetupControl();
        }
        else
        {
            Visible = false;
        }

    }


    /// <summary>
    /// Clears form controls content.
    /// </summary>
    public override void ClearForm()
    {
        txtFolderName.Text = "";
    }


    /// <summary>
    /// Reloads control.
    /// </summary>
    public override void ReloadData()
    {
        SetupControl();
    }


    /// <summary>
    /// Handles folder actions.
    /// </summary>
    public string ProcessFolderAction()
    {
        MediaLibraryInfo libInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(LibraryID);
        if (libInfo != null)
        {
            if (Action.ToLowerCSafe().Trim() == "new")
            {
                if (CheckAdvancedPermissions)
                {
                    CurrentUserInfo currUser = CMSContext.CurrentUser;

                    // Not a global admin
                    if (!currUser.IsGlobalAdministrator)
                    {
                        // Group library
                        bool isGroupLibrary = (libInfo.LibraryGroupID > 0);
                        if (!(isGroupLibrary && currUser.IsGroupAdministrator(libInfo.LibraryGroupID)))
                        {
                            // Checked resource name
                            string resource = (isGroupLibrary) ? "CMS.Groups" : "CMS.MediaLibrary";

                            // Check 'CREATE' & 'MANAGE' permissions
                            if (!(currUser.IsAuthorizedPerResource(resource, PERMISSION_MANAGE) || MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(libInfo, "foldercreate")))
                            {
                                ShowError(MediaLibraryHelper.GetAccessDeniedMessage("foldercreate"));
                                return null;
                            }
                        }
                    }
                }
                // Check 'Folder create' permission
                else if (!MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(libInfo, "foldercreate"))
                {
                    ShowError(MediaLibraryHelper.GetAccessDeniedMessage("foldercreate"));
                    return null;
                }
            }
            else
            {
                // Check 'Folder modify' permission
                if (!MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(libInfo, "foldermodify"))
                {
                    ShowError(MediaLibraryHelper.GetAccessDeniedMessage("foldermodify"));
                    return null;
                }
            }

            SiteInfo si = SiteInfoProvider.GetSiteInfo(libInfo.LibrarySiteID);
            if (si != null)
            {
                // Validate form entry
                string errMsg = ValidateForm(Action, si.SiteName);
                ErrorOccurred = !string.IsNullOrEmpty(errMsg);

                // If validation succeeded
                if (errMsg == "")
                {
                    try
                    {
                        // Update info only if folder was renamed
                        if (MediaLibraryHelper.EnsurePath(FolderPath) != MediaLibraryHelper.EnsurePath(mNewFolderPath))
                        {
                            if (Action.ToLowerCSafe().Trim() == "new")
                            {
                                // Create/Update folder according to action
                                MediaLibraryInfoProvider.CreateMediaLibraryFolder(si.SiteName, LibraryID, mNewFolderPath, false);
                            }
                            else
                            {
                                // Create/Update folder according to action
                                MediaLibraryInfoProvider.RenameMediaLibraryFolder(si.SiteName, LibraryID, FolderPath, mNewFolderPath, false);
                            }

                            // Inform the user on success
                            ShowChangesSaved();

                            // Refresh folder name
                            FolderPath = mNewFolderPath;
                            UpdateFolderName();

                            // Reload media library
                            if (OnFolderChange != null)
                            {
                                OnFolderChange(mNewTreePath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Display an error to the user
                        ShowError(GetString("general.erroroccurred") + " " + ex.Message);

                        mNewTreePath = null;
                    }
                }
                else
                {
                    // Display an error to the user
                    ShowError(errMsg);
                    mNewTreePath = null;
                }
            }
        }

        return mNewTreePath;
    }


    #region "Event handlers"

    protected void btnOk_Click(object sender, EventArgs e)
    {
        ProcessFolderAction();
    }


    protected void btnCancel_Click(object sender, EventArgs e)
    {
        // Let the parent control know about 'Cancel' button click
        if (CancelClick != null)
        {
            CancelClick();
        }
    }

    #endregion


    #region "Private methods"

    /// <summary>
    /// Initializes the control.
    /// </summary>
    private void SetupControl()
    {
        if (!String.IsNullOrEmpty(Action))
        {
            Visible = true;

            // Setup labels
            if (Action.ToLowerCSafe().Trim() == "new")
            {
                pnlFolderEdit.CssClass = "MediaLibraryNewFolder";
                ltlScript.Text = GetFocusScript();
                divButtons.Attributes["class"] = "PageFooterLine FloatRight";
                pnlTabs.ShowTabs = false;
            }
            else
            {
                if (!ErrorOccurred)
                {
                    // Fill the text box with the name of the currently processed folder
                    UpdateFolderName();
                }
                tabGeneral.HeaderText = GetString("general.general");
            }

            // Remove OnClick event when JavaScript is used to refresh page
            if (!string.IsNullOrEmpty(CustomScript))
            {
                btnOk.Attributes["onclick"] = CustomScript;
                txtFolderName.Attributes["onkeydown"] = "try { if (event.keyCode == 13) { " + CustomScript + " return false; } } catch (e) {}";
            }
            else
            {
                pnlContent.DefaultButton = "btnOk";
            }

            if (!Enabled)
            {
                DisableControls();
            }

            plcCancelArea.Visible = DisplayCancel;

            // Don't register Save header action in dialog
            btnOk.RegisterHeaderAction = !DisplayCancel;

            btnCancel.Text = GetString("general.cancel");
            btnOk.Text = GetString("general.ok");
        }
        else
        {
            // Hide control when action wasn't specified
            Visible = false;
        }
    }


    /// <summary>
    /// Updates folder name in the folder text box.
    /// </summary>
    private void UpdateFolderName()
    {
        string safeFolderPath = FolderPath.Replace("/", "\\");
        int folderNameStartIndex = safeFolderPath.LastIndexOfCSafe('\\') + 1;
        txtFolderName.Text = FolderPath.Substring(folderNameStartIndex);
    }


    /// <summary>
    /// Disables controls.
    /// </summary>
    private void DisableControls()
    {
        txtFolderName.Enabled = false;
        btnOk.Enabled = false;
        btnCancel.Enabled = false;
    }


    /// <summary>
    /// Validates form entries.
    /// </summary>    
    /// <param name="action">Action type</param>
    /// <param name="siteName">Site name</param>
    public string ValidateForm(string action, string siteName)
    {
        string errMsg = null;

        string newFolderName = txtFolderName.Text.Trim();

        errMsg = new Validator().NotEmpty(newFolderName, GetString("media.error.FolderNameIsEmpty")).
            IsFolderName(newFolderName, GetString("media.error.FolderNameIsNotValid")).Result;

        if (String.IsNullOrEmpty(errMsg))
        {
            // Check special folder names
            if ((newFolderName == ".") || (newFolderName == ".."))
            {
                errMsg = GetString("media.error.FolderNameIsRelative");
            }

            if (String.IsNullOrEmpty(errMsg))
            {
                bool mustExist = true;

                // Make a note that we are renaming existing folder
                if ((!String.IsNullOrEmpty(Action)) && (Action.ToLowerCSafe().Trim() == "new"))
                {
                    mustExist = false;
                }

                // Check if folder with specified name exists already if required
                if (mustExist)
                {
                    // Existing folder is being renamed
                    if (!Directory.Exists(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(siteName, DirectoryHelper.CombinePath(LibraryFolder, FolderPath))))
                    {
                        errMsg = GetString("media.error.FolderDoesNotExist");
                    }
                }

                if (String.IsNullOrEmpty(errMsg))
                {
                    string hiddenFolderName = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);
                    if ((newFolderName == hiddenFolderName) || ValidationHelper.IsSpecialFolderName(newFolderName))
                    {
                        errMsg = String.Format(GetString("media.error.FolderNameIsReserved"), hiddenFolderName);
                    }

                    if (String.IsNullOrEmpty(errMsg))
                    {
                        // Get new folder path
                        GetNewFolderPath(mustExist);

                        if (MediaLibraryHelper.EnsurePath(FolderPath) != MediaLibraryHelper.EnsurePath(mNewFolderPath))
                        {
                            // Check if new folder doesn't exist yet
                            if (Directory.Exists(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(siteName, DirectoryHelper.CombinePath(LibraryFolder, mNewFolderPath))))
                            {
                                errMsg = GetString("media.error.FolderExists");
                            }
                        }
                    }
                }
            }
        }

        return errMsg;
    }


    /// <summary>
    /// Sets the new folder path.
    /// </summary>    
    private void GetNewFolderPath(bool mustExist)
    {
        string trimFolderName = txtFolderName.Text.Trim();

        if (mustExist)
        {
            string folderPath = FolderPath.Replace('/', '\\');

            mNewFolderPath = GetParentPath(folderPath) + trimFolderName;
            if (folderPath.LastIndexOfCSafe("\\") > 0)
            {
                // Folder is in library tree
                mNewTreePath = DirectoryHelper.CombinePath(LibraryFolder, GetParentPath(folderPath), trimFolderName);
            }
            else
            {
                // Folder is in library root
                mNewTreePath = DirectoryHelper.CombinePath(LibraryFolder, trimFolderName);
            }
        }
        else
        {
            mNewFolderPath = DirectoryHelper.CombinePath(FolderPath, trimFolderName);
            if (FolderPath != "")
            {
                // Folder is in library tree
                mNewTreePath = DirectoryHelper.CombinePath(LibraryFolder, FolderPath, trimFolderName);
            }
            else
            {
                // Folder is in library root
                mNewTreePath = DirectoryHelper.CombinePath(LibraryFolder, trimFolderName);
            }
        }

        // Ensure paths are in correct format
        mNewFolderPath = mNewFolderPath.TrimStart('\\');
        mNewTreePath = mNewTreePath.Replace("\\\\", "\\").Replace('/', '\\');
    }


    /// <summary>
    /// Returns path of the parent folder of the specified folder.
    /// </summary>
    /// <param name="folderPath">Folder path</param>
    private static string GetParentPath(string folderPath)
    {
        if ((!string.IsNullOrEmpty(folderPath)) && (folderPath.LastIndexOfCSafe("\\") > -1))
        {
            return folderPath.Remove(folderPath.LastIndexOfCSafe("\\")) + "\\";
        }

        return "";
    }


    /// <summary>
    /// Returns script for focus folder name textbox.
    /// </summary>
    private string GetFocusScript()
    {
        string script = "function FocusFolderName(){\n" +
                        "var txtBox = document.getElementById('" + txtFolderName.ClientID + "');\n" +
                        "if (txtBox != null) { \n" +
                        "   try {\n" +
                        "       txtBox.focus();\n" +
                        "   } catch (e) {\n" +
                        "       setTimeout('FocusFolderName()',50);\n" +
                        "   }\n" +
                        "}\n" +
                        "}\n";

        return ScriptHelper.GetScript(script);
    }

    #endregion
}
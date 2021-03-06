using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.CMSHelper;
using CMS.ExtendedControls;
using CMS.GlobalHelper;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.SettingsProvider;

using MediaHelper = CMS.GlobalHelper.MediaHelper;


public partial class CMSModules_MediaLibrary_Controls_MediaLibrary_MediaFileMultipleImport : CMSAdminControl
{
    #region "Event & delegates"

    /// <summary>
    /// Delegate used to describe handler of the event required on new file being saved.
    /// </summary>
    /// <param name="file">Info on saved file</param>
    /// <param name="title">New file title</param>
    /// <param name="desc">New file description</param>
    /// <param name="name">New file name</param>
    /// <param name="filePath">Path to the file physical location</param>
    public delegate MediaFileInfo OnSaveRequired(FileInfo file, string title, string desc, string name, string filePath);


    /// <summary>
    /// Event fired when new file should be saved.
    /// </summary>
    public event OnSaveRequired SaveRequired;


    /// <summary>
    /// Event fired after saved succeeded.
    /// </summary>
    public event OnActionEventHandler Action;


    /// <summary>
    /// Delegate for the event fired when URL for item is required.
    /// </summary>
    /// <param name="fileName">Name of the file</param>   
    public delegate string OnGetItemUrl(string fileName);


    /// <summary>
    /// Event occurring when URL for item is required.
    /// </summary>
    public event OnGetItemUrl GetItemUrl;

    #endregion


    #region "Private variables"

    private const string FILES_NUMBERS_TEXT = "(<span style=\"font-weight:bold;\">{0}</span> / {1})";

    private MediaLibraryInfo mLibraryInfo = null;
    private SiteInfo mLibrarySiteInfo = null;

    #endregion


    #region "Public properties"

    /// <summary>
    /// Gets or sets library root folder path.
    /// </summary>
    public string RootFolderPath
    {
        get
        {
            return ValidationHelper.GetString(ViewState["RootFolderPath"], "");
        }
        set
        {
            ViewState["RootFolderPath"] = value;
        }
    }


    /// <summary>
    /// ID of the library files are being imported to.
    /// </summary>
    public int LibraryID
    {
        get
        {
            return ValidationHelper.GetInteger(ViewState["LibraryID"], 0);
        }
        set
        {
            ViewState["LibraryID"] = value;
        }
    }


    /// <summary>
    /// Gets library info object.
    /// </summary>
    public MediaLibraryInfo LibraryInfo
    {
        get
        {
            if ((mLibraryInfo == null) && (LibraryID > 0))
            {
                mLibraryInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(LibraryID);
            }
            return mLibraryInfo;
        }
        set
        {
            mLibraryInfo = value;
        }
    }


    /// <summary>
    /// Gets site info library is related to.
    /// </summary>
    public SiteInfo LibrarySiteInfo
    {
        get
        {
            if ((mLibrarySiteInfo == null) && (LibraryInfo != null))
            {
                mLibrarySiteInfo = SiteInfoProvider.GetSiteInfo(LibraryInfo.LibrarySiteID);
            }
            return mLibrarySiteInfo;
        }
        set
        {
            mLibrarySiteInfo = value;
        }
    }


    /// <summary>
    /// Index of the currently processed file.
    /// </summary>
    public int ImportCurrFileIndex
    {
        get
        {
            return ValidationHelper.GetInteger(ViewState["MediaCurrFileIndex"], 0);
        }
        set
        {
            ViewState["MediaCurrFileIndex"] = value;
        }
    }


    /// <summary>
    /// Overall number of the files to be imported.
    /// </summary>
    public int ImportFilesNumber
    {
        get
        {
            return ValidationHelper.GetInteger(ViewState["MediaFilesNumber"], 0);
        }
        set
        {
            ViewState["MediaFilesNumber"] = value;
        }
    }


    /// <summary>
    /// Paths of the files to be imported.
    /// </summary>
    public string ImportFilePaths
    {
        get
        {
            return ValidationHelper.GetString(ViewState["MediaFilePaths"], "");
        }
        set
        {
            ViewState["MediaFilePaths"] = value;
        }
    }


    /// <summary>
    /// Current folder path.
    /// </summary>
    public string FolderPath
    {
        get
        {
            return ValidationHelper.GetString(ViewState["FolderPath"], "");
        }
        set
        {
            ViewState["FolderPath"] = value;
        }
    }

    #endregion


    protected void Page_Load(object sender, EventArgs e)
    {
        if (!StopProcessing)
        {
            // Initialize nested controls
            SetupControls();
        }
        else
        {
            Visible = false;
        }
    }


    /// <summary>
    /// Initialize nested controls.
    /// </summary>
    private void SetupControls()
    {
        // File import
        chkImportDescriptionToAllFiles.Attributes["onclick"] = "AlterImportBtnText('" + GetString("media.file.import.importallfiles") +
                                                               "', '" + GetString("media.file.import.importfile") + "','" + chkImportDescriptionToAllFiles.ClientID + "','" + btnImportFile.ClientID + "');";

        importFilesTitleElem.TitleImage = ResolveUrl(GetImageUrl("Objects/Media_File/fileimport.png", IsLiveSite));
        importFilesTitleElem.TitleText = GetString("media.file.import.title");

        string script = @" function AlterImportBtnText(textChecked, textUnchecked, chkClientId, btnClientId) {
                                var btnImport = document.getElementById(btnClientId);
                                var chkApplyToAll = document.getElementById(chkClientId);
                                if ((btnClientId != null) && (chkApplyToAll != null)) {
                                    if (chkApplyToAll.checked) {
                                        btnImport.value = textChecked;
                                    }
                                    else {
                                        btnImport.value = textUnchecked;
                                    }
                                }
                            }

                            // Displays required preview according currently imported item type (image vs. media)
                            function DisplayPreview(typeFieldId, chkClientId) {
                                var chkDisplayPrev = document.getElementById(chkClientId);
                                if (chkDisplayPrev != null) {
                                    if (chkDisplayPrev.checked) {
                                        var typeField = document.getElementById(typeFieldId);
                                        if (typeField != null) {
                                            if (typeField.value == 'image') {
                                                // Display image preview
                                                $j('#divImagePreview').attr('style', 'display: block;');
                                                $j('#divMediaPreview').attr('style', 'display: none;');
                                                $j('#divOtherPreview').attr('style', 'display: none;');
                                            }
                                            else if(typeField.value == 'media') {
                                                // Display media preview            
                                                $j('#divMediaPreview').attr('style', 'display: block;');
                                                $j('#divOtherPreview').attr('style', 'display: none;');
                                                $j('#divImagePreview').attr('style', 'display: none;');
                                            }
                                            else
                                            {
                                                // Display other preview
                                                $j('#divOtherPreview').attr('style', 'display: block;');
                                                $j('#divImagePreview').attr('style', 'display: none;');
                                                $j('#divMediaPreview').attr('style', 'display: none;');
                                            }
                                        }
                                    }
                                    else {
                                        // Hide both image as well as media preview
                                        $j('#divImagePreview').attr('style', 'display: none;');
                                        $j('#divMediaPreview').attr('style', 'display: none;');
                                        $j('#divOtherPreview').attr('style', 'display: none;');
                                    }
                                }
                            }";
        ScriptHelper.RegisterStartupScript(Page, typeof(Page), "MultipleImportScripts", ScriptHelper.GetScript(script));

        chkDisplayPreview.Attributes["onclick"] = "DisplayPreview('" + hdnPreviewType.ClientID + "', '" + chkDisplayPreview.ClientID + "');";
    }


    #region "Public methods"

    /// <summary>
    /// Sets default values and clear textboxes.
    /// </summary>
    public void SetDefault()
    {
        txtImportFileTitle.Text = "";
        txtImportFileDescription.Text = "";
    }


    /// <summary>
    /// Setup all labels and buttons text.
    /// </summary>
    public void SetupTexts()
    {
        // Import files
        lblError.Visible = false;
        lblImportFileTitle.Text = GetString("media.file.filetitle");
        lblImportFileDescription.Text = GetString("general.description");
        chkImportDescriptionToAllFiles.Text = GetString("media.file.import.toall");
        btnImportFile.Text = chkImportDescriptionToAllFiles.Checked ? GetString("media.file.import.importallfiles") : GetString("media.file.import.importfile");
        btnImportCancel.Text = GetString("general.cancel");
        rfvImportFileName.ErrorMessage = GetString("general.requiresvalue");
        rfvImportFileTitle.ErrorMessage = GetString("general.requiresvalue");
    }


    /// <summary>
    /// Displays file import controls.
    /// </summary>
    public void SetupImport()
    {
        SetupImport(chkDisplayPreview.Checked);
    }


    /// <summary>
    /// Displays error message.
    /// </summary>
    /// <param name="message">Error message (plain text)</param>
    public void DisplayError(string message)
    {
        lblError.Visible = false;
        if (!string.IsNullOrEmpty(message))
        {
            SetupTexts();
            SetupImport();
            lblError.Text = message;
            lblError.Visible = true;
        }
    }


    /// <summary>
    /// Displays file import controls.
    /// </summary>
    public void SetupImport(bool displayPreview)
    {
        if (LibrarySiteInfo != null)
        {
            plcPreview.Visible = !MediaLibraryHelper.IsExternalLibrary(LibrarySiteInfo.SiteName, LibraryInfo.LibraryFolder);
        }

        // Show preview by default
        chkDisplayPreview.Checked = displayPreview;
        string script = "DisplayPreview('" + hdnPreviewType.ClientID + "', '" + chkDisplayPreview.ClientID + "');";

        ScriptHelper.RegisterStartupScript(Page, typeof(Page), "ShowPreview", ScriptHelper.GetScript(script));

        UpdateImportIndex();
        UpdateImportForm();
    }

    #endregion


    #region "Private methods"

    /// <summary>
    /// Raises action event.
    /// </summary>
    /// <param name="actionName">Name of tha action occuring</param>
    /// <param name="actionArgument">Argument related to the action</param>
    private void RaiseOnAction(string actionName, object actionArgument)
    {
        if (Action != null)
        {
            Action(actionName, actionArgument);
        }
    }


    /// <summary>
    /// Raises save action for specified file.
    /// </summary>
    /// <param name="file">Info on saved file</param>
    /// <param name="title">New file title</param>
    /// <param name="desc">New file description</param>
    /// <param name="name">New file name</param>
    /// <param name="filePath">Path to the file physical location</param>
    private MediaFileInfo RaiseOnSaveRequired(FileInfo file, string title, string desc, string name, string filePath)
    {
        // Save new file in the DB
        if (SaveRequired != null)
        {
            return SaveRequired(file, title, desc, name, filePath);
        }

        return null;
    }


    /// <summary>
    /// Updates information on index of currently processed file.
    /// </summary>
    private void UpdateImportIndex()
    {
        Literal ltlImportFilesNo = ControlsHelper.GetChildControl(importFilesTitleElem.RightPlaceHolder, typeof(Literal)) as Literal;
        if (ltlImportFilesNo == null)
        {
            ltlImportFilesNo = new Literal();
            importFilesTitleElem.RightPlaceHolder.Controls.Add(ltlImportFilesNo);
        }
        ltlImportFilesNo.Text = "<div style=\"white-space:nowrap;\">" + string.Format(FILES_NUMBERS_TEXT, ImportCurrFileIndex, ImportFilesNumber) + "</div>";
    }


    /// <summary>
    /// Handle all the necessary actions performed while file info is imported into the DB.
    /// </summary>
    /// <param name="importAll">Indicates whether the all files should be imported at once</param>
    private void HandleFileImport(bool importAll)
    {
        // Check 'File create' permission
        if (MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(LibraryInfo, "filecreate"))
        {
            // Get set of file paths        
            if (!string.IsNullOrEmpty(ImportFilePaths))
            {
                // Import single file
                if (importAll)
                {
                    // Import all files
                    HandleMultipleMediaFiles();
                }
                else
                {
                    HandleSingleMediaFile();
                }
            }
            else
            {
                // Inform user on error
                RaiseOnAction("importnofiles", null);
            }
        }
        else
        {
            RaiseOnAction("importerror", MediaLibraryHelper.GetAccessDeniedMessage("filecreate"));
        }
    }


    /// <summary>
    /// Handles storing of multiple media files at once.
    /// </summary>
    private void HandleMultipleMediaFiles()
    {
        try
        {
            string[] filePaths = ImportFilePaths.Split('|');
            if (filePaths.Length > 0)
            {
                bool first = true;
                string description = txtImportFileDescription.Text.Trim();
                string title = txtImportFileTitle.Text.Trim();
                string name = txtImportFileName.Text.Trim();

                // Check if the filename is in correct format
                if (!ValidationHelper.IsFileName(name))
                {
                    SetupTexts();

                    // Inform user on error
                    RaiseOnAction("importerror", GetString("media.rename.wrongformat"));
                    return;
                }

                // Go through the files and save one by one
                foreach (string path in filePaths)
                {
                    if (path.Trim() != "")
                    {
                        string filePath = DirectoryHelper.CombinePath(FolderPath, path);

                        // Get file and library info
                        FileInfo currFile = FileInfo.New(RootFolderPath + DirectoryHelper.CombinePath(LibraryInfo.LibraryFolder, filePath));
                        if (currFile != null)
                        {
                            if (first)
                            {
                                // Save media file info on current file
                                RaiseOnSaveRequired(currFile, title, description, URLHelper.GetSafeFileName(name, CMSContext.CurrentSiteName, false), filePath);

                                first = false;
                            }
                            else
                            {
                                string fileName = Path.GetFileNameWithoutExtension(currFile.Name);

                                // Save media file info on current file where title = file name
                                RaiseOnSaveRequired(currFile, fileName, description, URLHelper.GetSafeFileName(fileName, CMSContext.CurrentSiteName, false), filePath);
                            }
                        }
                    }
                }

                FinishImport();
            }
            else
            {
                // Inform user on error
                RaiseOnAction("importnofiles", null);
            }
        }
        catch (Exception ex)
        {
            SetupTexts();

            // Inform user on error
            RaiseOnAction("importerror", ex.Message);
        }
        finally
        {
            SetupImport();
        }
    }


    /// <summary>
    /// Handle storing single media file info into the DB.
    /// </summary>
    private void HandleSingleMediaFile()
    {
        string fileName = txtImportFileName.Text.Trim();
        // Check if the filename is in correct format
        if (!ValidationHelper.IsFileName(fileName))
        {
            SetupTexts();

            // Inform user on error
            RaiseOnAction("importerror", GetString("media.rename.wrongformat"));
            return;
        }

        bool wasLast = false;
        try
        {
            // Get info on current file path
            string currFileName = GetNextFileName();
            if (!String.IsNullOrEmpty(currFileName))
            {
                string filePath = (string.IsNullOrEmpty(FolderPath)) ? currFileName : DirectoryHelper.CombinePath(FolderPath, currFileName);

                // Get file and library info
                FileInfo currFile = FileInfo.New(MediaFileInfoProvider.GetMediaFilePath(CMSContext.CurrentSiteName, LibraryInfo.LibraryFolder, filePath));
                if (currFile != null)
                {
                    // Save media file info on current file
                    RaiseOnSaveRequired(currFile, txtImportFileTitle.Text.Trim(), txtImportFileDescription.Text.Trim(), URLHelper.GetSafeFileName(fileName, CMSContext.CurrentSiteName, false), filePath);
                }
            }
            else
            {
                wasLast = true;
            }

            // Update file paths set and store updated version in the ViewState
            int indexOfDel = ImportFilePaths.IndexOf("|");
            ImportFilePaths = (indexOfDel > 0 ? ImportFilePaths.Remove(0, indexOfDel + 1) : String.Empty);

            wasLast = String.IsNullOrEmpty(ImportFilePaths);
            if (wasLast)
            {
                FinishImport();
                return;
            }
            else
            {
                RaiseOnAction("singlefileimported", null);

                // Increment current file index
                ImportCurrFileIndex++;
            }
        }
        catch (Exception ex)
        {
            SetupTexts();

            // Inform user on error
            RaiseOnAction("importerror", ex.Message);
        }
        finally
        {
            SetupImport();
        }
    }


    /// <summary>
    /// Performs actions required when import finished recently.
    /// </summary>
    private void FinishImport()
    {
        // All files imported- clear ViewState and reload control
        ClearImportViewState();

        // Simulate import CANCEL
        RaiseOnAction("importcancel", null);
        RaiseOnAction("reloaddata", null);
    }


    /// <summary>
    /// Returns next file name for import.
    /// </summary>
    private string GetNextFileName()
    {
        string nextFileName;
        int indexOfDel = ImportFilePaths.IndexOf("|");
        if (indexOfDel >= 0)
        {
            nextFileName = ImportFilePaths.Substring(0, indexOfDel);
        }
        else
        {
            // Last file name
            nextFileName = ImportFilePaths.Trim();
        }
        return nextFileName;
    }


    /// <summary>
    /// Updates information displayed on import form.
    /// </summary>
    private void UpdateImportForm()
    {
        // Get info on next file name
        string nextFileName = GetNextFileName();

        if (!String.IsNullOrEmpty(nextFileName))
        {
            string ext = Path.GetExtension(nextFileName);

            if (!MediaLibraryHelper.IsExtensionAllowed(ext.TrimStart('.')))
            {
                lblError.Text = string.Format(GetString("attach.notallowedextension"), ext, MediaLibraryHelper.GetAllowedExtensions(CMSContext.CurrentSiteName).TrimEnd(';').Replace(";", ", "));
                lblError.Visible = true;
                SetFormEnabled(false);
            }
            else
            {
                SetFormEnabled(true);
                txtImportFileDescription.Text = "";
                txtImportFileName.Text = URLHelper.GetSafeFileName(Path.GetFileNameWithoutExtension(nextFileName), CMSContext.CurrentSiteName, false);
                txtImportFileTitle.Text = Path.GetFileNameWithoutExtension(nextFileName);

                LoadPreview(nextFileName);
            }

            InitializeImportBreadcrumbs(URLHelper.GetSafeFileName(nextFileName, CMSContext.CurrentSiteName));
        }
    }


    /// <summary>
    /// Sets enabled state of form.
    /// </summary>
    /// <param name="enabled">Enabled value</param>
    private void SetFormEnabled(bool enabled)
    {
        txtImportFileName.Enabled = enabled;
        txtImportFileTitle.Enabled = enabled;
        txtImportFileDescription.Enabled = enabled;
        chkImportDescriptionToAllFiles.Enabled = enabled;
        btnImportFile.Enabled = enabled;
    }


    /// <summary>
    /// Loads preview according item type.
    /// </summary>
    /// <param name="fileName">Name fo the file to load preview for</param>
    private void LoadPreview(string fileName)
    {
        // Load preview
        string url = "";
        if (GetItemUrl != null)
        {
            url = GetItemUrl(fileName);
        }

        string ext = Path.GetExtension(fileName);
        if (ImageHelper.IsImage(ext))
        {
            hdnPreviewType.Value = "image";

            // Get image physical path
            string filePath = (string.IsNullOrEmpty(FolderPath)) ? fileName : DirectoryHelper.CombinePath(FolderPath, fileName);
            filePath = MediaFileInfoProvider.GetMediaFilePath(CMSContext.CurrentSiteName, LibraryInfo.LibraryFolder, filePath);

            if (File.Exists(filePath))
            {
                // Load image info
                ImageHelper ih = new ImageHelper();
                ih.LoadImage(File.ReadAllBytes(filePath));

                // Get new dimensions
                int origWidth = ValidationHelper.GetInteger(ih.ImageWidth, 0);
                int origHeight = ValidationHelper.GetInteger(ih.ImageHeight, 0);
                int[] newDimensions = ImageHelper.EnsureImageDimensions(0, 0, 300, origWidth, origHeight);

                // Image preview
                imagePreview.ShowFileIcons = false;
                imagePreview.URL = url;
                imagePreview.SizeToURL = false;
                imagePreview.Width = newDimensions[0];
                imagePreview.Height = newDimensions[1];
                imagePreview.Alt = fileName;
                imagePreview.Tooltip = fileName;

                // Open link
                lnkOpenImage.Target = "_blank";
                lnkOpenImage.Text = GetString("media.file.openimg");
                lnkOpenImage.ToolTip = GetString("media.file.openimg");
                lnkOpenImage.NavigateUrl = url;
            }
        }
        else if (MediaHelper.IsAudioVideo(ext) || MediaHelper.IsFlash(ext))
        {
            hdnPreviewType.Value = "media";

            // Media preview
            mediaPreview.Height = 200;
            mediaPreview.Width = 300;
            mediaPreview.AVControls = true;
            mediaPreview.Loop = true;
            mediaPreview.Url = url;
            mediaPreview.Type = ext;

            // Open link
            lnkOpenMedia.Target = "_blank";
            lnkOpenMedia.Text = GetString("media.file.openimg");
            lnkOpenMedia.ToolTip = GetString("media.file.openimg");
            lnkOpenMedia.NavigateUrl = url;
        }
        else
        {
            hdnPreviewType.Value = "other";

            // Open link
            lnkOpenOther.Target = "_blank";
            lnkOpenOther.Text = GetString("general.open");
            lnkOpenOther.ToolTip = GetString("general.open");
            lnkOpenOther.NavigateUrl = url;
        }
    }


    /// <summary>
    /// Initializes breadcrumbs element placed on the import dialog with updated information on every file being imported.
    /// </summary>
    /// <param name="fileName">Name of the file being imported</param>
    private void InitializeImportBreadcrumbs(string fileName)
    {
        string[,] breadcrumbs = new string[2, 3];
        breadcrumbs[0, 0] = GetString("media.file.list");
        breadcrumbs[0, 1] = "javascript:" + ControlsHelper.GetPostBackEventReference(btnImportCancel, "list");

        breadcrumbs[1, 0] = TextHelper.LimitLength(fileName, 50);

        importFilesTitleElem.Breadcrumbs = breadcrumbs;
    }


    /// <summary>
    /// Removes all import related info stored in the ViewState.
    /// </summary>
    private void ClearImportViewState()
    {
        ViewState.Remove("MediaFilePaths");
        ViewState.Remove("MediaFilesNumber");
        ViewState.Remove("MediaCurrFileIndex");
    }

    #endregion


    #region "Event handlers"

    protected void btnImportCancel_Click(object sender, EventArgs e)
    {
        RaiseOnAction("importcancel", null);
    }


    protected void btnImportFile_Click(object sender, EventArgs e)
    {
        HandleFileImport(chkImportDescriptionToAllFiles.Checked);
    }

    #endregion
}
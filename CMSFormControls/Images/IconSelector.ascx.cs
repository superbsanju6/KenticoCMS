using System;
using System.Web.UI;
using System.Collections;

using CMS.ExtendedControls;
using CMS.FormControls;
using CMS.GlobalHelper;
using CMS.IO;
using CMS.SettingsProvider;

public partial class CMSFormControls_Images_IconSelector : FormEngineUserControl
{
    #region "Constants"

    /// <summary>
    /// Value for 'Do not display any icon'.
    /// </summary>
    private const string NOT_DISPLAY_ICON = "##NONE##";

    #endregion


    #region "Private variables"

    private string mIconsFolder = "Design/Controls/IconSelector/RSS";
    private string mAllowedIconExtensions = "png";

    // Default value for RSS
    private string mFolderPreviewImageName = "24.png";
    private string mFullIconFolderPath = String.Empty;
    private string mCurrentIconFolder = String.Empty;
    private string mMainPanelResourceName = "general.color";
    private string mChildPanelResourceName = "general.size";

    #endregion


    #region "Properties"

    /// <summary>
    /// Returns full path to file system.
    /// </summary>
    private string FullIconFolderPath
    {
        get
        {
            if (mFullIconFolderPath == String.Empty)
            {
                mFullIconFolderPath = GetImagePath(IconsFolder);
                if (mFullIconFolderPath.StartsWithCSafe("~"))
                {
                    mFullIconFolderPath = Server.MapPath(mFullIconFolderPath);
                }
            }
            return mFullIconFolderPath;
        }
    }


    /// <summary>
    /// Gets current action name.
    /// </summary>
    private string CurrentAction
    {
        get
        {
            return hdnAction.Value.ToLowerCSafe().Trim();
        }
        set
        {
            hdnAction.Value = value;
        }
    }


    /// <summary>
    /// Gets current action argument value.
    /// </summary>
    private string CurrentArgument
    {
        get
        {
            return hdnArgument.Value;
        }
    }


    /// <summary>
    /// Gets or set value of selected predefined icon folder.
    /// </summary>
    private string CurrentIconFolder
    {
        get
        {
            return ValidationHelper.GetString(ViewState["CurrentIconFolder"], String.Empty);
        }
        set
        {
            ViewState["CurrentIconFolder"] = value;
        }
    }


    /// <summary>
    /// Gets or sets value of selected predefined icon.
    /// </summary>
    private string CurrentIcon
    {
        get
        {
            return ValidationHelper.GetString(ViewState["CurrentIcon"], String.Empty);
        }
        set
        {
            ViewState["CurrentIcon"] = value;
        }
    }

    #endregion


    #region "Public properties"

    /// <summary>
    /// Gets or sets folder name from which icons will be taken.
    /// </summary>
    public string IconsFolder
    {
        get
        {
            return mIconsFolder;
        }
        set
        {
            mIconsFolder = value;
        }
    }


    /// <summary>
    /// Gets or sets the files pattern for the icons files.
    /// </summary>
    public string AllowedIconExtensions
    {
        get
        {
            return mAllowedIconExtensions;
        }
        set
        {
            mAllowedIconExtensions = value;
        }
    }


    /// <summary>
    /// Gets or sets default image displayed as preview of icon image set.
    /// </summary>
    public string FolderPreviewImageName
    {
        get
        {
            return mFolderPreviewImageName;
        }
        set
        {
            mFolderPreviewImageName = value;
        }
    }


    /// <summary>
    /// Gets or set resource name for main panel.
    /// </summary>
    public string MainPanelResourceName
    {
        get
        {
            return mMainPanelResourceName;
        }
        set
        {
            mMainPanelResourceName = value;
        }
    }


    /// <summary>
    /// Gets or set resource name for main panel.
    /// </summary>
    public string ChildPanelResourceName
    {
        get
        {
            return mChildPanelResourceName;
        }
        set
        {
            mChildPanelResourceName = value;
        }
    }


    /// <summary>
    /// Gets or sets field value.
    /// </summary>
    public override object Value
    {
        get
        {
            // Predefined icon
            if (UsingPredefinedIcon)
            {
                return IconsFolder + "/" + CurrentIconFolder + "/" + CurrentIcon;
            }
            // Custom icon
            else if (radCustomIcon.Checked)
            {
                string url = mediaSelector.Value;
                if (url.StartsWithCSafe("/"))
                {
                    return "~" + URLHelper.RemoveApplicationPath(url);
                }

                // Return value for 'Do not display icon'
                if (string.IsNullOrEmpty(url))
                {
                    return NOT_DISPLAY_ICON;
                }

                return url;
            }

            // None icon
            return NOT_DISPLAY_ICON;
        }
        set
        {
            // Initialize only for regular postback
            if (!RequestHelper.IsAsyncPostback())
            {
                string stringValue = ValidationHelper.GetString(value, String.Empty);

                // Check for String.Empty is because of backward compatibility(String.Empty was previous value for not displaying any icon)
                if ((stringValue != String.Empty) && (stringValue != NOT_DISPLAY_ICON) && (value != null))
                {
                    string virtualPath = URLHelper.GetVirtualPath(GetImagePath(stringValue));
                    string folder = GetImagePath(IconsFolder);

                    // Check if same with starting path for local icon set
                    if (virtualPath.StartsWithCSafe(folder))
                    {
                        string[] parts = virtualPath.Replace(folder, String.Empty).TrimStart('/').Split('/');
                        if (parts.Length == 2)
                        {
                            try
                            {
                                FileInfo fi = FileInfo.New(Server.MapPath(virtualPath));
                                if (fi.Exists)
                                {
                                    CurrentIconFolder = parts[0];
                                    CurrentIcon = parts[1];
                                }
                            }
                            catch (Exception ex)
                            {
                                lblError.Text += "[IconSelector.SetValue]: Error accessing selected icon. Original exception: " + ex.Message;
                            }
                        }

                        // Ensure controls are available
                        if (radPredefinedIcon == null)
                        {
                            pnlUpdateContent.LoadContainer();
                            pnlUpdate.LoadContainer();
                        }
                        radPredefinedIcon.Checked = true;
                    }
                    else
                    {
                        // Ensure controls are available
                        if (radCustomIcon == null)
                        {
                            pnlUpdateContent.LoadContainer();
                            pnlUpdate.LoadContainer();
                        }
                        radCustomIcon.Checked = true;
                        mediaSelector.Value = stringValue;
                    }
                }
                // First load when webpart is added to design tab
                else if (value == null)
                {
                    // Ensure controls are available
                    if (radPredefinedIcon == null)
                    {
                        pnlUpdateContent.LoadContainer();
                        pnlUpdate.LoadContainer();
                    }
                    radPredefinedIcon.Checked = true;
                }
                // Empty value or 'None icon'
                else if ((stringValue == String.Empty) || (stringValue == NOT_DISPLAY_ICON))
                {
                    // Ensure controls are available
                    if (radDoNotDisplay == null)
                    {
                        pnlUpdateContent.LoadContainer();
                        pnlUpdate.LoadContainer();
                    }
                    radDoNotDisplay.Checked = true;
                }
            }
        }
    }


    /// <summary>
    /// Indicates if predefined icons are used instead of custom icon.
    /// </summary>
    public bool UsingPredefinedIcon
    {
        get
        {
            return radPredefinedIcon.Checked;
        }
    }

    #endregion


    #region "Page events"

    /// <summary>
    /// Page load event.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!StopProcessing)
        {
            InitializeControlScripts();
            SetupControls();
        }
    }


    /// <summary>
    /// PreRender event handler.
    /// </summary>
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (!String.IsNullOrEmpty(lblError.Text))
        {
            lblError.Visible = true;
            pnlUpdate.Visible = false;

            pnlUpdateContent.Update();
        }
    }

    #endregion


    #region "Control methods"

    /// <summary>
    /// Setup all contained controls.
    /// </summary>
    private void SetupControls()
    {
        // Reset error label
        lblError.Text = String.Empty;
        lblError.Visible = false;

        // Setup main radio button controls
        radCustomIcon.Text = GetString("iconselector.custom");
        radCustomIcon.Attributes.Add("onclick", "SetAction_" + ClientID + "('switch','');RaiseHiddenPostBack_" + ClientID + "();");
        radPredefinedIcon.Text = GetString("iconselector.predefined");
        radPredefinedIcon.Attributes.Add("onclick", "SetAction_" + ClientID + "('switch','');RaiseHiddenPostBack_" + ClientID + "();");
        radDoNotDisplay.Text = GetString("iconselector.donotdisplay");
        radDoNotDisplay.Attributes.Add("onclick", "SetAction_" + ClientID + "('switch','');RaiseHiddenPostBack_" + ClientID + "();");

        // Setup panels
        pnlMain.GroupingText = GetString(MainPanelResourceName);
        pnlChild.GroupingText = GetString(ChildPanelResourceName);

        // Configuration of media dialog
        DialogConfiguration config = new DialogConfiguration();
        config.SelectableContent = SelectableContentEnum.OnlyImages;
        config.OutputFormat = OutputFormatEnum.URL;
        config.HideWeb = false;
        config.ContentSites = AvailableSitesEnum.All;
        config.DialogWidth = 90;
        config.DialogHeight = 80;
        config.UseRelativeDimensions = true;
        config.LibSites = AvailableSitesEnum.All;


        mediaSelector.UseCustomDialogConfig = true;
        mediaSelector.DialogConfig = config;
        mediaSelector.ShowPreview = false;
        mediaSelector.IsLiveSite = IsLiveSite;

        if (!RequestHelper.IsAsyncPostback())
        {
            // Load initial data and ensure something is selected
            if ((!radCustomIcon.Checked) && (!radDoNotDisplay.Checked) && (!radPredefinedIcon.Checked))
            {
                radPredefinedIcon.Checked = true;
            }
            HandleSwitchAction();
        }
    }


    /// <summary>
    /// Initializes all the script required for communication between controls.
    /// </summary>
    private void InitializeControlScripts()
    {
        // SetAction function setting action name and passed argument
        string setAction = "function SetAction_" + ClientID + "(action, argument) {                                              " +
                           "    var hdnAction = document.getElementById('" + hdnAction.ClientID + "');     " +
                           "    var hdnArgument = document.getElementById('" + hdnArgument.ClientID + "'); " +
                           @"    if ((hdnAction != null) && (hdnArgument != null)) {                             
                                   if (action != null) {                                                       
                                       hdnAction.value = action;                                               
                                   }                                                                           
                                   if (argument != null) {                                                     
                                       hdnArgument.value = argument;                                           
                                   }                                                                           
                               }                                                                               
                           }                                                                                    ";

        // Get reffernce causing postback to hidden button
        string postBackRef = ControlsHelper.GetPostBackEventReference(hdnButton, String.Empty);
        string raiseOnAction = " function RaiseHiddenPostBack_" + ClientID + "(){" + postBackRef + ";}\n";

        ltlScript.Text = ScriptHelper.GetScript(setAction + raiseOnAction);
    }

    #endregion


    #region "Helper methods"

    /// <summary>
    /// Check if file extension is allowed.
    /// </summary>
    /// <param name="info">FileInfo to check</param>
    /// <returns>True if extension isallowed otherwise false</returns>
    private bool IsAllowedFileExtension(FileInfo info)
    {
        string allowedExt = ";" + AllowedIconExtensions.Trim(';').ToLowerCSafe() + ";";
        try
        {
            return allowedExt.Contains(";" + info.Extension.ToLowerCSafe().TrimStart('.') + ";");
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Gets Arraylist filled with folder icons data.
    /// </summary>
    /// <returns>ArrayList with data for icon sets</returns>
    private ArrayList GetPredefinedIconFoldersSet()
    {
        ArrayList directories = new ArrayList();

        try
        {
            if (!String.IsNullOrEmpty(FullIconFolderPath))
            {
                DirectoryInfo di = DirectoryInfo.New(FullIconFolderPath);

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    bool containIcons = false;
                    string previewIconName = FolderPreviewImageName.ToLowerCSafe();
                    string firstIconName = String.Empty;

                    // Get files array and filter it
                    FileInfo[] files = dir.GetFiles();
                    files = Array.FindAll(files, IsAllowedFileExtension);

                    foreach (FileInfo fi in files)
                    {
                        firstIconName = String.Empty;

                        // Store first icon name to be used if default preview icon is missing
                        if (firstIconName == String.Empty)
                        {
                            firstIconName = fi.Name;
                            containIcons = true;
                        }

                        // Check for default icon
                        if (fi.Name.ToLowerCSafe() == previewIconName)
                        {
                            firstIconName = fi.Name;
                            break;
                        }
                    }

                    if (containIcons)
                    {
                        directories.Add(new string[] { dir.Name, firstIconName });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lblError.Text += "[IconSelector.GetPredefinedIconFoldersSet]: Error loading predefined icon set. Original exception: " + ex.Message;
        }

        return directories;
    }


    /// <summary>
    /// Render Folder icon preview HTML.
    /// </summary>
    /// <param name="disabled">Determine if input fields should be rendered as disabled</param>
    private void GetFolderIconPreview(bool disabled)
    {
        GetFolderIconPreview(disabled, CurrentIconFolder);
    }


    /// <summary>
    /// Render Folder icon preview HTML.
    /// </summary>
    /// <param name="disabled">Determine if input fields should be rendered as disabled</param>
    /// <param name="defaultValue">Determine default value which should be checked</param>
    private void GetFolderIconPreview(bool disabled, string defaultValue)
    {
        ltlFolders.Text = "<table><tr class=\"Row\">";
        int counter = 0;
        string defaultFolder = defaultValue;
        foreach (string[] folderInfo in GetPredefinedIconFoldersSet())
        {
            string dirName = folderInfo[0];
            string iconName = folderInfo[1];

            string path = GetImagePath(IconsFolder + "/" + dirName + "/" + iconName);

            // For each folder td element with icon preview is generated 
            ltlFolders.Text += "<td class=\"Cell\"><img src=\"" + UIHelper.ResolveImageUrl(path) +
                               "\" alt=\"" + iconName + "\" /><br />" +
                               "<input type=\"radio\" id=\"" + ClientID + "_" + "folder" +
                               counter + "\" name=\"" + ClientID + "_" + "folders\" value=\"" +
                               dirName + "\" ";

            // Check for selected value
            if ((defaultValue == String.Empty) || (dirName.ToLowerCSafe() == defaultValue.ToLowerCSafe()))
            {
                ltlFolders.Text += "checked=\"checked\" ";
                defaultValue = dirName.ToString();
            }

            // Check if disabled
            if (disabled)
            {
                ltlFolders.Text += "disabled=\"disabled\" ";
            }
            else
            {
                ltlFolders.Text += "onclick=\"SetAction_" + ClientID + "('changefolder','" +
                                   dirName.ToString() + "');RaiseHiddenPostBack_" + ClientID + "();\" ";
            }
            ltlFolders.Text += "/></td>";
            counter++;
        }
        ltlFolders.Text += "</tr></table>";

        // Aktualize value of current icon folder
        CurrentIconFolder = defaultValue;
    }


    /// <summary>
    /// Gets Array List with icons located in specified directory.
    /// </summary>
    /// <param name="di">DirectoryInfo of particular directory</param>
    /// <returns>ArrayList with contained icons</returns>
    private ArrayList GetIconsInFolder(DirectoryInfo di)
    {
        ArrayList icons = new ArrayList();
        try
        {
            // Get files array and filter it
            FileInfo[] files = di.GetFiles();
            files = Array.FindAll(files, IsAllowedFileExtension);
            foreach (FileInfo fi in files)
            {
                icons.Add(fi.Name);
            }
        }
        catch (Exception ex)
        {
            lblError.Text = "[IconSelector.GetIconsInFolder]: Error getting icons in source icon folder. Original exception: " + ex.Message;
        }
        return icons;
    }

    #endregion


    #region "Handler methods"

    /// <summary>
    /// Generate apropriate icons if source folder is changed.
    /// </summary>
    /// <param name="folderName">Name of selected folder</param>
    private void HandleChangeFolderAction(string folderName)
    {
        HandleChangeFolderAction(folderName, false, CurrentIcon);
    }


    /// <summary>
    /// Generate apropriate icons if source folder is changed.
    /// </summary>
    /// <param name="folderName">Name of selected folder</param>
    /// <param name="disabled">Determine if input fields should be rendered as disabled</param>
    /// <param name="defaultValue">Determine default value which should be checked</param>
    private void HandleChangeFolderAction(string folderName, bool disabled, string defaultValue)
    {
        try
        {
            DirectoryInfo di = DirectoryInfo.New(DirectoryHelper.CombinePath(FullIconFolderPath, folderName));
            string directoryName = di.Name;
            int counter = 0;
            ArrayList iconList = GetIconsInFolder(di);
            string defaultIcon = (iconList.Contains(defaultValue)) ? defaultValue : String.Empty;
            string labels = "<tr class=\"Row\">";
            string icons = "<tr class=\"Row\">";
            ltlIcons.Text = "<table><tr class=\"Row\">";
            foreach (string fileInfo in iconList)
            {
                // For each icon td element is generated
                labels += "<td class=\"Cell\">" + GetString("iconcaption." +
                                                            fileInfo.Remove(fileInfo.LastIndexOfCSafe('.'))) + "</td>";

                string path = GetImagePath(IconsFolder + "/" + directoryName + "/" + fileInfo);
                
                icons += "<td class=\"Cell\"><div class=\"Icon\"><img src=\"" + UIHelper.ResolveImageUrl(path) +
                         "\" alt=\"" + fileInfo + "\" /><br />" +
                         "<input type=\"radio\" id=\"" + ClientID + "_" + "icon" +
                         counter + "\" name=\"" + ClientID + "_" + "icons\" value=\"" +
                         directoryName + "\" ";

                // Check for selected value
                if ((defaultIcon == String.Empty) || (fileInfo.ToLowerCSafe() == defaultValue.ToLowerCSafe()))
                {
                    icons += "checked=\"checked\" ";
                    defaultIcon = fileInfo;
                }

                // Check if disabled
                if (disabled)
                {
                    icons += "disabled=\"disabled\"";
                }
                else
                {
                    icons += "onclick=\"SetAction_" + ClientID + "('select','" + fileInfo + "');RaiseHiddenPostBack_" + ClientID + "();\"";
                }

                icons += " /></div></td>";
                counter++;
            }
            ltlIcons.Text += labels + "</tr>" + icons + "</tr></table>";
            CurrentIcon = defaultIcon;
        }
        catch (Exception ex)
        {
            lblError.Text += "[IconSelector.HandleChangeFolderAction]: Error getting icons in selected icon folder. Original exception: " + ex.Message;
        }
        CurrentIconFolder = folderName;
        pnlUpdateIcons.Update();
    }


    /// <summary>
    /// Handle situation when type of choosing is changed.
    /// </summary>
    private void HandleSwitchAction()
    {
        if (radCustomIcon.Checked)
        {
            GetFolderIconPreview(true, CurrentIconFolder);
            HandleChangeFolderAction(CurrentIconFolder, true, CurrentIcon);
            mediaSelector.Enabled = true;
            pnlUpdate.Update();
        }
        else if (radPredefinedIcon.Checked)
        {
            GetFolderIconPreview(false, CurrentIconFolder);
            HandleChangeFolderAction(CurrentIconFolder, false, CurrentIcon);
            mediaSelector.Enabled = false;
            pnlUpdate.Update();
        }
        else
        {
            GetFolderIconPreview(true, CurrentIconFolder);
            HandleChangeFolderAction(CurrentIconFolder, true, CurrentIcon);
            mediaSelector.Enabled = false;
            pnlUpdate.Update();
        }
    }


    /// <summary>
    /// Behaves as mediator in communication line between control taking action and the rest of the same level controls.
    /// </summary>
    protected void hdnButton_Click(object sender, EventArgs e)
    {
        switch (CurrentAction)
        {
                // Switch from predefined icon to custom
            case "switch":
                HandleSwitchAction();
                break;

                // Change predefined icon folder
            case "changefolder":
                //this.CurrentIcon = String.Empty;
                HandleChangeFolderAction(CurrentArgument);
                break;

                // Select icon
            case "select":
                CurrentIcon = CurrentArgument;
                ClearActionElems();
                break;

                // By default do nothing
            default:
                break;
        }
    }


    /// <summary>
    /// Clears hidden control elements fo future use.
    /// </summary>
    private void ClearActionElems()
    {
        CurrentAction = String.Empty;
        hdnArgument.Value = String.Empty;
    }

    #endregion
}
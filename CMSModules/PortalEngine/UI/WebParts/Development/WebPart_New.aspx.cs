using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;

using CMS.ExtendedControls;
using CMS.GlobalHelper;
using CMS.IO;
using CMS.PortalEngine;
using CMS.SettingsProvider;
using CMS.UIControls;
using CMS.EventLog;
using CMS.CMSHelper;
using CMS.DataEngine;

public partial class CMSModules_PortalEngine_UI_WebParts_Development_WebPart_New : SiteManagerPage
{
    #region "Variables"

    private string[,] pageTitleTabs = new string[3, 4];

    #endregion


    protected void Page_Load(object sender, EventArgs e)
    {
        CurrentMaster.DisplaySiteSelectorPanel = true;

        // Setup page title text and image
        CurrentMaster.Title.TitleText = GetString("Development-WebPart_Edit.TitleNew");
        CurrentMaster.Title.TitleImage = GetImageUrl("Objects/CMS_WebPart/new.png");

        CurrentMaster.Title.HelpTopicName = "new_web_part";
        CurrentMaster.Title.HelpName = "helpTopic";

        // Initialize
        btnOk.Text = GetString("general.ok");
        rfvWebPartDisplayName.ErrorMessage = GetString("Development-WebPart_Edit.ErrorDisplayName");
        rfvWebPartName.ErrorMessage = GetString("Development-WebPart_Edit.ErrorWebPartName");

        webpartSelector.ShowInheritedWebparts = false;

        lblWebpartList.Text = GetString("DevelopmentWebPartEdit.InheritedWebPart");

        // Set breadcrumbs
        int i = 0;
        pageTitleTabs[i, 0] = GetString("Development-WebPart_Edit.WebParts");
        pageTitleTabs[i, 1] = URLHelper.ResolveUrl("~/CMSModules/PortalEngine/UI/WebParts/Development/Category_Frameset.aspx");
        pageTitleTabs[i, 2] = "";
        pageTitleTabs[i, 3] = "if (parent.frames['webparttree']) { parent.frames['webparttree'].location.href = '" + URLHelper.ResolveUrl("~/CMSModules/PortalEngine/UI/WebParts/Development/WebPart_Tree.aspx") + "'; }";
        i++;

        int parentid = QueryHelper.GetInteger("parentid", 0);
        WebPartCategoryInfo categoryInfo = WebPartCategoryInfoProvider.GetWebPartCategoryInfoById(parentid);

        // Check if the parent category is a root category, if not => display both (root + parent)
        if ((categoryInfo != null) && (categoryInfo.CategoryParentID != 0))
        {
            // Add a category tab
            pageTitleTabs[i, 0] = HTMLHelper.HTMLEncode(categoryInfo.CategoryDisplayName);
            pageTitleTabs[i, 1] = URLHelper.ResolveUrl("~/CMSModules/PortalEngine/UI/WebParts/Development/Category_Frameset.aspx") + "?categoryid=" + categoryInfo.CategoryID;
            pageTitleTabs[i, 2] = "";
            i++;
        }

        pageTitleTabs[i, 0] = GetString("Development-WebPart_Edit.New");
        pageTitleTabs[i, 1] = "";
        pageTitleTabs[i, 2] = "";

        CurrentMaster.Title.Breadcrumbs = pageTitleTabs;

        FileSystemDialogConfiguration config = new FileSystemDialogConfiguration();
        config.DefaultPath = "CMSWebParts";
        config.AllowedExtensions = "ascx";
        config.ShowFolders = false;
        FileSystemSelector.DialogConfig = config;
        FileSystemSelector.AllowEmptyValue = false;
        FileSystemSelector.SelectedPathPrefix = "~/CMSWebParts/";
    }


    /// <summary>
    /// Handles radio buttons change.
    /// </summary>
    protected void radNewWebPart_CheckedChanged(object sender, EventArgs e)
    {
        plcFileName.Visible = radNewWebPart.Checked;
        plcWebparts.Visible = radInherited.Checked;
    }


    /// <summary>
    /// Creates new web part.
    /// </summary>
    protected void btnOK_Click(object sender, EventArgs e)
    {
        // Validate the text box fields
        string errorMessage = new Validator().IsCodeName(txtWebPartName.Text, GetString("general.invalidcodename")).Result;

        // Check file name
        if (!chkGenerateFiles.Checked && radNewWebPart.Checked)
        {
            if (errorMessage == String.Empty)
            {
                string webpartPath = GetWebPartPhysicalPath(FileSystemSelector.Value.ToString());

                if (!radInherited.Checked)
                {
                    errorMessage = new Validator().IsFileName(Path.GetFileName(webpartPath), GetString("WebPart_Clone.InvalidFileName")).Result;
                }
            }
        }

        if (errorMessage != String.Empty)
        {
            ShowError(HTMLHelper.HTMLEncode(errorMessage));
            return;
        }

        // Run in transaction
        using (var tr = new CMSTransactionScope())
        {
            WebPartInfo wi = new WebPartInfo();

            // Check if new name is unique
            WebPartInfo webpart = WebPartInfoProvider.GetWebPartInfo(txtWebPartName.Text);
            if (webpart != null)
            {
                ShowError(GetString("Development.WebParts.WebPartNameAlreadyExist").Replace("%%name%%", txtWebPartName.Text));
                return;
            }


            string filename = FileSystemSelector.Value.ToString().Trim();
            if (filename.ToLowerCSafe().StartsWithCSafe("~/cmswebparts/"))
            {
                filename = filename.Substring("~/cmswebparts/".Length);
            }

            wi.WebPartDisplayName = txtWebPartDisplayName.Text.Trim();
            wi.WebPartFileName = filename;
            wi.WebPartName = txtWebPartName.Text.Trim();
            wi.WebPartCategoryID = QueryHelper.GetInteger("parentid", 0);
            wi.WebPartDescription = "";
            wi.WebPartDefaultValues = "<form></form>";
            // Initialize WebPartType - fill it with the default value
            wi.WebPartType = wi.WebPartType;

            // Inherited web part
            if (radInherited.Checked)
            {
                // Check if is selected webpart and isn't category item
                if (ValidationHelper.GetInteger(webpartSelector.Value, 0) <= 0)
                {
                    ShowError(GetString("WebPartNew.InheritedCategory"));
                    return;
                }

                int parentId = ValidationHelper.GetInteger(webpartSelector.Value, 0);
                var parent = WebPartInfoProvider.GetWebPartInfo(parentId);
                if (parent != null)
                {
                    wi.WebPartType = parent.WebPartType;
                    wi.WebPartResourceID = parent.WebPartResourceID;
                    wi.WebPartSkipInsertProperties = parent.WebPartSkipInsertProperties;
                }

                wi.WebPartParentID = parentId;

                // Create empty default values definition
                wi.WebPartProperties = "<defaultvalues></defaultvalues>";
            }
            else
            {
                // Check if filename was added
                if (!FileSystemSelector.IsValid())
                {
                    ShowError(FileSystemSelector.ValidationError);

                    return;
                }
                else
                {
                    wi.WebPartProperties = "<form></form>";
                    wi.WebPartParentID = 0;
                }
            }

            // Save the web part
            WebPartInfoProvider.SetWebPartInfo(wi);

            if (chkGenerateFiles.Checked && radNewWebPart.Checked)
            {
                string physicalFile = WebPartInfoProvider.GetFullPhysicalPath(wi);
                if (!File.Exists(physicalFile))
                {
                    string ascx = null;
                    string code = null;
                    string designer = null;

                    // Write the files
                    try
                    {
                        CodeTemplateGenerator.GenerateWebPartCode(wi, null, out ascx, out code, out designer);

                        string folder = Path.GetDirectoryName(physicalFile);

                        // Ensure the folder
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        File.WriteAllText(physicalFile, ascx);
                        File.WriteAllText(physicalFile + ".cs", code);

                        // Designer file
                        if (!String.IsNullOrEmpty(designer))
                        {
                            File.WriteAllText(physicalFile + ".designer.cs", designer);
                        }

                    }
                    catch (Exception ex)
                    {
                        LogAndShowError("WebParts", "GENERATEFILES", ex, true);
                        return;
                    }
                }
                else
                {
                    ShowError(String.Format(GetString("General.FileExistsPath"), physicalFile));
                    return;
                }
            }

            // Refresh web part tree
            ScriptHelper.RegisterStartupScript(this, typeof(string), "reloadframe", ScriptHelper.GetScript(
                "parent.frames['webparttree'].location.replace('" + URLHelper.ResolveUrl("~/CMSModules/PortalEngine/UI/WebParts/Development/WebPart_Tree.aspx") + "?webpartid=" + wi.WebPartID + "');" +
                "location.replace('" + URLHelper.ResolveUrl("~/CMSModules/PortalEngine/UI/WebParts/Development/WebPart_Edit_Frameset.aspx") + "?webpartid=" + wi.WebPartID + "')"
                                                                                        ));

            pageTitleTabs[1, 0] = HTMLHelper.HTMLEncode(wi.WebPartDisplayName);
            CurrentMaster.Title.Breadcrumbs = pageTitleTabs;

            ShowChangesSaved();
            plcTable.Visible = false;

            tr.Commit();
        }
    }


    private string GetWebPartPhysicalPath(string webpartPath)
    {
        webpartPath = webpartPath.Trim();

        if (webpartPath.StartsWithCSafe("~/"))
        {
            return Server.MapPath(webpartPath);
        }

        string fileName = webpartPath.Trim('/').Replace('/', '\\');
        return Path.Combine(Server.MapPath(WebPartInfoProvider.WebPartsDirectory), fileName);
    }
}
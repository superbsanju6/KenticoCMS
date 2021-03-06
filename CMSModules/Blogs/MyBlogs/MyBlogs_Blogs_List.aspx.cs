using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using CMS.Blogs;
using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.SettingsProvider;
using CMS.UIControls;
using CMS.DocumentEngine;
using CMS.DataEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;

public partial class CMSModules_Blogs_MyBlogs_MyBlogs_Blogs_List : CMSMyBlogsPage
{
    #region "Variables"

    protected CurrentUserInfo currentUser = null;
    protected bool isAuthorized = false;

    #endregion


    #region "Page events"

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        gridBlogs.ZeroRowsText = GetString("mydesk.ui.noblogs");
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        currentUser = CMSContext.CurrentUser;
        if (currentUser == null)
        {
            return;
        }

        // No cms.blog doc. type
        if (DataClassInfoProvider.GetDataClass("cms.blog") == null)
        {
            RedirectToInformation(GetString("blog.noblogdoctype"));
        }

        // Check if user is authorized to manage 
        isAuthorized = currentUser.IsAuthorizedPerResource("CMS.Blog", "Manage") || (currentUser.IsAuthorizedPerClassName("cms.blog", "Manage", CMSContext.CurrentSiteName) &&
                                                                                     currentUser.IsAuthorizedPerClassName("cms.blogpost", "Manage", CMSContext.CurrentSiteName));

        // Register grid events
        gridBlogs.OnExternalDataBound += gridBlogs_OnExternalDataBound;
        gridBlogs.OnDataReload += gridBlogs_OnDataReload;
        gridBlogs.ShowActionsMenu = true;
        gridBlogs.Columns = "BlogID, ClassName, BlogName, NodeID, DocumentCulture, NodeOwner, BlogModerators";

        // Get all possible columns to retrieve
        IDataClass nodeClass = DataClassFactory.NewDataClass("CMS.Tree");
        DocumentInfo di = new DocumentInfo();
        BlogInfo bi = new BlogInfo();
        gridBlogs.AllColumns = SqlHelperClass.MergeColumns(SqlHelperClass.MergeColumns(SqlHelperClass.MergeColumns(bi.ColumnNames), SqlHelperClass.MergeColumns(di.ColumnNames)), SqlHelperClass.MergeColumns(nodeClass.ColumnNames));

        // Get ClassID of the 'cms.blogpost' class
        DataClassInfo dci = DataClassInfoProvider.GetDataClass("cms.blogpost");
        string classId = "";
        string script = "";

        if (dci != null)
        {
            classId = dci.ClassID.ToString();
        }

        // Get script to redirect to new blog post page        
        script += "function NewPost(parentId, culture) { \n";
        script += "     if (parentId != 0) { \n";
        script += "         parent.parent.parent.location.href = \"" + ResolveUrl("~/CMSDesk/default.aspx") + "?section=content&action=new&nodeid=\" + parentId + \"&classid=" + classId + "&culture=\" + culture;";
        script += "}} \n";

        // Generate javascript code
        ltlScript.Text = ScriptHelper.GetScript(script);
    }

    #endregion


    #region "UniGrid Events"

    protected DataSet gridBlogs_OnDataReload(string completeWhere, string currentOrder, int currentTopN, string columns, int currentOffset, int currentPageSize, ref int totalRecords)
    {
        totalRecords = -1;
        return BlogHelper.GetBlogs(CMSContext.CurrentSiteName, currentUser.UserID, null, columns, completeWhere);
    }


    protected object gridBlogs_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        switch (sourceName.ToLowerCSafe())
        {
            case "edit":
            case "newpost":
                GridViewRow gvr = (parameter as GridViewRow);
                if (gvr != null)
                {
                    DataRowView dr = (gvr.DataItem as DataRowView);
                    if (dr != null)
                    {
                        TreeNode blogNode = TreeNode.New(dr.Row, ValidationHelper.GetString(dr.Row["ClassName"], ""));
                        if (blogNode != null)
                        {
                            isAuthorized = isAuthorized || BlogHelper.IsUserBlogOwner(currentUser.UserID, blogNode);
                            if (!isAuthorized)
                            {
                                ImageButton button = ((ImageButton)sender);
                                if (sourceName.ToLowerCSafe() == "edit")
                                {
                                    button.ImageUrl = GetImageUrl("Design/Controls/UniGrid/Actions/editdisabled.png");
                                }
                                else
                                {
                                    button.ImageUrl = GetImageUrl("Design/Controls/UniGrid/Actions/addpostdisabled.png");
                                }
                                button.Enabled = false;
                            }
                        }
                    }
                }
                break;
        }
        return parameter;
    }

    #endregion
}
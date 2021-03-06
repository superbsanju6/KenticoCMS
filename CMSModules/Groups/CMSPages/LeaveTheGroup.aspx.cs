using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using CMS.Community;
using CMS.UIControls;

public partial class CMSModules_Groups_CMSPages_LeaveTheGroup : CMSLiveModalPage
{
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        groupLeaveElem.LeaveButton = btnLeave;
        groupLeaveElem.CancelButton = btnCancel;
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        CurrentMaster.Title.TitleText = GetString("Groups.LeaveTheGroup");
        CurrentMaster.Title.TitleImage = GetImageUrl("CMSModules/CMS_Groups/leavethegroup.png");
        Title = GetString("Groups.LeaveTheGroup");

        if (CommunityContext.CurrentGroup != null)
        {
            groupLeaveElem.Group = CommunityContext.CurrentGroup;
        }
    }
}
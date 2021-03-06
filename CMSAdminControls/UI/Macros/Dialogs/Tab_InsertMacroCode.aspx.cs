﻿using System;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.PortalEngine;
using CMS.UIControls;

public partial class CMSAdminControls_UI_Macros_Dialogs_Tab_InsertMacroCode : CMSModalPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        macroEditor.ResolverName = QueryHelper.GetString("resolver", "");
    }


    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (CMSContext.ViewMode == ViewModeEnum.LiveSite)
        {
            // Register custom css if exists
            RegisterDialogCSSLink();
            SetLiveDialogClass();
        }

        string script = @"
function InsertMacro(macro) {
    if((wopener!=null) && (wopener.CMSPlugin.insertHtml))
    {
        wopener.CMSPlugin.insertHtml('{% ' + macro + ' %}');
    }
    return CloseDialog();
}
                
// Resize editor
jQuery.event.add(window, 'load', resizeEditor);
jQuery.event.add(window, 'resize', resizeEditor);

function resizeEditor() {
    $j('.CodeMirror-scroll').height($j(window).height() - 95);
};";
        ScriptHelper.RegisterJQuery(Page);
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "insertItem", script, true);
    }


    /// <summary>
    /// Called when Insert button is clicked - sends selected control info back to the CK editor.
    /// </summary>
    protected void btnInsert_Click(object sender, EventArgs e)
    {
        string macroValue = MacroResolver.RemoveDataMacroBrackets(macroEditor.Text.Trim());
        ScriptHelper.RegisterStartupScript(this, typeof(string), "InsertMacro", String.Format("InsertMacro('{0}');", ScriptHelper.GetString(macroValue, false)), true);
    }
}
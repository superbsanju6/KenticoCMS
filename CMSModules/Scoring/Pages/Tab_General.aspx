<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Tab_General.aspx.cs" Inherits="CMSModules_Scoring_Pages_Tab_General"
    MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Title="Score properties - General" Theme="Default" %>

<%@ Register Src="~/CMSModules/Scoring/Controls/UI/Score/Edit.ascx" TagName="ScoreEdit" TagPrefix="cms" %>
    
<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <cms:ScoreEdit ID="editElem" runat="server" IsLiveSite="false" />
</asp:Content>
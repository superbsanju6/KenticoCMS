<%@ Control Language="C#" AutoEventWireup="true"
    Inherits="CMSWebParts_Forums_Search_ForumSearch" CodeFile="~/CMSWebParts/Forums/Search/ForumSearch.ascx.cs" %>
<asp:Panel ID="pnlForumSearch" runat="server" DefaultButton="btnGo">
    <cms:LocalizedLabel ID="lblSearch" runat="server" AssociatedControlID="txtSearch" EnableViewState="false" />
    <cms:CMSTextBox ID="txtSearch" runat="server" />
    <cms:CMSButton ID="btnGo" runat="server" OnClick="btnGo_Click" CssClass="XShortButton" EnableViewState="false" /><br />
    <asp:HyperLink runat="server" ID="lnkAdvanceSearch" Visible="false" EnableViewState="false" />
</asp:Panel>

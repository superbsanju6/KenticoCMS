<%@ Control Language="C#" AutoEventWireup="true" Inherits="CMSModules_Ecommerce_Controls_UI_ProductDocuments"
    CodeFile="ProductDocuments.ascx.cs" %>
<%@ Register Src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" TagName="UniGrid" TagPrefix="cms" %>
<%@ Register Src="~/CMSFormControls/Filters/DocumentFilter.ascx" TagName="DocumentFilter"
    TagPrefix="cms" %>
<%@ Register Src="~/CMSModules/AdminControls/Controls/Documents/Documents.ascx" TagName="Documents"
    TagPrefix="cms" %>
<cms:LocalizedLabel ID="lblInfo" runat="server" ResourceString="com.product.documentstitle"
    Font-Bold="true" CssClass="InfoLabel"></cms:LocalizedLabel>
<asp:PlaceHolder ID="plcFilter" runat="server">
    <cms:DocumentFilter ID="filterDocuments" runat="server" LoadSites="true" />
    <br />
    <br />
</asp:PlaceHolder>
<cms:CMSUpdatePanel ID="pnlUpdate" runat="server" UpdateMode="Always">
    <ContentTemplate>
        <cms:Documents ID="docElem" runat="server" ListingType="ProductDocuments" IsLiveSite="false" />
    </ContentTemplate>
</cms:CMSUpdatePanel>

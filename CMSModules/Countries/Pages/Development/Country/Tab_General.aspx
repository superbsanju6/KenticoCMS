<%@ Page Language="C#" AutoEventWireup="true" Inherits="CMSModules_Countries_Pages_Development_Country_Tab_General"
    Theme="Default" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" Title="Country Edit - General"
    CodeFile="Tab_General.aspx.cs" %>

<asp:Content ID="cntBody" runat="server" ContentPlaceHolderID="plcContent">
    <cms:UIForm runat="server" ID="formElem" ObjectType="cms.country" RefreshTab="General">
        <LayoutTemplate>
            <cms:FormField runat="server" ID="fDisplayName" Field="CountryDisplayName" FormControl="LocalizableTextBox"
                ResourceString="Country_Edit.CountryDisplayNameLabel" Trim="true" />
            <cms:FormField runat="server" ID="fCountryName" Field="CountryName" FormControl="CodeName"
                ResourceString="Country_Edit.CountryNameLabel" Trim="true" />
            <cms:FormField runat="server" ID="fCountryCode2" Field="CountryTwoLetterCode" FormControl="TextBoxControl"
                ResourceString="Country_Edit.CountryTwoLetterCodeLabel" Trim="true" CheckUnique="true" DisplayColon="true" />
            <cms:FormField runat="server" ID="fCountryCode3" Field="CountryThreeLetterCode" FormControl="TextBoxControl"
                ResourceString="Country_Edit.CountryThreeLetterCodeLabel" Trim="true" CheckUnique="true" DisplayColon="true" />
            <cms:FormSubmit runat="server" ID="fSubmit" />
        </LayoutTemplate>
    </cms:UIForm>
</asp:Content>

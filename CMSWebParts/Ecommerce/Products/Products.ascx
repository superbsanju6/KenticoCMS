<%@ Control Language="C#" AutoEventWireup="true" CodeFile="~/CMSWebParts/Ecommerce/Products/Products.ascx.cs" Inherits="CMSWebParts_Ecommerce_Products_Products" %>
<%@ Register Src="~/CMSAdminControls/UI/UniGrid/UniGrid.ascx" TagName="UniGrid" TagPrefix="cms" %>
<%@ Register Namespace="CMS.UIControls.UniGridConfig" TagPrefix="ug" Assembly="CMS.UIControls" %>
<%@ Register TagPrefix="cms" Assembly="CMS.ExtendedControls" Namespace="CMS.ExtendedControls" %>
<cms:MessagesPlaceHolder ID="plcMessages" runat="server" WrapperControlID="pnlMessages" />
<cms:UniGrid runat="server" ID="gridElem" ObjectType="ecommerce.skulist" RememberDefaultState="true"
    Columns="SKUID, SKUName, SKUOptionCategoryID, SKUNumber, SKUPrice, SKUDepartmentID, SKUManufacturerID, SKUSupplierID, PublicStatusDisplayName,
    InternalStatusDisplayName, SKUReorderAt, SKUAvailableItems, SKUEnabled, SKUSiteID">
    <GridColumns>
        <ug:Column Source="SKUName" Caption="$product_list.productname$" Wrap="false" Localize="true"
            Width="100%" />
        <ug:Column Name="OptionCategory" Source="##ALL##" ExternalSourceName="OptionCategory"
            Caption="$com.productswidget.optioncategory$" Wrap="false" Localize="true" />
        <ug:Column Name="Number" Source="##ALL##" ExternalSourceName="SKUNumber" Sort="SKUNumber"
            Caption="$product_list.productnumber$" Wrap="false" />
        <ug:Column Name="Price" Source="##ALL##" ExternalSourceName="SKUPrice" Sort="SKUPrice"
            Caption="$product_list.productprice$" Wrap="false" CssClass="TextRight" />
        <ug:Column Name="Department" Source="##ALL##" ExternalSourceName="SKUDepartmentID"
            Caption="$com.productswidget.department$" Wrap="false" />
        <ug:Column Name="Manufacturer" Source="##ALL##" ExternalSourceName="SKUManufacturerID"
            Caption="$com.productswidget.manufacturer$" Wrap="false" Localize="true" />
        <ug:Column Name="Supplier" Source="##ALL##" ExternalSourceName="SKUSupplierID"
            Caption="$com.productswidget.supplier$" Wrap="false" Localize="true" />
        <ug:Column Name="PublicStatus" Source="PublicStatusDisplayName" Caption="$product_list.grid.storestatus$"
            Wrap="false" Localize="true" />
        <ug:Column Name="InternalStatus" Source="InternalStatusDisplayName" Caption="$product_list.grid.internalstatus$"
            Wrap="false" Localize="true" />
        <ug:Column Name="ReorderAt" Source="SKUReorderAt" Caption="$com.sku.reorderat$" Wrap="false" CssClass="TextRight" />
        <ug:Column Name="AvailableItems" Source="##ALL##" ExternalSourceName="SKUAvailableItems"
            Sort="SKUAvailableItems" Caption="$product_list.productavailableitems$" Wrap="false" CssClass="TextRight" />
        <ug:Column Name="ItemsToBeReordered" Source="##ALL##" ExternalSourceName="ItemsToBeReordered"
            Caption="$com.productswidget.itemstobereordered$" Wrap="false" CssClass="TextRight" />
        <ug:Column Name="AllowForSale" Source="SKUEnabled" ExternalSourceName="#yesno" Caption="$com.productlist.allowforsale$"
            Wrap="false" />
        <ug:Column Name="Global" Source="##ALL##" ExternalSourceName="SKUSiteID" Sort="SKUSiteID"
            Caption="$com.productlist.global$" Wrap="false" />
    </GridColumns>
</cms:UniGrid>

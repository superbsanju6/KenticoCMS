<%@ Page Language="C#" AutoEventWireup="true" Inherits="CMSModules_Ecommerce_Pages_Tools_Reports_Default"
    CodeFile="Default.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Frameset//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server" enableviewstate="false">
    <title>E-commerce - Reports</title>
</head>
<frameset border="0" cols="280,*" id="colsFrameset" runat="server">
    <frame name="ecommreportstree" 
        scrolling="no" frameborder="0" runat="server" ID="frmMenu"/>
    <frame name="ecommreports" scrolling="auto"
        frameborder="0" runat="server" />
    <cms:NoFramesLiteral ID="ltlNoFrames" runat="server" />
</frameset>
</html>

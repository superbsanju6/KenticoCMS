﻿var IsCMSDesk = true; 

function RefreshTree(expandNodeId, selectNodeId) {
    parent.RefreshTree(expandNodeId, selectNodeId);
}

function SelectNode(nodeId) {
    parent.SelectNode(nodeId);
}

function NewDocument(parentNodeId, className) {
    parent.NewDocument(parentNodeId, className);
}

function DeleteDocument(nodeId) {
    parent.DeleteDocument(nodeId);
}

function EditDocument(nodeId) {
    parent.EditDocument(nodeId);
}

function SetTabsContext(mode) {
    parent.SetTabsContext(mode);
}

function PerformSplitViewRedirect(originalUrl, newCulture, successCallback, errorCallback) {
    parent.PerformSplitViewRedirect(originalUrl, newCulture, successCallback, errorCallback);
}
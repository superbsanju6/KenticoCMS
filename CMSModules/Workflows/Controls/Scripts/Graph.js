﻿/*
*   Helper function to define classes.
*/
var $class = function (def) {
    //handles situations, where no constructor id in class
    var constructor = function () { };
    if (def.hasOwnProperty('constructor')) {
        constructor = def.constructor;
    }

    //splits class initialization to parts
    for (var name in $class.Initializers) {
        $class.Initializers[name].call(constructor, def[name], def);
    }

    return constructor;
};


/*
*   Helper class initializing classes.
*/
$class.Initializers = {
    //handles inheritances
    Extends: function (parent) {
        if (parent) {
            var F = function () { };
            this._superClass = F.prototype = parent.prototype;
            this.prototype = new F;
        }
    },
    //handles mix ins
    Mixins: function (mixins, def) {
        this.mixin = function (mixin) {
            for (var key in mixin) {
                if (key in $class.Initializers) continue;
                this.prototype[key] = mixin[key];
            }
            this.prototype.constructor = this;
        };
        var objects = [def].concat(mixins || []);
        for (var i = 0, l = objects.length; i < l; i++) {
            this.mixin(objects[i]);
        }
    }
};

/*
*   Class handling whole graph.
*/
var JsPlumbGraph = $class({

    constructor: function (readOnly, service, parentId, jsObjectName) {
        this.jsObjectName = jsObjectName;
        var parent = $j("#" + parentId)[0];

        //readOnly = true;
        this.readOnly = readOnly;

        this.myJsPlumb = jsPlumb.getInstance({ ParentElement: parent });
        this.myJsPlumb.setRenderMode(jsPlumb.CANVAS);

        if (service) {
            this.serviceHandler = new GraphSavingServiceRequest(service, this);
        }

        // Default settings
        this.myJsPlumb.Defaults.Connector = ["Flowchart", { stub: 15}];
        this.myJsPlumb.Defaults.PaintStyle = { lineWidth: 2, strokeStyle: 'rgba(255,255,255,1)' };
        this.myJsPlumb.Defaults.HoverPaintStyle = { lineWidth: 4, strokeStyle: 'rgba(255,255,255,1)' };
        this.myJsPlumb.Defaults.EndpointStyle = { fillStyle: "rgba(229,219,61,1)" };
        this.myJsPlumb.Defaults.Anchor = "Center";
        this.myJsPlumb.Defaults.Overlays = [["PlainArrow", { location: 0.7, width: 14, length: 8}]];
        this.myJsPlumb.Defaults.AppendElementsToBody = false;
        this.myJsPlumb.Defaults.DropOptions = { tolerance: 'touch' };
        this.myJsPlumb.Defaults.SelectedConnectionPaintStyle = { lineWidth: 4, strokeStyle: 'rgba(229,160,61,1)' };
        this.myJsPlumb.Defaults.LogEnabled = true; //TODO JiriS: for testing reasons

        this.nodes = new Object();

        this.sourcePointTranslation = new Array();
        this.sourcePointTranslation[0] = "standard";
        this.sourcePointTranslation[1] = "switchCase";
        this.sourcePointTranslation[2] = "switchDefault";
        this.sourcePointTranslation[3] = "timeout";

        this.setHtmlElement(parent);

        this.snapToGrid = $j.cookie('CMSUniGraph') != "false";
        this.gridWidth = 10;
        this.gridHeight = 10;
        this.gridSize = [this.gridWidth, this.gridHeight];


        this.nodePadding = 50;

        $j(window).unload(function (graph) {
            return function () {
                graph.myJsPlumb.unload();
                graph.myJsPlumb.unload();
                graph.myJsPlumb.unload();
                graph.myJsPlumb.unload();
                graph.myJsPlumb.unload();
                jsPlumb.unload();
                jsPlumb.unload();
                jsPlumb.unload();
                jsPlumb.unload();
                jsPlumb.unload();
            }
        } (this));

        $j.tools.tooltip.addEffect(
            "graphDelay",
            function (done, e) {
                this.getTip().delay(200).fadeIn(200);
                done();
            },
            function (done, e) {
                this.getTip().clearQueue();
                this.getTip().hide();
            }
        );

        $j.tools.tooltip.addEffect(
            "condition",
            function (done, e) {
                if (isRTL) {
                    this.getConf().offset[1] = -this.getTip().width();
                } else {
                    this.getConf().offset[1] = this.getTip().width();
                }
                this.getTip().delay(200).fadeIn(200);
                done();
            },
            function (done, e) {
                this.getTip().clearQueue();
                this.getTip().hide();
            }
        );

        this.tooltipConfiguration = {
            onBeforeShow: graphControlHandler.tooltipStopEventIfDragging,
            effect: "graphDelay",
            position: "bottom right"
        };
    },


    /*
    *   Toggles snapping to grid.
    */
    toggleSnapToGrid: function () {
        this.snapToGrid = !this.snapToGrid;
        for (var key in this.nodes) {
            this.nodes[key].setSnapToGridByGraph();
        }
        $j.cookie('CMSUniGraph', this.snapToGrid, { expires: 365, path: '/' })
    },


    roundWidthToGrid: function (x) {
        x = this.snapToGrid ? parseInt(x / this.gridWidth) * this.gridWidth : parseInt(x);
        return x <= 0 ? 1 : x;
    },


    roundHeightToGrid: function (y) {
        y = this.snapToGrid ? parseInt(y / this.gridHeight) * this.gridHeight : parseInt(y);
        return y <= 0 ? 1 : y;
    },


    /*
    *   Removes selected item.
    */
    removeSelectedItem: function () {
        var deselectItem = graphSelectHandler.getDeselectItemHandler(this);

        if (this.selectedItem == null) {
            alert(this.serviceHandler.resourceStrings["NoItemSelected"]);
            return;
        }

        if (this.selectedItem.removeReattachHelper) {
            this.removeConnectionFromDatabase(this.selectedItem);
        } else if (this.selectedItem.hasClass('Node')) {
            this.removeNodeFromDatabase(this.selectedItem.data("nodeObject"));
        }
        deselectItem();
    },


    /*
    *   Removes connection from graph and database.
    */
    removeConnectionFromDatabase: function (connection) {
        var cont = confirm(this.serviceHandler.resourceStrings["ConnectionDeleteConfirmation"]);
        if (cont) {
            this.serviceHandler.removeConnection(connection);
        }
    },


    /*
    *   Remove nodes from graph and database.
    */
    removeNodeFromDatabase: function (node) {
        if (!node.definition.IsDeletable) {
            alert(this.serviceHandler.resourceStrings["NondeletableNode"]);
            return;
        }
        var cont = confirm(this.serviceHandler.resourceStrings["NodeDeleteConfirmation"]);
        if (cont) {
            this.serviceHandler.removeNode(this.selectedItem.data('nodeObject').id);
        }
    },


    /*
    *   Ensures correct resource strings in read only mode.
    */
    getReadOnlyResourceString: function (resource, defaultResource) {
        if (this.readOnly) {
            return '';
        }
        var str = this.getResourceString(resource);
        if (str) {
            return str;
        }
        return this.getResourceString(defaultResource);
    },


    /*
    *   Returns default resource string.
    */
    getResourceString: function (resource) {
        return this.resourceStrings[resource];
    },


    /*
    *   Refreshes node.
    */
    refreshNode: function (id) {
        this.serviceHandler.refreshNode(id);
        this.graphJQ.parents(".GraphContainer").focus();
    },


    /*
    *   Method for setting limits on source points counts.
    */
    setSourcePointsLimits: function (limits) {
        this.sourcePointsLimits = limits;
    },


    /*
    *   Sets ID of HTML parental element.
    */
    setHtmlElement: function (parent) {
        this.graphJQ = $j(parent);
        this.graphJQ.data('graphObject', this);
        this.containerJQ = this.graphJQ.parents(".GraphContainer");

        this.myJsPlumb.Defaults.ParentElement = parent;
        this.myJsPlumb.Defaults.DragOptions = { containment: this.graphJQ.selector };
        this.graphJQ.parents("body").on("mouseover", ".tooltip", graphControlHandler.tooltipStopEventIfDragging);

        if (!this.readOnly) {
            this.setSelectHandlers();
            this.setControlHandlers();
        }
    },


    /*
    *   Sets handlers for selecting items.
    */
    setSelectHandlers: function () {
        this.graphJQ.on("click", "div.Node", graphSelectHandler.getNodeSelectHandler(this));
        this.graphJQ.on("dragstart", "div.Node", graphSelectHandler.getNodeSelectHandler(this));
        this.graphJQ.on("click", "._jsPlumb_endpoint", graphSelectHandler.getEndpointSelectHandler(this));
        this.graphJQ.on("dragstart", "._jsPlumb_endpoint", graphSelectHandler.getEndpointSelectHandler(this));
    },


    /*
    *   Sets handlers for editing graph.
    */
    setControlHandlers: function () {
        this.graphJQ.on("click", "div.plus", graphControlHandler.getAddSwitchCaseHandler(this));
        this.graphJQ.on("dblclick", "div.plus, div.Node, div.Case", function (e) { e.stopPropagation(); });
        this.graphJQ.on("click", ".Name .pen", graphControlHandler.editNodeHandler);
        this.graphJQ.on("click", ".Name .del", graphControlHandler.getRemoveNodeHandler(this));
        this.graphJQ.on("click", ".Cases .pen", graphControlHandler.editSwitchCaseHandler);
        this.graphJQ.on("click", ".Cases .del", graphControlHandler.getRemoveSwitchCaseHandler(this));
        this.graphJQ.on("dragstart", "div.Node", graphControlHandler.getStartDraggingHandler(this));
        this.graphJQ.on("dragstop", "div.Node", graphControlHandler.getSetNodePositionHandler(this));
        this.graphJQ.on("dragstop", "div.Node", graphControlHandler.getStopDraggingHandler(this.myJsPlumb));
        this.graphJQ.on("dblclick", ".Case", graphControlHandler.editSwitchCaseHandler);
        this.graphJQ.on("dblclick", "div.Node", graphControlHandler.editNodeHandler);
        this.graphJQ.on("dblclick", ".Editable", graphControlHandler.getShowTextboxInputHandler(this));
        this.graphJQ.on("dropover", ".Node", graphSelectHandler.highlightNode);
        this.graphJQ.on("dropout", ".Node", graphSelectHandler.removeHighlight);
        this.graphJQ.on("drop", ".Node", graphSelectHandler.removeHighlight);
        this.graphJQ.on("dropover", "._jsPlumb_endpoint", graphSelectHandler.highlightEndpoint);
        this.graphJQ.on("dropout", "._jsPlumb_endpoint", graphSelectHandler.removeHighlight);
        this.graphJQ.on("drop", "._jsPlumb_endpoint", graphSelectHandler.removeHighlight);
        this.myJsPlumb.bind("jsPlumbConnection", graphControlHandler.getAttachConnectionHandler(this));
        this.myJsPlumb.bind("click", graphSelectHandler.getConnectionSelectHandler(this));
    },


    /*
    *   Repaints node by given definition.
    */
    repaintNode: function (definition) {
        detinition = this.definitionTranslation(definition);
        this.nodes[definition.ID].repaint(definition);
    },


    /*
    *   Prints whole graph.
    */
    printGraph: function (definitions) {
        this.myJsPlumb.jsPlumbFillCanvases = false;
        if (definitions.ID != undefined) {
            this.ID = definitions.ID
        }

        if (!this.readOnly) {
            this.serviceHandler.resourceStrings = definitions.ServiceResourceStrings;
        }

        this.resourceStrings = definitions.GraphResourceStrings;
        this.addresses = definitions.Addresses;
        this.createNodes(definitions.Nodes);
        this.myJsPlumb.jsPlumbFillCanvases = true;
        for (key in this.nodes) {
            this.nodes[key].repaintAllEndpoints();
        }

        //endpoints needs to be painted before creating connections
        this.createConnections(definitions.Connections);
    },


    /*
    *   Returns node by given id.
    */
    getNode: function (id) {
        return this.nodes[id];
    },


    /*
    *   Adds nodes.
    */
    createNodes: function (definitions) {
        for (var i = 0; i < definitions.length; i++) {
            this.createNode(definitions[i]);
        }
    },


    /*
    *   Adds single node.
    */
    createNode: function (definition) {
        detinition = this.definitionTranslation(definition);

        var newNode = this.createNodeObject(definition);
        if (this.sourcePointsLimits) {
            newNode.setSourcePointsLimits(this.sourcePointsLimits.Min[definition.Type], this.sourcePointsLimits.Max[definition.Type]);
        }
        this.initNode(newNode);
        newNode.printNode();
        this.nodes[definition.ID] = newNode;
        return newNode;
    },


    /*
    *   Method rewriting int representation of nodes to more user friendly string.
    */
    definitionTranslation: function (definition) {
        if (definition.SourcePoints) {
            for (var i = 0; i < definition.SourcePoints.length; i++) {
                definition.SourcePoints[i].Type = this.sourcePointTranslation[definition.SourcePoints[i].Type];
            }
        }
        if (definition.Position.X < 0 && definition.Position.Y < 0) {
            var top = this.graphJQ.height() / 2;
            definition.Position.X = (this.getNodesCount() + 1) * 201 - this.graphJQ.offset().left;
            definition.Position.Y = -this.graphJQ.offset().left + 210 + top;

            if (definition.Position.X > this.graphJQ.width() - 200) {
                definition.Position.X = this.graphJQ.width() - 200;
            }

            definition.Position.X = this.roundWidthToGrid(definition.Position.X);
            definition.Position.Y = this.roundHeightToGrid(definition.Position.Y);
            if (this.serviceHandler) {
                this.serviceHandler.setNodePosition(definition.ID, definition.Position.X, definition.Position.Y);
            }
        }
        return definition;
    },


    /*
    *   Returns number of nodes.
    */
    getNodesCount: function () {
        var size = 0, key;
        for (key in this.nodes) {
            if (this.nodes[key]) size++;
        }
        return size;
    },


    /*
    *   Create new instance of node. 
    */
    createNodeObject: function (definition) {
        switch (definition.Type) {
            case 0: return new JsPlumbStandardNode(this.readOnly, this, definition);
            case 1: return new JsPlumbActionNode(this.readOnly, this, definition);
            case 2: return new JsPlumbConditionNode(this.readOnly, this, definition);
            case 3: return new JsPlumbMultichoiceNode(this.readOnly, this, definition);
            case 4: return new JsPlumbUserChoiceNode(this.readOnly, this, definition);
        }
    },


    /*
    *   Initializes mandatory parameters of node.
    */
    initNode: function (node) {
        node.setJsPlumbInstance(this.myJsPlumb);
    },


    /*
    *   Creates multiple connections.
    */
    createConnections: function (definitions) {
        if (definitions) {
            for (var i = 0; i < definitions.length; i++) {
                this.createConnection(definitions[i]);
            }
        }
    },


    /*
    *   Creates single connection.
    *   For purposes of creating new node, repacking of the definitions is needed.
    */
    createConnection: function (definition) {
        var targetNode = this.nodes[definition.TargetNodeID];
        var sourceNode = this.nodes[definition.SourceNodeID];
        if (sourceNode && targetNode) {
            sourceNode.createConnection({ sourcePointId: definition.SourcePointID, targetNode: targetNode, ID: definition.ID });
        }
    },


    /*
    *   Removes given node.
    */
    removeNode: function (id) {
        var node = this.nodes[id]
        this.myJsPlumb.deleteEndpointsOnElement(node.htmlId);
        node.nodeJQ.remove();
        delete node.id;
        $j(".tooltip:visible").hide();
    },


    /*
    *   Calculates ideal initial view point of graph
    */
    calculateInitialView: function () {
        var position = this.getNearestNodePosition();
        var maxW = position.X + this.containerJQ.width() - this.nodePadding * 2;
        var minH = position.Y - this.containerJQ.height() + this.nodePadding * 2;
        for (var i in this.nodes) {
            if (position.Y > this.nodes[i].position.Y && this.nodes[i].position.X < maxW && this.nodes[i].position.Y > minH) {
                position.Y = this.nodes[i].position.Y;
            }
        }
        return position;
    },


    /*
    *   Returns coordinates of nearest node.
    */
    getNearestNodePosition: function () {
        var position = { X: this.graphJQ.width(), Y: this.graphJQ.height() };
        for (var i in this.nodes) {
            if (position.X > this.nodes[i].position.X) {
                position = this.nodes[i].position;
            }
        }
        return position;
    },


    /*
    *   Returns point on center of ideal initial view.
    */
    getCenterOfInitialView: function () {
        var position = this.calculateInitialView();
        if (position) {
            return {
                left: position.X - this.nodePadding + this.containerJQ.width() / 2,
                top: position.Y - this.nodePadding + this.containerJQ.height() / 2
            };
        }

        return { top: this.graphJQ.height() / 2, left: this.graphJQ.width() / 2 };
    },


    /*
    *   Shows element for service work progress.
    */
    showProgress: function () {
        if (this.progressJQ) {
            this.progressJQ.show();
        }
    },


    /*
    *   Hides element for service work progress.
    */
    hideProgress: function () {
        if (this.progressJQ) {
            this.progressJQ.hide();
        }
    }
});

/*
*   Class handling the graph wrapper.
*/
var JsPlumbGraphWrapper = $class({

    constructor: function (parentId, droppableScope, preselectedNode, graph, isResizable) {
        this.id = parentId;
        this.droppableScope = droppableScope;
        this.containerJQ = $j("#" + parentId);
        this.preselectedNode = preselectedNode;
        this.wrapperPadding = 70;
        this.graph = graph;
        this.isResizable = isResizable;

        this.wrapContainer();
        this.wrapGraph();
        this.createLegend();
        this.setHandlers();
    },


    /*
    *   Creates element for service work progress.
    */
    setUpdateProgress: function (progressStr) {
        var progressJQ = $j("<div class='GraphProgress' id='" + this.id + "Progress'>" + progressStr + "</div>");
        this.containerJQ.append(progressJQ);
        this.graph.progressJQ = $j("#" + this.id + "Progress");
        this.graph.progressJQ.on("selectstart", "*", function () { return false; });
        this.graph.hideProgress();
    },


    /*
    *   Creates legend of graph.
    */
    createLegend: function () {
        var legendJQ = $j("<div id='" + this.id + "Legend' class='GraphLegend'>");
        this.containerJQ.append(legendJQ);
        legendJQ = $j("#" + this.id + "Legend");
        legendJQ.append("<div><div class='ConnectorExample Manual'></div><span>" + this.getManualConnectionTypeName() + "</span></div>");
        legendJQ.append("<div><div class='ConnectorExample Automatic'></div><span>" + this.getAutomaticConnectionTypeName() + "</span></div>");
        legendJQ.on("selectstart", "*", function () { return false; });
    },


    /*
    *   Returns resource string of name of manual connection type.
    */
    getManualConnectionTypeName: function () {
        return this.graph.getResourceString("ManualConnectionType");
    },


    /*
    *   Returns resource string of name automatic connection type.
    */
    getAutomaticConnectionTypeName: function () {
        return this.graph.getResourceString("AutomaticConnectionType");
    },


    /*
    *   Wraps graph view in another div, so UniGraph doesn't break surrounding elements.
    */
    wrapContainer: function () {
        var wrapperJQ = $j("<div id='" + this.id + "UniGraphControl' class='UniGraph'>");
        this.containerJQ.wrap(wrapperJQ);
        this.containerJQ = $j("#" + this.containerJQ.attr("id"));

        wrapperJQ = $j("#" + wrapperJQ.attr("id"));

        if (!this.isResizable.height) {
            wrapperJQ.css("height", this.containerJQ.height());
        }

        if (!this.isResizable.width) {
            wrapperJQ.css("width", this.containerJQ.width());
        }
    },


    /*
    *   Sets handlers for wrapper movement.
    */
    setHandlers: function () {
        this.setNonEditingHandlers();
        if (!this.graph.readOnly) {
            this.setEditingHandlers();
        }
    },


    /*
    *   Sets handlers used for non-editing actions.
    */
    setNonEditingHandlers: function () {
        this.graph.graphJQ.draggable({ containment: 'parent', distance: 10 });
        this.graph.graphJQ.bind("mousewheel", graphControlHandler.getWrapperMouseWheelHandler(this));
        this.graph.graphJQ.bind("dragstart", graphControlHandler.getStartDraggingHandler(this.graph));
        this.graph.graphJQ.bind("dragstop", graphControlHandler.getStopDraggingHandler(this.graph.myJsPlumb));

        $j(window).resize(graphControlHandler.getResizeWrapperHandler(this));
        $j(document).ready(graphControlHandler.getResizeWrapperHandler(this));
    },


    /*
    *   Sets handlers thrown from wrapper needed for editing graph.
    */
    setEditingHandlers: function () {
        this.containerJQ.droppable({ scope: this.droppableScope, drop: graphControlHandler.getCreateNodeHandler(this.graph), tolerance: "pointer" });
        this.containerJQ.mousedown(function () { if (typeof (this.focus) == 'function') this.focus(); });
        this.containerJQ.click(graphSelectHandler.getDeselectItemHandler(this.graph));
        this.containerJQ.keydown(graphControlHandler.getKeyDownHandler(this.graph));
        //this.containerJQ.on("drag", "*", graphControlHandler.getMovePaneWhileDraggingHandler(this)); //TODO JiriS: odkomentovat a dodelat funkcionalitu pro posouvani grafu tazenim nodu
    },


    /*
    *   Method used for wrapping graph in elements for limiting view.
    */
    wrapGraph: function () {
        if (!this.wrapperJQ) {
            this.createWrapper();
        }
        this.setWrapperDimensions();
        this.setDefaultView();
    },


    /*
    *   Creates wrapper around graph.
    */
    createWrapper: function () {
        var wrapperJQ = $j("<div id='" + this.id + "wrapper' class='GraphWrapper'>");
        this.graph.graphJQ.wrap(wrapperJQ);
        this.graph.graphJQ = $j("#" + this.graph.graphJQ.attr("id"));
        this.wrapperJQ = $j("#" + wrapperJQ.attr('id'));
    },


    /*
    *   Resizes graph wrapper to maximum available size. 
    */
    setWrapperDimensions: function () {
        var graphWidth = this.graph.graphJQ.outerWidth();
        var graphHeight = this.graph.graphJQ.outerHeight();
        var containerWidth = this.containerJQ.width();
        var containerHeight = this.containerJQ.height();

        var containerWidth = (graphWidth * 2) - containerWidth;
        var containerHeight = (graphHeight * 2) - containerHeight;

        this.wrapperJQ.css("position", "relative");
        this.wrapperJQ.css("width", containerWidth);
        this.wrapperJQ.css("height", containerHeight);
        this.wrapperJQ.css("left", -containerWidth + graphWidth);
        this.wrapperJQ.css("top", -containerHeight + graphHeight);
    },


    /*
    *   Sets default point of view on graph.
    */
    setDefaultView: function () {
        if (this.graph.nodes[this.preselectedNode]) {
            this.setViewToNode(this.graph.nodes[this.preselectedNode]);
        } else {
            var position = this.graph.getCenterOfInitialView();
            this.setViewCenterTo(position);
        }
    },


    /*
    *   Sets size of container (most upper) element of graph.
    */
    setContainerDimensions: function () {
        //this needs to be here twice - first for setting, second for occasions when the first time changed elements in window.
        this.setContainerDimensionsHelper();
        this.setContainerDimensionsHelper();
    },


    /*
    *   Helper method used to change size of graph view.
    */
    setContainerDimensionsHelper: function () {
        var parentContainerJQ = this.containerJQ.parent();

        if (this.isResizable.height) {
            var origParentHeight = $j(window).height() - parentContainerJQ.offset().top - this.containerJQ.outerHeight() + this.containerJQ.height();
        } else {
            var origParentHeight = parentContainerJQ.height();
        }

        var origParentWidth = parentContainerJQ.width() - this.containerJQ.outerWidth() + this.containerJQ.width();

        if (this.isResizable.height)
            this.containerJQ.css("height", origParentHeight);
        if (this.isResizable.width)
            this.containerJQ.css("width", origParentWidth);

        parentContainerJQ.css("height", origParentHeight);
    },


    /*
    *   Gets nearest top corner of view of graph.
    */
    getViewPosition: function () {
        var graphPosition = this.graph.graphJQ.position();
        return {
            left: graphPosition.left,
            top: graphPosition.top
        };
    },


    /*
    *   Sets center of view to given point.
    */
    setViewCenterTo: function (position) {
        position.left = this.wrapperJQ.width() / 2 - position.left;
        position.top = this.wrapperJQ.height() / 2 - position.top;
        this.setViewTo(position);
    },


    /*
    *   Sets left top corner of view to given point.
    */
    setViewTo: function (position) {
        position = this.ensureViewPosition(position);
        this.graph.graphJQ.css("left", position.left);
        this.graph.graphJQ.css("top", position.top);
    },


    /*
    *   Moves graph by one step.
    */
    moveViewBy: function (deltaX, deltaY) {
        var position = this.getViewPosition();
        deltaY *= 10;
        deltaX *= 10;
        position.top += deltaY;
        position.left += deltaX;
        position = this.ensureViewPosition(position);
        this.setViewTo(position);
    },


    /*
    *   Method correcting boundaries of graph position.
    */
    ensureViewPosition: function (position) {
        var graphJQ = this.graph.graphJQ;

        if (position.top < 0)
            position.top = 0;
        else if (position.top + graphJQ.outerHeight() > this.wrapperJQ.height())
            position.top = this.wrapperJQ.height() - graphJQ.outerHeight();

        if (position.left < 0)
            position.left = 0;
        else if (position.left + graphJQ.outerWidth() > this.wrapperJQ.width())
            position.left = this.wrapperJQ.width() - graphJQ.outerWidth();

        return position;
    },


    /*
    *   Sets center of view to given node.
    */
    setViewToNode: function (node) {
        var nodeJQ = node.nodeJQ;
        var graphJQ = this.graph.graphJQ;

        var position = {};
        position.left = nodeJQ.position().left + nodeJQ.outerWidth(true) / 2;
        position.top = nodeJQ.position().top + nodeJQ.outerHeight(true) / 2;

        this.setViewCenterTo(position);

        if (this.graph.readOnly) {
            nodeJQ.addClass('Selected');
        } else {
            nodeJQ.click();
        }
    }

});
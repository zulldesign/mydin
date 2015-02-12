if (typeof (Bitrix) == "undefined") {
    var Bitrix = new Object();
}

Bitrix.PMNewMsgDialogInit = function() {

    Bitrix.PMNewMsgDialog = function Bitrix$PMNewMsgDialog() {
        this.Bitrix$Dialog();
        this._textContainerElement = null;
        this._closeClick = Bitrix.TypeUtility.createDelegate(this, this._closeDialog);
        this._redirect = Bitrix.TypeUtility.createDelegate(this, this._redirectHandler);
        this._showNextMessage = Bitrix.TypeUtility.createDelegate(this, this._showNextMessageFunc);
        this._showPrevMessage = Bitrix.TypeUtility.createDelegate(this, this._showPrevMessageFunc);
    }
    Bitrix.TypeUtility.copyPrototype(Bitrix.PMNewMsgDialog, Bitrix.Dialog);
    Bitrix.PMNewMsgDialog.prototype.initialize = function(id, name, title, options) {
        this.Bitrix$Dialog.prototype.initialize.call(this, id, name, title, null, Bitrix.Dialog.buttonLayout.none, options);
        var content = new Array();

        var container = document.createElement("DIV");
        container.id = id + "_" + "container";
        container.className = this.getOption("_contentContainerClass");
        content.push(container);
        this._itemContainers = [];

        if (options._messages)
            for (var i = 0; i < options._messages.length; i++) {
            var message = options._messages[i];
            var mainTable = document.createElement("TABLE");

            var mainTr = mainTable.insertRow(-1);
            mainTr.vAlign = "top";
            var avTd = mainTr.insertCell(-1);
            var div = document.createElement("DIV");
            var strId;
            if (options._uniquePrefix) strId = options._uniquePrefix;
            else strId = "";
            mainTable.id = strId + "_itemTable_" + i;

            var avDiv = document.createElement("DIV");
            if (i != 0) mainTable.style.display = "none";

            avDiv.className = this.getOption("_avatarContainerClass");
            if (message.avatarHref != "") {
                var avImg = document.createElement("IMG");
                avImg.src = message.avatarHref;
                avImg.alt = message.avatarAlt;
                avImg.width = message.avatarWidth;
                avImg.height = message.avatarHeight;
                avDiv.appendChild(avImg);
            }
            avTd.appendChild(avDiv);

            var rightTd = mainTr.insertCell(-1);
            var table = document.createElement("TABLE");

            var authorCell = table.insertRow(-1);
            var tdAuthor = authorCell.insertCell(-1);
            tdAuthor.colSpan = "2";
            if (message.isFirst == "True") {
                tdAuthor.innerHTML = "<b>" + options["msgUserWantToStartConv"].replace("{0}", message.userName) + "</b>";
            }
            else {
                tdAuthor.innerHTML = "<b>" + options["msgUserUnswered"].replace("{0}", message.userName) + "</b>";
            }

            var topicCell = table.insertRow(-1);
            var tdTopic = topicCell.insertCell(-1);
            tdTopic.innerHTML = "<b>" + options["msgInTopic"] + "</b>" + ": " + message.topicTitle;

            var tdTopicName = topicCell.insertCell(-1);

            var dateCell = table.insertRow(-1);
            var tdDate = dateCell.insertCell(-1);
            tdDate.innerHTML = "<b>" + options["msgSentDate"] + "</b>" + ": " + message.sentDate; ;

            var tdDateValue = document.createElement("TD");

            var previewCell = table.insertRow(-1);
            var tdPreview = previewCell.insertCell(-1);
            tdPreview.innerHTML = message.previewHtml;
            tdPreview.colSpan = "2";

            div.appendChild(table);
            //div.innerHTML = options._messages[i].topicTitle + " " + options._messages[i].userName + " " + options._messages[i].sentDate + "<br>";
            rightTd.appendChild(div);
            container.appendChild(mainTable);
            this._itemContainers[i] = mainTable;

        }
        this._dispIndex = 0;
        var navDiv = document.createElement("DIV");
        if (options._messages.length > 1) {
            var prevLink = document.createElement("A");
            prevLink.href = "#";
            prevLink.style.display = "none";
            prevLink.onclick = this._showPrevMessage;
            prevLink.innerHTML = options["msgPrevMessage"];
            navDiv.appendChild(prevLink);
            this._prevLink = prevLink;

            var span = document.createElement("SPAN");
            span.id = strId + "_count";
            span.innerHTML = "&nbsp;1";

            navDiv.appendChild(span);
            navDiv.className = "bx-pmessages-new-navigation";


            navDiv.style.cssFloat = navDiv.style.styleFloat = "left";
            navDiv.style.marginTop = "20px";

            var spTitle = document.createElement("SPAN");
            spTitle.innerHTML = "&nbsp;" + options["msgOf"] + "&nbsp;" + options._messages.length + "&nbsp;";

            navDiv.appendChild(spTitle);
            var nextLink = document.createElement("A");
            nextLink.href = "#";
            nextLink.onclick = this._showNextMessage;
            nextLink.innerHTML = options["msgNextMessage"];
            this._nextLink = nextLink;
            navDiv.appendChild(nextLink);
        }

        container.appendChild(navDiv);

        var btnDiv = document.createElement("DIV");
        btnDiv.className = this.getOption("_buttonContainerClass");
        btnDiv.style.marginTop = "10px";

        var btn = document.createElement("INPUT");
        btn.type = "button";
        btn.value = options["msgIgnore"];
        btn.onclick = this._closeClick;
        btn.className = this.getOption("_buttonClass");
        btn.style.marginLeft = "10px";
        btn.style.cssFloat = btn.style.styleFloat = "right";
        btnDiv.appendChild(btn);

        var btnRead = document.createElement("INPUT");
        btnRead.type = "button";
        btnRead.value = options["msgReadMessage"];
        btnRead.className = this.getOption("_buttonClass");
        btnRead.onclick = this._redirect;


        btnRead.style.cssFloat = btnRead.style.styleFloat = "right";
        btnDiv.appendChild(btnRead);


        container.appendChild(btnDiv);
        this._indexSpan = span;

        this.setContent(content);

    }
    Bitrix.PMNewMsgDialog.prototype._getChildElementId = function(parentId, id) { return (parentId ? parentId : this.getId()) + "_" + id; }
    Bitrix.PMNewMsgDialog.prototype._getTextContainerId = function() { return this._getChildElementId(null, "TextContainer"); }
    Bitrix.PMNewMsgDialog.prototype._getAdditionalContainerWrapperClasses = function() { return this.getOption("_containerWrapperClass"); }
    Bitrix.PMNewMsgDialog.prototype.getOption = function(name, defaultValue) {
        if (!Bitrix.TypeUtility.isNotEmptyString(name)) return defaultValue;
        if (this._options && name in this._options) return this._options[name];
        else if (name in Bitrix.PMNewMsgDialog.defaults) return Bitrix.PMNewMsgDialog.defaults[name];
        else if (name in Bitrix.Dialog.defaults) return Bitrix.Dialog.defaults[name];
        else return defaultValue;
    }

    Bitrix.PMNewMsgDialog.prototype._showNextMessageFunc = function() {

        if (this._dispIndex >= this._itemContainers.length - 1) return false;
        this._itemContainers[this._dispIndex].style.display = "none";
        this._itemContainers[this._dispIndex + 1].style.display = "";
        this._dispIndex++;
        if (this._dispIndex == this._itemContainers.length - 1 && this._itemContainers.length > 1) {
            this._nextLink.style.display = "none";

        }
        if (this._prevLink.style.display != "" && this._itemContainers.length > 1)
            this._prevLink.style.display = "";
        this._indexSpan.innerHTML = "&nbsp;" + (this._dispIndex + 1);
        return false;
    }

    Bitrix.PMNewMsgDialog.prototype._showPrevMessageFunc = function() {

        if (this._dispIndex <= 0) return false;
        this._itemContainers[this._dispIndex].style.display = "none";
        this._itemContainers[this._dispIndex - 1].style.display = "";
        this._dispIndex--;
        if (this._dispIndex == 0 && this._itemContainers.length > 1) {
            this._prevLink.style.display = "none";

        }
        if (this._nextLink.style.display != "" && this._itemContainers.length > 1)
            this._nextLink.style.display = "";
        this._indexSpan.innerHTML = "&nbsp;" + (this._dispIndex + 1);
        return false;
    }

    Bitrix.PMNewMsgDialog.prototype._redirectHandler = function() {
        window.location = this._options._messages[this._dispIndex].url;

    }

    Bitrix.PMNewMsgDialog.prototype._handleSetOptions = function() {
        if (this._urlInputElement && this.getOption("_url")) this._urlInputElement.value = this.getOption("_url");
        if (this._textInputElement && this.getOption("_text")) this._textInputElement.value = this.getOption("_text");
    }
    Bitrix.PMNewMsgDialog.prototype._getMessageKeyPrefix = function() { return "PMNewMessagesDialog$msg$"; }
    Bitrix.PMNewMsgDialog.prototype._getMessageContainerName = function() { return "COMMUNICATION_UTILITY_DIALOG_MSG"; }


    Bitrix.PMNewMsgDialog.prototype._handleSelfClose = function(e) {
    }

    Bitrix.PMNewMsgDialog.prototype._closeDialog = function() {
        this.close();
    }

    Bitrix.PMNewMsgDialog.create = function(id, name, title, options) {
        if (this._items && id in this._items) throw "Item '" + id + "' already exists!";
        var self = new Bitrix.PMNewMsgDialog();
        self.initialize(id, name, title, options);
        Bitrix.Dialog._addItem(self);
        return self;
    }

    Bitrix.PMNewMsgDialog.defaults = {
        _containerWrapperClass: "bx-pmessages-new-dialog-container-wrapper",
        _contentContainerClass: "bx-pmessages-new-dialog-content-container",
        _contentContainerTableClass: "bx-link-paste-content-container-table",
        _avatarContainerClass: "bx-newpmessages-avatar-container",
        _buttonContainerClass: "bx-dialog-button-container",
        _buttonClass: "bx-pmessages-new-dialog-button"
    }

}

window.setTimeout(Bitrix.PMNewMsgDialogInit, 0);
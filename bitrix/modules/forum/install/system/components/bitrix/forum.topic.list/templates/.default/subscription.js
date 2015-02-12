if (typeof (Bitrix) == "undefined") {
    var Bitrix = new Object();
}

Bitrix.ForumSubscriptionDialog = function Bitrix$ForumSubscriptionDialog() 
{
    this.Bitrix$Dialog();
    this._closeHandler = Bitrix.TypeUtility.createDelegate(this, this._handleSelfClose);
    this.newTopicsInput = null;
    this.newPostsInput = null;
}

Bitrix.TypeUtility.copyPrototype(Bitrix.ForumSubscriptionDialog, Bitrix.Dialog);

Bitrix.ForumSubscriptionDialog.prototype.construct = function() 
{
    if (this._isConstructed)
        return;

    this.Bitrix$Dialog.prototype.construct.call(this);
    this.addCloseListener(this._closeHandler);
}

Bitrix.ForumSubscriptionDialog.prototype.initialize = function(id, name, title, options) {
    this.Bitrix$Dialog.prototype.initialize.call(this, id, name, title, null, Bitrix.Dialog.buttonLayout.cancelOk, options);

    var content = new Array();

    var container = document.createElement("DIV");
    container.id = id + "_" + "container";
    content.push(container);

    var title = document.createElement("SPAN");
    title.innerHTML = this.getOption("Subscribe.Label.Title");
    container.appendChild(title);

    var br = document.createElement("BR");
    container.appendChild(br);

    this.newTopicsInput = Bitrix.ForumSubscriptionDialog.createRadioElement("bx-forum-subscription", true);
    this.newTopicsInput.value = "NewTopics";
    this.newTopicsInput.id = "bx-forum-subscription-newtopics";

    container.appendChild(this.newTopicsInput);

    var newTopicsLabel = document.createElement("LABEL");
    newTopicsLabel.innerHTML = this.getOption("Subscribe.Label.NewTopics");
    newTopicsLabel.htmlFor = this.newTopicsInput.id;
    container.appendChild(newTopicsLabel);

    br = document.createElement("BR");
    container.appendChild(br);

    this.newPostsInput = Bitrix.ForumSubscriptionDialog.createRadioElement("bx-forum-subscription", false);
    this.newPostsInput.value = "NewPosts";
    this.newPostsInput.id = "bx-forum-subscription-newposts";

    container.appendChild(this.newPostsInput);

    var newPostsLabel = document.createElement("LABEL");
    newPostsLabel.innerHTML = this.getOption("Subscribe.Label.NewPosts");
    newPostsLabel.htmlFor = this.newPostsInput.id;
    container.appendChild(newPostsLabel);

    this.setContent(content);
}



Bitrix.ForumSubscriptionDialog.prototype._handleSelfClose = function(sender, args) 
{
    if (!sender || !args || !("buttonId" in args))
        return;
        
    if (args["buttonId"] == Bitrix.Dialog.button.bOk)
    {
        var redirectUrl = null;
        
        if (this.newTopicsInput != null && this.newTopicsInput.checked)
            redirectUrl = sender.getOption("Subscribe.ActionUrl.NewTopics");
        else if (this.newPostsInput != null && this.newPostsInput.checked)
            redirectUrl = sender.getOption("Subscribe.ActionUrl.NewPosts");

        if (redirectUrl && redirectUrl.length > 0)
            window.location = redirectUrl;
    }
}

Bitrix.ForumSubscriptionDialog.create = function(id, name, title, options) 
{
	if(this._items && id in this._items) 
	    throw "Item '"+ id +"' already exists!";
	    
	var self = new Bitrix.ForumSubscriptionDialog();
	self.initialize(id, name, title, options);
	Bitrix.Dialog._addItem(self);
	return self;
}


Bitrix.ForumSubscriptionDialog.createRadioElement = function(name, checked)
{
    var radioHtml = '<input type="radio" name="' + name + '"';
    if (checked)
        radioHtml += ' checked="checked"';
        
    radioHtml += '/>';

    var radioFragment = document.createElement('div');
    radioFragment.innerHTML = radioHtml;

    return radioFragment.firstChild;
}

<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.posts/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.Blog.Components.BlogPostsTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.Blog.Components" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>

<script runat="server">
	private BXParamsBag<object>[] postData = null;
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
		
		if(!Component.EnableVotingForPost)
			return;
			
        if ((this.postData = Component.ComponentCache.Get<BXParamsBag<object>[]>("PostData")) != null)
        {
            repeater.DataSource = this.postData;
            repeater.DataBind();
        }
    }   
		
    private Dictionary<string, IncludeComponent> votingDic = null;
    private void OnPostDataBound(Object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        BXParamsBag<object> data = (BXParamsBag<object>)e.Item.DataItem;
        IncludeComponent c = (IncludeComponent)e.Item.FindControl("voting");

        if (c.Component == null)
            return;

		string id = data.GetString("Id");
		
		c.Component.ID = Component.ClientID + ClientIDSeparator + "PostVote" + id;
        c.Component.Parameters["RolesAuthorizedToVote"] = Component.Parameters.Get("RolesAuthorizedToVote", string.Empty);
        c.Component.Parameters["BoundEntityTypeId"] = "BlogPost";
        c.Component.Parameters["BoundEntityId"] = id;
        c.Component.Parameters["CustomPropertyEntityId"] = BXBlogPost.GetCustomFieldsKey();
        c.Component.Parameters["BannedUsers"] = data.GetString("AuthorId");

        if (votingDic == null)
            votingDic = new Dictionary<string, IncludeComponent>();
        votingDic.Add(id, c);
    }

    private IncludeComponent GetVotingComponent(string key)
    {
        IncludeComponent r;
        return votingDic != null && votingDic.TryGetValue(key, out r) ? r : null;
    } 	
</script>

<% if(Component.EnableVotingForPost && this.postData != null) {%>
<% repeater.Visible = false; %>
<asp:Repeater runat="server" ID="repeater" OnItemDataBound="OnPostDataBound">
    <ItemTemplate>
       <bx:IncludeComponent 
            id="voting" 
            runat="server" 
            componentname="bitrix:rating.vote" 
            Template=".default" />			           
    </ItemTemplate>
</asp:Repeater>

<% foreach(BXParamsBag<object> data in this.postData) {%>
	<%string id = data.GetString("Id"); %>
	<%IncludeComponent voting = GetVotingComponent(id); %>
	<%if (voting == null) continue; %>
	<%string wrapperClientId = Component.ClientID + ClientIDSeparator + "PostVotingWrapper" + id;%>
	<div id="<%= wrapperClientId %>" style="display:none;">
		<% voting.RenderControl(CurrentWriter); %>
	</div>
	<script type="text/javascript">
		(function() {
			var c = document.getElementById("<%= Component.ClientID + ClientIDSeparator + "PostVoting" + id %>");
			if(!c) return;
			var w = document.getElementById("<%= wrapperClientId %>"); 
			c.appendChild(w);
			w.style.display = "";
		})();
	</script>
<%} %>		

<%} %>
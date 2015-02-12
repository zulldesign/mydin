<%@ Reference Control="~/bitrix/components/bitrix/system.login/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Main.Components.SystemLoginTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Security" %>
<%@ Import Namespace="Bitrix.Blog" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Configuration" %>


<script runat="server">

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		ErrorMessage.ValidationGroup = ClientID;
		LoginRequired.ValidationGroup = ClientID;
		PasswordRequired.ValidationGroup = ClientID;
		LoginButton.ValidationGroup = ClientID;

		replace["UserId"] = BXIdentity.Current.Id;

		if (BXIdentity.Current.Id > 0)
		{
			ErrorMessage.Enabled = LoginRequired.Enabled = PasswordRequired.Enabled = LoginButton.Enabled = false;
			
			BXBlogCollection blogs = BXBlog.GetList(
				new BXFilter(
					new BXFilterItem(BXBlog.Fields.Active, BXSqlFilterOperators.Equal, true),
					new BXFilterItem(BXBlog.Fields.Owner.Id, BXSqlFilterOperators.Equal, BXIdentity.Current.Id),
					new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.Site.ID, BXSqlFilterOperators.Equal, BXSite.Current.Id)
				),
				null
			);

			if (blogs.Count > 0)
			{
				blog = blogs[0];
				replace["BlogSlug"] = blog.Slug;
			}

			PrivateMessages.Component.Parameters["MessagesReadUrl"] = UserMailUrl;
			PrivateMessages.Component.Parameters["MessageReadUrlTemplate"] = UserMailReadUrl;
		}
	}
	
	protected void LoginButton_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			var parameters = new BXParamsBag<object>();
			parameters.Add("Login", LoginField.Text);
			parameters.Add("Password", PasswordField.Text);
			parameters.Add("Remember", CheckBoxRemember.Checked);
			var errors = new List<string>();
			if (!Component.ProcessCommand("login", parameters, errors))
			{
				foreach (var error in errors)
					ErrorMessage.AddErrorMessage(error);
				container.Style["display"] = "none";
				tryContainer.Style["display"] = "block";
			}
		}
	}

	BXParamsBag<object> replace = new BXParamsBag<object>();
	BXBlog blog = null;
	
	protected string UserProfileUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserProfileUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string BlogUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "BlogUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string UserMailUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserMailUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}	

	protected string NewBlogUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "NewBlogUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string NewPostUrl
	{
		get
		{
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "NewPostUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	protected string UserMailReadUrl
	{
		get
		{
			// Hack to retain the escaped ##
			return Bitrix.Services.Text.BXStringUtility.ReplaceIgnoreCase(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "UserMailReadUrlTemplate", "", BXSite.Current.Id), "#UserId#", BXIdentity.Current.Id.ToString());
		}
	}
	
	

	protected string LogoutUrl
	{
		get
		{
			UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
			NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
			query.Remove("ReturnUrl");
			uri.Query = query.ToString();
			replace["ReturnUrl"] = uri.Uri.PathAndQuery;
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "LogoutUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}

	string LoginUrl
	{
		get
		{
			UriBuilder uri = new UriBuilder(Bitrix.Services.BXSefUrlManager.CurrentUrl);
			NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
			query.Remove("ReturnUrl");
			uri.Query = query.ToString();
			replace["ReturnUrl"] = uri.Uri.PathAndQuery;
			return Component.ResolveTemplateUrl(BXOptionManager.GetOptionString("Bitrix.CommunitySite", "LoginUrlTemplate", "", BXSite.Current.Id), replace);
		}
	}
	
</script>
	
<% if (BXIdentity.Current.IsAuthenticated) {%>

	<div id="user-menu">
	
		<div id="user-name"><%= string.Format(GetMessageRaw("Welcome"), UserProfileUrl, HttpUtility.HtmlEncode(BXIdentity.Current.User.GetDisplayName())) %></div>
	
		<ul class="mdash-list">
			<li><a href="<%= UserProfileUrl %>" title="<%= GetMessageRaw("ProfileUrl") %>"><%= GetMessageRaw("ProfileUrl") %></a></li>
			
			<% if (BXPrincipal.Current.IsCanOperate(BXBlog.Operations.Blogging)) {%>
				<% if (blog != null) { %>
					<li><a href="<%= BlogUrl %>" title="<%= GetMessageRaw("BlogUrlTooltip") %>"><%= GetMessageRaw("BlogUrl") %></a></li>
					<li><a href="<%= NewPostUrl %>" title="<%= GetMessageRaw("WriteUrlTooltip") %>"><%= GetMessageRaw("WriteUrl") %></a></li>
				<% } else { %>
					<li><a href="<%= NewBlogUrl %>" title="<%= GetMessageRaw("CreateUrlTooltip") %>"><%= GetMessageRaw("CreateUrl") %></a></li>
				<% } 
			} %>
			
			<li><a href="<%= UserMailUrl %>" title="<%= GetMessageRaw("MessagesUrlTooltip") %>"><%= GetMessageRaw("MessagesUrl") %></a><bx:IncludeComponent id="PrivateMessages" runat="server" MessagesReadUrl="" MessageReadUrlTemplate="" componentname="bitrix:pmessages.alert" template="new-messages" /></li>
		</ul>
		
		<a id="logout" href="<%= LogoutUrl %>" title="<%= GetMessageRaw("LogoutUrl") %>"><%= GetMessageRaw("LogoutUrl") %></a>
	</div>

<% } else { %>

	<div runat="server" style="display:none;" id ="tryContainer">
	<bx:BXValidationSummary runat="server" ID="ErrorMessage" CssClass="errortext" ForeColor="" />
		<a href="#" style="color:#FFFFFF" onclick="return <%=ClientID %>switch('<%= tryContainer.ClientID %>','<%=container.ClientID %>')"><%=GetMessage("TryAgain") %></a>
	</div>
	<div id="container" runat="server">
		<table id="auth-form" cellspacing="0">
		<tr>
			<td colspan="2" align="right"><a href="<%= Component.RegisterPath %>"><%= GetMessageRaw("Register") %></a><% if (Component.UseOpenIdAuth)  { %>&nbsp;&nbsp;&nbsp;<a href="<%= LoginUrl %>">OpenID</a><%} %>&nbsp;&nbsp;&nbsp;<a href="<%= Component.PasswordRecoveryPath %>" title="<%= GetMessageRaw("ForgotPassword") %>">?</a></td>
		</tr>
		<tr>
			<td class="field-name"><label for="login-textbox"><asp:RequiredFieldValidator ID="LoginRequired" runat="server" ControlToValidate="LoginField" CssClass="starrequired" Display="Static" ErrorMessage="<%$ LocRaw:Message.LoginRequired %>" >*</asp:RequiredFieldValidator><%= GetMessageRaw("Login") %>:</label></td>
			<td><asp:TextBox ID="LoginField" runat="server" CssClass="textbox" /></td>
		</tr>
		<tr>
			<td class="field-name"><label for="password-textbox"><asp:RequiredFieldValidator ID="PasswordRequired" CssClass="starrequired" runat="server" ControlToValidate="PasswordField" Display="Static" ErrorMessage="<%$ LocRaw:Message.PasswordRequired %>" >*</asp:RequiredFieldValidator><%= GetMessageRaw("Password") %>:</label></td>
			<td>
			<asp:TextBox ID="PasswordField" runat="server" TextMode="Password" CssClass="textbox" />
			</td>
		</tr>
		<tr>
			<td>&nbsp;</td>
			<td><input ID="CheckBoxRemember" type="checkbox" runat="server" class="checkbox" /><label for="<%=CheckBoxRemember.ClientID %>" class="remember"><%= GetMessageRaw("RememberMe") %></label></td>
		</tr>								
		<tr>
			<td>&nbsp;</td>
			<td><asp:Button ID="LoginButton" runat="server" Text="<%$ LocRaw:ButtonText.Login %>" OnClick="LoginButton_Click" CssClass="input-submit" /></td>
		</tr>					
	</table>
	</div>
<%} %>

<script type="text/javascript">
function <%=ClientID %>switch(from,to)
{
	var fromElement = document.getElementById(from);
	var toElement = document.getElementById(to);
	if( fromElement && toElement )
	{
		fromElement.style.display="none";
		toElement.style.display = "block";
	}
	return false;
}

</script>


<%@ Page Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXPublicPage, Main" Title="People" %>

<script runat="server" id="@__bx_pagekeywords">
	public override void SetPageKeywords(System.Collections.Generic.IDictionary<string, string> keywords)
	{
		keywords[@"keywords"]=@"";
		keywords[@"description"]=@"";
	}

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		string userName = Request.Form["UserName"];

		if (!Bitrix.Services.Text.BXStringUtility.IsNullOrTrimEmpty(userName) && userName.Length >= 2)
		{
			UserList.Component.Parameters["FilterByNameToShowUp"] = Request.Form["UserName"];
			UserList.Component.Parameters["PagingAllow"] = "False";
			UserList.Component.Parameters["PagingRecordsPerPage"] = "50";
		}

		Page.Form.Action = Bitrix.Services.BXSefUrlManager.CurrentUrl.ToString();
	}
</script>
<asp:Content ID="Content1" ContentPlaceHolderID="bxcontent" runat="server">



<asp:ScriptManager runat="server" />
<div class="content-rounded-box user-search-box" onkeypress="return FireDefaultButton(event, '<%= button.ClientID %>')">
	<b class="r1"></b><b class="r0"></b>
	<div class="inner-box">
		<input name="UserName" value="search people" onclick="if (this.value=='search people')this.value=''" onblur="if (this.value=='')this.value='search people'" onkeyup="AsyncPostBack();" />
	</div>
	<b class="r0"></b><b class="r1"></b>
</div>

<div class="hr"></div>

<script type="text/javascript">
var asyncPostBackId = null;
function AsyncPostBack() 
{
	if (asyncPostBackId != null)
		clearTimeout(asyncPostBackId);
		
	asyncPostBackId = setTimeout(function() {<%=Page.ClientScript.GetPostBackEventReference(button, "") %>; asyncPostBackId = null;}, 500);
}
</script>
<asp:Button runat="server" Id="button" style="display:none;" />
<asp:UpdatePanel ID="UserListUpdatePanel" runat="server" UpdateMode="Always">
	<ContentTemplate>
		<bx:IncludeComponent 
			id="UserList" 
			runat="server" 
			componentname="bitrix:user.list" 
			template="userlist" 
			UserProfileUrlTemplate="<%$ Options:Bitrix.CommunitySite:UserProfileUrlTemplate %>" 
			FilterByGender="None" 
			FilterByMonthOfBirth="" 
			FilterByDayOfBirth="" 
			SortBy="-RATING" 
			SortDirection="Desc" 
			ProhibitedUserIds="" 
			PagingAllow="True" 
			PagingMode="direct" 
			PagingTemplate=".default" 
			PagingShowOne="False" 
			PagingRecordsPerPage="15" 
			PagingTitle="Pages" 
			PagingPosition="bottom" 
			PagingMaxPages="10" 
			PagingMinRecordsInverse="1" 
			PagingPageID="<%$ Request:page %>" 
			PagingIndexTemplate="<%$ Options:Bitrix.CommunitySite:PeopleSefFolder:~{0} %>" 
			PagingPageTemplate="?page=#pageid#" 
			CacheMode="None" 
			CacheDuration="30" 
			FilterByNameToShowUp="" 
			FilterByUserCustomProperty="False" 
			UserCustomPropertyFilterSettings="p:o:0:{};" 
		/>
	</ContentTemplate>
	<Triggers>
		<asp:AsyncPostBackTrigger ControlID="button" EventName="Click" />
	</Triggers>
 </asp:UpdatePanel>
 
</asp:Content>
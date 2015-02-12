<%@ Reference VirtualPath="~/bitrix/components/bitrix/pmessages.alert/component.ascx" %>
<%@ Control Language="C#" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessageAlertTemplate" EnableViewState="false" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Import Namespace="Bitrix.CommunicationUtility" %>
<%@ Import Namespace="Bitrix.UI" %>

<a href="<%=Component.MessagesReadUrlResolved %>"><%=GetMessage("Title.PrivateMessages")%><%=Component.UnreadMessagesCount > 0 ? "&nbsp;" + String.Format( GetMessage("Title.NewMessagesCount"),Component.UnreadMessagesCount) : ""%></a>

<script runat="server">
	

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		if (Component.UnnotifiedMessagesCount > 0)
		{
			string script = string.Format(@"window.setTimeout({0}$ShowDialog,10);", ClientID);
			Page.ClientScript.RegisterStartupScript(GetType(), ClientID, script, true);
		}
	}

</script>

<%
    if (Component.UnnotifiedMessagesCount > 0) { %>

<script type="text/javascript">



function <%=ClientID %>$ShowDialog()
{
    if ( typeof(Bitrix)=="undefined")
        return;
    if ( typeof(Bitrix.Dialog)=="undefined")
        return;   
   var options = {};
   options._uniquePrefix = "<%=UniqueID %>";
   options._messages = <%=messageListString %>;
   options["msgNextMessage"] = '<%= GetMessageJS("NextMessage") %>';
   options["msgInTopic"] = '<%= GetMessageJS("InTopic") %>';
   options["msgPrevMessage"] = '<%= GetMessageJS("PrevMessage") %>';
   options["msgReadMessage"] = '<%= GetMessageJS("ReadMessage") %>';
   options["msgUserWantToStartConv"] = '<%= GetMessageJS("UserWantToStartConv") %>';
   options["msgUserUnswered"] = '<%= GetMessageJS("UserUnswered") %>';
   options["msgIgnore"] = '<%= GetMessageJS("Ignore") %>';
   options["msgSentDate"] = '<%= GetMessageJS("SentDate") %>';
   options["msgOf"] = '<%= GetMessageJS("Of") %>';
   options["msgMessage"] = '<%= GetMessageJS("Message") %>';
   
   var dialog = Bitrix.PMNewMsgDialog.create("<%=UniqueID %>_NewMessagesDialog",
                                            "<%=UniqueID %>_NewMessagesDialog",
                                            '<%= String.Format(GetMessage("ThereAreNewMessages"),Component.UnnotifiedMessagesCount) %>',
                                            options);
   dialog.setWidth("750px");
   dialog._isModal = true;
   dialog.open();
}

</script>

<%} %>
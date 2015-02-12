<%@ Reference Control="~/bitrix/components/bitrix/pmessages.topic.read/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.CommunicationUtility.Components.PrivateMessageTopicReadTemplate" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Components" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.Security" %>

<% 
	if (Component.FatalError != PrivateMessageTopicReadComponent.ErrorCode.None)
	{ 
		%>
		<div class="forum-content">
		<div class="forum-note-box forum-note-error">
			<div class="forum-note-box-text"><%= Component.GetErrorHtml(Component.FatalError) %></div>
		</div>	
		</div>
		<% 
		return;
	}
%>

<bx:InlineScript runat="server" ID="Script">
<script type="text/javascript">

function <%= ClientID %>_Reply2Author(authorName, enableBBCode, parentPostId)
{
	if (typeof window.GetForumPostTextArea == 'function')
	{
		var textarea = window.GetForumPostTextArea();
		
		if (textarea != null)
		{
			if (enableBBCode)
				textarea.value += "[b]"+authorName+"[/b]"+", ";
			else
				textarea.value += authorName + ", ";
				
			textarea.focus();
			
			return false;
		}
	}
	return true;
}

function <%= ClientID %>_PrepareCodeBlockForQuote(str, codeBlock){
    if(typeof(Bitrix.ForumQuotationProcessors) == "undefined") return "[code]\r\n" + codeBlock + "[/code]";
    var r = null;
    for(var p in Bitrix.ForumQuotationProcessors)
        if((r = Bitrix.ForumQuotationProcessors[p](codeBlock))!= null) 
            break; 
             
    return "[code]\r\n" + (r ? r : codeBlock) + "[/code]";
}

function <%= ClientID %>_Quote(authorInfo, containerId, enableBBCode)
{
	var dstTextArea = null;
	if (typeof window.GetForumPostTextArea != 'function' || !(dstTextArea = window.GetForumPostTextArea()))
	    return true;

    var text = enableBBCode ? "[quote]" : "";
    
    if(authorInfo)
        text += authorInfo.toString() + ":\n";
        
    if(!enableBBCode)
		text += "===========================\n";
    
    var selectedText = Bitrix.DocumentSelection.create().getSelectedText();
    if(selectedText.length == 0){
        var messageContainer = null;
        var messageId = parseInt(containerId.replace(/forum_post_text_/gi, ""));
        if(messageId > 0 && (messageContainer = document.getElementById(containerId))){
            var messageContainerHtml = messageContainer.innerHTML; 
			messageContainerHtml = messageContainerHtml.replace(/\<br(\s)*(\/)*\>[\r\n]*/gi, "\r\n");
			
			messageContainerHtml = messageContainerHtml.replace(/\<(\/?)(b|i|s|u)\>/gi, "[$1$2]");
			
			messageContainerHtml = messageContainerHtml.replace(/\<script[^\>]*>[\r\n]*/gi, "\001").replace(/\<\/script[^\>]*>[\r\n]*/gi, "\002").replace(/\001([^\002]*)\002/gi, "");
			messageContainerHtml = messageContainerHtml.replace(/\<noscript[^\>]*>[\r\n]*/gi, "\001").replace(/\<\/noscript[^\>]*>[\r\n]*/gi, "\002").replace(/\001([^\002]*)\002/gi, "");
			// Quote & Code
			messageContainerHtml = messageContainerHtml.replace(/\<table\s*class\s*\=\s*(\"|\')?forum-quote(\"|\')?([^>]*)\>\s*\<tbody\>\s*(\<tr\>\s*\<th\>\s*([^<]*)\s*\<\/th\>\s*\<\/tr\>\s*)?\s*\<tr\>\s*\<td\>/gi, "\001");
			messageContainerHtml = messageContainerHtml.replace(/\<\/td\>\<\/tr\>\<\/tbody\>\<\/table\>/gi, "\003");                  
			var i = 0;
			while(i < 50 && (messageContainerHtml.search(/\002([^\002\003]*)\003/gi) >= 0 || messageContainerHtml.search(/\001([^\001\003]*)\003/gi) >= 0))
			{
				i++;
				messageContainerHtml = messageContainerHtml.replace(/\001([^\001\003]*)\003/gi, "[quote]$1[/quote]");				
			}
			messageContainerHtml = messageContainerHtml.replace(/[\001\002\003]/gi, "");
			messageContainerHtml = messageContainerHtml.replace(/\<div[^\>]*class\s*=\s*(?:\"|\')?forum-code-box(?:\"|\')?[^\>]*\>([\w\W]*?)\<\/div\><!--ForumCodeBoxEnd-->/ig, <%= ClientID %>_PrepareCodeBlockForQuote);				
			
			// Hrefs 
			messageContainerHtml = messageContainerHtml.replace(/\<a[^>]+href=[\"]([^\"]+)\"[^>]+\>([^<]+)\<\/a\>/gi, "[url=$1]$2[/url]");
			messageContainerHtml = messageContainerHtml.replace(/\<a[^>]+href=[\']([^\']+)\'[^>]+\>([^<]+)\<\/a\>/gi, "[url=$1]$2[/url]");
			
            messageContainerHtml = messageContainerHtml.replace(/\<[^\>]+\>/gi, "");        
            selectedText = Bitrix.HttpUtility.htmlDecode(messageContainerHtml);
        }
    }
    
    if(selectedText.length == 0)
        return true;
        
    text += selectedText;
    text += enableBBCode ? "[/quote]" : "\n===========================\n";
    
    if(dstTextArea.value.length > 0)
        dstTextArea.value += "\n";
	dstTextArea.value += text;
	dstTextArea.focus();
	return false;    
}
</script>
</bx:InlineScript>

<div class="pmessages-content">

<div class="forum-info-box forum-users-online">
<div class = "forum-info-box-inner">
<span class="forum-users-online" id = "PUsers" runat="server"></span>
</div>
</div>

<div class="forum-navigation-box forum-navigation-top">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" CssClassPrefix="forum-" />
	</div>


	    	<div class="forum-new-post">
<% if (Component.Auth.CanReply)
   { %>

		<a  href="<%= Component.TopicReplyHref %>" 
		    onclick="return <%= ClientID %>_Reply2Author('<%= JSEncode(Component.Topic.StarterName) %>', <%="true"%>,
		    0)"><span><%= GetMessage("UrlTitle.Reply")%></span></a>

	<%} %>
</div>
	<div class="forum-clear-float"></div>
</div>



<div class="forum-header-box">
	<div class="forum-header-options">
	</div>
	<div class="forum-header-title">
		<span class="forum-header-title"><%=Component.TopicTitleHtml %></span>
	</div>
</div>



<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<% 
                if (Component.Messages.Count > 0)
                {
                    for (int i = 0; i < Component.Messages.Count; i++)
                    {
                        PrivateMessageTopicReadComponent.MessageInfo post = Component.Messages[i];
			%>
			<table class="forum-post-table <%= (i == 0) ? "forum-post-first " : "" %><%= (i == Component.Messages.Count - 1) ? "forum-post-last " : "" %><%= (i % 2 == 0) ? "forum-post-odd" : "forum-post-even" %>" cellspacing="0" id="msg<%= post.Message.Id %>" >
				<tbody>
					<tr>
						<td class="forum-cell-user">
							<div class="forum-user-info">
								<div class="forum-user-name">
								<% if (!string.IsNullOrEmpty(post.UserProfileHref))
           { %>
								<a href="<%= post.UserProfileHref %>"><span><%= post.AuthorNameHtml%></span></a>
								<% }
           else
           { %>
								<span><%= post.AuthorNameHtml%></span>
								<% } %>
								</div>
								
								<% if (post.Author == null || post.Author.User == null)
           { %>
								<div class="forum-user-guest-avatar"></div>
								<div class="forum-user-status"></div>
								<% }
           else if (post.Author.User.Image == null)
           { %>
								<div class="forum-user-register-avatar"></div>
								<% }
           else
           { %>
								<div class="forum-user-avatar"><img src="<%= post.Author.User.Image.FilePath %>" alt="<%= post.Author.User.Image.Description %>" width="<%= post.Author.User.Image.Width %>" height="<%= post.Author.User.Image.Height %>" /></div>
								<% } %>
								
								<div class="forum-user-additional">
									<% if (post.Author != null && post.Author.User != null)
            { %>
									
									
									<span><%= GetMessage("RegistrationDate")%>: <span><%= post.Author.User.CreationDate.ToString("d")%></span></span>
									<% 
            }%>
								</div>
							</div>
						</td>
						<td class="forum-cell-post">
							<div class="forum-post-date">
								<div class="forum-post-number">
									<a href="<%= post.ThisMsgHref %>" onclick="prompt('<%= JSEncode(GetMessageRaw("Dialog.CopyThisLinkToClipboard")) %>', this.href); return false;" title="<%= GetMessage("ToolTip.PermanentLink") %>" rel="nofollow" >#<%= post.Num%></a><% 
									%>
								</div>

								<span title="<%= Encode(post.Message.SentDate.ToString()) %>"><%= Encode(post.Message.SentDate.ToString("g"))%></span>
							</div>
							<div class="forum-post-entry">
								<div class="forum-post-text" id="forum_post_text_<%= post.Message.Id %>"><%= post.ContentHtml%></div>
							</div>
						</td>
					</tr>
					
					<tr>
						<td class="forum-cell-contact">
							<div class="forum-contact-links">&nbsp;</div>
						</td>
						
						<td class="forum-cell-actions">
							<div class="forum-action-links">
						<% 	if (post.Message.Topic.FirstMessageId == post.Message.Id)
            { %>
				        <% if (post.Auth.CanEditThisTopic)
               { %>
				            &nbsp;&nbsp;<span   class="forum-action-edit">
								                <a href="<%= Component.TopicEditHref %>" rel="nofollow">
								                   <%= GetMessage("UrlTitle.EditTopic")%></a>
								            </span>&nbsp;&nbsp
								            <span   class="forum-action-edit">
								                <a href="<%= Component.InviteUserHref %>" rel="nofollow">
								                   <%= GetMessage("UrlTitle.InviteUser")%></a>
								            </span>
				        
				        <%}
            }
            else
            { %>
							<% if (post.Auth.CanEditThisMessage)
          { %>
							&nbsp;&nbsp;<span   class="forum-action-edit">
								                <a href="<%= post.MsgEditHref %>" rel="nofollow">
								                   <%= GetMessage("UrlTitle.EditPost")%></a>
								            </span>
								            <%}
            } %>
							<% if (Component.Auth.CanReply)
                               { %>
								&nbsp;&nbsp;<span   class="forum-action-quote">
								                <a href="<%= post.MsgQuoteHref %>" rel="nofollow"  
								                   onclick="return <%= ClientID %>_Quote('<%= JSEncode(post.Message.FromUserName) %>', 
								                            'forum_post_text_<%= post.Message.Id %>', <%="true" %>);">
								                   <%= GetMessage("UrlTitle.QuotePost")%></a>
								            </span>
								&nbsp;&nbsp;<span class="forum-action-reply">
								                <a  href="#postform" rel="nofollow" 
								                    onclick="return <%= ClientID %>_Reply2Author('<%= JSEncode(post.Message.FromUserName) %>', 
								                            <%="true" %>,<%= post.Message.Id %>)" title="<%= GetMessage("UrlTitle.Reply2Author.Title")%>">
								                    <%= GetMessage("UrlTitle.Reply2Author")%>
								                </a>
								             </span>
                            <%} %>
							</div>
						</td>
					</tr>
					</tbody>
					<% if (i == Component.Messages.Count - 1)
        { %>
					<tfoot>
					<tr>
						<td colspan="2" class="forum-column-footer">
							<div class="forum-footer-inner">&nbsp;
							</div>
						</td>
					</tr>
				</tfoot>
				<% } %>
			</table>
			<%
                }
                
			%>
			<% }
                else
                { %>

<div class="forum-block-container">
	<div class="forum-block-outer">
		<div class="forum-block-inner">
			<table cellspacing="0" class="forum-table forum-topic-list">

			<tbody>
				<tr class="forum-row-first forum-row-last forum-row-odd">

					<td class="forum-column-alone">
						<div class="forum-empty-message">
						<%=Component.EmptyMessage %></div>
					</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class="forum-column-footer">
						<div class="forum-footer-inner" >&nbsp;</div>
					</td>
				</tr>
			</tfoot>

			</table>
		</div>
	</div>

</div>
			<% } %>
		</div>
	</div>
</div>

<div class="forum-navigation-box forum-navigation-bottom">
	<div class="forum-page-navigation">
		<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="bottom" CssClassPrefix="forum-" />
	</div>
	<div class="forum-new-post">
<% if (Component.Auth.CanReply)
   { %>

		<a href="<%= Component.TopicReplyHref %>" onclick="return <%= ClientID %>_Reply2Author('<%= JSEncode(Component.Topic.StarterName) %>', <%="true" %>,0)"><span><%= GetMessage("UrlTitle.Reply") %></span></a>

	<%} %>
		</div>
	<div class="forum-clear-float"></div>
</div>

</div>

<script runat="server">

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		
		if (Component.FatalError != PrivateMessageTopicReadComponent.ErrorCode.None)
			return;
		Bitrix.DataTypes.BXParamsBag<object> replace = new Bitrix.DataTypes.BXParamsBag<object>();
        StringBuilder s = new StringBuilder();
        s.Append(GetMessage("ParticipatingUsers")).Append(":&nbsp;");
        if (Component.ShowParticipants)
        {
            string userProfileHref = String.Empty;
            foreach (PrivateMessageTopicReadComponent.UserInfo u in Component.ParticipatingUsers)
            {

                    replace["UserId"] = u.User.UserId;
                    userProfileHref = Component.ResolveTemplateUrl(Component.Parameters.GetString("UserProfileUrlTemplate", ""), replace);

                    s.AppendFormat("<a href=\"{0}\">{1}</a>{2}, ",
                        userProfileHref,
                        u.User.GetDisplayName(),
                        u.MappingDeleted ? String.Concat("(", GetMessage("Title.UserMappingDeleted"), ")") : "");

            }
            s.Remove(s.Length - 2, 2);
            PUsers.InnerHtml = s.ToString();
        }
	}
	
</script>

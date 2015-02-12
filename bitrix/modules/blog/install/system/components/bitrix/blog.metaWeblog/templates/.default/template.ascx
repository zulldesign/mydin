<%@ Reference VirtualPath="~/bitrix/components/bitrix/blog.metaWeblog/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.Blog.Components.BlogMetaWeblogTemplate"  %>

<%= String.Format(
		GetMessageRaw("Information"), 
		VirtualPathUtility.ToAbsolute(this.AppRelativeTemplateSourceDirectory),
		Encode(Component.BlogUrl),
		Encode(Bitrix.Services.BXSefUrlManager.CurrentUrl.ToString())
	) %>
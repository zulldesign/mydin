<%@ Reference VirtualPath="~/bitrix/components/bitrix/includeArea/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="IncludeAreaComponentTemplate" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.IO"  %>

<script runat="server">
	protected override void OnLoad(EventArgs e)
	{
		Control content = GetContentControl();
		if (content != null)
		{
			string path = BXPath.Combine(content.AppRelativeTemplateSourceDirectory, "sidebar-header.ascx");
			if (System.IO.File.Exists(BXPath.ToPhysicalPath(path)))
			{
				Control headerControl = LoadControl(path);
				if (headerControl != null)
					SideBar.Controls.Add(headerControl);
			}
			
			//Control bannerControl = LoadControl("~/bitrix/components/bitrix/advertising.banner/component.ascx");
			//if (bannerControl != null)
			//{
			//    BXComponent bannerComponent = (BXComponent)bannerControl;
			//    bannerComponent.Parameters["template"] = "sidebar";
			//    bannerComponent.Parameters["Space"] = "Sidebar";
			//    SideBar.Controls.Add(bannerComponent);
			//}

			SideBar.Controls.Add(content);
		}
		else
			Component.Visible = false;
		
		base.OnLoad(e);
	}
</script>

<div class="rounded-block">
	<div class="corner left-top"></div><div class="corner right-top"></div>
	<div class="block-content">
		<bx:IncludeComponent
			id="SearchForm"
			runat="server"
			componentname="bitrix:search.form"
			template=".default"
			SearchUrlTemplate="<%$ Options:Bitrix.CommunitySite:SearchPageUrlTemplate %>"
		/>
	</div>
	<div class="corner left-bottom"></div><div class="corner right-bottom"></div>
</div>		

<asp:PlaceHolder ID="SideBar" runat="server"></asp:PlaceHolder>
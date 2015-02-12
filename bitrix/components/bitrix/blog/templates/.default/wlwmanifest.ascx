<?xml version="1.0" encoding="utf-8" ?>
<manifest xmlns="http://schemas.microsoft.com/wlw/manifest/weblog">
	<options>
		<clientType>Metaweblog</clientType>
		<supportsEmptyTitles>No</supportsEmptyTitles>
		<requiresXHTML>Yes</requiresXHTML>
	
		<supportsEmbeds>No</supportsEmbeds>
		<supportsScripts>No</supportsScripts>

		<supportsExtendedEntries>Yes</supportsExtendedEntries>
		
		<maxCategoryNameLength>64</maxCategoryNameLength>
		<supportsNewCategories>Yes</supportsNewCategories>
		<supportsNewCategoriesInline>Yes</supportsNewCategoriesInline>
		
		<supportsAutoUpdate>Yes</supportsAutoUpdate>
	</options>
</manifest>
<%@ Control Language="C#" %>
<script runat="server">
	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		if (Bitrix.Configuration.BXConfigurationUtility.IsDesignMode)
			return;
		Response.ContentType = "text/xml";
		using (var w = new HtmlTextWriter(Response.Output))
			Render(w);
		Response.End();
	}
	protected override void Render(HtmlTextWriter writer)
	{
		if (Bitrix.Configuration.BXConfigurationUtility.IsDesignMode)
		{
			writer.Write("<pre>");
			using (var s = new System.IO.StringWriter())
			{
				using (var w = new HtmlTextWriter(s))
					base.Render(w);
				writer.WriteEncodedText(s.ToString());
			}
			writer.Write("</pre>");
			return;
		}
		base.Render(writer);
	}
</script>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.BXComponent" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<%@ Import Namespace="Bitrix.Components" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.UI" %>
<%@ Import Namespace="Bitrix.Modules" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.Search" %>

<script runat="server">
	BXParamsBag<object> request = new BXParamsBag<object>();
	BXParamsBag<string> variableAliases = new BXParamsBag<string>();

	void GetParameters()
	{
		Parameters["SearchUrlTemplate"] = Parameters.Get("SearchUrlTemplate", @"~/search.aspx?q=#query#");
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		IncludeComponentTemplate();
	}

	public override bool ProcessCommand(string commandName, BXParamsBag<object> commandParameters, List<string> commandErrors)
	{
		if (commandName.Equals("search", StringComparison.OrdinalIgnoreCase))
		{
			string param = HttpUtility.UrlEncode(((string)commandParameters["query"]).Trim());

			Response.Redirect(Bitrix.Services.Text.BXStringUtility.ReplaceIgnoreCase(Parameters["SearchUrlTemplate"], "#query#", param));
			return true;
		}
		return false;
	}

    protected override void PreLoadComponentDefinition()
	{
		Title = GetMessage("Title");
		Description = GetMessage("Description");
		Icon = "images/search_form.gif";

		Group = BXSearchModule.GetComponentGroup();
		
		ParamsDefinition["SearchUrlTemplate"] = 
			new BXParamText(
				GetMessageRaw("SearchUrlTemplate"),
				@"~/search.aspx?q=#query#",
				BXCategory.SearchSettings
		);
	}
	
</script>


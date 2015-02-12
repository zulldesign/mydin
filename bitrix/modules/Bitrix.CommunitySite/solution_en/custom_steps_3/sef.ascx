<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Bitrix" %>
<%@ Import Namespace="Bitrix.Install" %>
<%@ Import Namespace="Bitrix.IO" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.DataLayer" %>

<script runat="server">
	
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var view = Result.Render("Solution Installation");
		view.AutoRedirect = true;
		view.RedirectAction = "next";
		return view;
	}

	private string GetRuleExpression(string rule, string siteDir)
	{
		return rule.Replace("~/site/", siteDir);
	}

	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);
		
		 BXSefUrlManager.RefreshComponentSefState(siteId);

		 BXSefUrlRuleManager.BeginUpdate();

		
		string[][] rules = new string[][] {
			new string[] {"Bitrix.CommunitySite.UserProfile", @"^~/site/people/([0-9]+)/?(?:\?(.*))?$", @"~/site/people/profile/default.aspx?user=$1&$2", "40"},
			new string[] {"Bitrix.CommunitySite.UserMail", @"^~/site/people/([0-9]+)/mail[^\?]*(?:\?(.*))?$", @"~/site/people/profile/mail.aspx?user=$1&$2", "50"},
			new string[] {"Bitrix.CommunitySite.UserPages", @"^~/site/people/([0-9]+)/([a-zA-Z-]+){1,15}/?(?:\?(.*))?$", @"~/site/people/profile/$2.aspx?user=$1&$3", "60"}
		};
		
		foreach (var rule in rules)
		{
			var sefUrls = BXSefUrlRuleManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("HelperId", rule[0], BXSqlFilterOperators.Equal), 
					new BXFormFilterItem("SiteId", siteId, BXSqlFilterOperators.Equal)
				)
			);
			
			BXSefUrlRule sefUrl = sefUrls.Count > 0 ? sefUrls[0] : new BXSefUrlRule();
			sefUrl.MatchExpression = GetRuleExpression(rule[1], site.UrlVirtualPath);
			sefUrl.ReplaceExpression = GetRuleExpression(rule[2], site.UrlVirtualPath);

			int sort = 10;
			int.TryParse(rule[3], out sort);
			sefUrl.Sort = sort;
			sefUrl.SiteId = siteId;
			sefUrl.HelperId = rule[0];

			BXSefUrlRuleManager.Add(sefUrl);
		}
		
		BXSefUrlRuleManager.EndUpdate();

		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.CommunitySite", 9);
		return new BXWizardResultFinish();
	}
	
	
</script>
Configure Search Engine Friendly (SEF) URL’s
<% UI.ProgressBar("Installer.ProgressBar"); %>
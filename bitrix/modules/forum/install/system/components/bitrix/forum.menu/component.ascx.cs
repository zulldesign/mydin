using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Bitrix.Components;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Security;
using Bitrix.Services;
using System.Collections.Specialized;

namespace Bitrix.Forum.Components
{
    public partial class ForumMenuComponent : BXComponent
    {
		public string ForumIndexUrl
		{
			get
			{
				return Parameters.GetString("ForumIndexUrl");
			}
			set
			{
				Parameters["ForumIndexUrl"] = value;
			}
		}
		public string UserProfileUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserProfileUrlTemplate");
			}
			set
			{
				Parameters["UserProfileUrlTemplate"] = value;
			}
		}
		public string SearchUrl
		{
			get
			{
				return Parameters.GetString("SearchUrl");
			}
			set
			{
				Parameters["SearchUrl"] = value;
			}
		}
		public string ForumRulesUrl
		{
			get
			{
				return Parameters.GetString("ForumRulesUrl");
			}
			set
			{
				Parameters["ForumRulesUrl"] = value;
			}
		}
		public string ForumHelpUrl
		{
			get
			{
				return Parameters.GetString("ForumHelpUrl");
			}
			set
			{
				Parameters["ForumHelpUrl"] = value;
			}
		}

		private string currentUrl;
		public string CurrentUrl
		{
			get
			{
				if (currentUrl == null)
				{
					UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
					NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
					query.Remove("ReturnUrl");
					uri.Query = query.ToString();
					currentUrl = HttpUtility.UrlEncode(uri.Uri.PathAndQuery);
				}

				return currentUrl;
			}
		}

		private string userProfileUrl;
		public string UserProfileUrl
		{
			get
			{
				return userProfileUrl;
			}
		}

        public string ActiveTopicsUrl
        {
            get
            {
                return Parameters.GetString("ActiveTopicsUrl");
            }
            set
            {
                Parameters["ActiveTopicsUrl"] = value;
            }
        }
        public string UnAnsweredTopicsUrl
        {
            get
            {
                return Parameters.GetString("UnAnsweredTopicsUrl");
            }
            set
            {
                Parameters["UnAnsweredTopicsUrl"] = value;
            }
        }

		public string UserSubscriptionsUrlTemplate
		{
			get
			{
				return Parameters.GetString("UserSubscriptionsUrlTemplate");
			}
			set
			{
				Parameters["UserSubscriptionsUrlTemplate"] = value;
			}
		}

        private List<MenuItem> menuItems;
		public List<MenuItem> MenuItems
		{
			get
			{
				return menuItems;
			}
		}

		private List<MenuItem> submenuItems;
		public List<MenuItem> SubMenuItems
		{
			get
			{
				return submenuItems;
			}
		
		}

		private BXUser currentUser;
		public BXUser CurrentUser
		{
			get
			{
				if (currentUser == null && BXPrincipal.Current != null)
				{
					BXIdentity identity = BXPrincipal.Current.GetIdentity();
					currentUser = identity != null ? identity.User : null;
				}

				return currentUser;
			}
		}

		private string guestMenuText;
		public string GuestMenuText
		{
			get
			{
				if (guestMenuText == null)
					guestMenuText = MakeLink(Parameters.GetString("GuestMenuTextTemplate", ""), GetMenuTextReplace());
				return guestMenuText;
			}
		}

		private string userMenuText;
		public string UserMenuText
		{
			get
			{
				if (userMenuText == null)
					userMenuText = MakeLink(Parameters.GetString("UserMenuTextTemplate", ""), GetMenuTextReplace());
				return userMenuText;
			}
		}

		private BXParamsBag<object> GetMenuTextReplace()
		{
			BXParamsBag<object> replace = new BXParamsBag<object>();
			replace["AppRoot"] = BXUri.ApplicationPath;
			replace["ReturnUrl"] = CurrentUrl;

			if (CurrentUser != null)
			{
				replace["UserName"] = HttpUtility.HtmlEncode(CurrentUser.UserName);
				replace["UserDisplayName"] = HttpUtility.HtmlEncode(CurrentUser.GetDisplayName());
				replace["UserId"] = CurrentUser.UserId;
				replace["UserFirstName"] = HttpUtility.HtmlEncode(CurrentUser.FirstName);
				replace["UserLastName"] = HttpUtility.HtmlEncode(CurrentUser.LastName);
				replace["UserEmail"] = HttpUtility.HtmlEncode(CurrentUser.Email);
			}
			return replace;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			menuItems = new List<MenuItem>();
			submenuItems = new List<MenuItem>();
			
			if (!string.IsNullOrEmpty(ForumIndexUrl)) 
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Forums"), ResolveTemplateUrl(ForumIndexUrl, null), "forums", ""));

			if (!string.IsNullOrEmpty(SearchUrl))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Search"), ResolveTemplateUrl(SearchUrl, null), "search", ""));

			if (BXPrincipal.Current.Identity.IsAuthenticated)
			{
				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("UserId", BXIdentity.Current.Id);

				if (!String.IsNullOrEmpty(UserProfileUrlTemplate))
				{
					userProfileUrl = ResolveTemplateUrl(UserProfileUrlTemplate, replace);
					menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Profile"), userProfileUrl, "profile", ""));
				}

				if (!String.IsNullOrEmpty(UserSubscriptionsUrlTemplate))
					menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Subscriptions"), ResolveTemplateUrl(UserSubscriptionsUrlTemplate, replace), "subscriptions", ""));
			}

			if (!string.IsNullOrEmpty(ForumRulesUrl))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Rules"), ResolveTemplateUrl(ForumRulesUrl, null), "rules", ""));

			if (!string.IsNullOrEmpty(ForumHelpUrl))
				menuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.Help"), ResolveTemplateUrl(ForumHelpUrl, null), "help", ""));

			if (!string.IsNullOrEmpty(ActiveTopicsUrl))
				submenuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.ActiveTopics"), ResolveTemplateUrl(ActiveTopicsUrl, null), "active-topics", ""));

			if (!string.IsNullOrEmpty(UnAnsweredTopicsUrl))
				submenuItems.Add(new MenuItem(GetMessageRaw("UrlTitle.UnAnsweredTopics"), ResolveTemplateUrl(UnAnsweredTopicsUrl, null), "unanswered-topics", ""));

			IncludeComponentTemplate();
		}
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			foreach (string param in new string[] { Parameters.GetString("ThemeCssFilePath"), Parameters.GetString("ColorCssFilePath") })
			{
				if (BXStringUtility.IsNullOrTrimEmpty(param))
					continue;

				string path = param;
				try
				{
					path = BXPath.ToVirtualRelativePath(path);
					if (BXSecureIO.FileExists(path))
						BXPage.RegisterStyle(path);
				}
				catch {}
			}
		}
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);

			BXCategory urlCategory = BXCategory.UrlSettings;
            BXCategory mainCategory = BXCategory.Main;
			BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", mainCategory));

			ParamsDefinition.Add("ForumIndexUrl", new BXParamText(GetMessageRaw("Param.ForumIndexUrl"), "", urlCategory));
			ParamsDefinition.Add("UserProfileUrlTemplate", new BXParamText(GetMessageRaw("Param.USerProfileUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("UserSubscriptionsUrlTemplate", new BXParamText(GetMessageRaw("Param.SubscriptionUrlTemplate"), "", urlCategory));

			ParamsDefinition.Add("SearchUrl", new BXParamText(GetMessageRaw("Param.SearchUrl"), "", urlCategory));
			ParamsDefinition.Add("ForumRulesUrl", new BXParamText(GetMessageRaw("Param.ForumRulesUrl"), "", urlCategory));
			ParamsDefinition.Add("ForumHelpUrl", new BXParamText(GetMessageRaw("Param.ForumHelpUrl"), "", urlCategory));

			ParamsDefinition["GuestMenuTextTemplate"] = new BXParamMultilineText(GetMessageRaw("Param.GuestMenuTextTemplate"), GetMessageRaw("Param.GuestMenuTextTemplate.Default"), additionalSettingsCategory);
			ParamsDefinition["UserMenuTextTemplate"] = new BXParamMultilineText(GetMessageRaw("Param.UserMenuTextTemplate"), GetMessageRaw("Param.UserMenuTextTemplate.Default"), additionalSettingsCategory);
        }

		public class MenuItem
		{
			private string href;
			private string titleHtml;
			private string className;
			private string tooltipHtml;

			public string TooltipHtml
			{
				get
				{
					return tooltipHtml;
				}
				internal set
				{
					tooltipHtml = value;
				}
			}
			public string ClassName
			{
				get
				{
					return className;
				}
				internal set
				{
					className = value;
				}
			}
            public string TitleHtml
			{
				get
				{
					return titleHtml;
				}
				internal set
				{
					titleHtml = value;
				}
			}
			public string Href
			{
				get
				{
					return href;
				}
				internal set
				{
					href = value;
				}
			}


			internal MenuItem(string title, string url, string cssClass, string tooltip)
			{
				titleHtml = title != null ? HttpUtility.HtmlEncode(title) : null;
				href = url != null ? HttpUtility.HtmlEncode(url) : null;
				className = cssClass != null ? HttpUtility.HtmlEncode(cssClass) : null;
				tooltipHtml = tooltip != null ? HttpUtility.HtmlEncode(tooltip) : null;
			}
		}
    }

	public class ForumMenuTemplate : BXComponentTemplate<ForumMenuComponent>
    {
        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            if (IsComponentDesignMode)
            {
               
            }
            base.Render(writer);
        }
    }
}

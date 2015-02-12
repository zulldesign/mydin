using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Bitrix.CommunicationUtility;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Services;

namespace Bitrix.Forum.Components
{
    public partial class ForumListComponent : BXComponent
    {
		private List<int> availableForums;
		public List<int> AvailableForums
		{
			get
			{
				return availableForums ?? (availableForums = Parameters.GetListInt("AvailableForums"));
			}
		}
		
		public string PostReadUrlTemplate
		{
			get
			{
				return Parameters.GetString("PostReadUrlTemplate");
			}
			set
			{
				Parameters["PostReadUrlTemplate"] = value;
			}
		}
		public string TopicListUrlTemplate
		{
			get
			{
				return Parameters.GetString("TopicListUrlTemplate");
			}
			set
			{
				Parameters["TopicListUrlTemplate"] = value;
			}
		}


		private List<ForumListItem> forumList;
		public List<ForumListItem> ForumList
		{
			get { return forumList; }
		}

		public string ForumListTitle
		{
			get { return HttpUtility.HtmlDecode(Parameters.GetString("ForumListTitle", String.Empty)); }
		}
	
		protected void Page_Load(object sender, EventArgs e)
		{
			BXFilter forumsFilter = new BXFilter();
			if (AvailableForums.Count > 0)
				forumsFilter.Add(new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.In, AvailableForums));
			forumsFilter.Add(new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true));
			forumsFilter.Add(new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));
			forumsFilter.Add(new BXFilterItem(BXForum.Fields.CheckPermissions, BXSqlFilterOperators.Equal, BXForum.Operations.ForumPublicRead));
			
			BXForumCollection forums = BXForum.GetList(
				forumsFilter,
				new BXOrderBy(
					new BXOrderByPair(BXForum.Fields.Category.Sort, BXOrderByDirection.Asc),
					new BXOrderByPair(BXForum.Fields.Category.Id, BXOrderByDirection.Asc),
					new BXOrderByPair(BXForum.Fields.Sort, BXOrderByDirection.Asc),
					new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)
				),
				new BXSelectAdd(BXForum.Fields.Category, BXForum.Fields.LastPost.Topic.Name),
				null,
				BXTextEncoder.HtmlTextEncoder
			);

			forumList = new List<ForumListItem>(forums.Count);
			foreach (BXForum forum in forums)
			{
				ForumListItem item = new ForumListItem();

				item.Forum = forum;
				item.ForumId = forum.Id;
				item.Category = forum.Category;

				if (item.Forum.LastPost != null && item.Forum.LastPost.Topic != null && !String.IsNullOrEmpty(item.Forum.LastPost.Topic.Name))
				{
					item.LastTopicTitleFull = item.Forum.LastPost.Topic.Name;

					string title = item.LastTopicTitleFull;
					if (title.Length > 30)
					{
						title = forum.TextEncoder.Decode(item.LastTopicTitleFull);
						title = 
							title.Length > 30 
							? BXWordBreakingProcessor.Break(title.Substring(0, 30), Parameters.GetInt("MaxWordLength", 15), true) + "..."
							: item.LastTopicTitleFull;
					}

					item.LastTopicTitleHtml = title;
					
				}

				if (!BXForum.IsUserCanOperate(item.Forum.Id, BXForum.Operations.ForumTopicApprove))
				{
					item.Forum.QueuedReplies = 0;
					item.Forum.QueuedTopics = 0;
				}

				item.LastTopicAuthorHtml = BXWordBreakingProcessor.Break(item.Forum.TextEncoder.Decode(item.Forum.LastPosterName), Parameters.GetInt("MaxWordLength", 15), true);

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace.Add("ForumId", item.Forum.Id);
				replace.Add("ForumCode", item.Forum.Code);
				item.TopicListHref = Encode(ResolveTemplateUrl(TopicListUrlTemplate, replace));

				if (item.Forum.LastPostId > 0 && item.Forum.LastPost != null)
				{
					replace.Add("TopicId", item.Forum.LastPost.TopicId);
					replace.Add("PostId", item.Forum.LastPostId);
					item.LastPostHref = Encode(ResolveTemplateUrl(PostReadUrlTemplate, replace));
				}

				forumList.Add(item);
			}

			IncludeComponentTemplate();
		}

        private List<LinkInfo> footerLinks;
        public List<LinkInfo> FooterLinks
        {
            get
            {
                if (footerLinks != null)
                    return footerLinks;

                footerLinks = new List<LinkInfo>();
                string param = Parameters.GetString("FooterLinks");
                if (string.IsNullOrEmpty(param))
                    return footerLinks;

                List<BXParamsBag<object>> links = BXSerializer.Deserialize(param) as List<BXParamsBag<object>>;
                if (links == null)
                    return footerLinks;

                foreach (BXParamsBag<object> linkData in links)
                {
                    string url = linkData.GetString("url");
                    string title = linkData.GetString("title");
                    string cssClass = linkData.GetString("class");

                    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title))
                        continue;

                    BXParamsBag<object> replace = new BXParamsBag<object>();
                    footerLinks.Add(new LinkInfo(url, title, cssClass));
                }
                return footerLinks;
            }
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
				catch { }
			}
		}

		public class ForumListItem
		{
			private BXForum forum;
			public BXForum Forum
			{
				get { return forum; }
				set { forum = value; }
			}

			private int forumId;
			public int ForumId
			{
				get { return forumId; }
				set { forumId = value; }
			}

			private BXForumCategory category;
			public BXForumCategory Category
			{
				get { return category; }
				set { category = value; }
			}

			private string topicListHref;
			public string TopicListHref
			{
				get { return topicListHref; }
				set { topicListHref = value; }
			}

			private string lastPostHref;
			public string LastPostHref
			{
				get { return lastPostHref; }
				set { lastPostHref = value; }
			}

			public bool IsNewPostExists
			{
				get { return false; }
			}

			private string lastTopicAuthor;
			public string LastTopicAuthorHtml
			{
				get { return lastTopicAuthor; }
				set { lastTopicAuthor = value; }
			}

			private string lastTopicTitleFull;
			public string LastTopicTitleFull
			{
				get { return lastTopicTitleFull; }
				set { lastTopicTitleFull = value; }
			}

			private string lastTopicTitle;
			public string LastTopicTitleHtml
			{
				get { return lastTopicTitle; }
				set { lastTopicTitle = value; }
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

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("ThemeCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("ColorCssFilePath"), "~/bitrix/components/bitrix/forum/templates/.default/themes/default/style.css", mainCategory));

			ParamsDefinition.Add("PostReadUrlTemplate", new BXParamText(GetMessageRaw("PostReadUrl"), "viewtopic.aspx?post=#PostId###post#PostId#", urlCategory));
			ParamsDefinition.Add("TopicListUrlTemplate", new BXParamText(GetMessageRaw("TopicListUrl"), "viewforum.aspx?forum=#ForumId#", urlCategory));

			ParamsDefinition.Add("AvailableForums", new BXParamMultiSelection(GetMessageRaw("Param.AvailableForums"), "", additionalSettingsCategory));
			ParamsDefinition.Add("ForumListTitle", new BXParamText(GetMessageRaw("ForumListTitle"), GetMessageRaw("ForumListTitleDefault"), additionalSettingsCategory));
            ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("MaxWordLength"), "15", additionalSettingsCategory));
        }

		protected override void LoadComponentDefinition()
		{
			BXForumCollection forums = BXForum.GetList(
				new BXFilter(
					new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
				),
				new BXOrderBy(
					new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)
				),
				new BXSelect(BXForum.Fields.Id, BXForum.Fields.Name),
				null,
				BXTextEncoder.EmptyTextEncoder
			);
			ParamsDefinition["AvailableForums"].Values = forums.ConvertAll<BXParamValue>(delegate(BXForum input)
			{
				return new BXParamValue(input.Name, input.Id.ToString());
			});
		}
        public class LinkInfo
        {
            private string title;
            private string url;
            private string cssClass;

            public string Title
            {
                get
                {
                    return HttpUtility.HtmlEncode(title ?? string.Empty);
                }
            }
            public string Url
            {
                get
                {
                    return url;
                }
            }
            public string Href
            {
                get
                {
                    return HttpUtility.HtmlAttributeEncode(url ?? string.Empty);
                }
            }
            public string CssClass
            {
                get
                {
                    return cssClass != null ? HttpUtility.HtmlAttributeEncode(cssClass) : null;
                }
            }

            internal LinkInfo(string url, string title, string cssClass)
            {
                this.url = url;
                this.title = title;
                this.cssClass = cssClass;
            }
        }
    }

	public class ForumListTemplate : BXComponentTemplate<ForumListComponent>
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

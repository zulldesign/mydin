using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;
using Bitrix.Modules;
using Bitrix.Services.Js;
using Bitrix.Services.Image;
using System.Web.Hosting;
using System.Configuration;
using System.Web.Configuration;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;

namespace Bitrix.Blog.Components
{
	public partial class BlogPostFormComponent : BXComponent
	{
		private string blogSlug;
		private int? postId;
		private Mode? componentMode;
		private readonly List<string> validationErrors = new List<string>();
		private ErrorCode fatalError = ErrorCode.FatalComponentNotExecuted;
		private Exception fatalException;
		private bool templateIncluded;
		private BXBlog blog;
		private BXBlogPost post;
		private BXBlogPostChain bbcodeProcessor;
		private BXBlogPostHtmlProcessor htmlProcessor;
		private BXBlogPostFullHtmlProcessor fullHtmlProcessor;
		private ComponentData data;
		private int maxWordLength = -1;
		private BXBlogAuthorization auth;

		public ErrorCode FatalError
		{
			get
			{
				return fatalError;
			}
		}
		public Exception FatalException
		{
			get
			{
				return fatalException;
			}
		}
		public string BlogSlug
		{
			get
			{
				return blogSlug ?? (blogSlug = Parameters.GetString("BlogSlug"));
			}
		}
		public int PostId
		{
			get
			{
				return (postId ?? (postId = Parameters.GetInt("PostId"))).Value;
			}
		}
		public BXBlog Blog
		{
			get
			{
				LoadData();
				return blog;
			}
		}
		public BXBlogPost Post
		{
			get
			{
				LoadData();
				return post;
			}
		}
		public ComponentData Data
		{
			get
			{
				LoadData();
				return data;
			}
		}
		public BXBlogAuthorization Auth
		{
			get
			{
				LoadData();
				return auth;
			}
		}
		public List<string> ValidationErrors
		{
			get
			{
				return validationErrors;
			}
		}
		internal IBXBlogPostProcessor GetProcessor(BXBlogPostContentType contentType)
		{			
			if (contentType == BXBlogPostContentType.FullHtml)
				return FullHtmlProcessor;
			if (contentType == BXBlogPostContentType.FilteredHtml)
				return HtmlProcessor;		
			return BBCodeProcessor;
		}

		internal BXBlogPostChain BBCodeProcessor
		{
			get
			{
				if (bbcodeProcessor == null)
				{
					bbcodeProcessor = new BXBlogPostChain(blog);
					bbcodeProcessor.MaxWordLength = MaxWordLength;
					bbcodeProcessor.HideCut = false;
				}
				return bbcodeProcessor;
			}
		}

		internal BXBlogPostHtmlProcessor HtmlProcessor
		{
			get
			{
				if (htmlProcessor == null)
				{
					htmlProcessor = new BXBlogPostHtmlProcessor();
					htmlProcessor.HideCut = false;
				}
				return htmlProcessor;
			}
		}

		internal BXBlogPostFullHtmlProcessor FullHtmlProcessor
		{
			get
			{
				if (fullHtmlProcessor == null)
				{
					fullHtmlProcessor = new BXBlogPostFullHtmlProcessor();
					fullHtmlProcessor.HideCut = false;
				}
				return fullHtmlProcessor;
			}
		}

		public int MaxWordLength
		{
			get
			{
				return (maxWordLength != -1) ? maxWordLength : (maxWordLength = Math.Max(0, Parameters.GetInt("MaxWordLength", 15)));
			}
			set
			{
				maxWordLength = Math.Max(0, value);
				Parameters["MaxWordLength"] = maxWordLength.ToString();
			}
		}


		private int cutThreshold = -1;
		public int CutThreshold
		{
			get
			{
				return (cutThreshold >= 0) ? cutThreshold : (cutThreshold = Math.Max(0, Parameters.GetInt("CutThreshold", 0)));
			}
			set
			{
				cutThreshold = Math.Max(0, value);
				Parameters["CutThreshold"] = cutThreshold.ToString();
			}
		}

		public bool SetPageTitle
		{
			get
			{
				return Parameters.GetBool("SetPageTitle", true);
			}
			set
			{
				Parameters["SetPageTitle"] = value.ToString();
			}
		}

		public Mode ComponentMode
		{
			get
			{
				return (componentMode ?? (componentMode = PostId == 0 ? Mode.Add : Mode.Edit)).Value;
			}
		}

		public int MaximalFileSizeInKBytes
		{
			get { return Parameters.GetInt("MaximalFileSizeInKBytes", 1024); }
			set { Parameters["MaximalFileSizeInKBytes"] = value.ToString(); }
		}

		public int MaximalAllFilesSizeInKBytes
		{
			get { return Parameters.GetInt("MaximalAllFilesSizeInKBytes", 8192); }
			set { Parameters["MaximalAllFilesSizeInKBytes"] = value.ToString(); }
		}

		public int MaximalImageWidthInPixels
		{
			get { return Parameters.GetInt("MaximalImageWidthInPixels", 0); }
			set { Parameters["MaximalImageWidthInPixels"] = value.ToString(); }
		}

		public int MaximalImageHeightInPixels
		{
			get { return Parameters.GetInt("MaximalImageHeightInPixels", 0); }
			set { Parameters["MaximalImageHeightInPixels"] = value.ToString(); }
		}

		public int CategoryId
		{
			get { return Parameters.GetInt("CategoryId", 0); }
		}

		public bool Validate()
		{
			if (fatalError != ErrorCode.None)
				return false;

			data.Title = data.Title != null ? data.Title.Trim() : null;
			if (string.IsNullOrEmpty(data.Title))
				validationErrors.Add(GetValidationMessageHtml(ValidationMessageCode.TitleIsRequired));

			if (data.Content == null)
				data.Content = "";
			if (data.IsNew || data.contentChanged)
			{
				switch (data.ContentType)
				{
					case BXBlogPostContentType.FullHtml:
						data.Content = FullHtmlProcessor.Correct(data.Content);
						break;
					case BXBlogPostContentType.FilteredHtml:
						data.Content = HtmlProcessor.Correct(data.Content);
						break;
					default:
						data.Content = BBCodeProcessor.CorrectBBCode(data.Content);
						break;
				}

				if (data.ContentType == BXBlogPostContentType.FilteredHtml && !Auth.CanWriteFilteredHtml
					|| data.ContentType == BXBlogPostContentType.FullHtml && !Auth.CanWriteFullHtml)
					validationErrors.Add(GetValidationMessageHtml(ValidationMessageCode.NotAllowedToWriteHtml));
			}

			//проверка превышения допустимой длины текста не заключённого в теги CUT
			if (CutThreshold > 0 && CutThreshold < data.Content.Length)
			{
				IBXBlogPostProcessor proc = null;
				if (data.ContentType == BXBlogPostContentType.FullHtml)
					proc = new BXBlogPostFullHtmlProcessor();
				else if (data.ContentType == BXBlogPostContentType.FilteredHtml)
					proc = new BXBlogPostHtmlProcessor();
				else
				{
					BXBlogPostChain bbcodeProc = new BXBlogPostChain(blog);
					bbcodeProc.MaxWordLength = MaxWordLength;
					proc = bbcodeProc;
				}
				proc.HideCut = true;
				string txt = BXStringUtility.HtmlToText(proc.Process(data.Content));
				if (txt.Length > CutThreshold)
					validationErrors.Add(string.Format(GetMessageRaw("Error.ContentLengthExceedCutThreshold"), txt.Length.ToString(), CutThreshold.ToString()));
			}

			if (validationErrors.Count > 0)
				return false;

			List<string> tags = string.IsNullOrEmpty(data.Tags) ? null : new List<string>(data.Tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
			if (tags != null && tags.Count != 0)
			{
				int originalCount = tags.Count;
				Dictionary<string, BXCommandResult> rd = new BXCommand("Bitrix.Search.GetInvalidTags").AddP("tags", tags).Send().CommandResultDictionary;
				foreach (BXCommandResult r in rd.Values)
				{
					if (r.CommandResult != BXCommandResultType.Ok || r.Result == null)
						continue;

					foreach (string invalid in (IEnumerable<string>)r.Result)
						tags.Remove(invalid);
				}

				if (tags.Count < originalCount)
					data.Tags = string.Join(",", tags.ToArray());
			}

			List<string> customFieldErrors = new List<string>();
			List<string> edCustomFieldNames = new List<string>();
			foreach (BlogPostFormCustomFieldEditor ed in PostCustomFieldEditors)
			{
				edCustomFieldNames.Add(ed.Field.CorrectedName);
				ed.Save(data.CustomProperties, customFieldErrors);
			}

			if (customFieldErrors.Count > 0)
			{
				foreach (string customFieldError in customFieldErrors)
					validationErrors.Add(customFieldError);
				return false;
			}                

			if (data.IsNew)
			{
				BXCustomFieldCollection customFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.PostCustomFieldEntityId);
				foreach (BXCustomField f in customFields)
				{
					if (edCustomFieldNames.FindIndex(
						delegate(string s)
						{
							return string.Equals(s, f.CorrectedName, StringComparison.Ordinal);
						}) >= 0)
						continue;

					object defaultVal;
					BXCustomType t = BXCustomTypeManager.GetCustomType(f.CustomTypeId);
					if (t == null || !t.TryGetDefaultValue(f, out defaultVal))
						continue;

					data.CustomProperties[f.Name] = new BXCustomProperty(f.Name, f.Id, t.DbType, t.IsClonable ? false : f.Multiple, defaultVal);
				}
			}

			return true;
		}
		public void Save()
		{
			if (fatalError != ErrorCode.None)
				return;

			try
			{
				if (data.IsNew)
				{
					post = new BXBlogPost(BXTextEncoder.EmptyTextEncoder);
					post.BlogId = blog.Id;
					post.DateCreated = DateTime.Now;
					post.AuthorId = BXIdentity.Current.Id;
					post.AuthorIP = Request.UserHostAddress;
				}

				List<int> boundFileList = null;
				Regex rx = new Regex(
					data.ContentType == BXBlogPostContentType.FilteredHtml || data.ContentType == BXBlogPostContentType.FullHtml
						? @"<img\s+[^>]*?src\s*=\s*""(?<url>[^"">]*?)""[^>]*>|<a\s+[^>]*?href\s*=\s*""(?<url>[^"">]*?)""[^>]*>"
						: @"\[img(?:=[^\]]*)?]\s*(?<url>.*?)\s*\[/img\]",
					RegexOptions.IgnoreCase
				);
				string uploadPath = VirtualPathUtility.ToAbsolute(VirtualPathUtility.AppendTrailingSlash(BXFile.UploadFolder)).ToLowerInvariant();
				if (!string.IsNullOrEmpty(data.Content))
				{
					System.Text.RegularExpressions.Match m = rx.Match(data.Content);
					for (; m.Success; m = m.NextMatch())
					{
						System.Text.RegularExpressions.Group g = m.Groups["url"];
						string url = string.Empty;
						if (g.Success && !string.IsNullOrEmpty(url = g.Value))
						{
							url = HttpUtility.HtmlDecode(url);
							try
							{
								try
								{
									url = new Uri(BXSefUrlManager.CurrentUrl, url).AbsolutePath;
								}
								catch
								{
								}
								url = url.ToLowerInvariant();

								if (!url.StartsWith(uploadPath))
									continue;
								string folder = string.Empty,
									name = string.Empty;
								url = url.Substring(uploadPath.Length);
								int whatInd = url.IndexOf('?');
								if (whatInd >= 0)
									url = url.Substring(0, whatInd);

								int lastSlashInd = url.LastIndexOf("/");
								if (lastSlashInd >= 0)
								{
									folder = url.Substring(0, lastSlashInd);
									name = url.Substring(lastSlashInd + 1);
								}
								else
									name = url;

								BXFilter f = new BXFilter();
								f.Add(new BXFilterItem(BXFile.Fields.OwnerModuleId, BXSqlFilterOperators.Equal, BXBlogModuleConfiguration.Id));
								if (string.IsNullOrEmpty(folder))
									f.Add(new BXFilterItem(BXFile.Fields.Folder, BXSqlFilterOperators.Equal, folder));
								f.Add(new BXFilterItem(BXFile.Fields.FileName, BXSqlFilterOperators.Equal, name));
								BXFileCollection c = BXFile.GetList(f, null, new BXSelect(BXSelectFieldPreparationMode.Normal, BXFile.Fields.Id), new BXQueryParams(new BXPagingOptions(0, 1)));
								if (c.Count == 0)
									continue;
								if (boundFileList == null)
									boundFileList = new List<int>();
								if (boundFileList.FindIndex(delegate(int obj) { return obj == c[0].Id; }) < 0)
									boundFileList.Add(c[0].Id);
							}
							catch
							{
							}
						}
					}
				}

				post.Title = data.Title;
				post.Tags = data.Tags;
				post.IsPublished = data.IsPublished;
				post.DatePublished = (data.DatePublished != DateTime.MinValue || !data.IsPublished) ? data.DatePublished : DateTime.Now;
				post.Flags = data.Flags;
				if (data.IsNew || data.contentChanged)
				{
					post.Content = data.Content;
					post.ContentType = data.ContentType;
				}
				post.SetFileIds(boundFileList != null && boundFileList.Count > 0 ? boundFileList.ToArray() : null);
				post.CustomValues.Assign(data.CustomProperties);

				using (BXSqlConnection con = new BXSqlConnection())
				{
					con.Open();
					using (BXSqlTransaction tran = con.BeginTransaction())
					{
						post.Save(tran.Connection, tran);
						//BXCustomEntityManager.SaveEntity(BXBlogModuleConfiguration.PostCustomFieldEntityId, post.Id, data.CustomProperties, tran);
						tran.Commit();
					}
				}
				data = new ComponentData(post);
			}
			catch (BXEventException ex)
			{
				//обработка ошибок валидации BXModifyStatus.TryThrowException
				IList<string> messages = ex.Messages;
				if (messages.Count == 0)
					Fatal(ex);
				else
					for (int i = 0; i < messages.Count; i++)
						validationErrors.Add(HttpUtility.HtmlEncode(messages[i]));
				return;
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}

			string redirectUrl = Parameters.GetString(data.IsPublished ? "PublishUrlTemplate" : "DraftUrlTemplate");
			if (BXStringUtility.IsNullOrTrimEmpty(redirectUrl))
				redirectUrl = Parameters.GetString("RedirectUrlTemplate");
			if (BXStringUtility.IsNullOrTrimEmpty(redirectUrl))
				Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
			RedirectTemplateUrl(redirectUrl, GetReplaceParameters());
		}
		public void Preview(string input, HtmlTextWriter writer)
		{
			Preview(input, writer, BXBlogPostContentType.BBCode);
		}
		public string Preview(string input)
		{
			return Preview(input, BXBlogPostContentType.BBCode);
		}
		public void Preview(string input, HtmlTextWriter writer, BXBlogPostContentType contentType)
		{
			GetProcessor(contentType).Process(input, writer);
		}
		public string Preview(string input, BXBlogPostContentType contentType)
		{
			using (StringWriter s = new StringWriter())
			{
				GetProcessor(contentType).Process(input, s);
				return s.ToString();
			}
		}
		public string GetErrorHtml(ErrorCode code)
		{
			switch (code)
			{
				case ErrorCode.FatalException:
					return BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.SystemMaintenance) ? ("<pre>" + Encode(fatalException.ToString()) + "</pre>") : GetMessage("Error.Unknown");
				case ErrorCode.FatalPostNotFound:
					return GetMessage("Error.PostNotFound");
				case ErrorCode.FatalBlogNotFound:
					return GetMessage("Error.BlogNotFound");
				case ErrorCode.FatalBlogIsNotActive:
					return GetMessage("Error.BlogIsDisabled");
				case ErrorCode.UnauthorizedCreatePost:
					return GetMessage("Error.UnauthorizedCreate");
				case ErrorCode.UnauthorizedEditPost:
					return GetMessage("Blog.UnauthorizedEdit");
				default:
					return GetMessage("Error.Unknown");
			}
		}
		public string GetValidationMessageHtml(ValidationMessageCode code)
		{
			switch (code)
			{
				case ValidationMessageCode.TitleIsRequired:
					return GetMessage("Error.TitleIsNotSpecified");
				case ValidationMessageCode.NotAllowedToWriteHtml:
					return GetMessage("Error.NotAllowedToWriteHtml");
				default:
					return null;
			}
		}

		private BXParamsBag<object> GetReplaceParameters()
		{
			BXParamsBag<object> replace = new BXParamsBag<object>();
			replace["BlogId"] = blog.Id;
			replace["BlogSlug"] = blog.Slug;
			replace["PostId"] = post.Id;
			DateTime date = post.DatePublished;
			replace["PostYear"] = date.Year;
			replace["PostMonth"] = date.Month;
			replace["PostDay"] = date.Day;
			replace["UserId"] = post.AuthorId;
			return replace;
		}
		private bool CheckBlog()
		{
			if (ComponentMode == Mode.Add && auth.CanCreatePost || ComponentMode == Mode.Edit && auth.CanEditThisPost)
				return true;

			if (!blog.Active)
				Fatal(ErrorCode.FatalBlogIsNotActive);
			else
				Fatal(ComponentMode == Mode.Add ? ErrorCode.UnauthorizedCreatePost : ErrorCode.UnauthorizedEditPost);
			return false;
		}
		private void Fatal(ErrorCode code)
		{
			if (code == ErrorCode.FatalException)
				throw new InvalidOperationException("Use method with Exception argument");
			fatalError = code;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}
		private void Fatal(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			fatalError = ErrorCode.FatalException;
			fatalException = ex;
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
			}
		}

		private bool _isDataLoaded = false;
		/// <summary>
		/// Загрузка сообщения и блога
		/// </summary>
		private void LoadData()
		{
			if (_isDataLoaded)
				return;

			_isDataLoaded = true;

			if (ComponentMode == Mode.Edit)
			{
				var filter = new BXFilter(
						new BXFilterItem(BXBlogPost.Fields.Id, BXSqlFilterOperators.Equal, PostId),
						new BXFilterItem(BXBlogPost.Fields.Blog.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
					);

				if (CategoryId > 0)
					filter.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

				BXBlogPostCollection posts = BXBlogPost.GetList(
					filter,
					null,
					new BXSelectAdd(
						BXBlogPost.Fields.Blog,
						BXBlogPost.Fields.CustomFields.DefaultFields
						),
					null
				);

				if (posts.Count > 0)
					post = posts[0];

				if (post == null)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return;
				}

				blog = post.Blog;
				if (blog == null || !string.IsNullOrEmpty(BlogSlug) && blog.Slug != BlogSlug)
				{
					Fatal(ErrorCode.FatalPostNotFound);
					return;
				}
			}
			else
			{
				var blogFilter = new BXFilter(
						new BXFilterItem(BXBlog.Fields.Slug, BXSqlFilterOperators.Equal, BlogSlug),
						new BXFilterItem(BXBlog.Fields.Categories.Category.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
					);

				if (CategoryId > 0)
					blogFilter.Add(new BXFilterItem(BXBlog.Fields.Categories.CategoryId, BXSqlFilterOperators.Equal, CategoryId));

				BXBlogCollection blogs = BXBlog.GetList(
					blogFilter,
					null
				);

				if (blogs.Count > 0)
					blog = blogs[0];

				if (blog == null)
				{
					Fatal(ErrorCode.FatalBlogNotFound);
					return;
				}
			}

			auth = new BXBlogAuthorization(blog, post);
			if (CheckBlog())
				data = new ComponentData(post);
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			(new BlogPostFormImageHandler(this)).Process();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fatalError = ErrorCode.None;
			try
			{
				LoadData();
				//Set page title
				BXPublicPage bitrixPage = Page as BXPublicPage;
				if (bitrixPage != null && !IsComponentDesignMode && SetPageTitle)
				{
					bitrixPage.Title = bitrixPage.MasterTitleHtml = Encode(
						ComponentMode == Mode.Add
						? GetMessageRaw("PageTitle.CreatePost")
						: GetMessageRaw("PageTitle.EditPost")
					);
				}
			}
			catch (Exception ex)
			{
				Fatal(ex);
				return;
			}
			if (!templateIncluded)
			{
				templateIncluded = true;
				IncludeComponentTemplate();
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
				catch
				{
				}
			}
		}
		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = BXBlogModuleConfiguration.GetComponentGroup();

			BXCategory mainCategory = BXCategory.Main,
				urlCategory = BXCategory.UrlSettings,
				additionalSettingsCategory = BXCategory.AdditionalSettings,
				customFieldCategory = BXCategory.CustomField;

			ParamsDefinition.Add("ThemeCssFilePath", new BXParamText(GetMessageRaw("Param.ThemeCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/style.css", mainCategory));
			ParamsDefinition.Add("ColorCssFilePath", new BXParamText(GetMessageRaw("Param.ColorCssFilePath"), "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css", mainCategory));

			ParamsDefinition.Add("BlogSlug", new BXParamText(GetMessageRaw("Param.BlogSlug"), "", mainCategory));
			ParamsDefinition.Add("PostId", new BXParamText(GetMessageRaw("Param.PostId"), "", mainCategory));
			ParamsDefinition["CategoryId"] = new BXParamSingleSelection(GetMessageRaw("Param.CategoryId"), "", mainCategory);
			ParamsDefinition.Add("MaxWordLength", new BXParamText(GetMessageRaw("Param.MaxWordLength"), "15", additionalSettingsCategory));
			ParamsDefinition.Add("SetPageTitle", new BXParamYesNo(GetMessageRaw("Param.SetPageTitle"), true, additionalSettingsCategory));

			ParamsDefinition.Add("CutThreshold", new BXParamText(GetMessageRaw("Param.CutThreshold"), "0", additionalSettingsCategory));

			ParamsDefinition.Add("PublishUrlTemplate", new BXParamText(GetMessageRaw("Param.PublishUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("DraftUrlTemplate", new BXParamText(GetMessageRaw("Param.DraftUrlTemplate"), "", urlCategory));
			ParamsDefinition.Add("RedirectUrlTemplate", new BXParamText(GetMessageRaw("Param.RedirectUrlTemplate"), "", urlCategory));

			//File limitations
			ParamsDefinition.Add("MaximalFileSizeInKBytes", new BXParamText(GetMessageRaw("Param.MaximalFileSizeInKBytes"), "1024", additionalSettingsCategory));
			ParamsDefinition.Add("MaximalAllFilesSizeInKBytes", new BXParamText(GetMessageRaw("Param.MaximalAllFilesSizeInKBytes"), "8192", additionalSettingsCategory));
			//Image limitations
			ParamsDefinition.Add("MaximalImageWidthInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageWidthInPixels"), "0", additionalSettingsCategory));
			ParamsDefinition.Add("MaximalImageHeightInPixels", new BXParamText(GetMessageRaw("Param.MaximalImageHeightInPixels"), "0", additionalSettingsCategory));

			ParamsDefinition["AvailablePostCustomFieldsForAuthor"] =
				new BXParamMultiSelection(
					GetMessageRaw("Param.AvailablePostCustomFieldsForAuthor"),
					string.Empty,
					customFieldCategory
					);

			ParamsDefinition["AvailablePostCustomFieldsForModerator"] =
				new BXParamMultiSelection(
					GetMessageRaw("Param.AvailablePostCustomFieldsForModerator"),
					string.Empty,
					customFieldCategory
					);
		}

		protected override void LoadComponentDefinition()
		{
			//AvailablePostCustomFieldsForAuthor, AvailablePostCustomFieldsForModerator
			IList<BXParamValue> availablePostCustomFieldsForAuthor = ((BXParamMultiSelection)ParamsDefinition["AvailablePostCustomFieldsForAuthor"]).Values;
			if (availablePostCustomFieldsForAuthor.Count > 0)
				availablePostCustomFieldsForAuthor.Clear();

			IList<BXParamValue> availablePostCustomFieldsForModerator = ((BXParamMultiSelection)ParamsDefinition["AvailablePostCustomFieldsForModerator"]).Values;
			if (availablePostCustomFieldsForModerator.Count > 0)
				availablePostCustomFieldsForModerator.Clear();

			BXCustomFieldCollection fields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.PostCustomFieldEntityId);
			foreach (BXCustomField field in fields)
			{
				BXParamValue param = new BXParamValue(field.TextEncoder.Decode(field.EditFormLabel), field.Name);
				availablePostCustomFieldsForAuthor.Add(param);
				availablePostCustomFieldsForModerator.Add(param);
			}

			IList<BXParamValue> categoryIds = ((BXParamSingleSelection)ParamsDefinition["CategoryId"]).Values;
			if (categoryIds.Count > 0)
				categoryIds.Clear();

			BXBlogCategoryCollection categories = BXBlogCategory.GetList(
			   new BXFilter(new BXFilterItem(BXBlogCategory.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)),
			   new BXOrderBy(new BXOrderByPair(BXBlogCategory.Fields.Name, BXOrderByDirection.Asc)),
			   new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogCategory.Fields.Id, BXBlogCategory.Fields.Name),
			   null,
			   BXTextEncoder.EmptyTextEncoder
			   );
			categoryIds.Add(new BXParamValue(GetMessage("Kernel.All"), ""));
			foreach (BXBlogCategory category in categories)
			{
				BXParamValue value = new BXParamValue(category.Name, category.Id.ToString());
				categoryIds.Add(value);
			}
		}

		private BXCustomField[] availablePostCustomFieldsForAuthor = null;
		public BXCustomField[] AvailablePostCustomFieldsForAuthor
		{
			get
			{
				if (this.availablePostCustomFieldsForAuthor != null)
					return this.availablePostCustomFieldsForAuthor;

				List<string> names = null;
				BXCustomFieldCollection allFields = null;
				if ((names = Parameters.GetListString("AvailablePostCustomFieldsForAuthor")).Count == 0
					|| (allFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.PostCustomFieldEntityId)).Count == 0)
					return (this.availablePostCustomFieldsForAuthor = new BXCustomField[0]);

				List<BXCustomField> l = new List<BXCustomField>();
				foreach (string name in names)
				{
					int fIndex = allFields.FindIndex(delegate(BXCustomField obj) { return string.Equals(obj.Name, name, StringComparison.InvariantCultureIgnoreCase); });
					if (fIndex < 0)
						continue;
					l.Add(allFields[fIndex]);
				}
				return (this.availablePostCustomFieldsForAuthor = l.ToArray());
			}
		}
		private BXCustomField[] availablePostCustomFieldsForModerator = null;
		public BXCustomField[] AvailablePostCustomFieldsForModerator
		{
			get
			{
				if (this.availablePostCustomFieldsForModerator != null)
					return this.availablePostCustomFieldsForModerator;

				List<string> names = null;
				BXCustomFieldCollection allFields = null;
				if ((names = Parameters.GetListString("AvailablePostCustomFieldsForModerator")).Count == 0
					|| (allFields = BXCustomEntityManager.GetFields(BXBlogModuleConfiguration.PostCustomFieldEntityId)).Count == 0)
					return (this.availablePostCustomFieldsForModerator = new BXCustomField[0]);

				List<BXCustomField> l = new List<BXCustomField>();
				foreach (string name in names)
				{
					int fIndex = allFields.FindIndex(delegate(BXCustomField obj) { return string.Equals(obj.Name, name, StringComparison.InvariantCultureIgnoreCase); });
					if (fIndex < 0)
						continue;
					l.Add(allFields[fIndex]);
				}
				return (this.availablePostCustomFieldsForModerator = l.ToArray());
			}
		}
		private BlogPostFormCustomFieldEditor[] postCustomFieldEditors = null;
		public BlogPostFormCustomFieldEditor[] PostCustomFieldEditors
		{
			get
			{
				if (this.postCustomFieldEditors != null)
					return this.postCustomFieldEditors;

				BXCustomField[] authorFields = AvailablePostCustomFieldsForAuthor,
					moderatorFields = auth.CanEditPost ? AvailablePostCustomFieldsForModerator : new BXCustomField[0];
				if (authorFields.Length == 0 && moderatorFields.Length == 0)
					return (this.postCustomFieldEditors = new BlogPostFormCustomFieldEditor[0]);

				List<BlogPostFormCustomFieldEditor> l = new List<BlogPostFormCustomFieldEditor>();
				if (authorFields.Length > 0)
					for (int i = 0; i < authorFields.Length; i++)
					{
						try
						{
							BlogPostFormCustomFieldEditor ed = new BlogPostFormTemplate.CustomFieldEditor(this, authorFields[i]);
							ed.Load(data.CustomProperties);
							l.Add(ed);
						}
						catch (Exception /*exc*/) { }
					}
				if (moderatorFields.Length > 0)
					for (int i = 0; i < moderatorFields.Length; i++)
					{
						BXCustomField f = moderatorFields[i];
						if (l.FindIndex(
								delegate(BlogPostFormCustomFieldEditor obj)
								{ return string.Equals(obj.Field.Name, f.Name, StringComparison.InvariantCulture); }) >= 0)
							continue;
						try
						{
							BlogPostFormCustomFieldEditor ed = new BlogPostFormTemplate.CustomFieldEditor(this, f);
							ed.Load(data.CustomProperties);
							l.Add(ed);
						}
						catch
						{
						}
					}
				return (this.postCustomFieldEditors = l.ToArray());
			}
		}
		public enum Mode
		{
			Add,
			Edit
		}
		public enum ErrorCode
		{
			None = 0,
			Fatal = 1,
			Unauthorized = 2,
			FatalComponentNotExecuted = Fatal | (0 << 2),
			FatalException = Fatal | (1 << 2),
			FatalBlogNotFound = Fatal | (2 << 2),
			FatalPostNotFound = Fatal | (3 << 2),
			FatalBlogIsNotActive = Fatal | (4 << 2),
			UnauthorizedCreatePost = Unauthorized | (0 << 2),
			UnauthorizedEditPost = Unauthorized | (1 << 2)
		}
		public enum ValidationMessageCode
		{
			None,
			TitleIsRequired,
			NotAllowedToWriteHtml
		}
		public class ComponentData
		{
			private BXBlogPost post;
			private string title;
			private string content;
			private DateTime datePublished;
			private readonly bool isNew;
			private bool isPublished;
			private string tags;
			private BXBlogPostContentType contentType;
			private BXBlogPostFlags flags = BXBlogPostFlags.EnableComments;
			private BXCustomPropertyCollection customProperties;
			internal bool contentChanged;

			internal ComponentData(BXBlogPost post)
			{
				this.post = post;
				if (this.post == null)
				{
					isNew = true;
					datePublished = DateTime.Now;
					return;
				}

				isPublished = this.post.IsPublished;
				title = this.post.TextEncoder.Decode(this.post.Title);
				content = this.post.Content;
				datePublished = this.post.DatePublished;
				tags = this.post.TextEncoder.Decode(this.post.Tags);
				contentType = this.post.ContentType;
				flags = this.post.Flags;
			}

			public bool IsPublished
			{
				get
				{
					return isPublished;
				}
				set
				{
					isPublished = value;
				}
			}
			public bool IsNew
			{
				get
				{
					return isNew;
				}
			}
			public string Title
			{
				get
				{
					return title;
				}
				set
				{
					title = value;
				}
			}
			public string Content
			{
				get
				{
					return content;
				}
				set
				{
					if (!contentChanged && content != value)
						contentChanged = true;
					content = value;
				}
			}
			public string Tags
			{
				get
				{
					return tags;
				}
				set
				{
					tags = value;
				}
			}
			public BXBlogPostContentType ContentType
			{
				get
				{
					return contentType;
				}
				set
				{
					if (contentType != value)
						contentChanged = true;
					contentType = value;
				}
			}
			public BXBlogPostFlags Flags
			{
				get
				{
					return flags;
				}
				set
				{
					flags = value;
				}
			}
			public DateTime DatePublished
			{
				get
				{
					return datePublished;
				}
				set
				{
					datePublished = value;
				}
			}
			public bool EnableComments
			{
				get 
				{
					return (flags & BXBlogPostFlags.EnableComments) == BXBlogPostFlags.EnableComments;
				}
				set 
				{
					if (value)
						flags |= BXBlogPostFlags.EnableComments;
					else
						flags &= ~BXBlogPostFlags.EnableComments;
				}
			}

			public int Id
			{
				get
				{
					return this.post != null ? this.post.Id : 0;
				}
			}
			public BXCustomPropertyCollection CustomProperties
			{
				//get{ return this.customProperties ?? (this.customProperties = Id > 0 ? BXCustomEntityManager.GetProperties(BXBlogModuleConfiguration.PostCustomFieldEntityId, Id) : new BXCustomPropertyCollection()); }
				get 
				{
					if (this.customProperties != null)
						return this.customProperties;

					this.customProperties = new BXCustomPropertyCollection();
					if (this.post != null)
						this.customProperties.Assign(this.post.CustomValues);
					return this.customProperties;
				}
			}
		}
	}

	public class BlogPostFormCustomFieldEditor
	{
		private BlogPostFormComponent parent = null;
		private BXCustomField field = null;
		private BXCustomType fieldType = null;
		private BXCustomTypePublicEdit editor = null;
		private string formName = string.Empty;
		private string clientId = string.Empty;
		public BlogPostFormCustomFieldEditor(BlogPostFormComponent parent, BXCustomField field)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");
			this.parent = parent;

			if (field == null)
				throw new ArgumentNullException("field");
			this.field = field;

			this.clientId = string.Concat(parent.ClientID, "_", this.field.Name);
			this.formName = string.Concat(parent.UniqueID, "$", this.field.Name);

			this.fieldType = BXCustomTypeManager.GetCustomType(this.field.CustomTypeId);
			if (this.fieldType == null)
				throw new InvalidOperationException("Could not get custom field type!");

			this.editor = this.fieldType.CreatePublicEditor();
			if (this.editor == null)
				throw new InvalidOperationException("Could not create custom field editor!");
			this.editor.Init(this.field);
		}

		public string ClientID
		{
			get { return this.clientId; }
		}
		public string FormName
		{
			get { return this.formName; }
		}

		public BXCustomField Field
		{
			get { return this.field; }
		}

		public string Caption
		{
			get
			{
				return HttpUtility.HtmlEncode(this.field.TextEncoder.Decode(this.field.EditFormLabel));
			}
		}
		public bool IsRequired
		{
			get { return this.field.Mandatory; }
		}
		public void Load(BXCustomPropertyCollection properties)
		{
			if (properties == null || properties.Count == 0)
				return;

			BXCustomProperty p = null;
			if (properties.TryGetValue(this.field.Name, out p))
				this.editor.Load(p);
		}
		public string Render()
		{
			return this.editor.Render(this.formName, this.clientId);
		}
		public void Save(BXCustomPropertyCollection properties, ICollection<string> errors)
		{
			this.editor.DoSave(this.formName, properties, errors);
		}
		public void Validate(ICollection<string> errors)
		{
			this.editor.DoValidate(this.formName, errors);
		}
	}

	public class BlogPostFormTemplate : BXComponentTemplate<BlogPostFormComponent>
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if ((Component.FatalError & BlogPostFormComponent.ErrorCode.Fatal) != 0)
				BXError404Manager.Set404Status(Response);
			else if (Page.Form != null && string.IsNullOrEmpty(Page.Form.Enctype))
				Page.Form.Enctype = "multipart/form-data";   
		}
		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			base.Render(writer);
		}

		// compatibility
		public class CustomFieldEditor : BlogPostFormCustomFieldEditor
		{
			public CustomFieldEditor(BlogPostFormComponent parent, BXCustomField field)
				:base(parent, field)
			{
			}
		}
	}

	//---BlogPostFormImageHandler
	public class BlogPostFormImageHandler
	{
		public BlogPostFormImageHandler(BlogPostFormComponent component)
		{
			if (component == null)
				throw new ArgumentNullException("component");
			_component = component;
		}

		public HttpContext Context
		{
			get { return HttpContext.Current; }
		}
		private BlogPostFormComponent _component = null;
		public BlogPostFormComponent Component
		{
			get { return _component; }
		}

		#region Устаревший код (только для совместимости)
		private static string _thumbnailDirectoryVirtualPath = null;
		public string ThumbnailDirectoryVirtualPath
		{
			get
			{
				return _thumbnailDirectoryVirtualPath ?? (_thumbnailDirectoryVirtualPath = string.Concat(VirtualPathUtility.AppendTrailingSlash(BXBlogModuleConfiguration.ImageThumbnailDirectoryVirtualPath), !string.IsNullOrEmpty(BlogSlug) ? BlogSlug : BlogId.ToString(), "/"));
			}
		}
		private static string _thumbnailDirectoryPhysicalPath = null;
		public string ThumbnailDirectoryPhysicalPath
		{
			get
			{
				return _thumbnailDirectoryPhysicalPath ?? (_thumbnailDirectoryPhysicalPath = HostingEnvironment.MapPath(ThumbnailDirectoryVirtualPath));
			}
		}
		#endregion

		private BXPrincipal _currentPrincipal = null;
		public BXPrincipal CurrentPrincipal
		{
			get
			{
				return _currentPrincipal ?? (_currentPrincipal = HttpContext.Current.User as BXPrincipal);
			}
		}
		private BXIdentity _currentIdentity = null;
		public BXIdentity CurrentIdentity
		{
			get
			{
				return _currentIdentity ?? (_currentIdentity = CurrentPrincipal != null ? CurrentPrincipal.Identity as BXIdentity : null);
			}
		}
		public BXUser CurrentUser
		{
			get
			{
				return CurrentIdentity != null ? CurrentIdentity.User : null;
			}
		}
		public int MaximalFileSizeInKBytes
		{
			get { return Component.MaximalFileSizeInKBytes; }
		}
		public int MaximalAllFilesSizeInKBytes
		{
			get { return Component.MaximalAllFilesSizeInKBytes; }
		}
		public int MaximalImageWidthInPixels
		{
			get { return Component.MaximalImageWidthInPixels; }
		}
		public int MaximalImageHeightInPixels
		{
			get { return Component.MaximalImageHeightInPixels; }
		}
		private string GetMessageRaw(string key)
		{
			return Component.GetMessageRaw(key);
		}
		private void GetFileNameAndExtension(string fileName, out string name, out string ext)
		{
			int index = !string.IsNullOrEmpty(fileName) ? fileName.LastIndexOf('.') : -1;
			if (index >= 0)
			{
				name = fileName.Substring(0, index);
				ext = fileName.Substring(index + 1);
			}
			else
				name = ext = string.Empty;
		}
		private string AddFileExtension(string path, string ext)
		{
			if (string.IsNullOrEmpty(path))
				return string.Empty;

			int dotInd = path.LastIndexOf('.');
			return dotInd >= 0 ? path.Substring(0, dotInd) + ext : path + ext;
		}

		private void GetThumbWidthAndHeight(out int width, out int height)
		{
			HttpRequest req = Context.Request;
			width = 100;
			if (!string.IsNullOrEmpty(req.Params["thumbWidth"]))
				try
				{
					width = Convert.ToInt32(req.Params["thumbWidth"]);
				}
				catch (Exception /*exc*/)
				{
				}

			height = 100;
			if (!string.IsNullOrEmpty(req.Params["thumbHeight"]))
				try
				{
					height = Convert.ToInt32(req.Params["thumbHeight"]);
				}
				catch (Exception /*exc*/)
				{
				}
		}
		private int? _blogId = null;
		public int BlogId
		{
			get
			{
				if (_blogId.HasValue)
					return _blogId.Value;

				BXBlog blog = _component.Blog;
				return (_blogId = blog != null ? blog.Id : 0).Value;

				//HttpRequest req = Context.Request;
				//if (!string.IsNullOrEmpty(req.Params["blogId"]))
				//{
				//    try
				//    {
				//        _blogId = Convert.ToInt32(req.Params["blogId"]);
				//    }
				//    catch (Exception /*exc*/)
				//    {
				//        _blogId = 0;
				//    }
				//}
				//else
				//    _blogId = 0;
				//return _blogId.Value;
			}
		}
		private string _blogSlug = null;
		public string BlogSlug
		{
			get
			{
				//return _blogSlug ?? (_blogSlug = Context.Request.Params["blogSlug"] ?? string.Empty);
				return _blogSlug ?? (_blogSlug = _component.BlogSlug);
			}
		}
		private int? _postId = null;
		public int PostId
		{
			get
			{
				return (_postId ?? (_postId = _component.PostId)).Value;
				//if (_postId.HasValue)
				//    return _postId.Value;

				//HttpRequest req = Context.Request;
				//if (!string.IsNullOrEmpty(req.Params["postId"]))
				//{
				//    try
				//    {
				//        _postId = Convert.ToInt32(req.Params["postId"]);
				//    }
				//    catch (Exception /*exc*/)
				//    {
				//        _postId = 0;
				//    }
				//}
				//else
				//    _postId = 0;
				//return _postId.Value;
			}
		}
		private int? _fileId = null;
		public int FileId
		{
			get
			{
				if (_fileId.HasValue)
					return _fileId.Value;

				HttpRequest req = Context.Request;
				if (!string.IsNullOrEmpty(req.Params["imgId"]))
				{
					try
					{
						_fileId = Convert.ToInt32(req.Params["imgId"]);
					}
					catch (Exception /*exc*/)
					{
						_fileId = 0;
					}
				}
				else
					_fileId = 0;
				return _fileId.Value;
			}
		}
		public void Process()
		{
			HttpRequest req = Context.Request;
			if (!string.Equals(req.HttpMethod, "POST", StringComparison.Ordinal))
				return;

			string action = req.Params["action"];
			if (string.IsNullOrEmpty(action))
				return;

			if (!(CurrentIdentity != null && CurrentIdentity.IsAuthenticated))
				return;

			action = action.ToUpperInvariant();
			if (string.Equals(action, "UPLOAD", StringComparison.InvariantCulture))
				HandleUpload();
			else if (string.Equals(action, "DELETE", StringComparison.InvariantCulture))
				HandleDelete();
			else if (string.Equals(action, "LIST", StringComparison.InvariantCulture))
				HandleList();
		}
		/// <summary>
		/// "Список"
		/// </summary>
		/// <param name="context"></param>
		private void HandleList()
		{
			if (!_component.Auth.CanEditThisPost)
			{
				Terminate((int)HttpStatusCode.Forbidden, "list", GetMessageRaw("Error.Forbidden"));
				return;
			}
			BXBlogPost2FileCollection blogFiles = null;
			try
			{
				BXFilter f = new BXFilter(new BXFilterItem(BXBlogPost2File.Fields.BlogId, BXSqlFilterOperators.Equal, BlogId));
				if (PostId <= 0)
				{
					f.Add(new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, null));
					//f.Add(new BXFilterItem(BXBlogPost2File.Fields.UserId, BXSqlFilterOperators.Equal, CurrentUser.UserId));                
				}
				else
				{
					f.Add(
						new BXFilterOr(
							new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, PostId),
							new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, null)
							)
						);
					/*f.Add(
						new BXFilterOr(
							new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, postId),
							new BXFilter(
								new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, null),
								new BXFilterItem(BXBlogPost2File.Fields.UserId, BXSqlFilterOperators.Equal, CurrentUser.UserId)
								)
							)
						);*/
				}

				blogFiles = BXBlogPost2File.GetList(
					f,
					new BXOrderBy(new BXOrderByPair(BXBlogPost2File.Fields.Id, BXOrderByDirection.Asc)),
					new BXSelect(BXSelectFieldPreparationMode.Normal,
						BXBlogPost2File.Fields.Id,
						BXBlogPost2File.Fields.File.FileNameOriginal,
						BXBlogPost2File.Fields.File.Folder,
						BXBlogPost2File.Fields.File.FileName),
					null,
					BXTextEncoder.EmptyTextEncoder
					);
			}
			catch
			{
				Terminate((int)HttpStatusCode.InternalServerError, "list", GetMessageRaw("Error.List.GeneralError"));
				return;
			}

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			int width, height;
			GetThumbWidthAndHeight(out width, out height);
			StringBuilder result = new StringBuilder();
			//EnsureThumbDirecrory();
			foreach (BXBlogPost2File blogFile in blogFiles)
			{
				BXFile file = blogFile.File;
				if (file == null)
					continue;

				string fileName = file.FileNameOriginal;
				string name, ext;
				GetFileNameAndExtension(fileName, out name, out ext);
				ext = ext.ToUpperInvariant();
				if (!(string.Equals(ext, "JPG", StringComparison.InvariantCulture)
					|| string.Equals(ext, "PNG", StringComparison.InvariantCulture)
					|| string.Equals(ext, "GIF", StringComparison.InvariantCulture)))
					continue;

                string thumbUrl = BXImageUtility.GetResizedImage(file, width, height).GetUri();

				if (result.Length != 0)
					result.Append(",");
                result.Append("{\"imgId\":\"").Append(blogFile.Id.ToString()).Append("\", \"imgUrl\":\"").Append(BXJSUtility.Encode(file.FilePath)).Append("\", \"thumbUrl\":\"").Append(BXJSUtility.Encode(thumbUrl)).Append("\"}");
			}

			if (result.Length > 0)
				parameters.Add("thumbs", result.Insert(0, "[").Append("]").ToString());

			WriteResult((int)HttpStatusCode.OK, "list", parameters);
			Context.Response.Flush();
			Context.ApplicationInstance.CompleteRequest();
			Context.Response.End();
		}
		/// <summary>
		/// "Удалить"
		/// </summary>
		/// <param name="context"></param>
		private void HandleDelete()
		{
			if (!_component.Auth.CanEditThisPost)
			{
				Terminate((int)HttpStatusCode.Forbidden, "delete", GetMessageRaw("Error.Forbidden"));
				return;
			}

			if (FileId <= 0)
			{
				Terminate((int)HttpStatusCode.InternalServerError, "delete", GetMessageRaw("Error.Delete.NotFound"));
				return;
			}

			BXBlogPost2FileCollection blogFiles = null;
			try
			{
				BXFilter f = new BXFilter(new BXFilterItem(BXBlogPost2File.Fields.Id, BXSqlFilterOperators.Equal, FileId),
					//new BXFilterItem(BXBlogPost2File.Fields.UserId, BXSqlFilterOperators.Equal, CurrentUser.UserId),
						new BXFilterItem(BXBlogPost2File.Fields.BlogId, BXSqlFilterOperators.Equal, BlogId));
				if (PostId > 0)
					f.Add(
						new BXFilterOr(
							new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, PostId),
							new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, null)
							)
						);

				blogFiles = BXBlogPost2File.GetList(
					f,
					new BXOrderBy(new BXOrderByPair(BXBlogPost2File.Fields.Id, BXOrderByDirection.Asc)),
					new BXSelect(BXSelectFieldPreparationMode.Normal,
						BXBlogPost2File.Fields.File.Id,
						BXBlogPost2File.Fields.File.Folder,
						BXBlogPost2File.Fields.File.FileName),
					null,
					BXTextEncoder.EmptyTextEncoder
					);
			}
			catch
			{
				Terminate((int)HttpStatusCode.InternalServerError, "delete", GetMessageRaw("Error.Delete.GeneralError"));
				return;
			}

			BXFile file = null;
			if (blogFiles.Count == 0 || (file = blogFiles[0].File) == null)
			{
				Terminate((int)HttpStatusCode.InternalServerError, "delete", GetMessageRaw("Error.Delete.NotFound"));
				return;
			}

			try
			{
                BXImageUtility.DeleteResizedImages(file);
                file.Delete();
				BXBlogPost2File.Delete(FileId);
			}
			catch
			{
				Terminate((int)HttpStatusCode.InternalServerError, "delete", GetMessageRaw("Error.Delete.CouldNotDelete"));
				return;
			}

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("imgUrl", string.Concat("\"", file.FilePath, "\""));
			WriteResult((int)HttpStatusCode.OK, "delete", parameters);
			Context.Response.Flush();
			Context.ApplicationInstance.CompleteRequest();
			Context.Response.End();
		}

		/// <summary>
		/// "Загрузка"
		/// </summary>
		/// <param name="context"></param>
		private void HandleUpload()
		{
			if (!_component.Auth.CanEditThisPost)
			{
				Terminate((int)HttpStatusCode.Forbidden, "upload", GetMessageRaw("Error.Forbidden"));
				return;
			}
			HttpRequest req = Context.Request;
			HttpFileCollection postFiles = req.Files;
			HttpPostedFile postFile = postFiles != null && postFiles.Count > 0 ? req.Files[0] : null;
			if (postFile != null)
			{
				string nameAndExt = postFile.FileName;
				int lastBackSlashInd = nameAndExt.LastIndexOf('\\'); //FOR IE
				if (lastBackSlashInd >= 0)
					nameAndExt = nameAndExt.Substring(lastBackSlashInd + 1);
				string name, ext;
				GetFileNameAndExtension(nameAndExt, out name, out ext);
				if (string.IsNullOrEmpty(ext))
				{
					Terminate((int)HttpStatusCode.InternalServerError, "upload", string.Format(GetMessageRaw("Error.Upload.NoExtension"), ext));
					return;
				}
				string extUpper = ext.ToUpperInvariant();
				if (!(string.Equals(extUpper, "JPG", StringComparison.Ordinal)
					|| string.Equals(extUpper, "JPEG", StringComparison.Ordinal)
					|| string.Equals(extUpper, "GIF", StringComparison.Ordinal)
					|| string.Equals(extUpper, "PNG", StringComparison.Ordinal)))
				{
					Terminate((int)HttpStatusCode.InternalServerError, "upload", string.Format(GetMessageRaw("Error.Upload.IsNotImageExtension"), ext));
					return;
				}
				int width, height;
				GetThumbWidthAndHeight(out width, out height);
				string fileMimeType = postFile.ContentType.ToUpperInvariant();
				if (
					!string.Equals(fileMimeType, "IMAGE/JPEG", StringComparison.Ordinal)
					&& !string.Equals(fileMimeType, "IMAGE/PJPEG", StringComparison.Ordinal) //FOR IE
					&& !string.Equals(fileMimeType, "IMAGE/JPG", StringComparison.Ordinal)
					&& !string.Equals(fileMimeType, "IMAGE/GIF", StringComparison.Ordinal)
					&& !string.Equals(fileMimeType, "IMAGE/PNG", StringComparison.Ordinal)
					&& !string.Equals(fileMimeType, "IMAGE/X-PNG", StringComparison.Ordinal) //FOR IE
				)
				{
					/*
					 * Нельзя из-за IE8: на код 500 он загрузит во фрейм данные из ресурса res://ieframe.dll/http_500.htm
					 * и доступ к данным станет невозможен из-за кроссдоменности
					 */
					//Terminate((int)HttpStatusCode.InternalServerError, "upload", string.Format(GetMessageRaw("Error.Upload.IsNotImage"), fileMimeType));
					Terminate((int)HttpStatusCode.OK, "upload", string.Format(GetMessageRaw("Error.Upload.IsNotImage"), fileMimeType));

					return;
				}
				if (MaximalFileSizeInKBytes > 0)
				{
					//проверка размера файла
					if (postFile.ContentLength > (MaximalFileSizeInKBytes * 1024))
					{
						//Terminate((int)HttpStatusCode.InternalServerError, "upload", string.Format(GetMessageRaw("Error.Upload.SizeLimitIsExceeded"), (postFile.ContentLength / 1024).ToString(), MaximalFileSizeInKBytes.ToString()));
						Terminate((int)HttpStatusCode.OK, "upload", string.Format(GetMessageRaw("Error.Upload.SizeLimitIsExceeded"), (postFile.ContentLength / 1024).ToString(), MaximalFileSizeInKBytes.ToString()));
						return;
					}
				}
				if (MaximalAllFilesSizeInKBytes > 0)
				{
					//проверка общего размера
					BXBlogPost2FileCollection presentFiles = null;
					try
					{
						BXFilter f = new BXFilter(new BXFilterItem(BXBlogPost2File.Fields.BlogId, BXSqlFilterOperators.Equal, BlogId));
						if (PostId <= 0)
							f.Add(new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, null));
						else
						{
							f.Add(
								new BXFilterOr(
									new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, PostId),
									new BXFilterItem(BXBlogPost2File.Fields.PostId, BXSqlFilterOperators.Equal, null)
									)
								);
						}

						presentFiles = BXBlogPost2File.GetList(
							f,
							new BXOrderBy(new BXOrderByPair(BXBlogPost2File.Fields.FileSize, BXOrderByDirection.Desc)),
							new BXSelect(BXSelectFieldPreparationMode.Normal, BXBlogPost2File.Fields.FileSize),
							null,
							BXTextEncoder.EmptyTextEncoder
							);
					}
					catch
					{
						//Terminate((int)HttpStatusCode.InternalServerError, "upload", GetMessageRaw("Error.Upload.GeneralError"));
						Terminate((int)HttpStatusCode.OK, "upload", GetMessageRaw("Error.Upload.GeneralError"));
						return;
					}

					if (presentFiles.Count > 0)
					{
						int limit = MaximalAllFilesSizeInKBytes * 1024,
							current = 0;
						foreach (BXBlogPost2File presentFile in presentFiles)
						{
							current += presentFile.FileSize;
							if (current >= limit)
							{
								//Terminate((int)HttpStatusCode.InternalServerError, "upload", string.Format(GetMessageRaw("Error.Upload.OverallSizeLimitIsExceeded"), (current / 1024).ToString(), MaximalAllFilesSizeInKBytes.ToString()));
								Terminate((int)HttpStatusCode.OK, "upload", string.Format(GetMessageRaw("Error.Upload.OverallSizeLimitIsExceeded"), (current / 1024).ToString(), MaximalAllFilesSizeInKBytes.ToString()));
								return;
							}
						}
						//current += postFile.ContentLength; //разрешаем
						//if (current > limit)
						//    Terminate((int)HttpStatusCode.InternalServerError, "upload", "ALL_FILES_SIZE_LIMIT_IS_EXCEEDED");
					}
				}

				string fileFolderRelativePath = string.Concat(BXBlogModuleConfiguration.ImageDirectoryName, "/", !string.IsNullOrEmpty(BlogSlug) ? BlogSlug : BlogId.ToString());
				BXFile file = new BXFile(postFile, fileFolderRelativePath, BXBlogModuleConfiguration.Id, string.Empty);
				//проверка ширины и высоты
				int fileWidth = file.Width, 
					fileHeight = file.Height;

				if ((MaximalImageWidthInPixels <= 0 || fileWidth <= MaximalImageWidthInPixels) && (MaximalImageHeightInPixels <= 0 || fileHeight <= MaximalImageHeightInPixels))
				{
					try
					{
						file.Create();
					}
					catch
					{
						//Terminate((int)HttpStatusCode.InternalServerError, "upload", GetMessageRaw("Error.Upload.CouldNotSave"));
						Terminate((int)HttpStatusCode.OK, "upload", GetMessageRaw("Error.Upload.CouldNotSave"));
						return;
					}
				}
				else
				{
					using (MemoryStream ms = new MemoryStream())
					{
						try
						{
							using (Image srcImage = Image.FromStream(postFile.InputStream))
								using (Image scaledImage = BXImageUtility.Resize(srcImage, MaximalImageWidthInPixels, MaximalImageHeightInPixels))
								{
									BXImageFormatInfo scaledImageFormat = PrepareImageFormat(scaledImage, srcImage.RawFormat);
									nameAndExt = scaledImageFormat.AppendFileExtension(nameAndExt);
									scaledImage.Save(ms, scaledImageFormat.Format);
								}
						}
						catch
						{
							//Terminate((int)HttpStatusCode.InternalServerError, "upload", GetMessageRaw("Error.Upload.CouldNotResize"));
							Terminate((int)HttpStatusCode.OK, "upload", GetMessageRaw("Error.Upload.CouldNotResize"));
							return;
						}

						file = new BXFile(ms, nameAndExt, fileFolderRelativePath, BXBlogModuleConfiguration.Id, string.Empty);
                        file.ContentType = postFile.ContentType;
						try
						{
							file.Create();
						}
						catch
						{
							//Terminate((int)HttpStatusCode.InternalServerError, "upload", GetMessageRaw("Error.Upload.CouldNotSave"));
							Terminate((int)HttpStatusCode.OK, "upload", GetMessageRaw("Error.Upload.CouldNotSave"));
							return;
						}
					}
				}

                string thumbUrl = string.Empty;
				//string thumbPath = GetThumbPath(file, width, height);
				//EnsureThumbDirecrory();
				try
				{
                    thumbUrl = BXImageUtility.GetResizedImage(file, width, height).GetUri();
                    //Directory.CreateDirectory(HostingEnvironment.MapPath(VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.GetDirectory(thumbPath))));
                    //using (Image image = Image.FromFile(HostingEnvironment.MapPath(file.FileVirtualPath)))
                    //    using (Image thumbImage = BXImageUtility.Resize(image, width, height))
                    //    {

                    //        BXImageFormatInfo thumbImageFormat = PrepareImageFormat(thumbImage, image.RawFormat);
                    //        thumbPath = thumbImageFormat.AppendFileExtension(thumbPath);
                    //        thumbImage.Save(HostingEnvironment.MapPath(thumbPath), thumbImageFormat.Format);
                    //    }
				}
				catch
				{
					try
					{
						file.Delete();
					}
					catch
					{
					}
					//Terminate((int)HttpStatusCode.InternalServerError, "upload", GetMessageRaw("Error.Upload.CouldNotResize"));
					Terminate((int)HttpStatusCode.OK, "upload", GetMessageRaw("Error.Upload.CouldNotResize"));
					return;
				}

				BXBlogPost2File blogFile = new BXBlogPost2File();
				blogFile.Id = file.Id;
				blogFile.UserId = CurrentUser.UserId;
				blogFile.BlogId = BlogId;
				try
				{
					blogFile.Create();
				}
				catch
				{
					try
					{
						file.Delete();
					}
					catch
					{
					}
					//Terminate((int)HttpStatusCode.InternalServerError, "upload", GetMessageRaw("Error.Upload.CouldNotSave"));
					Terminate((int)HttpStatusCode.OK, "upload", GetMessageRaw("Error.Upload.CouldNotSave"));
				}

				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters.Add("imgId", string.Concat("\"", blogFile.Id.ToString(), "\""));
				parameters.Add("imgUrl", string.Concat("\"", file.TextEncoder.Decode(file.FilePath), "\""));
                parameters.Add("thumbUrl", string.Concat("\"", thumbUrl, "\""));
				WriteResult((int)HttpStatusCode.OK, "upload", parameters);
				Context.Response.Flush();
				Context.ApplicationInstance.CompleteRequest();
				Context.Response.End();
			}
		}

		private BXImageFormatInfo PrepareImageFormat(Image srcImage, ImageFormat srcFormat)
		{
			if (srcImage == null)
				throw new ArgumentNullException("srcImage");

			ImageFormat f = srcFormat != null ? srcFormat : srcImage.RawFormat;
			if (f == null)
				throw new InvalidOperationException("Could not find image format");
			/*
			 * GIF в PNG всегда - из-за возможной потери качества при преобразовании в 256 цветов
			 * JPG в PNG если площадь изображения не превышает 128x128
			 */
			if (f.Guid == ImageFormat.Gif.Guid || (f.Guid == ImageFormat.Jpeg.Guid && srcImage.Width * srcImage.Height <= 16384))
				return new BXImageFormatInfo(ImageFormat.Png);
			else
				return new BXImageFormatInfo(f);
		}

		private void WriteResult(int statusCode, string action, Dictionary<string, string> parameters)
		{
			HttpResponse res = Context.Response;
			res.ContentType = "text/html";
			res.StatusCode = statusCode;

			StringBuilder result = new StringBuilder();
			result.Append("{");
			result.Append("\"action\":\"" + action + "\"");
			result.Append(",\"params\":{");
			System.Text.StringBuilder parametersResult = new System.Text.StringBuilder();
			foreach (System.Collections.Generic.KeyValuePair<string, string> kv in parameters)
			{
				if (parametersResult.Length > 0)
					parametersResult.Append(",");
				parametersResult.Append("\"").Append(kv.Key).Append("\":").Append(kv.Value);
			}
			result.Append(parametersResult.ToString());
			result.Append("}");
			result.Append("}");
			res.Write(string.Concat("<html><head></head><body><input id=\"r\" type=\"hidden\" value=\"" + HttpUtility.HtmlAttributeEncode(result.ToString()) + "\"></body></html>"));
		}

		private void Terminate(int statusCode, string action, string text)
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();
			parameters.Add("error", string.Concat("\"", text, "\""));
			WriteResult(statusCode, action, parameters);
			Context.Response.Flush();
			Context.ApplicationInstance.CompleteRequest();
			Context.Response.End();
		}
	}
	//---
}


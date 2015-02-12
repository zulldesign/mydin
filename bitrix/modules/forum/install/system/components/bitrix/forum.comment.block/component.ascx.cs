using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using System.Collections.ObjectModel;
using Bitrix.DataLayer;
using Bitrix.Security;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.CommunicationUtility;
using Bitrix.Services.Text;
using System.Text.RegularExpressions;
using Bitrix.Services;
using System.Threading;
using System.Xml;
using System.Collections;

namespace Bitrix.Forum.Components
{
    /// <summary>
    /// Компонент "Блок комментариев на основе сообщений форума"
    /// </summary>
    public partial class ForumCommentBlockComponent : BXComponent
    {
		bool cssFromBlog;
		bool colorCssFromBlog;
        public delegate void ForumTopicCreatedHandler(BXComponent sender, BXForumTopic topic);

        protected string GetParameterKey(ForumCommentBlockParameter parameter)
        {
            return parameter.ToString("G");
        }

        protected string GetParameterTitle(ForumCommentBlockParameter parameter)
        {
            return GetMessageRaw(string.Concat("Param.", parameter.ToString("G")));
        }

        public string GetComponentErrorMessage(ForumCommentBlockError error)
        {
            return error != ForumCommentBlockError.None ? GetMessageRaw(string.Concat("Error.", error.ToString("G"))) : string.Empty;
        }
        public string[] GetComponentErrorMessages()
        {
            if (this.error == ForumCommentBlockError.None)
                return new string[0];

            List<string> result = new List<string>();
            if ((this.error & ForumCommentBlockError.ForumTopicNotDefined) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.ForumTopicNotDefined));
            if ((this.error & ForumCommentBlockError.ForumTopicNotFound) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.ForumTopicNotFound));
            if ((this.error & ForumCommentBlockError.ForumNotFound) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.ForumNotFound));
            if ((this.error & ForumCommentBlockError.NoReadingPermission) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.NoReadingPermission));
            if ((this.error & ForumCommentBlockError.InvalidPage) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.InvalidPage));
            if ((this.error & ForumCommentBlockError.NoPostApprovementPermission) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.NoPostApprovementPermission));
            if ((this.error & ForumCommentBlockError.NoPostDeletionPermission) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.NoPostDeletionPermission));
            if ((this.error & ForumCommentBlockError.PostSavingFailded) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.PostSavingFailded));
            if ((this.error & ForumCommentBlockError.PostDeletionFailded) != 0)
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.PostDeletionFailded));
			if ((this.error & ForumCommentBlockError.ForumNotDefined) != 0)
				result.Add(GetComponentErrorMessage(ForumCommentBlockError.ForumNotDefined));
            if ((this.error & ForumCommentBlockError.General) != 0)
            {
                result.Add(GetComponentErrorMessage(ForumCommentBlockError.General));
                if(this.exception != null)
                    result.Add(this.exception.Message);
            }
            return result.ToArray();
        }

        private BXParamsBag<object> replaceParams = null;
        internal BXParamsBag<object> InternalReplaceParams
        {
            get { return this.replaceParams ?? (this.replaceParams = new BXParamsBag<object>()); }
        }

		private BXForumPostChain postChain = null;
		internal BXForumPostChain PostChain
        {
            get { return this.postChain; }
        }

        private BXForumSignatureChain signChain = null;
        internal BXForumSignatureChain SignChain
        {
            get { return this.signChain; }
        }

        private List<PostItem> posts = null;
        private BXForumPost FindPost(long postId)
        {
            if (postId <= 0 || this.posts == null || this.posts.Count == 0) return null;
            int index = this.posts.FindIndex(
                delegate(PostItem item) { return item.PostId == postId; }
                );
            return index >= 0 ? this.posts[index].Post : null;
        }

		protected void DeleteAllComments()
		{
			if ( TopicId > 0 )
				BXForumTopic.Delete(TopicId);
		}

		public void DeleteCommentsForEntityList()
		{
			var entityList = Parameters.GetListInt("EntityList");
			if (entityList == null || entityList.Count == 0 || BXForumTopic.Fields.CustomFields == null || BXForumTopic.Fields.CustomFields.DefaultFields == null ||
					BXForumTopic.Fields.CustomFields.DefaultFields.GetFieldByKey(identityPropertyName) == null)
				return;
			var topicCol = BXForumTopic.GetList(new BXFilter(
				new BXFilterItem(BXForumTopic.Fields.CustomFields.DefaultFields.GetFieldByKey(identityPropertyName), BXSqlFilterOperators.In, 
					entityList)),null);
			foreach (var topic in topicCol)
				try
				{
					topic.Delete();
				}
				catch { }
		}

		void ProcessNode(ForumPostTreeInfo node,ref int position)
		{

			node.Position = ++position;

			foreach (ForumPostTreeInfo info in node.Children)
				ProcessNode(info,ref position);
		}

        PostOperationInfo operationInfo = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //без кеширования
            CacheMode = BXCacheMode.None;



            this.operationInfo = new PostOperationInfo(this);

            //не определён Ид темы, но определён Ид форума
            //делаем попытку найти ранее созданную тему по идентифицирующему пользовательскому свойству
            if (TopicId <= 0 && ForumId > 0 && !string.IsNullOrEmpty(IdentityPropertyName) && !string.IsNullOrEmpty(IdentityPropertyValue))
            {
                string customPropertyName = IdentityPropertyName;
                BXCustomType customPropertyType = IdentityPropertyType;
                object customPropertyValue;
                if (customPropertyType == null || !customPropertyType.TryParseValue(IdentityPropertyValue, out customPropertyValue))
                    customPropertyValue = IdentityPropertyValue;

                BXSchemeFieldBase customPropertyField = BXForumTopic.Fields.CustomFields != null && BXForumTopic.Fields.CustomFields.DefaultFields != null ?
                    BXForumTopic.Fields.CustomFields.DefaultFields.GetFieldByKey(customPropertyName) : null;

                if (customPropertyField != null)
                {
                    BXForumTopicCollection topicCol = BXForumTopic.GetList(
                    new BXFilter(
                        new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId),
                        new BXFilterItem(BXForumTopic.Fields.Forum.Active, BXSqlFilterOperators.Equal, true),
                        new BXFilterItem(BXForumTopic.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite),
                        new BXFilterItem(customPropertyField, BXSqlFilterOperators.Equal, customPropertyValue)
                        ),
                        null,
                        new BXSelectAdd(
                            BXForumTopic.Fields.Forum,
                            BXForumTopic.Fields.FirstPost
                            ),
                        null
                        );

                    if (topicCol.Count > 0)
                        this.topic = topicCol[0];
                }
            }

            if (TopicId > 0)
            {
                //задан Ид темы форума
                if (this.topic == null)
                {
                    BXForumTopicCollection topicCol = BXForumTopic.GetList(
                        new BXFilter(
                            new BXFilterItem(BXForumTopic.Fields.Id, BXSqlFilterOperators.Equal, TopicId),
                            new BXFilterItem(BXForumTopic.Fields.Forum.Active, BXSqlFilterOperators.Equal, true),
                            new BXFilterItem(BXForumTopic.Fields.Forum.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
                            ),
                            null,
                            new BXSelectAdd(
                                BXForumTopic.Fields.Forum,
                                BXForumTopic.Fields.FirstPost
                                ),
                            null
                            );

                    if (topicCol.Count > 0)
                        this.topic = topicCol[0];
                    else
                    {
                        FailLoading(ForumCommentBlockError.ForumTopicNotFound);
                        return;
                    }
                }

                if (this.topic.Forum == null)
                {
                    FailLoading(ForumCommentBlockError.ForumNotFound);
                    return;
                }

                this.forum = this.topic.Forum;
                this.firstPost = this.topic.FirstPost;
            }
            else if (ForumId > 0)
            {
                //задан Ид темы форума
                BXForumCollection forumCol = BXForum.GetList(
                    new BXFilter(
                        new BXFilterItem(BXForum.Fields.Id, BXSqlFilterOperators.Equal, ForumId),
                        new BXFilterItem(BXForum.Fields.Active, BXSqlFilterOperators.Equal, true),
                        new BXFilterItem(BXForum.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite)
                        ),
                    null,
                    null,
                    null
                    );
                if (forumCol.Count == 0)
                {
                    FailLoading(ForumCommentBlockError.ForumNotFound);
                    return;
                }

                this.forum = forumCol[0];
                this.topic = null;
                this.firstPost = null;
            }
            else
            {
                //не задано ни Ид форума, ни Ид темы
                FailLoading(ForumCommentBlockError.ForumNotDefined);
                return;                
            }

            this.auth = new BXForumAuthorization(this.forum.Id, UserId, this.topic);
            if (!this.auth.CanRead)
            {
                FailLoading(ForumCommentBlockError.NoReadingPermission);
                return;
            }

            this.postChain = new BXForumPostChain(this.forum, BXForumPostChainStyle.Blog);
            this.postChain.MaxWordLength = MaxWordLength;
            this.postChain.EnableImages = true;

            this.signChain = new BXForumSignatureChain();
            this.signChain.MaxWordLength = MaxWordLength;

            if (this.topic != null)
            {
                //определение редактируемого сообщения
                if (PostId > 0)
                {
                    BXFilter postFilter = new BXFilter(
                        new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, PostId),
                        new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, this.forum.Id),
                        new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, this.topic.Id)
                        );
                    if (!this.auth.CanApprove)
                        postFilter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

                    BXForumPostCollection postCol = BXForumPost.GetList(
                         postFilter,
                         null,
                         new BXSelectAdd(
                             BXForumPost.Fields.Topic,
                             BXForumPost.Fields.Forum,
                             BXForumPost.Fields.Author,
                             BXForumPost.Fields.Author.User,
                             BXForumPost.Fields.Author.User.Image
                             ),
                         null
                         );

                    if (postCol.Count > 0)
                    {
                        this.post = postCol[0];
                        PostData data = ComponentPostData;
                        data.SetPostContent(this.post.Post, false);
                        data.GuestName = this.post.AuthorName;
                        data.GuestEmail = this.post.AuthorEmail;
                        data.IsApproved = this.post.Approved;
                        data.ParentPostId = this.post.ParentPostId;
                    }
                }

					//выбираем сообщения в теме
					BXFilter filter = new BXFilter(
						new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, this.forum.Id),
						new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, this.topic.Id)
						);

					if (/*!ShowFirstPost &&*/ this.topic.FirstPostId > 0)
						filter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.NotEqual, this.topic.FirstPostId));

					if (!this.auth.CanApprove && !UseTreeComments)
						filter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

					if (UseTreeComments)
						filter.Add(new BXFilterItem(BXForumPost.Fields.ParentPostId, BXSqlFilterOperators.Equal, 0));

					int count = BXForumPost.Count(filter);

					
				BXPagingParams pagingParams = PreparePagingParams();
					if (this.post != null)
					{
						BXFilter postIndexFilter = new BXFilter();
						if (!UseTreeComments)
						{
							postIndexFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Less, this.post.Id));
						}
						else // если используем древовидные комменты, то постраничка идет по корневым элементам
						{
							long rootPostId = post.ParentPostId==0 ? post.Id : BXForumPost.GetRootPostId(post.Id);
							postIndexFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Less, rootPostId));
						}
						postIndexFilter.Add(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, this.forum.Id));
						postIndexFilter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, this.topic.Id));

						if (/*!ShowFirstPost &&*/ this.topic.FirstPostId > 0)
							postIndexFilter.Add(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.NotEqual, this.topic.FirstPostId));

						if (!this.auth.CanApprove && !UseTreeComments)
							postIndexFilter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));
						if (UseTreeComments)
							postIndexFilter.Add(new BXFilterItem(BXForumPost.Fields.ParentPostId, BXSqlFilterOperators.Equal, 0));
						int postIndex = BXForumPost.Count(postIndexFilter);

						BXPagingHelper helper = ResolvePagingHelper(count, pagingParams);
						pagingParams.Page = helper.GetOuterIndex(helper.GetPageIndexForItem(postIndex));
					}

					InternalReplaceParams["ForumId"] = this.forum.Id;
					InternalReplaceParams["TopicId"] = this.topic.Id;

					bool isLegal;
					BXQueryParams queryParams = PreparePaging(
						pagingParams,
						delegate { return BXForumPost.Count(filter); },
						InternalReplaceParams,
						out isLegal
					);

					if (!isLegal)
					{
						FailLoading(ForumCommentBlockError.InvalidPage);
						return;
					}
				BXForumPostCollection col = new BXForumPostCollection();
				int postNumber = 0;
				this.posts = new List<PostItem>();
				Dictionary<long,int> postIds = new Dictionary<long,int>();
				if (!UseTreeComments){
					col = BXForumPost.GetList(
						 filter,
						 new BXOrderBy(new BXOrderByPair(BXForumPost.Fields.Id, BXOrderByDirection.Asc)),
						 new BXSelectAdd(
						//BXForumPost.Fields.Topic,
							 BXForumPost.Fields.Author,
							 BXForumPost.Fields.Author.User,
							 BXForumPost.Fields.Author.User.Image
							 ),
						 queryParams
						 );

					postNumber = queryParams != null && queryParams.AllowPaging ? queryParams.PagingStartIndex : 0;
				}
				else // если включены древовидные комментарии
				{
					BXFilter countFilter = new BXFilter(new BXFilterItem(BXForumPost.Fields.Forum.Id,BXSqlFilterOperators.Equal,this.forum.Id),
														new BXFilterItem(BXForumPost.Fields.Topic.Id,BXSqlFilterOperators.Equal,this.topic.Id),
														new BXFilterItem(BXForumPost.Fields.Id,BXSqlFilterOperators.NotEqual,this.topic.FirstPostId),
														new BXFilterItem(BXForumPost.Fields.ParentPostId,BXSqlFilterOperators.Equal,0));
					//if (!this.auth.CanApprove)
						//countFilter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

					Dictionary<long,ForumPostTreeInfo> postDic = 
						BXForumPost.GetCommentsTree(this.topic.Id, this.topic.FirstPostId, 0,true,
						queryParams.PagingStartIndex+1,queryParams.PagingStartIndex+queryParams.PagingRecordCount);

					List<ForumPostTreeInfo> postList = new List<ForumPostTreeInfo>();
					foreach (ForumPostTreeInfo info in postDic.Values)
						postList.Add(info);
					//нужно отсортировать пришедшие комментарии правильно для вывода
					foreach (ForumPostTreeInfo info in postList)
					{
						postIds.Add(info.PostId,info.Depth);
						info.Children.AddRange( postList.FindAll(x=>x.ParentPostId==info.PostId ));
					}
					int position = 0;
					foreach (ForumPostTreeInfo root in postList.FindAll(x => x.ParentPostId == 0))
						ProcessNode(root,ref position);



					col = BXForumPost.GetList(new BXFilter(new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.In, postIds.Keys)), null, new BXSelectAdd(
							 BXForumPost.Fields.Author,
							 BXForumPost.Fields.Author.User,
							 BXForumPost.Fields.Author.User.Image
							 ),null);

					col.Sort(delegate(BXForumPost a, BXForumPost b)
					{
						return postDic[a.Id].Position.CompareTo(postDic[b.Id].Position);
					});
				}
               
                foreach (BXForumPost curPost in col)
                {
                    PostItem item = new PostItem(curPost, this, operationInfo, ++postNumber);
					if (UseTreeComments)
						item.Level = postIds.ContainsKey(curPost.Id) ? postIds[curPost.Id] : 0;
                    Controls.Add(item);
                    this.posts.Add(item);
                }

                if (this.operationInfo.HasOperation())
                    try
                    {
                        this.operationInfo.Execute();
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception ex)
                    {
                        this.exception = ex;
                        this.error = ForumCommentBlockError.General;
                        return;
                    }
            }
            IncludeComponentTemplate();
        }

        private bool isTempalteIncluded = false;
        private void FailLoading(ForumCommentBlockError error) 
        {
            this.error |= error;
            if (!this.isTempalteIncluded)
            {
                IncludeComponentTemplate();
                this.isTempalteIncluded = true;
            }
        }

        public bool IsAuthentificated
        {
            get { return BXPrincipal.Current != null && BXPrincipal.Current.Identity != null && BXPrincipal.Current.Identity.IsAuthenticated; }
        }

		public bool ShowPostForm
		{
			get { return Parameters.GetBool("ShowPostForm", false); }
		}

        private BXIdentity identity = null;
        public BXIdentity Identity
        {
            get 
            {
                if (this.identity != null)
                    return this.identity;

                if (BXPrincipal.Current == null)
                    return null;

                return this.identity = (BXIdentity)BXPrincipal.Current.Identity;
            }
        }

        public BXUser User
        {
            get { return Identity != null ? Identity.User : null; }
        }

        public int UserId
        {
            get { return Identity != null && Identity.User != null ? Identity.User.UserId : 0; }
        }

        private string userName = null;
        public string UserName
        {
            get 
            {
                if (this.userName != null)
                    return this.userName;

                BXUser current = User;
                return (this.userName = current != null ? current.TextEncoder.Decode(current.GetDisplayName()) : string.Empty);
            }
        }

        private BXForumAuthorization auth = null;
        public BXForumAuthorization Authorization
        {
            get { return this.auth; }
        }

        private int? topicId = null;
        /// <summary>
        /// Ид темы форума
        /// </summary>
        public int TopicId
        {
            get { return this.topic != null ? this.topic.Id : this.topicId ?? Parameters.GetInt(GetParameterKey(ForumCommentBlockParameter.ForumTopicId), 0); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.ForumTopicId)] = (this.topicId = value > 0 ? value : 0).Value.ToString(); }
        }

        /// <summary>
        /// Имя темы форума
        /// </summary>
        public string TopicName
        {
            get { return this.topic != null ? this.topic.Name : Parameters.Get(GetParameterKey(ForumCommentBlockParameter.ForumTopicName), string.Empty); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.ForumTopicName)] = value ?? string.Empty; }
        }


        private BXForumTopic topic = null;
        /// <summary>
        /// Тема форума
        /// </summary>
        public BXForumTopic Topic
        {
            get { return this.topic; }
        }

        /// <summary>
        /// Ид форума
        /// </summary>
        public int ForumId
        {
            get { return this.forum != null ? this.forum.Id : Parameters.GetInt(GetParameterKey(ForumCommentBlockParameter.ForumId), 0); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.ForumId)] = (value > 0 ? value : 0).ToString(); }
        }

        private BXForum forum = null;
        /// <summary>
        /// Форум
        /// </summary>
        public BXForum Forum
        {
            get { return this.forum; }
        }

        private long? postId = null;
        /// <summary>
        /// Ид редактируемого сообщения
        /// </summary>
        public long PostId
        {
            get { return this.post != null ? this.post.Id : this.postId ?? (this.postId = Parameters.GetLong(GetParameterKey(ForumCommentBlockParameter.PostId), 0L)).Value; }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.PostId)] = (this.postId = value > 0L ? value : 0L).Value.ToString(); }
        }

        private BXForumPost post = null;
        /// <summary>
        /// Редактируемый пост
        /// </summary>
        public BXForumPost Post
        {
            get { return this.post; }
        }

        private PostOperation? operation = null;
        /// <summary>
        /// Операция
        /// </summary>
        public PostOperation Operation
        {
            get { return (operation ?? (operation = Parameters.GetEnum<PostOperation>(GetParameterKey(ForumCommentBlockParameter.PostOperation), PostOperation.Add))).Value; }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.PostOperation)] = (operation = value).Value.ToString(); }
        }

        private int? maxWordLength = null;
        /// <summary>
        /// Максимальная длина слова
        /// </summary>
        public int MaxWordLength
        {
            get { return (maxWordLength ?? (maxWordLength = Math.Max(0, Parameters.GetInt(GetParameterKey(ForumCommentBlockParameter.MaxWordLength), 15)))).Value; }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.MaxWordLength)] = (maxWordLength = (value > 0 ? value : 0)).ToString(); }
        }

        public string UserProfileUrlTemplate
        {
            get { return Parameters.GetString(GetParameterKey(ForumCommentBlockParameter.UserProfileUrlTemplate)); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.UserProfileUrlTemplate)] = value; }
        }

		public bool UseTreeComments
		{
			get
			{
				return Parameters.GetBool("UseTreeComments", false);
			}
		}

        public string PostOperationUrlTemplate
        {
            get { return Parameters.GetString(GetParameterKey(ForumCommentBlockParameter.PostOperationUrlTemplate)); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.PostOperationUrlTemplate)] = value; }
        }

        private string redirectUrl = null;
        public string RedirectUrl
        {
            get { return this.redirectUrl ?? (this.redirectUrl = Parameters.GetString(GetParameterKey(ForumCommentBlockParameter.RedirectUrl), string.Empty).Trim()); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.RedirectUrl)] = this.redirectUrl = value != null ? value.Trim() : string.Empty; }
        }

        private string postReadUrlTemplate = null;
        public string PostReadUrlTemplate
        {
            get 
            {
                if (this.postReadUrlTemplate != null)
                    return this.postReadUrlTemplate;

                this.postReadUrlTemplate = Parameters.GetString(GetParameterKey(ForumCommentBlockParameter.PostReadUrlTemplate));
                if (this.postReadUrlTemplate.IndexOf("##") < 0)
                    this.postReadUrlTemplate = string.Concat(this.postReadUrlTemplate, PostItem.PostBookmarkTemplate);

                return this.postReadUrlTemplate; 
            }
            set 
            { 
                Parameters[GetParameterKey(ForumCommentBlockParameter.PostReadUrlTemplate)] = value;
                this.postReadUrl = null;
            }
        }

        private string postReadUrl = null;
        public string PostReadUrl
        {
            get
            {
                if (this.postReadUrl != null)
                    return this.postReadUrl;

                string t = PostReadUrlTemplate;
                if (string.IsNullOrEmpty(t))
                    return this.postReadUrl = string.Empty;

                InternalReplaceParams["PostId"] = PostId;
                InternalReplaceParams["CommentId"] = PostId;
                this.postReadUrl = ResolveTemplateUrl(t);
                InternalReplaceParams.Remove("PostId");
                InternalReplaceParams.Remove("CommentId");
                return this.postReadUrl;
            }
        }

        internal string ResolveTemplateUrl(string template)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            string url = ResolveTemplateUrl(template, InternalReplaceParams);
            if (url.StartsWith("?", StringComparison.Ordinal))
            {
                UriBuilder uri = new UriBuilder(BXSefUrlManager.CurrentUrl);
                uri.Query = url.Substring(1);
                url = uri.Uri.ToString();
            }
            return url;
        }

        private ReadOnlyCollection<PostItem> postsRO = null;
        /// <summary>
        /// Сообщения - "комментарии"
        /// </summary>
        public IList<PostItem> Posts
        {
            get 
            {
                if (this.postsRO != null)
                    return this.postsRO;

                return (this.postsRO = this.posts != null ? new ReadOnlyCollection<PostItem>(this.posts) : new ReadOnlyCollection<PostItem>(new PostItem[0]));
            }
        }

        public int PostCount
        {
            get { return this.posts != null ? this.posts.Count : 0; }
        }

        public bool CanViewIP
        {
            get { return this.auth != null && this.auth.CanViewIP; }
        }

        public bool CanReplyToTopic
        {
            get { return this.auth != null && this.auth.CanTopicReply; }
        }

        public bool CanOpenCloseTopic
        {
            get { return this.auth != null && this.auth.CanOpenCloseThisTopic; }
        }

        public bool CanApprove
        {
            get { return this.auth != null && this.auth.CanApprove; }
        }

        public bool IsBbCodeAllowed
        {
            get { return Forum != null && Forum.AllowBBCode; }
        }

        //private string topicReplyHref = null;
        //public string TopicReplyHref
        //{
        //    get { return this.topicReplyHref ?? (this.topicReplyHref = !string.IsNullOrEmpty(TopicReplyUrlTemplate) ? ResolveTemplateUrl(TopicReplyUrlTemplate, InternalReplaceParams) : string.Empty); }
        //}

        private int? topicAuthorId = null;
        public int TopicAuthorId
        {
            get { return this.topic != null ? this.topic.AuthorId : this.topicAuthorId ?? (this.topicAuthorId = Parameters.GetInt(GetParameterKey(ForumCommentBlockParameter.ForumTopicAuthorId), 0)).Value; }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.ForumTopicAuthorId)] = (this.topicAuthorId = value > 0 ? value : 0).Value.ToString(); }
        }

        private string topicAuthorName = null;
        public string TopicAuthorName
        {
            get 
            {
                if (this.topic != null)
                    return this.topic.AuthorName;

                if (this.topicAuthorName != null)
                    return this.topicAuthorName;

                BXUser user = null;

                if (TopicAuthorId > 0 && (user = BXUser.GetById(TopicAuthorId, BXTextEncoder.EmptyTextEncoder)) != null)
                    return (this.topicAuthorName = user.GetDisplayName());

                return (this.topicAuthorName = string.Empty);
            }
        }

        private BXForumPost firstPost = null;
        public BXForumPost TopicFirstPost
        {
            get { return this.firstPost; }
        }

        public long TopicFirstPostId
        {
            get { return this.firstPost != null ? this.firstPost.Id : 0L; }
        }

        /// <summary>
        /// Содержание первого сообщения темы
        /// </summary>
        public string TopicFirstPostText
        {
            get { return this.firstPost != null ? this.firstPost.Post : Parameters.Get(GetParameterKey(ForumCommentBlockParameter.FirstPostText), string.Empty); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.FirstPostText)] = value ?? string.Empty; }
        }

        public bool IsTopicClosed
        {
            get { return Topic != null ? Topic.Closed : false; }
        }

        private string identityPropertyName = null;
        public string IdentityPropertyName
        {
            get { return this.identityPropertyName ?? (this.identityPropertyName = Parameters.GetString(GetParameterKey(ForumCommentBlockParameter.IdentityPropertyName))); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.IdentityPropertyName)] = this.identityPropertyName = value ?? string.Empty; }
        }

        public string identityPropertyTypeName = null;
        public string IdentityPropertyTypeName
        {
            get { return identityPropertyTypeName ?? (this.identityPropertyTypeName =  Parameters.GetString(GetParameterKey(ForumCommentBlockParameter.IdentityPropertyTypeName))); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.IdentityPropertyTypeName)] = this.identityPropertyTypeName = value ?? string.Empty; }
        }

        public BXCustomType IdentityPropertyType
        {
            get { return !string.IsNullOrEmpty(IdentityPropertyTypeName) ? BXCustomTypeManager.GetCustomType(IdentityPropertyTypeName) : null; }
        }

        private string identityPropertyValue = null;
        public string IdentityPropertyValue
        {
            get { return this.identityPropertyValue ?? (this.identityPropertyValue = Parameters.GetString(GetParameterKey(ForumCommentBlockParameter.IdentityPropertyValue))); }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.IdentityPropertyValue)] = this.identityPropertyValue = value ?? string.Empty; }
        }

        private bool? showGuestEmail = null;
        public bool ShowGuestEmail
        {
            get { return (this.showGuestEmail ?? (this.showGuestEmail = Parameters.GetBool(GetParameterKey(ForumCommentBlockParameter.ShowGuestEmail), true))).Value; }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.ShowGuestEmail)] = (this.showGuestEmail = value).ToString(); }
        }

        private bool? requireGuestEmail = null;
        public bool RequireGuestEmail
        {
            get { return (this.requireGuestEmail ?? (this.requireGuestEmail = Parameters.GetBool(GetParameterKey(ForumCommentBlockParameter.RequireGuestEmail), true))).Value; }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.RequireGuestEmail)] = (this.requireGuestEmail = value).ToString(); }
        }

        private bool? showGuestCaptcha = null;
        public bool ShowGuestCaptcha
        {
            get { return (this.showGuestCaptcha ?? (this.showGuestCaptcha = Parameters.GetBool(GetParameterKey(ForumCommentBlockParameter.ShowGuestCaptcha), true))).Value; }
            set { Parameters[GetParameterKey(ForumCommentBlockParameter.ShowGuestCaptcha)] = (this.showGuestCaptcha = value).ToString(); }
        }

        private BXCaptchaEngine captcha;
        private BXCaptchaEngine InnerCaptcha
        {
            get
            {
                if (this.captcha != null)
                    return this.captcha;

                captcha = BXCaptchaEngine.Create();
                captcha.MaxTimeout = 1800;

                return captcha;
            }
        }

        private string captchaGuid = null;
        public string CaptchaGuid 
        {
            get { return this.captchaGuid ?? (this.captchaGuid = InnerCaptcha.Id); }
        }

        private string captchaHref = null;
        public string CaptchaHref
        {
            get { return this.captchaHref ?? (this.captchaHref = InnerCaptcha.Store()); }
        }

        private PostData postData = null;
        public PostData ComponentPostData
        {
            get { return this.postData ?? (this.postData = new PostData(this)); }
        }


        private ForumTopicCreatedHandler topicCreatedHandler = null;
        public ForumTopicCreatedHandler TopicCreatedHandler
        {
            get { return this.topicCreatedHandler; }
            set { this.topicCreatedHandler = value; }
        }

        private ForumCommentBlockError error = ForumCommentBlockError.None;
        /// <summary>
        /// Ошибка
        /// </summary>
        public ForumCommentBlockError ComponentError
        {
            get { return this.error; }
        }

        private Exception exception = null;
        public Exception ComponentException
        {
            get { return this.exception; }
        }

        private List<string> postDataValidationErrors;
        private IList<string> InnerPostDataValidationErrors
        {
            get { return this.postDataValidationErrors ?? (this.postDataValidationErrors = new List<string>());}
        }

        private ReadOnlyCollection<string> postDataValidationErrorsRO;
        public IList<string> ComponentPostDataValidationErrors
        {
            get { return this.postDataValidationErrorsRO ?? (this.postDataValidationErrorsRO = new ReadOnlyCollection<string>(InnerPostDataValidationErrors)); }
        }

        public bool TrySavePostData()
        {
            if (ComponentError != ForumCommentBlockError.None)
                return false;

            try
            {
                PostData data = ComponentPostData;
                IList<string> errors = InnerPostDataValidationErrors;
                if(errors.Count > 0)
                    errors.Clear();

                if (!IsAuthentificated)
                {
                    if (data.GuestName.Length == 0)
                        errors.Add(GetMessage("Error.NameRequired"));

                    if (RequireGuestEmail)
                    {
                        if (data.GuestEmail.Length == 0)
                            errors.Add(GetMessage("Error.EmailRequired"));
                        else if (!data.IsGuestEmailValid())
                            errors.Add(GetMessage("Error.EmailInvalid"));
                    }

                    if (ShowGuestCaptcha)
                    {
                        captcha = BXCaptchaEngine.Get(data.GuestCaptchaGuid.Length > 0 ? data.GuestCaptchaGuid : CaptchaGuid);
                        string captchaError = captcha.Validate(data.GuestCapthca);
                        if (!string.IsNullOrEmpty(captchaError))
                            errors.Add(Encode(captchaError));
                        data.GuestCaptchaGuid = string.Empty;
                    }
                }

                if (data.PostContent.Length == 0)
                    errors.Add(GetMessage("Error.PostContentRequired"));

                if (errors.Count > 0)
                    return false;

                if (this.topic == null) 
                {
					if (ForumId <= 0)
						throw new InvalidOperationException("could not find ForumId!");
                    Dictionary<string, object> identityData = new Dictionary<string, object>();
                    identityData["forumId"] = ForumId;
                    identityData["customPropertyName"] = IdentityPropertyName;

                    BXCustomType customPropertyType = IdentityPropertyType;
                    if (customPropertyType == null)
                    {
                        identityData["customPropertyTypeId"] = "Bitrix.System.Text";
                        identityData["customPropertyValue"] = IdentityPropertyValue;
                    }
                    else
                    {
                        identityData["customPropertyTypeId"] = customPropertyType.TypeName;
                        object customPropertyValue;
                        customPropertyType.TryParseValue(IdentityPropertyValue, out customPropertyValue);
                        identityData["customPropertyValue"] = customPropertyValue;
                    }

                    Dictionary<string, object> additionalData = new Dictionary<string, object>();
                    additionalData["name"] = TopicName;
                    additionalData["firstPostText"] = TopicFirstPostText;
                    if (TopicAuthorId > 0)
                    {
                        additionalData["authorId"] = TopicAuthorId;
                        additionalData["authorName"] = TopicAuthorName;
                    }
                    else if (IsAuthentificated)
                    {
                        additionalData["authorId"] = UserId;
                        additionalData["authorName"] = UserName;
                    }
                    else
                    {
                        additionalData["authorId"] = 0;
                        additionalData["authorName"] = "Guest";
                    }
					if (String.IsNullOrEmpty(IdentityPropertyValue) || String.IsNullOrEmpty(IdentityPropertyName)
						|| String.IsNullOrEmpty(IdentityPropertyTypeName) || IdentityPropertyType == null)
					{
						if (this.topicCreatedHandler == null)
							throw new InvalidOperationException("could not find required topicCreateHandler!");
						this.topic = new BXForumTopic(TopicName, ForumId, string.Empty);
						this.topic.AuthorId = IsAuthentificated ? UserId : 0;
						this.topic.AuthorName = IsAuthentificated ? UserName : "Guest";
						this.topic.Save();
						this.topicCreatedHandler(this, this.topic);

						string firstPostText = data.PostContent;

						//создаём первое сообщение
						BXForumPost firstPost = new BXForumPost();
						firstPost.Post = "-";
						firstPost.ForumId = topic.ForumId;
						firstPost.TopicId = topic.Id;
						firstPost.AuthorId = IsAuthentificated ? UserId : 0;
						firstPost.AuthorName = IsAuthentificated ? UserName : "Guest";
						firstPost.Save();
					}
					else
						if (BXForumTopic.CreateIfNeed(identityData, additionalData, out this.topic) && this.topicCreatedHandler != null)
							this.topicCreatedHandler(this, this.topic);
                }

                BXForumPost curPost = Operation == PostOperation.Edit && Post != null ? Post : new BXForumPost();

                curPost.ForumId = ForumId;
                curPost.TopicId = TopicId;

                if (!IsAuthentificated)
                {
                    curPost.AuthorId = 0;
                    curPost.AuthorName = data.GuestName;
                    if (ShowGuestEmail)
                        curPost.AuthorEmail = data.GuestEmail;
                }
                else
                {
                    curPost.AuthorId = UserId;
                    curPost.AuthorName = UserName;
                }
                curPost.AuthorIP = Request.UserHostAddress;

                if (CanApprove)
                    curPost.Approved = data.IsApproved;

                curPost.Post = data.PostContent;

                if (curPost.IsNew && data.ParentPostId > 0)
                    curPost.ParentPostId = data.ParentPostId;

                curPost.Save();

                if (!string.IsNullOrEmpty(PostReadUrlTemplate))
                {
                    InternalReplaceParams["ForumId"] = curPost.ForumId;
                    InternalReplaceParams["PostId"] = curPost.Id;
                    InternalReplaceParams["CommentId"] = curPost.Id;
                    Response.Redirect(ResolveTemplateUrl(PostReadUrlTemplate,InternalReplaceParams));
                }
                else
                    Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                this.exception = ex;
                this.error = ForumCommentBlockError.General;
                return false;
            }
            return true;
        }

        #region BXComponent
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            Group = new BXComponentGroup("forum", GetMessageRaw("Group"), 100, BXComponentGroup.Communication);
            SortIndex = 1000;

            BXCategory mainCategory = BXCategory.Main;

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.ForumId),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.ForumId),
                    "0",
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.ForumTopicId),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.ForumTopicId),
                    "0",
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.PostId),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.PostId),
                    "0",
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.PostOperation),
                new BXParamSingleSelection(
                    GetParameterTitle(ForumCommentBlockParameter.PostOperation),
                    PostOperation.Add.ToString("G"),
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.ForumTopicAuthorId),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.ForumTopicAuthorId),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.ForumTopicName),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.ForumTopicName),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.FirstPostText),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.FirstPostText),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.IdentityPropertyName),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.IdentityPropertyName),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.IdentityPropertyTypeName),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.IdentityPropertyTypeName),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.IdentityPropertyValue),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.IdentityPropertyValue),
                    string.Empty,
                    mainCategory
                    )
                );

			ParamsDefinition.Add(
				GetParameterKey(ForumCommentBlockParameter.ShowPostForm),
				new BXParamYesNo(
					GetParameterTitle(ForumCommentBlockParameter.ShowPostForm),
					false,
					mainCategory
					)
				);

			string path = "~/bitrix/components/bitrix/blog/templates/.default/style.css";
			if (!Bitrix.IO.BXSecureIO.FileExists(path))
			{
				path = "~/bitrix/components/bitrix/forum.comment.block/templates/.default/themes/style.css";

			}
			else cssFromBlog = true;

			ParamsDefinition.Add(
				GetParameterKey(ForumCommentBlockParameter.CommentColorCss),
				new BXParamSingleSelectionWithText(
					GetParameterTitle(ForumCommentBlockParameter.CommentColorCss),
					path,
					mainCategory
					)
				);

			path = "~/bitrix/components/bitrix/blog/templates/.default/themes/default/style.css";
			if (!Bitrix.IO.BXSecureIO.FileExists(path))
			{
				path = "~/bitrix/components/bitrix/forum.comment.block/templates/.default/themes/default/style.css";

			}
			else colorCssFromBlog = true;

			ParamsDefinition.Add(
				GetParameterKey(ForumCommentBlockParameter.CommentThemeCss),
				new BXParamText(
					GetParameterTitle(ForumCommentBlockParameter.CommentThemeCss),
					path,
					mainCategory
					)
				);

            BXCategory urlCategory = BXCategory.UrlSettings;
            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.UserProfileUrlTemplate), 
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.UserProfileUrlTemplate), 
                    "profile.aspx?user=#UserId#", 
                    urlCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.PostOperationUrlTemplate), 
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.PostOperationUrlTemplate),
                    "comment.aspx?act=#Operation#&comment=#CommentId#", 
                    urlCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.RedirectUrl),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.RedirectUrl),
                    "comments.aspx",
                    urlCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.PostReadUrlTemplate),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.PostReadUrlTemplate),
                    "comments.aspx?comment=#CommentId#", 
                    urlCategory
                    )
                );
            
            #region deferred
            //ParamsDefinition.Add(
            //    GetParameterKey(ForumCommentBlockParameter.PostQuoteUrlTemplate), 
            //    new BXParamText(
            //        GetParameterTitle(ForumCommentBlockParameter.PostQuoteUrlTemplate),
            //        "newpost.aspx?topic=#TopicId#&quote=#PostId#", 
            //        urlCategory
            //        )
            //    );
            //ParamsDefinition.Add(
            //    GetParameterKey(ForumCommentBlockParameter.TopicReplyUrlTemplate),
            //    new BXParamText(
            //        GetMessageRaw(GetParameterTitle(ForumCommentBlockParameter.TopicReplyUrlTemplate)), 
            //        "newpost.aspx?topic=#TopicId#", 
            //        urlCategory
            //        )
            //    );
            //ParamsDefinition.Add(
            //    GetParameterKey(ForumCommentBlockParameter.PostsPerPage),
            //    new BXParamText(
            //        GetParameterTitle(ForumCommentBlockParameter.PostsPerPage),
            //        "0",
            //        mainCategory
            //        )
            //    );
            #endregion

            BXCategory additionalSettingsCategory = BXCategory.AdditionalSettings;

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.ShowGuestEmail),
                new BXParamYesNo(
                    GetParameterTitle(ForumCommentBlockParameter.ShowGuestEmail),
                    true,
                    additionalSettingsCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.RequireGuestEmail),
                new BXParamYesNo(
                    GetParameterTitle(ForumCommentBlockParameter.RequireGuestEmail),
                    true,
                    additionalSettingsCategory
                    )
                );

			ParamsDefinition.Add(
				GetParameterKey(ForumCommentBlockParameter.ShowGuestCaptcha),
				new BXParamYesNo(
					GetParameterTitle(ForumCommentBlockParameter.ShowGuestCaptcha),
					true,
					additionalSettingsCategory
					)
				);

            ParamsDefinition.Add(
                GetParameterKey(ForumCommentBlockParameter.MaxWordLength),
                new BXParamText(
                    GetParameterTitle(ForumCommentBlockParameter.MaxWordLength), 
                    "15",
                    additionalSettingsCategory
                    )
                );

			ParamsDefinition.Add(
				GetParameterKey(ForumCommentBlockParameter.UseTreeComments),
				new BXParamYesNo(
					GetParameterTitle(ForumCommentBlockParameter.UseTreeComments),
					false,
					mainCategory
					)
				);

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
        }

        protected override void LoadComponentDefinition()
        {
            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);

            IList<BXParamValue> operationList = ParamsDefinition[GetParameterKey(ForumCommentBlockParameter.PostOperation)].Values;
            operationList.Add(new BXParamValue(GetMessageRaw("PostOperation.Add"), PostOperation.Add.ToString("G")));
            operationList.Add(new BXParamValue(GetMessageRaw("PostOperation.Edit"), PostOperation.Edit.ToString("G")));
            operationList.Add(new BXParamValue(GetMessageRaw("PostOperation.Delete"), PostOperation.Delete.ToString("G")));
            operationList.Add(new BXParamValue(GetMessageRaw("PostOperation.Approve"), PostOperation.Approve.ToString("G")));
            operationList.Add(new BXParamValue(GetMessageRaw("PostOperation.Disapprove"), PostOperation.Disapprove.ToString("G")));
        }

        #endregion
        internal void ApprovePost(long postId, bool approve) 
        {
            if (this.error != ForumCommentBlockError.None)
                return;

            BXForumPost post = FindPost(postId);
            if (post == null)
            {
                BXFilter filter = new BXFilter(
                    new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, postId),
                    new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId),
                    new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId)
                    );
                if (!this.auth.CanApprove)
                    filter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

                BXForumPostCollection col = BXForumPost.GetList(filter, null);
                if(col.Count > 0)
                    post = col[0];
            }

            if(post == null)
                return;

            if (!CanApprove)
            {
                this.error |= ForumCommentBlockError.NoPostApprovementPermission;
                return;
            }

            try
            {
                post.Approved = approve;
                post.Save();

                if (!string.IsNullOrEmpty(PostReadUrlTemplate))
                {
                    InternalReplaceParams["ForumId"] = post.ForumId;
                    InternalReplaceParams["PostId"] = post.Id;
                    InternalReplaceParams["CommentId"] = post.Id; 
                    Response.Redirect(ResolveTemplateUrl(PostReadUrlTemplate));
                }
                else
                    Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
            }
            catch (ThreadAbortException)
            { 
            }
            catch (Exception)
            {
                this.error |= ForumCommentBlockError.PostSavingFailded;
            }
        }

		internal void DeleteBranch(Dictionary<long, ForumPostTreeInfo> posts)
		{
			bool someDeleted = false;
			foreach (int postId in posts.Keys)
			{

				if (this.error != ForumCommentBlockError.None)
					return;

				BXForumPost post = FindPost(postId);
				if (post == null)
				{
					BXFilter filter = new BXFilter(
						new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, postId),
						new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId),
						new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId)
						);
					if (!this.auth.CanApprove)
						filter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

					BXForumPostCollection col = BXForumPost.GetList(filter, null);
					if (col.Count > 0)
						post = col[0];
				}

				if (post == null)
					return;

				if (!new BXForumAuthorization(this.ForumId, this.UserId, post).CanDeleteThisPost)
				{
					this.error |= ForumCommentBlockError.NoPostDeletionPermission;
					return;
				}

				try
				{
					post.Delete();
					someDeleted = true;
				}
				catch (ThreadAbortException)
				{
				}
				catch (Exception)
				{
					this.error |= ForumCommentBlockError.PostDeletionFailded;
				}
			}
			if (someDeleted)
			{
				if (!string.IsNullOrEmpty(RedirectUrl))
					Response.Redirect(RedirectUrl);
				else
					Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
			}
		}

		internal void DeletePost(long postId)
		{
			DeletePost(postId, true);
		}

        internal void DeletePost(long postId, bool redirectAfterDelete)
        {
            if (this.error != ForumCommentBlockError.None)
                return;

            BXForumPost post = FindPost(postId);
            if (post == null)
            {
                BXFilter filter = new BXFilter(
                    new BXFilterItem(BXForumPost.Fields.Id, BXSqlFilterOperators.Equal, postId),
                    new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, ForumId),
                    new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, TopicId)
                    );
                if (!this.auth.CanApprove)
                    filter.Add(new BXFilterItem(BXForumPost.Fields.Approved, BXSqlFilterOperators.Equal, true));

                BXForumPostCollection col = BXForumPost.GetList(filter, null);
                if (col.Count > 0)
                    post = col[0];
            }

            if (post == null)
                return;

            if (!new BXForumAuthorization(this.ForumId, this.UserId, post).CanDeleteThisPost)
            {
                this.error |= ForumCommentBlockError.NoPostDeletionPermission;
                return;
            }

            try
            {
                post.Delete();
				if (redirectAfterDelete)
				{
					if (!string.IsNullOrEmpty(RedirectUrl))
						Response.Redirect(RedirectUrl);
					else
						Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
				}
            }
            catch (ThreadAbortException)
            { 
            }
            catch (Exception)
            {
                this.error |= ForumCommentBlockError.PostDeletionFailded;
            }
        }

		private bool HaveApprovedChildren(ForumPostTreeInfo node)
		{
			if (node.Children.Count == 0) return false;
			foreach (ForumPostTreeInfo info in node.Children)
				if (info.Approved) return true;
			foreach (ForumPostTreeInfo info in node.Children)
				if (HaveApprovedChildren(info)) return true;
			return false;
		}

		private void DeleteNode(ForumPostTreeInfo node)
		{
			foreach (ForumPostTreeInfo info in node.Children)
				DeleteNode(info);
			try
			{
				DeletePost(node.PostId,false);
			}
			catch(Exception e){

			}
		}

		Dictionary<long, ForumPostTreeInfo> postsTree;
		internal void DeletePostAndBranch(long postId)
		{
			// получим дерево, в котором находится текущий элемент, начиная от корня
			long rootId = BXForumPost.GetRootPostId(postId);
			if (postsTree == null) postsTree = BXForumPost.GetCommentsTree(rootId, true);

			List<ForumPostTreeInfo> postList = new List<ForumPostTreeInfo>();
			foreach (ForumPostTreeInfo info in postsTree.Values)
				postList.Add(info);

			foreach (ForumPostTreeInfo info in postList)
			{
				info.Children.AddRange(postList.FindAll(x => x.ParentPostId == info.PostId));
			}
			//найдем наиболее старшего предка, у которого есть только скрытые потомки

			long curPostId = postId;
			
			ForumPostTreeInfo current = postList.Find(x=>x.PostId == curPostId);
			ForumPostTreeInfo prev = current;

			if (HaveApprovedChildren(current))
			{
				ApprovePost(postId, false);
			}
			else
			{
				current = postList.Find(x => x.PostId == current.ParentPostId);
				while (current!=null)
				{
					if (current.Children.Count>1 || (current.Approved && current.PostId!=postId))
						break;
					prev = current;
					curPostId = current.ParentPostId;
					current = postList.Find(x => x.PostId == curPostId);
				}
				long parentId = prev.ParentPostId;
				DeleteNode(prev);
				
				if (!String.IsNullOrEmpty(PostReadUrlTemplate) && parentId!=0)
				{
					BXParamsBag<object> replace = new BXParamsBag<object>();
					replace.Add("CommentId", parentId);
					RedirectTemplateUrl(PostReadUrlTemplate, replace);
				}
				if (!string.IsNullOrEmpty(RedirectUrl))
					Response.Redirect(RedirectUrl);
				else
					Response.Redirect(BXSefUrlManager.CurrentUrl.ToString());
			}
		}

        /*
        //Только при обработке событий механизмом Postback
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if (this.posts != null)
                foreach (PostItem item in this.posts)
                    item.RegisterForEventValidation();
        }
        */
        public enum PostOperation
        {
            Add = 1,
            Edit,
            Delete,
            Approve,
            Disapprove,
			DeleteAll,
			DeleteCommentsForEntityList
        }

        public class PostOperationInfo
        {
            private ForumCommentBlockComponent component = null;
            public PostOperationInfo(ForumCommentBlockComponent component) 
            {
                if (component == null)
                    throw new ArgumentNullException("component");

                this.component = component;
            }

            private HttpRequest request = null;
            private HttpRequest InnerRequest
            {
                get { return this.request ?? (this.request = this.component.Page != null ? this.component.Page.Request : null); }
            }

            public bool HasOperation()
            {
                return Operation != 0;
            }

            public void Execute()
            {
                if (!(HasOperation() && BXCsrfToken.CheckToken(Token)))
                    return;

				if (Operation == PostOperation.Approve)
					component.ApprovePost(PostId, true);
				else if (Operation == PostOperation.Disapprove)
					component.ApprovePost(PostId, false);
				else if (Operation == PostOperation.Delete)
				{
					if (!component.UseTreeComments)
						component.DeletePost(PostId);
					else if (component.UseTreeComments)
						component.DeletePostAndBranch(PostId);
				}
				else if (Operation == PostOperation.DeleteAll)
					component.DeleteAllComments();
				else if (Operation == PostOperation.DeleteCommentsForEntityList)
					component.DeleteCommentsForEntityList();
            }

            public PostOperation Operation
            {
                get { return this.component.Operation; }
            }

            public long PostId
            {
                get { return this.component.PostId; }
            }

            private string token = null;
            public string Token 
            {
                get { return this.token ?? (this.token = InnerRequest != null ? (InnerRequest[BXCsrfToken.TokenKey] ?? string.Empty) : string.Empty); }
            }

            private string securityTokenPair = null;
            internal string SecurityTokenPair
            {
                get { return securityTokenPair ?? (securityTokenPair = BXCsrfToken.BuildQueryStringPair()); }
            }

            public string GetClientHyperlink(PostOperation operation, long postId)
            {
                string t = this.component.PostOperationUrlTemplate;
                if (string.IsNullOrEmpty(t))
                    return string.Empty;

                BXParamsBag<object> replace = this.component.InternalReplaceParams;
                replace["PostId"] = postId;
                replace["CommentId"] = postId;
                replace["Operation"] = operation.ToString("G").ToLowerInvariant();

                string result = this.component.ResolveTemplateUrl(t);
                if (operation != PostOperation.Edit && !string.IsNullOrEmpty(result))
                    result = string.Concat(result, result.IndexOf('?') >= 0 ? "&" : "?", SecurityTokenPair); 

                replace.Remove("PostId");
                replace.Remove("CommentId");
                replace.Remove("Operation");
                return result;                
            }
        }

        public class PostItem : Control/*, IPostBackEventHandler*/
        {
            private ForumCommentBlockComponent component = null;
            public PostItem(BXForumPost post, ForumCommentBlockComponent component, PostOperationInfo operationInfo, int number) 
            {
                if (post == null)
                    throw new ArgumentNullException("post");

                if (component == null)
                    throw new ArgumentNullException("component");

                this.post = post;
                this.component = component;
                this.operationInfo = operationInfo;
                Number = number;

                ID = string.Concat("Post_", post.Id.ToString());
            }

			private int level;//для древовидных комментариев
			public int Level
			{
				get { return level; }
				set { level = value; }
			}

            public static string PostBookmarkTemplate
            {
                get { return "##comment#CommentId#"; }
            }

            public string PostBookmark
            {
                get { return string.Concat("comment", PostId.ToString()); }
            }

            private int number = 0;
            public int Number
            {
                get { return this.number; }
                set { this.number = value >= 0 ? value : 0; }
            }

            private PostOperationInfo operationInfo = null;
            public PostOperationInfo OperationInfo
            {
                get { return this.operationInfo ?? (this.operationInfo = new PostOperationInfo(this.component)); }
            }

            protected override void OnInit(EventArgs e)
            {
                //ClientScriptManager cs = Page.ClientScript;
                //if (cs != null)
                //{
                //    this.approveHyperlink = cs.GetPostBackClientHyperlink(this, PostOperation.Approve.ToString("G").ToLowerInvariant(), false);
                //    this.disapproveHyperlink = cs.GetPostBackClientHyperlink(this, PostOperation.Disapprove.ToString("G").ToLowerInvariant(), false);
                //    this.deleteHyperlink = cs.GetPostBackClientHyperlink(this, PostOperation.Delete.ToString("G").ToLowerInvariant(), false);
                //}
                this.editHyperlink = OperationInfo.GetClientHyperlink(PostOperation.Edit, PostId);
                this.approveHyperlink = OperationInfo.GetClientHyperlink(PostOperation.Approve, PostId);
                this.disapproveHyperlink = OperationInfo.GetClientHyperlink(PostOperation.Disapprove, PostId);
                this.deleteHyperlink = OperationInfo.GetClientHyperlink(PostOperation.Delete, PostId);

                base.OnInit(e);
            }

            /*
            internal void RegisterForEventValidation()
            {
                ClientScriptManager cs = Page != null ? Page.ClientScript : null;
                if (cs == null)
                    return;
                cs.RegisterForEventValidation(UniqueID, PostOperation.Approve.ToString("G").ToLowerInvariant());
                cs.RegisterForEventValidation(UniqueID, PostOperation.Disapprove.ToString("G").ToLowerInvariant());
                cs.RegisterForEventValidation(UniqueID, PostOperation.Delete.ToString("G").ToLowerInvariant());
            }

            public string GetPostOperationClientHyperlink(PostOperation operation)
            {
                return Page != null && Page.ClientScript != null ? Page.ClientScript.GetPostBackClientHyperlink(this, operation.ToString("G").ToLowerInvariant(), false) : string.Empty;
            }
            */
            private string editHyperlink = string.Empty;
            public string EditHyperlink
            {
                get { return this.editHyperlink; }
            }

            private string approveHyperlink = string.Empty;
            public string ApproveHyperlink
            {
                get { return this.approveHyperlink; }
            }

            private string disapproveHyperlink = string.Empty;
            public string DisapproveHyperlink
            {
                get { return this.disapproveHyperlink; }
            }

            private string deleteHyperlink = string.Empty;
            public string DeleteHyperlink
            {
                get { return this.deleteHyperlink; }
            }

            private BXForumPost post;
            public BXForumPost Post
            {
                get { return this.post; }
            }

            public long PostId
            {
                get { return this.post.Id; }
            }

            public bool HasAuthor
            {
                get { return this.post.Author != null; }
            }

            public BXForumUser Author
            {
                get { return this.post.Author; }
            }

            public BXUser User
            {
                get { return this.post.Author != null ? this.post.Author.User : null; }
            }

            private string authorName = null;
            public string AuthorName
            {
                get
                {
                    if (this.authorName != null)
                        return this.authorName;

                    this.authorName = this.post.TextEncoder.Decode(this.post.AuthorName);
                    if(this.post.AuthorId == 0)
                        this.authorName = string.Concat(this.authorName, " (", this.component.GetMessageRaw("Msg.GuestName"), ")");

                    return this.authorName;
                }
            }

            private string authorNameHtml = null;
            public string AuthorNameHtml
            {
                get 
                {
                    if (this.authorNameHtml != null)
                        return this.authorNameHtml;

                    return (this.authorNameHtml = BXWordBreakingProcessor.Break(AuthorName, this.component.MaxWordLength, true));
                }
            }

            public string AuthorIP
            {
                get { return this.post.AuthorIP; }
            }

            private string userProfileHref = null;
            public string UserProfileHref
            {
                get 
                {
                    if (this.userProfileHref != null)
                        return this.userProfileHref;

                    if (string.IsNullOrEmpty(component.UserProfileUrlTemplate) || User == null)
                        return this.userProfileHref = string.Empty;

                    BXParamsBag<object> replace = component.InternalReplaceParams;
                    replace["UserId"] = User.UserId;
                    this.userProfileHref = component.ResolveTemplateUrl(component.UserProfileUrlTemplate);
                    replace.Remove("UserId");
                    return this.userProfileHref;
                }
            }

            //private string postEditHref = null;
            //public string PostEditHref
            //{
            //    get 
            //    {
            //        if (this.postEditHref != null)
            //            return this.postEditHref;

            //        if (string.IsNullOrEmpty(component.PostEditUrlTemplate))
            //            return this.postEditHref = string.Empty;

            //        BXParamsBag<object> replace = component.InternalReplaceParams;
            //        string postId = PostId > 0 ? PostId.ToString() : string.Empty;
            //        replace["PostId"] = postId;
            //        replace["CommentId"] = postId;
            //        this.postEditHref = component.ResolveTemplateUrl(component.PostEditUrlTemplate);
            //        replace.Remove("PostId");
            //        replace.Remove("CommentId");
            //        return this.postEditHref;
            //    }
            //}

            private string postPermanentHref = null;
            public string PostPermanentHref
            {
                get
                {
                    if (this.postPermanentHref != null)
                        return this.postPermanentHref;

                    if (string.IsNullOrEmpty(component.PostReadUrlTemplate))
                        return this.postPermanentHref = string.Empty;

                    BXParamsBag<object> replace = component.InternalReplaceParams;
                    string postId = PostId > 0 ? PostId.ToString() : string.Empty;
                    replace["PostId"] = postId;
                    replace["CommentId"] = postId; this.postPermanentHref = component.ResolveTemplateUrl(component.PostReadUrlTemplate);
                    replace.Remove("PostId");
                    replace.Remove("CommentId");
                    return this.postPermanentHref;
                }
            }

            //private string postQuoteHref = null;
            //public string PostQuoteHref
            //{
            //    get { return this.postQuoteHref ?? (this.postQuoteHref = !string.IsNullOrEmpty(component.PostQuoteUrlTemplate) ? component.ResolveTemplateUrl(component.PostQuoteUrlTemplate, component.InternalReplaceParams) : string.Empty); }
            //}

            public bool HasAvatar
            {
                get { return this.post.Author != null && this.post.Author.User != null && this.post.Author.User.Image != null; }
            }

            public BXFile Avatar
            {
                get { return this.post.Author != null && this.post.Author.User != null ? this.post.Author.User.Image : null; }
            }


            private string dateOfCreationString = null;
            public string DateOfCreationString
            {
                get { return this.dateOfCreationString ?? (this.dateOfCreationString = this.post.DateCreate.ToString("g")); }
            }

            private string contentHtml = null;
            public string ContentHtml
            {
                get {
					return contentHtml ?? (contentHtml = component.PostChain.Process(this.post.Post)); }
            }

            private string authorSignatureHtml = null;
            public string AuthorSignatureHtml
            {
                get
                {
                    if (this.authorSignatureHtml != null)
                        return this.authorSignatureHtml;

                    this.authorSignatureHtml = this.post.Author != null ? this.post.Author.TextEncoder.Decode(this.post.Author.Signature) : string.Empty;
                    if (!string.IsNullOrEmpty(this.authorSignatureHtml))
                        this.authorSignatureHtml = component.SignChain.Process(this.authorSignatureHtml);
                    return this.authorSignatureHtml;
                }
            }

            public bool IsApproved
            {
                get { return this.post.Approved; }
            }

            public bool IsDeleteable
            {
                get { return this.component.Authorization.CanDeleteThisPost; }
            }

            public bool IsEditable
            {
                get { return this.component.Authorization.CanEditThisPost; }
            }

            /*
            #region IPostBackEventHandler Members
            void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
            {
                if (string.IsNullOrEmpty(eventArgument))
                    return;

                ClientScriptManager cs = Page != null ? Page.ClientScript : null;

                if (string.Equals(eventArgument, PostOperation.Approve.ToString("G").ToLowerInvariant(), StringComparison.Ordinal))
                {
                    if(cs != null)
                        cs.ValidateEvent(UniqueID, PostOperation.Approve.ToString("G").ToLowerInvariant());
                    component.ApprovePost(this.post, true);
                }
                else if (string.Equals(eventArgument, PostOperation.Disapprove.ToString("G").ToLowerInvariant(), StringComparison.Ordinal))
                {
                    if(cs != null)
                        cs.ValidateEvent(UniqueID, PostOperation.Disapprove.ToString("G").ToLowerInvariant());
                    component.ApprovePost(this.post, false);
                }
                else if (string.Equals(eventArgument, PostOperation.Delete.ToString("G").ToLowerInvariant(), StringComparison.Ordinal))
                {
                    if (cs != null)
                        cs.ValidateEvent(UniqueID, PostOperation.Delete.ToString("G").ToLowerInvariant());
                    component.DeletePost(this.post);
                    Page.Response.Redirect(Page.Request.RawUrl);
                }
            }
            #endregion
            */
        }

        public class PostData
        {
            private ForumCommentBlockComponent component;

            public PostData(ForumCommentBlockComponent component)
            {
                if (component == null)
                    throw new ArgumentNullException("component");

                this.component = component;
            }

            private string postContent = string.Empty;
            public string PostContent
            {
                get { return this.postContent; }
                set { SetPostContent(value, true); }
            }

            internal void SetPostContent(string content, bool correct) 
            {
                if (string.Equals(this.postContent, content, StringComparison.InvariantCulture))
                    return;

                this.postContent = content != null ? content.Trim() : string.Empty;
                if (correct && this.postContent.Length > 0 && this.component.PostChain != null)
                    this.postContent = this.component.PostChain.CorrectBBCode(this.postContent);
            } 

            private string guestName = string.Empty;
            public string GuestName
            {
                get { return this.guestName; }
                set { this.guestName = value != null ? value.Trim() : string.Empty; }
            }

            private string guestEmail = string.Empty;
            public string GuestEmail
            {
                get { return this.guestEmail; }
                set { this.guestEmail = value != null ? value.Trim() : string.Empty; }
            }

            private static readonly Regex EmailValidationRx = new Regex(@"^\s*[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]+\s*$", RegexOptions.Compiled | RegexOptions.Singleline);
            public bool IsGuestEmailValid()
            {
                return EmailValidationRx.IsMatch(this.guestEmail);
            }

            private string guestCaptcha = string.Empty;
            public string GuestCapthca
            {
                internal get { return this.guestCaptcha; }
                set { this.guestCaptcha = value; }
            }

            private string guestCaptchaGuid = string.Empty;
            public string GuestCaptchaGuid
            {
                get { return this.guestCaptchaGuid; }
                set { this.guestCaptchaGuid = value; }
            }

            private bool isApproved = true;
            public bool IsApproved
            {
                get { return this.isApproved; }
                set { this.isApproved = value; }
            }

            private long parentPostId = 0;
            public long ParentPostId
            {
                get { return this.parentPostId; }
                set { this.parentPostId = value; }
            }
        }
    }

    /// <summary>
    /// Параметры компонента "Блок комментариев на основе сообщений форума"
    /// </summary>
    public enum ForumCommentBlockParameter
    {
        /// <summary>
        /// Ид форума
        /// </summary>
        ForumId = 1,
        /// <summary>
        /// Ид темы форума
        /// </summary>
        ForumTopicId,
        /// <summary>
        /// Ид редактируемого сообщения
        /// </summary>
        PostId,
        /// <summary>
        /// Ид автора темы
        /// </summary>
        ForumTopicAuthorId,
        /// <summary>
        /// Имя темы
        /// </summary>
        ForumTopicName,
        /// <summary>
        /// Содержание первого сообщения
        /// </summary>
        FirstPostText,
        /// <summary>
        /// Количество сообщений - "комментариев" на странице
        /// </summary>
        //PostsPerPage,
        /// <summary>
        /// Максимальная длина слова
        /// </summary>
        MaxWordLength,
        /// <summary>
        /// Шаблон ссылки на страницу профиля пользователя
        /// </summary>
        UserProfileUrlTemplate,
        /// <summary>
        /// Шаблон ссылки на страницу редактирования сообщения
        /// </summary>
        //PostEditUrlTemplate,
        /// <summary>
        /// Шаблон ссылки на страницу цитирования сообщения
        /// </summary>
        //PostQuoteUrlTemplate,
        /// <summary>
        /// Шаблон ссылки на страницу выполнения операции над сообщением
        /// </summary>
        PostOperationUrlTemplate,
        /// <summary>
        /// Шаблон ссылки на страницу создания нового сообщения
        /// </summary>
        //TopicReplyUrlTemplate
        RedirectUrl,
        PostReadUrlTemplate,
        /// <summary>
        /// Имя идентифицирующего свойства
        /// </summary>
        IdentityPropertyName,
        /// <summary>
        /// Тип идентифицирующего свойства
        /// </summary>
        IdentityPropertyTypeName,
        /// <summary>
        /// Значение идентифицирующего свойства
        /// </summary>
        IdentityPropertyValue,
        /// <summary>
        /// Показывать e-mail для гостя
        /// </summary>
        ShowGuestEmail,
        /// <summary>
        /// Требовать указания е-mail от гостя
        /// </summary>
        RequireGuestEmail,
        /// <summary>
        /// Показывать поле captcha для гостя
        /// </summary>
        ShowGuestCaptcha,
        /// <summary>
        /// Режим редактирования
        /// </summary>
        //EditorMode,
        /// <summary>
        /// Опереция
        /// </summary>
        PostOperation,
		/// <summary>
		/// Использовать древовидные комментарии
		/// </summary>
		UseTreeComments,
		/// <summary>
		/// Показывать форму добавления комментария
		/// </summary>
		ShowPostForm,
		CommentColorCss,
		CommentThemeCss
    }

    /// <summary>
    /// Ошибка
    /// </summary>
    [Flags]
    public enum ForumCommentBlockError
    {
        None = 0x0,
        /// <summary>
        /// Общая
        /// </summary>
        General = 0x1,
        /// <summary>
        /// Не определен форума
        /// </summary>
        ForumNotDefined = 0x2,
        /// <summary>
        /// Не найден форум
        /// </summary>
        ForumNotFound = 0x4,
        /// <summary>
        /// Не определена тема форума
        /// </summary>
        ForumTopicNotDefined = 0x8,
        /// <summary>
        /// Не найдена тема форума
        /// </summary>
        ForumTopicNotFound = 0x10,
        /// <summary>
        /// Нет прав на чтение
        /// </summary>
        NoReadingPermission = 0x20,
        /// <summary>
        /// Нет прав на скрытие/отображение сообщений
        /// </summary>
        NoPostApprovementPermission = 0x40,
        /// <summary>
        /// Нет прав на удаление сообщений
        /// </summary>
        NoPostDeletionPermission = 0x80,
        /// <summary>
        /// Страница не существует
        /// </summary>
        InvalidPage = 0x100,
        /// <summary>
        /// Не удалось сохранить сообщение
        /// </summary>
        PostSavingFailded = 0x200,
        /// <summary>
        /// Не удалось удалить сообщение
        /// </summary>
        PostDeletionFailded = 0x400
    }

    /// <summary>
    /// Шаблон компонента "Блок комментариев на основе сообщений форума"
    /// </summary>
    public class ForumCommentBlockTemplate : BXComponentTemplate<ForumCommentBlockComponent>
    {
		protected override void OnPreRender(EventArgs e)
		{

			var cssParams = new string[] {Parameters.GetString("CommentColorCss", ""), Parameters.GetString("CommentThemeCss") };
			foreach (string css in cssParams)
			{
				if (!BXStringUtility.IsNullOrTrimEmpty(css))
				{
					try
					{
						string vCss = BXPath.ToVirtualRelativePath(css);
						if (BXSecureIO.FileExists(vCss))
							BXPage.RegisterStyle(vCss);
					}
					catch
					{
					}
				}
			}
			base.OnPreRender(e);
		}

        protected string[] GetErrorMessages()
        {
            string[] result = Component.GetComponentErrorMessages();
            if (result.Length > 0)
                for (int i = 0; i < result.Length; i++)
                    result[i] = HttpUtility.HtmlEncode(result[i]);

            return result;
        }
        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            base.Render(writer);
        }

        private bool previewPost = false;
        public bool PreviewPost
        {
            get { return this.previewPost; }
            set { this.previewPost = value; }
        }

        protected void PreparePreview(string input, HtmlTextWriter writer)
        {
            BXForumPostChain processor = Component.PostChain;
            if (processor == null)
                return;
            processor.Process(processor.CorrectBBCode(input), writer);
        }

        protected virtual void GetPostData(ForumCommentBlockComponent.PostData data)
        {
        }

        protected virtual void SetPostData(ForumCommentBlockComponent.PostData data)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!IsPostBack 
                && Component.ComponentError == ForumCommentBlockError.None
                && Component.Operation == ForumCommentBlockComponent.PostOperation.Edit)
                SetPostData(Component.ComponentPostData);
            base.OnLoad(e);
        }

        protected void SavePost()
        {
            GetPostData(Component.ComponentPostData);
            Component.TrySavePostData();
        }
    }
}

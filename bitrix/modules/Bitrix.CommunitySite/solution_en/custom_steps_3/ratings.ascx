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
<%@ Import namespace="Bitrix.CommunicationUtility.Rating" %>
<%@ Import namespace="Bitrix.Services.Rating" %>
<%@ Import namespace="Bitrix.Security" %>
<%@ Import namespace="Bitrix.Forum" %>
<%@ Import namespace="Bitrix.Blog" %>

<script runat="server">
	
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		var view = Result.Render("Solution Installation");
		view.AutoRedirect = true;
		view.RedirectAction = "next";
		return view;
	}

	string siteId;
	protected override BXWizardResult OnActionNext(Bitrix.DataTypes.BXCommonBag parameters)
	{
		siteId = WizardContext.State.GetString("Installer.SiteId");
		var site = BXSite.GetById(siteId, BXTextEncoder.EmptyTextEncoder);

		if (WizardContext.State.Get<Bitrix.DataTypes.BXParamsBag<object>>("Bitrix.CommunitySite.Settings").GetBool("InstallDemoData"))
		{
			int[] users = new int[] { GetUserId("fadeeva"), GetUserId("mihalych"), GetUserId("kostik"), GetUserId("artem") };

			long[] forumPosts = new long[] {
				GetForumPost("post02", "topic01","Bitrix.CommunitySite.Forum.#SiteId#.general"), 
				GetForumPost("post03", "topic01", "Bitrix.CommunitySite.Forum.#SiteId#.general"), 
				GetForumPost("post11", "topic02", "Bitrix.CommunitySite.Forum.#SiteId#.general"), 
				GetForumPost("post31", "topic04", "Bitrix.CommunitySite.Forum.#SiteId#.about") 
			};

			foreach (var postId in forumPosts)
			{
				var voting = BXRatingVoting.CreateIfNeed(new BXRatedItem("ForumPost", postId.ToString(), "ForumPost"));
				if (voting.TotalVotes == 0)
					VoteForVoting(voting, users);
			}

			int[] blogPosts = new int[] { 
				GetBlogPost("post05", "Bitrix.CommunitySite.Blog.#SiteId#.Notes"),
				GetBlogPost("post04", "Bitrix.CommunitySite.Blog.#SiteId#.Notes"),
				GetBlogPost("post03", "Bitrix.CommunitySite.Blog.#SiteId#.Mihalych"),
				GetBlogPost("post02", "Bitrix.CommunitySite.Blog.#SiteId#.Mihalych"),
				GetBlogPost("post01", "Bitrix.CommunitySite.Blog.#SiteId#.Fadeeva")
			};

			foreach (var blogPostId in blogPosts)
			{
				var voting = BXRatingVoting.CreateIfNeed(new BXRatedItem("BlogPost", blogPostId.ToString(), "BlogPost"));
				if (voting.TotalVotes == 0)
				{
					VoteForVoting(voting, users);

					BXBlogCommentCollection comments = BXBlogComment.GetList(
						new BXFilter(new BXFilterItem(BXBlogComment.Fields.Post.Id, BXSqlFilterOperators.Equal, blogPostId)),
						null
					);

					foreach (var comment in comments)
					{
						var commentVoting = BXRatingVoting.CreateIfNeed(new BXRatedItem("BlogComment", comment.Id.ToString(), "BlogComment"));
						if (commentVoting.TotalVotes == 0)
							VoteForVoting(commentVoting, users);
					}
				}
			}
		}

		//User Rating
		BXRatingCollection userRatings = BXRating.GetList(
			new BXFilter(new BXFilterItem(BXRating.Fields.XmlId, BXSqlFilterOperators.Equal, "UserRating")), null
		);

        if (userRatings.Count == 0)
		{
			BXRating rating = new BXRating();
			rating.BoundEntityTypeId = "User";
			rating.CalculationMethod = BXRatingCalculationMethod.Sum;
			rating.CustomPropertyEntityId = "USER";
			rating.Name = "User Rating";
			rating.XmlId = "UserRating";
			rating.CurValCustomFieldName = "RATING";
			rating.PrevValCustomFieldName = "RATING_PREVIOUS";

			var userVoting = new Bitrix.CommunicationUtility.Rating.BXRatingVotingConfig();
			userVoting.Coefficient = 2.25D;

			var blogPostVoting = new Bitrix.Blog.Rating.BXBlogPostVotingConfig();
			blogPostVoting.Coefficient = 0.50D;

			var blogCommentVoting = new Bitrix.Blog.Rating.BXBlogCommentVotingConfig();
			blogCommentVoting.Coefficient = 0.05D;

			var forumTopicVoting = new Bitrix.Forum.Rating.BXForumTopicVotingConfig();
			forumTopicVoting.Coefficient = 0.25D;

			var forumPostVoting = new Bitrix.Forum.Rating.BXForumPostVotingConfig();
			forumPostVoting.Coefficient = 0.05D;

			rating.AddComponentConfig(userVoting);
			rating.AddComponentConfig(blogPostVoting);
			rating.AddComponentConfig(blogCommentVoting);
			rating.AddComponentConfig(forumTopicVoting);
			rating.AddComponentConfig(forumPostVoting);

			rating.Save();
		}

        //Blog Rating
        BXRatingCollection blogRatings = BXRating.GetList(
            new BXFilter(new BXFilterItem(BXRating.Fields.XmlId, BXSqlFilterOperators.Equal, "BlogRating")), null
        );

        if (blogRatings.Count == 0)
        {
            BXRating rating = new BXRating();
            rating.BoundEntityTypeId = "Blog";
            rating.CalculationMethod = BXRatingCalculationMethod.Sum;
            rating.CustomPropertyEntityId = "BLOG";
            rating.Name = "Blog Rating";
            rating.XmlId = "BlogRating";
            rating.CurValCustomFieldName = "RATING";
            rating.PrevValCustomFieldName = "RATING_PREVIOUS";

            var postingActivity = new Bitrix.Blog.Rating.BXBlogPostingActivityConfig();
            postingActivity.MonthPostCoef = 0.1D;
            postingActivity.WeekPostCoef = 0.5D;
            postingActivity.TodayPostCoef = 2.5D;

            rating.AddComponentConfig(postingActivity);
            rating.Save();
        }        

		UI.SetProgressBarValue("Installer.ProgressBar", "Bitrix.CommunitySite", 6);
		return Result.Next();
	}

	private int GetUserId(string login)
	{
		if (!String.IsNullOrEmpty(login))
		{
			BXUserCollection users = Bitrix.Security.BXUser.GetList(
				new BXFilter(new BXFilterItem(Bitrix.Security.BXUser.Fields.UserName, BXSqlFilterOperators.Equal, login)),
				null
			);

			if (users.Count > 0)
				return users[0].UserId;
		}

		return 0;
	}

	private int GetBlogPost(string xmlId, string blogXmlId)
	{
		BXBlog blog = null;
		if (!String.IsNullOrEmpty(blogXmlId))
			blog = GetBlog(blogXmlId);

		int postId = 0;
		if (!String.IsNullOrEmpty(xmlId))
		{
			var f = new BXFilter(new BXFilterItem(BXBlogPost.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId));
			if (blog != null)
				f.Add(new BXFilterItem(BXBlogPost.Fields.Blog.Id, BXSqlFilterOperators.Equal, blog.Id));

			var posts = BXBlogPost.GetList(
				f,
				null
			);
			if (posts.Count > 0)
				postId = posts[0].Id;
		}

		return postId;
	}

	private BXBlog GetBlog(string xmlId)
	{
		BXBlog blog = null;
		if (!string.IsNullOrEmpty(xmlId))
		{
			var blogs = BXBlog.GetList(
				new BXFilter(new BXFilterItem(BXBlog.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
				null
			);
			if (blogs.Count > 0)
				blog = blogs[0];
		}

		return blog;
	}

	private long GetForumPost(string xmlId, string topicXmlId, string forumXmlId)
	{
		long postId = 0;

		BXForum forum = null;
		if (!String.IsNullOrEmpty(forumXmlId))
			forum = GetForum(ProcessValue(forumXmlId));

		BXForumTopic topic = null;
		if (!String.IsNullOrEmpty(topicXmlId))
			topic = GetTopic(ProcessValue(topicXmlId), forum != null ? (int?)forum.Id : null);

		if (!string.IsNullOrEmpty(xmlId))
		{
			var filter = new BXFilter(new BXFilterItem(BXForumPost.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId));

			if (topic != null)
				filter.Add(new BXFilterItem(BXForumPost.Fields.Topic.Id, BXSqlFilterOperators.Equal, topic.Id));

			if (forum != null)
				filter.Add(new BXFilterItem(BXForumPost.Fields.Forum.Id, BXSqlFilterOperators.Equal, forum.Id));

			var posts = BXForumPost.GetList(
				filter,
				null,
				new BXSelectAdd(BXForumPost.Fields.CustomFields.DefaultFields),
				null,
				Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder
			);
			if (posts.Count > 0)
				postId = posts[0].Id;
		}

		return postId;
	}

	private BXForum GetForum(string xmlId)
	{
		BXForum forum = null;
		if (!string.IsNullOrEmpty(xmlId))
		{
			var forums = BXForum.GetList(
				new BXFilter(new BXFilterItem(BXForum.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId)),
				null,
				new BXSelectAdd(BXForum.Fields.Category, BXForum.Fields.Sites, BXForum.Fields.CustomFields.DefaultFields),
				null,
				Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder
			);
			if (forums.Count > 0)
				forum = forums[0];
		}

		return forum;
	}

	private BXForumTopic GetTopic(string xmlId, int? forumId)
	{
		BXForumTopic topic = null;
		if (!string.IsNullOrEmpty(xmlId))
		{
			var filter = new BXFilter(new BXFilterItem(BXForumTopic.Fields.XmlId, BXSqlFilterOperators.Equal, xmlId));
			if (forumId != null)
				filter.Add(new BXFilterItem(BXForumTopic.Fields.Forum.Id, BXSqlFilterOperators.Equal, forumId));

			var topics = BXForumTopic.GetList(
				filter,
				null,
				new BXSelectAdd(BXForum.Fields.CustomFields.DefaultFields),
				null,
				Bitrix.Services.Text.BXTextEncoder.EmptyTextEncoder
			);
			if (topics.Count > 0)
				topic = topics[0];
		}

		return topic;
	}

	string ProcessValue(string input)
	{
		if (string.IsNullOrEmpty(input))
			return input;

		return Regex.Replace(input, "#([^#]*)#", GetReplacement);
	}

	private string GetReplacement(Match m)
	{
		string key = m.Groups[1].Value;
		if (key == "")
			return "#";

		if (string.Equals(key, "SiteId", StringComparison.OrdinalIgnoreCase))
			return siteId;

		return m.Value;
	}

	private void VoteForVoting(BXRatingVoting voting, int[] users)
	{
		if (voting == null || users == null)
			return;

		Random rand = new Random();
		foreach (var userId in users)
		{
			if (userId < 1)
				continue;

			var vote = new BXRatingVote();
			vote.Active = true;
			vote.RatingVotingId = voting.Id;
			vote.Value = rand.Next(0, 4) % 5 == 0 ? -1 : 1;
			vote.UserId = userId;
			vote.UserIP = "";
			vote.Create();
		}
	}	
	
</script>
Setup Ratings And Votings
<% UI.ProgressBar("Installer.ProgressBar"); %>
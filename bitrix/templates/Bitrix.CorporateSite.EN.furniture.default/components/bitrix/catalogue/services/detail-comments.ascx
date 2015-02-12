<%@ Control Language="C#" Inherits="Bitrix.UI.BXControl" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.Components" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Import Namespace="Bitrix.DataLayer" %>
<%@ Import Namespace="Bitrix.Forum" %>
<%@ Import Namespace="Bitrix.Forum.Components" %>
<%@ Import Namespace="Bitrix.Services.Text" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/forum.comment.block/component.ascx" %>
<%@ Reference VirtualPath="~/bitrix/components/bitrix/catalogue/component.ascx" %>
<%@ Reference VirtualPath="detail.ascx" %>
<script runat="server">
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

        Control parent = Parent;
        CatalogueComponent parentComponent = null;
        while (parentComponent == null && parent != null && parent != Page)
        {
            parentComponent = parent as CatalogueComponent;
            parent = parent.Parent;
        }

        int blockId = 0, elementId = 0;

        if (!(parentComponent != null && parentComponent.AllowComments && (blockId = parentComponent.IBlockId) > 0 && (elementId = parentComponent.ComponentCache.GetInt("ElementId", 0)) > 0))
        {
            commentBlock.Visible = false;
            return;
        }

        BXIBlockElement element = null;
        BXIBlockElementCollection elementCol = BXIBlockElement.GetList(
            new BXFilter(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId)),
            null,
            new BXSelectAdd(BXIBlockElement.Fields.CustomFields[blockId]),
            null,
            BXTextEncoder.EmptyTextEncoder
            );

        if (elementCol.Count > 0)
            element = elementCol[0];

        if (element == null)
        {
            commentBlock.Visible = false;
            return;             
        }

        string customPropertyName = parentComponent.ComponentCache.Get("_BXCommentCustomProperty", "_BXCommentForumTopicID");
        
        ForumCommentBlockComponent commentComponent = (ForumCommentBlockComponent)commentBlock.Component;
        commentComponent.TopicCreatedHandler =
            delegate(BXComponent sender, BXForumTopic topic)
            {
                if (elementId <= 0 || topic == null)
                    return;

                BXIBlockElement.CreateCustomFieldIfNeed(blockId, customPropertyName, "Bitrix.System.Int");
                BXIBlockElementCollection c = BXIBlockElement.GetList(
                    new BXFilter(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId)),
                    null,
                    new BXSelectAdd(BXIBlockElement.Fields.CustomFields[blockId]),
                    null
                    );

                if (c.Count == 0)
                    return;

                BXIBlockElement el = c[0];
                el.CustomPublicValues[customPropertyName] = topic.Id;
                el.Save();
            };
        
        BXComponentParameters parentParams = parentComponent.Parameters;
        BXComponentParameters commentParams = commentComponent.Parameters;
        BXComponentResults parentResults = parentComponent.ComponentCache;

        int topicId = element.CustomPublicValues.GetInt32(customPropertyName, 0);
        if (topicId > 0)
            commentParams["ForumTopicId"] = topicId.ToString();
        else
        {
            commentParams["ForumTopicName"] = element.Name;
            commentParams["FirstPostText"] = element.DetailText;
        }
                
        commentParams["ForumId"] = parentParams["CommentsForumId"];
        //режим редактирования
        commentParams["PostId"] = parentResults.Get("CommentId", "0");
        commentParams["PostOperation"] = parentResults.Get("CommentOperation", "add");
        commentParams["ShowFirstPost"] = false.ToString();
        
        commentParams["IdentityPropertyName"] = "_IBlockElementID";
        commentParams["IdentityPropertyTypeName"] = "Bitrix.System.Int";
        commentParams["IdentityPropertyValue"] = elementId.ToString();
        commentParams["UserProfileUrlTemplate"] = parentParams["CommentAuthorProfileUrlTemplate"];
        commentParams["PostOperationUrlTemplate"] = parentResults.Get("CommentOperationUrlTemplate", string.Empty);
        commentParams["RedirectUrl"] = parentResults.Get("CommentReturnUrl", string.Empty);
        commentParams["PostReadUrlTemplate"] = parentResults.Get("CommentReadUrlTemplate", string.Empty);
        commentParams["ShowGuestEmail"] = parentParams["DisplayEmailForGuestComment"];
        commentParams["RequireGuestEmail"] = parentParams["RequireEmailForGuestComment"];
        commentParams["ShowGuestCaptcha"] = parentParams["DisplayCaptchaForGuestComment"];
        commentParams["MaxWordLength"] = parentParams["CommentMaxWordLength"];
        int commentsPerPage = parentParams.GetInt("CommentsPerPage", 0);
        commentParams["PagingAllow"] = (commentsPerPage > 0).ToString();
        commentParams["PagingRecordsPerPage"] = (commentsPerPage > 0 ? commentsPerPage : 0).ToString();
        commentParams["PagingPageID"] = parentResults.Get("CommentPage", string.Empty);
        commentParams["PagingPageTemplate"] = parentResults.Get("CommentPageTemplate", string.Empty);
        commentParams["PagingIndexTemplate"] = parentResults.Get("CommentPageIndexTemplate", string.Empty);
        commentParams["ForumTopicAuthorId"] = parentResults.Get("CommentOriginAuthorId", string.Empty);
    }
</script>

<bx:IncludeComponent runat="server" ID="commentBlock" ComponentName="bitrix:forum.comment.block" Template=".default" />
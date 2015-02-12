<%@ Reference VirtualPath="~/bitrix/components/bitrix/rating.vote/component.ascx" %>
<%@ Control Language="C#" ClassName="template" AutoEventWireup="false" Inherits="Bitrix.CommunicationUtility.Components.RatingVoteTemplate" %>
<%@ Import Namespace="Bitrix.CommunicationUtility.Handlers" %>
<script runat="server">
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		RequiresEpilogue = true;
	}

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
	}

	protected override void RenderEpilogue(HtmlTextWriter writer)
	{
        StringBuilder sb = new StringBuilder();
        sb.Append("totalsContainer:\"").Append(ClientID).Append("_result\"");
		sb.Append(", ratingContainer:\"").Append(ClientID).Append("_rating\"");
		sb.Append(", plusLink:\"").Append(ClientID).Append("_plus\"");
		sb.Append(", minusLink:\"").Append(ClientID).Append("_minus\"");
        sb.Append(", isAllowed:").Append(Component.IsVotingAllowed.ToString().ToLowerInvariant());
		sb.Append(", hasVoted:").Append(Component.HasVoted.ToString().ToLowerInvariant());
        sb.Append(", titleTemplate:\"").AppendFormat(JSEncode(GetMessageRaw("TitleTemplate.Standard")), "#votes#", "#posVotes#", "#negVotes#").Append("\"");
        sb.Append(", noVotesMsg:\"").Append(GetMessageRaw("TitleTemplate.NoVotes")).Append("\"");
		sb.Append(@", positiveVoteValue:""").Append(Component.PositiveVoteValue).Append(@"""");
		sb.Append(@", negativeVoteValue:""").Append(Component.NegativeVoteValue).Append(@"""");

        sb.Append(@", votingActionUrl:""").Append(Component.GetVotingActionUrl("#value#", "#securityToken#")).Append(@"""");

        sb.Append(", totals:").Append(Component.VotingTotalsToJson());
        sb.Append("}").Insert(0, "{");
        string options = sb.ToString();
        sb.Length = 0;
        sb.AppendFormat(@"Bitrix.RatingVoting.create(""{0}"", {1}).prepare();", Component.ComponentGuid, options);
		
		writer.WriteLine(@"<script type=""text/javascript"">");
		writer.WriteLine(sb.ToString());
		writer.Write(@"<");
		writer.WriteLine(@"/script>");
	} 
</script>
<% if(Component.ComponentError != Bitrix.CommunicationUtility.Components.RatingVoteComponentError.None) return; %>
<span class="rating-vote"  id="<%= ClientID + "_rating" %>"> 
	<a href="javascript:void(0);" title="<%= GetMessage("TitleTemplate.Love") %>" class="rating-vote-plus" id="<%= ClientID + "_plus" %>" onclick="<%= GetVoteClientClick(true) %>"></a><a href="javascript:void(0);" title="<%= GetMessage("TitleTemplate.Hate") %>" id="<%= ClientID + "_minus" %>" class="rating-vote-minus" onclick="<%= GetVoteClientClick(false) %>"></a>
	<span title="<%= (Component.TotalVotes > 0 ? GetMessageFormat("TitleTemplate.Standard", Component.TotalVotes, Component.TotalPositiveVotes, Component.TotalNegativeVotes) : GetMessage("TitleTemplate.NoVotes")) %>" class="rating-vote-result <%= (Component.CurrentValue >= 0 ? "rating-vote-result-plus" : "rating-vote-result-minus") %>" ID="<%= ClientID + "_result" %>"><%= Component.CurrentValue.ToString() %></span>
</span>

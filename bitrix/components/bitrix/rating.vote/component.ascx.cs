using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Services.Rating;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using System.Text;
using Bitrix.CommunicationUtility.Rating;
using Bitrix.Security;
using Bitrix.Services;
using System.Net;
using System.Globalization;
using Bitrix.CommunicationUtility.Handlers;
using System.Security.Cryptography;

namespace Bitrix.CommunicationUtility.Components
{
    /// <summary>
    /// Компонент "Рейтинговое голосование"
    /// </summary>
    public partial class RatingVoteComponent : BXComponent, ICallbackEventHandler
    {
        protected override void OnLoad(EventArgs e)
        {
			base.OnLoad(e);

			AbortCache();

            string entityTypeId = BoundEntityTypeId,
                entityId = BoundEntityId;

            if (string.IsNullOrEmpty(entityTypeId))
                this.componentError |= RatingVoteComponentError.BoundEntityTypeIdIsNotDefined;
            else if (string.IsNullOrEmpty(entityId))
                this.componentError |= RatingVoteComponentError.BoundEntityIdIsNotDefined;
			else
			{
				this.votingTotals = BXRatingVotingTotals.Create(entityTypeId, entityId, CurrentUserId);

				this.voting = BXRatingVoting.CreateIfNeed(new BXRatedItem(entityTypeId, entityId, CustomPropertyEntityId));

                BXTagCachingScope scope = GetCurrentTagCachingScope();
                if (scope != null)
                {
                    this.voting.PrepareForTagCaching(scope);
                }

				if (this.voting == null)
					this.componentError |= RatingVoteComponentError.RatingVotingIsNotFound;
			
				if ((BannedUsers.Length == 0 || Array.FindIndex<int>(BannedUsers, x => x == CurrentUserId) < 0) 
					&& CurrentPricipal != null && RolesAuthorizedToVote.Count > 0)
                    foreach (string r in CurrentPricipal.GetAllRoles())
                        if (RolesAuthorizedToVote.Contains(r))
						{
                            this.isVotingAllowed = true;
							break;
						}

				//BXRatingVotingHandler.RegisterVotingPermission(ComponentGuid, this.voting.Id, CurrentUserId, this.isVotingAllowed, Context);
			}

            IncludeComponentTemplate();
        }

		private string componentGuid = null;
		public string ComponentGuid
		{
			get
			{
				if(this.componentGuid != null)
					return this.componentGuid;
			

				StringBuilder sb = new StringBuilder();
				MD5 md5 = MD5.Create();

                if (Component != null)
                {
                    foreach (byte b in md5.ComputeHash(Encoding.Unicode.GetBytes(string.Concat(Component.Page.AppRelativeVirtualPath, "$", ClientID))))
                        sb.Append(b.ToString("x2"));
                }
                else
                {
                    // ns: обеспечение совместимости с визуальным редактором
                    this.componentGuid = Guid.NewGuid().ToString("N");
                }

				return this.componentGuid = sb.ToString();
			}
		}

        [Obsolete("Please use method GetVotingActionUrl(string valuePlaceHolder, string securityTokenPlaceHolder) isted")]
		public string GetVotingActionUrl(string valuePlaceHolder)
		{
			return BXRatingVotingHandler.GetVotingActionUrl(ComponentGuid, "~/bitrix/handlers/CommunicationUtility/RatingVoting.ashx", VotingId, valuePlaceHolder);
		}

        public string GetVotingActionUrl(string valuePlaceHolder, string securityTokenPlaceHolder)
        {
            return BXRatingVotingHandler.GetVotingActionUrl(ComponentGuid, "~/bitrix/handlers/CommunicationUtility/RatingVoting.ashx", VotingId, valuePlaceHolder, securityTokenPlaceHolder);
        }

        private string GetParameterKey(RatingVoteComponentParameter param)
        {
            return param.ToString("G");
        }
        private string GetParameterTitle(RatingVoteComponentParameter param)
        {
            return GetMessageRaw(string.Concat("Param.", param.ToString("G")));
        }
        #region BXComponent
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";

            Group = new BXComponentGroup("Rating", GetMessageRaw("Group"), 100, BXComponentGroup.Service);

            BXCategory mainCategory = BXCategory.Main;

            ParamsDefinition.Add(
                GetParameterKey(RatingVoteComponentParameter.BoundEntityTypeId),
                new BXParamSingleSelection(
                    GetParameterTitle(RatingVoteComponentParameter.BoundEntityTypeId),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(RatingVoteComponentParameter.BoundEntityId),
                new BXParamText(
                    GetParameterTitle(RatingVoteComponentParameter.BoundEntityId),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(RatingVoteComponentParameter.CustomPropertyEntityId),
                new BXParamText(
                    GetParameterTitle(RatingVoteComponentParameter.CustomPropertyEntityId),
                    string.Empty,
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(RatingVoteComponentParameter.PositiveVoteValue),
                new BXParamText(
                    GetParameterTitle(RatingVoteComponentParameter.PositiveVoteValue),
                    "1",
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(RatingVoteComponentParameter.NegativeVoteValue),
                new BXParamText(
                    GetParameterTitle(RatingVoteComponentParameter.NegativeVoteValue),
                    "-1",
                    mainCategory
                    )
                );

            ParamsDefinition.Add(
                GetParameterKey(RatingVoteComponentParameter.RolesAuthorizedToVote),
                new BXParamMultiSelection(
                    GetParameterTitle(RatingVoteComponentParameter.RolesAuthorizedToVote),
                    string.Empty,
                    mainCategory
                    )
                );

            //ParamsDefinition.Add(
            //    GetParameterKey(RatingVoteComponentParameter.EnableAjax),
            //    new BXParamYesNo(
            //        GetParameterTitle(RatingVoteComponentParameter.EnableAjax),
            //        true,
            //        mainCategory
            //        )
            //    );
        }

        protected override void LoadComponentDefinition()
        {
            //BoundEntityType
            IList<BXParamValue> boundEntityTypeValues = ParamsDefinition[GetParameterKey(RatingVoteComponentParameter.BoundEntityTypeId)].Values;
            boundEntityTypeValues.Clear();
            boundEntityTypeValues.Add(new BXParamValue(GetMessageRaw("SelectBoundEntityType"), string.Empty));
            foreach (BXRatableEntityTypeInfo typeInfo in BXRatingManager.GetRatableEntityTypeInfos())
                boundEntityTypeValues.Add(new BXParamValue(typeInfo.Description, typeInfo.Name));
            ParamsDefinition[GetParameterKey(RatingVoteComponentParameter.BoundEntityTypeId)].RefreshOnDirty = true;

            //Rating
            //IList<BXParamValue> ratingValues = ParamsDefinition[GetParameterKey(RatingVoteComponentParameter.RatingId)].Values;
            //ratingValues.Clear();
            //ratingValues.Add(new BXParamValue(GetMessageRaw("SelectRating"), string.Empty));
            //BXFilter ratingFilter = new BXFilter(new BXFilterItem(BXRating.Fields.Active, BXSqlFilterOperators.Equal, true));
            //string boundEntityTypeId = string.Empty;
            //if (Parameters.TryGetString(GetParameterKey(RatingVoteComponentParameter.BoundEntityTypeId), out boundEntityTypeId)
            //    && !string.IsNullOrEmpty(boundEntityTypeId))
            //    ratingFilter.Add(new BXFilterItem(BXRating.Fields.BoundEntityTypeId, BXSqlFilterOperators.Equal, boundEntityTypeId));

            //BXRatingCollection ratings = BXRating.GetList(
            //    ratingFilter,
            //    new BXOrderBy(new BXOrderByPair(BXRating.Fields.Name, BXOrderByDirection.Asc))
            //    );

            //foreach (BXRating rating in ratings)
            //    ratingValues.Add(new BXParamValue(rating.Name, rating.Id.ToString()));

            IList<BXParamValue> rolesValues = ParamsDefinition[GetParameterKey(RatingVoteComponentParameter.RolesAuthorizedToVote)].Values;
            rolesValues.Clear();

            foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
            {
                if (string.Equals(r.RoleName, "Guest", StringComparison.Ordinal))
                    continue;
                rolesValues.Add(new BXParamValue(r.Title, r.RoleName));
            }
        }
        #endregion
        private RatingVoteComponentError componentError = RatingVoteComponentError.None;
        public RatingVoteComponentError ComponentError
        {
            get { return this.componentError; }
            internal set { this.componentError = value; }
        }

        private List<string> generalErrorMessages = null;
        protected void AddGeneralErrorMessage(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            (this.generalErrorMessages ?? (this.generalErrorMessages = new List<string>())).Add(msg);
        }

        public string[] GetComponentErrorMessages(bool encode)
        {
            if (this.componentError == RatingVoteComponentError.None)
                return new string[0];

            List<string> r = new List<string>();
            if ((this.componentError & RatingVoteComponentError.UserIsNotAllowedToVote) != 0)
                r.Add(GetMessage("Error.UserIsNotAllowedToVote", encode));
            if ((this.componentError & RatingVoteComponentError.UserHasAlreadyVoted) != 0)
                r.Add(GetMessage("Error.UserHasAlreadyVoted", encode));
            if ((this.componentError & RatingVoteComponentError.BoundEntityTypeIdIsNotDefined) != 0)
                r.Add(GetMessage("Error.BoundEntityTypeIdIsNotDefined", encode));
            if ((this.componentError & RatingVoteComponentError.BoundEntityIdIsNotDefined) != 0)
                r.Add(GetMessage("Error.BoundEntityIdIsNotDefined", encode));
            //if ((this.componentError & RatingVoteComponentError.RatingCounterIsNotFound) != 0)
            //    r.Add(GetMessage("Error.RatingCounterIsNotFound", encode));
            //if ((this.componentError & RatingVoteComponentError.RatingIsNotFound) != 0)
            //    r.Add(GetMessage("Error.RatingIsNotFound", encode));
            if ((this.componentError & RatingVoteComponentError.RatingVotingIsNotFound) != 0)
                r.Add(GetMessage("Error.RatingVotingIsNotFound", encode));
            if ((this.componentError & RatingVoteComponentError.General) != 0)
            {
                if (this.generalErrorMessages != null && this.generalErrorMessages.Count > 0)
                    foreach (string msg in this.generalErrorMessages)
                        r.Add(encode ? Encode(msg) : msg);
                else
                    r.Add(GetMessage("Error.General", encode));
            }
            return r.ToArray();
        }

        public string BoundEntityTypeId
        {
            get { return Parameters.Get<string>(GetParameterKey(RatingVoteComponentParameter.BoundEntityTypeId), string.Empty) ?? string.Empty; }
            set { Parameters[GetParameterKey(RatingVoteComponentParameter.BoundEntityTypeId)] = value ?? string.Empty; }
        }

        public string BoundEntityId
        {
            get { return Parameters.Get<string>(GetParameterKey(RatingVoteComponentParameter.BoundEntityId), string.Empty) ?? string.Empty; }
            set { Parameters[GetParameterKey(RatingVoteComponentParameter.BoundEntityId)] = value ?? string.Empty; }
        }

        public string CustomPropertyEntityId
        {
            get { return Parameters.Get<string>(GetParameterKey(RatingVoteComponentParameter.CustomPropertyEntityId), string.Empty) ?? string.Empty; }
            set { Parameters[GetParameterKey(RatingVoteComponentParameter.CustomPropertyEntityId)] = value ?? string.Empty; }
        }

        //public int RatingId
        //{
        //    get { return Parameters.Get<int>(GetParameterKey(RatingVoteComponentParameter.RatingId), 0); }
        //    set
        //    {
        //        Parameters[GetParameterKey(RatingVoteComponentParameter.RatingId)] = value.ToString();
        //        this.rating = null;
        //        this.isRatingLoaded = false;
        //    }
        //}
        //private bool isRatingLoaded = false;
        //private BXRating rating = null;
        //public BXRating Rating
        //{
        //    get 
        //    {
        //        if (!this.isRatingLoaded)
        //        {
        //            this.rating = BXRating.GetById(RatingId);
        //            this.isRatingLoaded = true;
        //        }
        //        return this.rating; 
        //    }
        //}
        private IList<string> rolesAuthorizedToVote = null;
        public IList<string> RolesAuthorizedToVote
        {
            get { return this.rolesAuthorizedToVote ?? (this.rolesAuthorizedToVote = Parameters.GetListString(GetParameterKey(RatingVoteComponentParameter.RolesAuthorizedToVote))); }
            set { Parameters[GetParameterKey(RatingVoteComponentParameter.RolesAuthorizedToVote)] = BXStringUtility.ListToCsv(this.rolesAuthorizedToVote = value ?? new List<string>()); }
        }

        private BXPrincipal currentPricipal = null;
        public BXPrincipal CurrentPricipal
        {
            get { return this.currentPricipal ?? (this.currentPricipal = Context.User as BXPrincipal); }
        }
        private int? currentUserId = null;
        public int CurrentUserId
        {
            get { return this.currentUserId ?? (this.currentUserId = CurrentPricipal != null && CurrentPricipal.Identity.IsAuthenticated ? ((BXIdentity)CurrentPricipal.Identity).Id : 0).Value; }
        }

        private bool isVotingAllowed = false;
        public bool IsVotingAllowed
        {
            get { return this.isVotingAllowed; }
        }

        private BXRatingVoting voting = null;
        public BXRatingVoting Voting
        {
            get { return this.voting; }
        }

		public int VotingId
		{
			get { return this.voting != null ? this.voting.Id : 0; }
		}

        private BXRatingVotingTotals votingTotals = null;
        public BXRatingVotingTotals VotingTotals
        {
            get { return this.votingTotals; }
        }

        public double CurrentValue
        {
            get { return VotingTotals != null ? VotingTotals.Value : 0; }
        }

        public int TotalVotes
        {
            get { return VotingTotals != null ? VotingTotals.TotalVotes : 0; }
        }

        public int TotalPositiveVotes
        {
            get { return VotingTotals != null ? VotingTotals.TotalPositiveVotes : 0; }
        }

        public int TotalNegativeVotes
        {
            get { return VotingTotals != null ? VotingTotals.TotalNegativeVotes : 0; }
        }

        public int PositiveVoteValue
        {
            get { return Parameters.Get<int>(GetParameterKey(RatingVoteComponentParameter.PositiveVoteValue), 1); }
            set { Parameters[GetParameterKey(RatingVoteComponentParameter.PositiveVoteValue)] = value.ToString(); }
        }

        public int NegativeVoteValue
        {
            get { return Parameters.Get<int>(GetParameterKey(RatingVoteComponentParameter.NegativeVoteValue), -1); }
            set { Parameters[GetParameterKey(RatingVoteComponentParameter.NegativeVoteValue)] = value.ToString(); }
        }

        //public bool EnableAjax
        //{
        //    get { return Parameters.Get<bool>(GetParameterKey(RatingVoteComponentParameter.EnableAjax), true); }
        //    set { Parameters[GetParameterKey(RatingVoteComponentParameter.EnableAjax)] = value.ToString(); }
        //}

        private int[] bannedUsers = null;
        /// <summary>
        /// Список ИД пользователей, которым запрещено голосовать
        /// </summary>
        public int[] BannedUsers
        {
            get
            {
                if (this.bannedUsers != null)
                    return this.bannedUsers;

                List<int> lst = Parameters.GetListInt(GetParameterKey(RatingVoteComponentParameter.BannedUsers));
                return this.bannedUsers = lst != null ? lst.ToArray() : new int[0];
            }
            set
            {
                this.bannedUsers = value ?? new int[0];
                Parameters[GetParameterKey(RatingVoteComponentParameter.BannedUsers)] = BXStringUtility.ListToCsv(
                    Array.ConvertAll<int, string>(
                        this.bannedUsers,
                        delegate(int obj) { return obj.ToString(); }
                        )
                    );
            }
        }

        public bool HasVoted
        {
            get { return VotingTotals != null ? VotingTotals.UserHasVoted : false; }
        }

        //public bool HasVoted
        //{
        //    get { return false; }
        //}
        /// <summary>
        /// Голосовать (+/-)
        /// </summary>
        public void Vote(bool sign)
        {
            try
            {
                if (componentError != RatingVoteComponentError.None)
                    return;

                if (CurrentPricipal.Identity == null 
                    || !CurrentPricipal.Identity.IsAuthenticated 
                    || !IsVotingAllowed)
                {
                    this.componentError |= RatingVoteComponentError.UserIsNotAllowedToVote;
                    return;
                }

                if (VotingTotals.UserHasVoted)
                {
                    this.componentError |= RatingVoteComponentError.UserHasAlreadyVoted;
                    return;
                }

                BXRatingVote vote = new BXRatingVote();
                vote.Active = true;
                vote.RatingVotingId = Voting.Id;
                //vote.Value = sign ? Voting.Config.PlusValue : Voting.Config.MinusValue;
                vote.Value = sign ? PositiveVoteValue : NegativeVoteValue;
                vote.UserId = CurrentUserId;
                vote.UserIP = Request.UserHostAddress;

                vote.Create();
                this.votingTotals = BXRatingVotingTotals.Create(BoundEntityTypeId, BoundEntityId, CurrentUserId);
            }
            catch (Exception e)
            {
                this.componentError |= RatingVoteComponentError.General;
                AddGeneralErrorMessage(e.Message);
            }
            if (this.componentError == RatingVoteComponentError.None && !Page.IsCallback)
                Response.Redirect(Request.RawUrl);
        }

        public string VotingTotalsToJson()
        {
            if (VotingTotals == null)
                return "{}";

            return string.Format(
                "{{value:{0}, votes:{1}, posVotes:{2}, negVotes:{3}, hasVoted:{4}}}",
                VotingTotals.Value.ToString("N0", CultureInfo.InvariantCulture),
                VotingTotals.TotalVotes.ToString(),
                VotingTotals.TotalPositiveVotes.ToString(),
                VotingTotals.TotalNegativeVotes.ToString(),
                VotingTotals.UserHasVoted.ToString().ToLowerInvariant()
                );
        }

        private void PrepareCallbackEventHandlerResult()
        {
            if (ComponentError != RatingVoteComponentError.None)
            {
                string[] errors = GetComponentErrorMessages(false);
                StringBuilder errorHtml = new StringBuilder();
                foreach (string error in errors)
                    errorHtml.Append("<li>").Append(Encode(error)).Append("</li>");
                errorHtml.Insert(0, "<ul>").Append("</ul>");

                this.callbackEventHandlerResult =
                    string.Format(
                    "{{error: \"{0}\"}}",
                    JSEncode(errorHtml.ToString())
                    );
                return;
            }

            this.callbackEventHandlerResult =
                string.Format(
                    "{{id:\"{0}\", totals:{1}}}",
                    ClientID,
                    VotingTotalsToJson()
                    );
        }

        #region ICallbackEventHandler Members
        private string callbackEventHandlerResult = string.Empty;
        public string GetCallbackResult()
        {
            return this.callbackEventHandlerResult;
        }

        public void RaiseCallbackEvent(string eventArgument)
        {
            if (string.Equals("VOTE$+", eventArgument, StringComparison.Ordinal))
            {
                Vote(true);
                PrepareCallbackEventHandlerResult();
            }
            else if (string.Equals("VOTE$-", eventArgument, StringComparison.Ordinal))
            {
                Vote(false);
                PrepareCallbackEventHandlerResult();
            }
        }

        #endregion
    }

    public class RatingVoteTemplate : BXComponentTemplate<RatingVoteComponent>
    {

        protected override void Render(HtmlTextWriter writer)
        {
            StartWidth = "100%";
            if (IsComponentDesignMode)
            {
                if (IsComponentDesignMode && Component.ComponentError != RatingVoteComponentError.None)
                    writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
                else
                    base.Render(writer);
                return;
            }
            base.Render(writer);
        }

        public string GetVoteClientClick(bool sign)
        {
            return HttpUtility.HtmlAttributeEncode(string.Concat(@"Bitrix.RatingVoting.vote(""", Component.ComponentGuid, @""", ", sign.ToString().ToLowerInvariant(), @"); return false;"));
        }
    }

    /// <summary>
    /// Параметр компонента "Рейтинговое голосование"
    /// </summary>
    public enum RatingVoteComponentParameter
    {
        BoundEntityTypeId = 1,
        BoundEntityId,
        CustomPropertyEntityId,
        //RatingId,
        PositiveVoteValue,
        NegativeVoteValue,
        RolesAuthorizedToVote,
        //EnableAjax,
        BannedUsers
    }

    [Flags]
    public enum RatingVoteComponentError
    {
        None = 0x0,
        General = 0x1,
        UserIsNotAllowedToVote = 0x2,
        UserHasAlreadyVoted = 0x4,
        BoundEntityTypeIdIsNotDefined = 0x8,
        BoundEntityIdIsNotDefined = 0x10,
        //RatingIsNotFound = 0x20,
        //RatingCounterIsNotFound = 0x40,
        RatingVotingIsNotFound = 0x20
    }
}

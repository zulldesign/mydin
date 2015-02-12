using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Components;
using Bitrix.Security;
using System.Collections.ObjectModel;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.IO;
using System.Text;
using Bitrix.DataTypes;
using Bitrix.Configuration;
using Bitrix.Components.Editor;

namespace Bitrix.Main.Components
{
    /// <summary>
    /// Параметры
    /// </summary>
    public enum UserListComponentParameter
    {
        FilterByGender = 1,
        FilterByDayOfBirth,
        FilterByMonthOfBirth,
        SortBy,
        SortDirection,
        //PermittedGroupIds,
        //ProhibitedGroupIds,
        ProhibitedUserIds,
        UserProfileUrlTemplate,
		FilterByNameToShowUp,
		FilterByUserCustomProperty,
		UserCustomPropertyFilterSettings
        
        
    }

    /// <summary>
    /// Фильтрация по полу
    /// </summary>
    public enum UserListComponentFilterByGender
    {
        /// <summary>
        /// без фильтрации
        /// </summary>
        None = 0,
        /// <summary>
        /// мужской
        /// </summary>
        Male,
        /// <summary>
        /// женский
        /// </summary>
        Female
    }

    public enum UserListComponentError
    {
        None = 0,
        General = 1,
        PageDoesNotExist = 2,
        DataReadingFailed = 4
    }

    public partial class UserListComponent : BXComponent
    {
        /// <summary>
        /// Получить ключ параметра
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetParameterKey(UserListComponentParameter parameter)
        {
            return parameter.ToString();
        }

        /// <summary>
        /// Получить ключ заголовка параметра
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static string GetParameterTilteKey(UserListComponentParameter parameter)
        {
            return string.Concat("Param.", parameter.ToString());
        }

        private UserListComponentError _componentError = UserListComponentError.None;
        public UserListComponentError ComponentError
        {
            get { return _componentError; }
            protected set { _componentError = value; }
        }

        public string[] GetComponentErrorMessages()
        {
            if(_componentError == UserListComponentError.None)
                return new string[0];

            List<string> result = new List<string>();
            if ((_componentError & UserListComponentError.General) != 0)
                result.Add(GetMessageRaw(string.Concat("Error.", UserListComponentError.General.ToString("G"))));
            if ((_componentError & UserListComponentError.PageDoesNotExist) != 0)
                result.Add(GetMessageRaw(string.Concat("Error.", UserListComponentError.PageDoesNotExist.ToString("G"))));
            if ((_componentError & UserListComponentError.DataReadingFailed) != 0)
                result.Add(GetMessageRaw(string.Concat("Error.", UserListComponentError.DataReadingFailed.ToString("G"))));

            return result.ToArray();
        }

        private IList<UserWrapper> _userListRO = null;
        /// <summary>
        /// Список пользователей
        /// </summary>
        public IList<UserWrapper> UserList
        {
            get { return _userListRO ?? (_userListRO = new ReadOnlyCollection<UserWrapper>(InternalUserList)); }
        }

        public string UserProfileUrlTemplate
        {
            get
            {
                return Parameters.GetString(GetParameterKey(UserListComponentParameter.UserProfileUrlTemplate), DefaultUserProfileUrlTemplate);
            }
        }

        private List<UserWrapper> _userList = null;
        protected IList<UserWrapper> InternalUserList
        {
            get { return _userList ?? (_userList = new List<UserWrapper>()); }
        }

        public string DefaultUserProfileUrlTemplate
        {
            get { return "UserProfile.aspx?id=#UserId#"; }
        }
        public string DefaultSortBy
        {
            get { return "NameToShowUp"; }
        }

        public string SortBy
        {
            get
            {
                return Parameters.GetString(GetParameterKey(UserListComponentParameter.SortBy), DefaultSortBy);
            }
            set
            {
                Parameters[GetParameterKey(UserListComponentParameter.SortBy)] = value;
            }
        }

        public BXOrderByDirection SortDirection
        {
            get
            {
                string r = Parameters.GetString(GetParameterKey(UserListComponentParameter.SortDirection));
                if (string.IsNullOrEmpty(r))
                    return BXOrderByDirection.Asc;

                BXOrderByDirection result = BXOrderByDirection.Asc;
                try
                {
                    result = (BXOrderByDirection)Enum.Parse(typeof(BXOrderByDirection), r);
                }
                catch (Exception /*exc*/)
                {
                }
                return result;
            }
            set
            {
                Parameters[GetParameterKey(UserListComponentParameter.SortDirection)] = value.ToString("G");
            }
        }

        public UserListComponentFilterByGender FilterByGender
        {
            get
            {
                string r = Parameters.GetString(GetParameterKey(UserListComponentParameter.FilterByGender));
                return !string.IsNullOrEmpty(r) ? (UserListComponentFilterByGender)Enum.Parse(typeof(UserListComponentFilterByGender), r) : UserListComponentFilterByGender.None;
            }
            set
            {
                Parameters[GetParameterKey(UserListComponentParameter.FilterByGender)] = value.ToString("G");
            }
        }

        public int FilterByMonthOfBirth
        {
            get { return Parameters.GetInt(GetParameterKey(UserListComponentParameter.FilterByMonthOfBirth), 0); }
            set
            {
                if (value < 1 || value > 12)
                    value = 0;
                Parameters[GetParameterKey(UserListComponentParameter.FilterByMonthOfBirth)] = value.ToString();
            }
        }

        public int FilterByDayOfBirth
        {
            get { return Parameters.GetInt(GetParameterKey(UserListComponentParameter.FilterByDayOfBirth), 0); }
            set
            {
                if (value < 1 || value > 31)
                    value = 0;
                Parameters[GetParameterKey(UserListComponentParameter.FilterByDayOfBirth)] = value.ToString();
            }
        }

		public string FilterByNameToShowUp
		{
			get { return Parameters.GetString(GetParameterKey(UserListComponentParameter.FilterByNameToShowUp), String.Empty); }

			set
			{
				Parameters[GetParameterKey(UserListComponentParameter.FilterByNameToShowUp)] = value.ToString();
			}
		}

        #region UserRoles
        /*
        public string[] PermittedGroupIds
        {
            get 
            {
                List<string> r = Parameters.GetListString(GetParameterKey(UserListComponentParameter.PermittedGroupIds));
                return r != null && r.Count > 0 ? r.ToArray() : new string[0];
            }
            set 
            {
                Parameters[GetParameterKey(UserListComponentParameter.PermittedGroupIds)] = BXStringUtility.ListToCsv(value);
            }
        }

        public string[] ProhibitedGroupIds
        {
            get
            {
                List<string> r = Parameters.GetListString(GetParameterKey(UserListComponentParameter.ProhibitedGroupIds));
                return r != null && r.Count > 0 ? r.ToArray() : new string[0];
            }
            set
            {
                Parameters[GetParameterKey(UserListComponentParameter.ProhibitedGroupIds)] = BXStringUtility.ListToCsv(value);
            }
        }
        */
        #endregion

        private IList<int> _prohibitedUserIds = null;
        public IList<int> ProhibitedUserIds
        {
            get
            {
                if (_prohibitedUserIds != null)
                    return _prohibitedUserIds;

                string sourceStr = Parameters[GetParameterKey(UserListComponentParameter.ProhibitedUserIds)];
                if (string.IsNullOrEmpty(sourceStr) || (sourceStr = sourceStr.Trim()).Length == 0)
                    return new ReadOnlyCollection<int>(new int[0]);

                List<int> result = new List<int>();
                string[] sourceArray = sourceStr.Split(new char[] { ',' });
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    int id;
                    string idStr = sourceArray[i];
                    if (idStr == null || (idStr = idStr.Trim()).Length == 0 || !Int32.TryParse(idStr, out id))
                        continue;
                    result.Add(id);
                }
                return new ReadOnlyCollection<int>(result);
            }
            set
            {
                if (value == null || value.Count == 0)
                    Parameters[GetParameterKey(UserListComponentParameter.ProhibitedUserIds)] = string.Empty;
                else
                {
                    string[] r = new string[value.Count];
                    for (int i = 0; i < value.Count; i++)
                        r[i] = value[i].ToString();
                     Parameters[GetParameterKey(UserListComponentParameter.ProhibitedUserIds)] = string.Join(",", r);
                }
            }
        }

        private BXParamsBag<object> _replaceParams = null;
        public BXParamsBag<object> ReplaceParams
        {
            get { return _replaceParams != null ? _replaceParams : (_replaceParams = new BXParamsBag<object>()); }
        }

		public BXParamsBag<object> UserCustomPropertyFilterSettings
		{
			get
			{
				return BXParamsBag<object>.FromString(Parameters.GetString("UserCustomPropertyFilterSettings", string.Empty));
			}
			set
			{
				Parameters["UserCustomPropertyFilterSettings"] = value.ToString();
			}
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            BXPagingParams pagingParams = PreparePagingParams();
            if (IsCached(pagingParams))
                return;
 
            BXFilter f = new BXFilter(
                new BXFilterItem(BXUser.Fields.IsApproved, BXSqlFilterOperators.Equal, true),
                new BXFilterItem(BXUser.Fields.IsLockedOut, BXSqlFilterOperators.Equal, false));

			//
			string filterByNameToShowUp = FilterByNameToShowUp;
			if (filterByNameToShowUp != null && filterByNameToShowUp.Length > 0)
				f.Add(new BXFilterItem(BXUser.Fields.NameToShowUp, BXSqlFilterOperators.Like, filterByNameToShowUp));

            //пол
            UserListComponentFilterByGender filterByGender = FilterByGender;
            if (filterByGender != UserListComponentFilterByGender.None)
                f.Add(new BXFilterItem(BXUser.Fields.Gender, BXSqlFilterOperators.Equal, filterByGender == UserListComponentFilterByGender.Male ? "M" : "F"));

            //месяц даты рождения
            int filterByMonthOfBirth = FilterByMonthOfBirth;
            if (filterByMonthOfBirth > 0)
                f.Add(new BXFilterItem(BXUser.Fields.MonthOfBirth, BXSqlFilterOperators.Equal, filterByMonthOfBirth));

            //день даты рождения
            int filterByDayOfBirth = FilterByDayOfBirth;
            if (filterByDayOfBirth > 0)
                f.Add(new BXFilterItem(BXUser.Fields.DayOfBirth, BXSqlFilterOperators.Equal, filterByDayOfBirth));

			if (UserCustomPropertyFilterSettings != null && UserCustomPropertyFilterSettings.Count > 0)
			{
				BXCustomFieldCollection userCustomFields = BXCustomEntityManager.GetFields(BXUser.GetCustomFieldsKey());
				foreach (KeyValuePair<string, object> kv in UserCustomPropertyFilterSettings)
				{
					BXCustomField userCustomField;
					if (!userCustomFields.TryGetValue(kv.Key, out userCustomField))
						continue;
					f.Add(new BXFilterItem(BXUser.Fields.GetCustomField(kv.Key), userCustomField.Multiple ? BXSqlFilterOperators.In : BXSqlFilterOperators.Equal, kv.Value));
				}
			}

            IList<int> prohibitedUserIds = ProhibitedUserIds;
            if (prohibitedUserIds.Count > 0)
                f.Add(new BXFilterNot(new BXFilterItem(BXUser.Fields.UserId, BXSqlFilterOperators.In, prohibitedUserIds)));

            BXOrderBy o = new BXOrderBy();

            string sortBy = SortBy;
            if (string.IsNullOrEmpty(sortBy))
                sortBy = DefaultSortBy;
            if (sortBy[0] == '-')
                o.Add(BXUser.Fields.GetCustomField(sortBy.Substring(1)), SortDirection);
            else
                o.Add(BXUser.Fields.GetFieldByKey(sortBy), SortDirection);

            BXParamsBag<object> replaceItems = ReplaceParams;
            bool isPageLegal = true;
            BXQueryParams q = PreparePaging(
                pagingParams,
                delegate() { return BXUser.Count(f); },
                replaceItems,
                out isPageLegal
            );


            if (!isPageLegal)
            {
                _componentError |= UserListComponentError.PageDoesNotExist;
                return;
            }

            BXUserCollection c = null;
            try
            {
                c = BXUser.GetList(
                    f,
                    o,
                    new BXSelectAdd(
						BXUser.Fields.CustomFields.DefaultFields,
                        BXUser.Fields.NameToShowUp,
                        BXUser.Fields.Image
                        ),
                    q,
                    BXTextEncoder.EmptyTextEncoder
                    );
            }
            catch
            {
                _componentError |= UserListComponentError.DataReadingFailed;
            }

            if(c != null)
                for (int i = 0; i < c.Count; i++)
                    InternalUserList.Add(new UserWrapper(c[i], this));

            IncludeComponentTemplate();
        }

        #region BXComponent
        protected override void PreLoadComponentDefinition()
        {
            Title = GetMessageRaw("Title");
            Description = GetMessageRaw("Description");
            Icon = "images/icon.gif";
            Group = new BXComponentGroup("Auth", GetMessageRaw("Category"), 100, BXComponentGroup.Utility);
            BXCategory mainCategory = BXCategory.Main;

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.UserProfileUrlTemplate),
                new BXParamText(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.UserProfileUrlTemplate)), DefaultUserProfileUrlTemplate, mainCategory)
                );


			ParamsDefinition.Add(
				GetParameterKey(UserListComponentParameter.FilterByNameToShowUp),
				new BXParamText(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.FilterByNameToShowUp)), String.Empty, mainCategory)
			);

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.FilterByGender),
                new BXParamSingleSelection(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.FilterByGender)), UserListComponentFilterByGender.None.ToString("G"), mainCategory)
                );

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.FilterByMonthOfBirth),
                new BXParamText(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.FilterByMonthOfBirth)), string.Empty, mainCategory)
                );

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.FilterByDayOfBirth),
                new BXParamText(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.FilterByDayOfBirth)), string.Empty, mainCategory)
                );

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.SortBy),
                new BXParamSingleSelection(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.SortBy)), DefaultSortBy, mainCategory)
                );

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.SortDirection),
                new BXParamSingleSelection(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.SortDirection)), BXOrderByDirection.Asc.ToString("G"), mainCategory)
                );

            #region UserRoles
            /*
            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.PermittedGroupIds),
                new BXParamMultiSelection(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.PermittedGroupIds)), string.Empty, mainCategory)
                );

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.ProhibitedGroupIds),
                new BXParamMultiSelection(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.ProhibitedGroupIds)), string.Empty, mainCategory)
                );
           */
            #endregion

            ParamsDefinition.Add(
                GetParameterKey(UserListComponentParameter.ProhibitedUserIds),
                new BXParamMultilineText(GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.ProhibitedUserIds)), string.Empty, mainCategory)
                );

			BXCategory customFieldCategory = BXCategory.CustomField;

			ParamsDefinition.Add(GetParameterKey(UserListComponentParameter.FilterByUserCustomProperty),
				new BXParamYesNo(
					GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.FilterByUserCustomProperty)),
					false,
					customFieldCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, GetParameterKey(UserListComponentParameter.FilterByUserCustomProperty),
						GetParameterKey(UserListComponentParameter.FilterByUserCustomProperty), string.Empty)
					)
				);

			ParamsDefinition.Add(GetParameterKey(UserListComponentParameter.UserCustomPropertyFilterSettings),
				new BXParamCustomFieldFilter(
					GetMessageRaw(GetParameterTilteKey(UserListComponentParameter.UserCustomPropertyFilterSettings)),
					string.Empty,
					customFieldCategory,
					BXUser.GetCustomFieldsKey(),
					new ParamClientSideActionGroupViewMember(ClientID, GetParameterKey(UserListComponentParameter.UserCustomPropertyFilterSettings),
						new string[] { GetParameterKey(UserListComponentParameter.FilterByUserCustomProperty) })
					)
				);


            ParamsDefinition.Add(BXParametersDefinition.Cache);
            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisableLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
        }

        private IList<BXParamValue> BuildUpParameterValueList(UserListComponentParameter parameter, Type parameterValueEnumType, IList<BXParamValue> parameterValueList)
        {
            string key = GetParameterKey(parameter);
            if (parameterValueList == null)
            {
                parameterValueList = ParamsDefinition[key].Values;
                if (parameterValueList.Count > 0)
                    parameterValueList.Clear();
            }
            foreach (string s in Enum.GetNames(parameterValueEnumType))
                parameterValueList.Add(new BXParamValue(GetMessageRaw(string.Concat(GetParameterTilteKey(parameter), ".", s)), s));
            return parameterValueList;
        }

        protected override void LoadComponentDefinition()
        {
            BuildUpParameterValueList(UserListComponentParameter.FilterByGender, typeof(UserListComponentFilterByGender), null);

            IList<BXParamValue> sortByValues = ParamsDefinition[GetParameterKey(UserListComponentParameter.SortBy)].Values;
            if (sortByValues.Count > 0)
                sortByValues.Clear();
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.NameToShowUp"), "NameToShowUp"));
            sortByValues.Add(new BXParamValue(GetMessageRaw("Param.SortBy.DateOfRegistration"), "CreationDate"));

            BXCustomFieldCollection customFields = BXCustomEntityManager.GetFields(BXUser.GetCustomFieldsKey());
			foreach (BXCustomField customField in customFields)
			{
				string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel);
				string code = customField.Name.ToUpper();
				sortByValues.Add(new BXParamValue(title, "-" + code));
			}

            BuildUpParameterValueList(UserListComponentParameter.SortDirection, typeof(BXOrderByDirection), null);

            #region UserRoles
            /*
            IList<BXParamValue> permittedGroupValues = ParamsDefinition[GetParameterKey(UserListComponentParameter.PermittedGroupIds)].Values,
                prohibitedGroupValues = ParamsDefinition[GetParameterKey(UserListComponentParameter.ProhibitedGroupIds)].Values;
            if(permittedGroupValues.Count > 0)
                permittedGroupValues.Clear();
            if(prohibitedGroupValues.Count > 0)
                prohibitedGroupValues.Clear();
            foreach(BXRole r in BXRoleManager.GetList(null, new BXOrderBy_old("RoleName", "Asc")))
            {
                BXParamValue v = new BXParamValue(r.RoleName, r.RoleId.ToString());
                permittedGroupValues.Add(v);
                prohibitedGroupValues.Add(v);
            }
            */
            #endregion

            //IList<BXParamValue> prohibitedUserIdValues = ParamsDefinition[GetParameterKey(UserListComponentParameter.ProhibitedUserIds)].Values;
            //foreach (BXUser user in GetAllUsers())
            //    prohibitedUserIdValues.Add(new BXParamValue(user.NameToShowUp, user.UserId.ToString()));

            BXParametersDefinition.SetPaging(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
            BXParametersDefinition.SetPagingUrl(ParamsDefinition, ClientID, BXParametersDefinition.PagingParamsOptions.DisablePreLoad | BXParametersDefinition.PagingParamsOptions.DisableShowAll);
        }
        #endregion

        //private string GetParameterValueByCustomFieldName(string name)
        //{
        //    return string.Concat("-", name.ToUpperInvariant());
        //}

        //private string GetCustomFieldNameByParameterValue(string val)
        //{
        //    return val[0] == '-' ? val.Substring(1) : val;
        //}

    }

    public class UserWrapper
    {
        private BXUser _user = null;
        private UserListComponent _component = null;
        public UserWrapper(BXUser user, UserListComponent component)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            _user = user;

            if (component == null)
                throw new ArgumentNullException("component");
            _component = component;
        }

        public BXUser User
        {
            get { return _user; }
        }

        private BXCustomPropertyCollection _customValues = null;
        public BXCustomPropertyCollection CustomValues
        {
            get { return _customValues ?? (_customValues = _user.CustomValues); }
        }

        public string NameToShowUp
        {
            get { return _user.NameToShowUp; }
        }

        static private int _defaultImageWidth = 100;
        static public int DefaultImageWidth
        {
            get { return _defaultImageWidth; }
            set { _defaultImageWidth = value > 0 ? value : 90; }
        }

        static private int _defaultImageHeight = 100;
        static public int DefaultImageHeight
        {
            get { return _defaultImageHeight; }
            set { _defaultImageHeight = value > 0 ? value : 90; }
        }

        private int? _imageWidth = null;
        public int ImageWidth
        {
            get
            {
                CalculateImageSize();
                return _imageWidth ?? DefaultImageWidth;
            }
        }

        private int? _imageHeight = null;
        public int ImageHeight
        {
            get
            {
                CalculateImageSize();
                return _imageHeight ?? DefaultImageHeight;
            }
        }

        private bool _isImageSizeCalculated = false;
        private void CalculateImageSize()
        {
            if (_isImageSizeCalculated)
                return;

            _imageWidth = null;
            _imageHeight = null;
            _isImageSizeCalculated = true;

            BXFile f = _user.Image;
            if (f == null)
                return;

            int srcWidth = f.Width,
                srcHeight = f.Height;
            if (srcHeight <= 0 || srcWidth <= 0)
                return;

            int maxWidth = BXConfigurationUtility.Options.User.AvatarMaxWidth, 
                maxHeight = BXConfigurationUtility.Options.User.AvatarMaxHeight;

            if (((maxWidth == 0 || srcWidth <= maxWidth) && (maxHeight == 0 || srcHeight <= maxHeight)))
            {
                _imageWidth = srcWidth;
                _imageHeight = srcHeight;
                return;
            }

            int w = srcWidth,
                h = srcHeight;
            double coef = (double)w / h;
            if (maxWidth > 0 && w > maxWidth)
            {
                w = maxWidth;
                h = Convert.ToInt32(Math.Truncate(maxWidth / coef));
            }
            if (maxHeight > 0 && h > maxHeight)
            {
                h = maxHeight;
                w = Convert.ToInt32(Math.Truncate(maxHeight * coef));
            }
            _imageWidth = w;
            _imageHeight = h;
        }

        private string _imageFileUrl = null;
        public string ImageFileUrl
        {
            get
            {
                if (_imageFileUrl != null)
                    return _imageFileUrl;
                BXFile f = _user.Image;
                return (_imageFileUrl = f != null ? f.TextEncoder.Decode(f.FilePath) : string.Empty);
            }
        }

        private string _userProfileUrl = null;
        public string UserProfileUrl
        {
            get
            {
                if (_userProfileUrl != null)
                    return _userProfileUrl;
                string template = _component.UserProfileUrlTemplate;
                if (string.IsNullOrEmpty(template))
                    return (_userProfileUrl = string.Empty);

				BXParamsBag<object> replace = new BXParamsBag<object>();
				replace["UserId"] = _user.UserId;

				_userProfileUrl = _component.ResolveTemplateUrl(template, replace);
				return _userProfileUrl;

				//StringBuilder sb = new StringBuilder();
				//string userId = _user.UserId.ToString();
				//int curIndex = 0,
				//    paramInd = -1;
				//while (curIndex < template.Length - 1 && (paramInd = template.IndexOf("#USERID#", curIndex, StringComparison.InvariantCultureIgnoreCase)) >= 0)
				//{
				//    sb.Append(template.Substring(curIndex, paramInd - curIndex));
				//    sb.Append(userId);
				//    curIndex = paramInd + 8;
				//}

				//sb.Append(template.Substring(curIndex));
				//return (_userProfileUrl = sb.ToString());
            }
        }

        public DateTime DateOfRegistration
        {
            get { return _user.CreationDate; }
        }
    }

    public class UserListTemplate : BXComponentTemplate<UserListComponent>
    { 
        protected string[] GetErrorMessages()
        {
            string[] result = Component.GetComponentErrorMessages();
            if (result.Length > 0)
                for (int i = 0; i < result.Length; i++)
                    result[i] = HttpUtility.HtmlEncode(result[i]);

            return result;
        }
    }
}

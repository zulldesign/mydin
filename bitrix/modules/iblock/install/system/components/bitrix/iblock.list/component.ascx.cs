using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Components;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using System.Collections.Specialized;
using Bitrix.Security;
using System.Net;
using Bitrix.DataTypes;
using System.Collections.ObjectModel;
using System.Web.SessionState;
using Bitrix.Components.Editor;
using System.Web.Hosting;
using Bitrix.IO;

namespace Bitrix.IBlock.Components
{
	public partial class IBlockListComponent : BXComponent
	{
		private IBlockListComponentError error = IBlockListComponentError.None;
		public IBlockListComponentError ComponentError
		{
			get { return this.error; }
		}

		public Dictionary<int, bool> isSelected = new Dictionary<int, bool>();

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var iblocks = BXIBlock.GetList(new BXFilter(
											new BXFilterItem(BXIBlock.Fields.CheckPermissions, BXSqlFilterOperators.Equal, "Y"),
											new BXFilterItem(BXIBlock.Fields.ID, BXSqlFilterOperators.In, IBlockIds)),
										null);
			var replace = new BXParamsBag<object>();

			foreach (var iblock in iblocks)
			{
				var sectionCollection = new BXIBlockSectionCollection();
				if (ShowSections)
				{
					sectionCollection = BXIBlockSection.GetList(
							new BXFilter(new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblock.Id)),
							new BXOrderBy(new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)),
							null,
							null,
							BXTextEncoder.HtmlTextEncoder
						);
				}
				replace.Add("IBlockId", iblock.Id);
				replace.Add("IBlockCode", iblock.Code);
				var iblockUrl = ResolveTemplateUrl(IBlockUrlTemplate, replace);

				bool selected = false;
				bool existsSelected = false;
				List<SectionWrapper> sWrappers = new List<SectionWrapper>();

				for (var i=0; i< sectionCollection.Count;i++ )
				{
					var section = sectionCollection[i];
					replace.Clear();
					replace.Add("IBlockId", iblock.Id);
					replace.Add("SectionId", section.Id);
					replace.Add("IBlockCode", iblock.Code);
					
					var link = ResolveTemplateUrl(SectionUrlTemplate, replace);
					var url = BXPath.ToVirtualRelativePath(BXUri.GetPath(BXUri.ToRelativeUri(BXSefUrlManager.CurrentUrl.ToString())));

				

					var wrapper = new SectionWrapper(section);
					
					sWrappers.Add(wrapper);
				}

				IBlocks.Add(new IBlockWrapper(iblock, sWrappers, this, iblockUrl, SectionUrlTemplate));
			}

			IncludeComponentTemplate();
		}

		public int IBlockTypeId
		{
			get { return ComponentCache.GetInt("IBlockTypeID", 0); }
			set { ComponentCache["IBlockTypeID"] = value; }
		}

		string sortBy = null;
		public string SortBy
		{
			get { return this.sortBy ?? (this.sortBy = Parameters.GetString("SortBy", string.Empty)); }
			set { Parameters["SortBy"] = this.sortBy = value ?? string.Empty; }
		}

		public string IBlockUrlTemplate
		{
			get { return Parameters.GetString("IBlockUrlTemplate", "/#IBlockCode#/list.aspx"); }
			set { Parameters["IBlockUrlTemplate"] = value; }
		}

		public string SectionUrlTemplate
		{
			get { return Parameters.GetString("SectionUrlTemplate", "/#IBlockCode#/list.aspx?section_id=#SectionId#"); }
			set { Parameters["SectionUrlTemplate"] = value; }
		}

		string sortOrder = null;
		public string SortOrder
		{
			get { return this.sortOrder ?? (this.sortOrder = Parameters.GetString("SortOrder", "Desc")); }
			set { Parameters["SortOrder"] = this.sortOrder = value ?? string.Empty; }
		}

		List<int> iblockTypeIds;

		List<int> IBlockTypeIds
		{
			get
			{
				return iblockTypeIds ?? (iblockTypeIds = Parameters.GetListInt("IBlockTypeIds"));
			}
		}

		List<int> iblockIds;

		List<int> IBlockIds
		{
			get
			{
				return iblockIds ?? (iblockIds = Parameters.GetListInt("IBlocks"));
			}
		}

		List<IBlockWrapper> iblocks;

		public List<IBlockWrapper> IBlocks
		{
			get { return iblocks ?? (iblocks = new List<IBlockWrapper>()); }
		}

		public bool ShowSections
		{
			get { return Parameters.GetBool("ShowSections", true); }
			set { Parameters["ShowSections"] = value.ToString(); }
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";

			Group = new BXComponentGroup("iblocks", GetMessageRaw("Group"), 10, BXComponentGroup.Content);

			BXCategory mainCategory = BXCategory.Main,
				addSettingsCategory = BXCategory.AdditionalSettings,
				dataSourceCategory = BXCategory.DataSource;

			#region Main

			ParamsDefinition.Add(
				"IBlockTypeIds",
				new BXParamMultiSelection(
					GetMessageRaw("Param.IBlockTypeIds"), 
					string.Empty, 
					mainCategory
					)
				);

			ParamsDefinition.Add(
				"IBlocks",
				new BXParamDoubleList(
					GetMessageRaw("Param.IBlocks"),
					string.Empty,
					mainCategory
					)
				);

			ParamsDefinition.Add(
				"ShowSections",
				new BXParamYesNo(
					GetMessageRaw("Param.ShowSections"),
					true,
					mainCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "ShowSections", "SectionsOn", "SectionsOff")
				)
			);

			ParamsDefinition.Add(
				"IBlockUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.IBlockUrlTemplate"),
					"/#IBlockCode#/list.aspx",
					BXCategory.UrlSettings
					)
				);

			ParamsDefinition.Add(
				"SectionUrlTemplate",
				new BXParamText(
					GetMessageRaw("Param.SectionUrlTemplate"),
					"/#IBlockCode#/list.aspx?section=#SectionId#",
					BXCategory.UrlSettings
					)
				);

			#endregion

			#region DataSourceCategory
			ParamsDefinition.Add(
				"SortBy",
				new BXParamSingleSelection(
					GetMessageRaw("Param.SortBy"), string.Empty, dataSourceCategory));

			ParamsDefinition.Add(
				"SortOrder",
				new BXParamSort(
					GetMessageRaw("Param.SortOrder"), true, dataSourceCategory));
			#endregion

		}

		protected override void LoadComponentDefinition()
		{
			#region IBlockType
			List<BXParamValue> typeValues = new List<BXParamValue>();

			BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(
				null,
				new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)),
				null,
				null,
				BXTextEncoder.EmptyTextEncoder);

			foreach (BXIBlockType iblockType in iblockTypes)
				typeValues.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

			ParamsDefinition["IBlockTypeIds"].Values = typeValues;
			ParamsDefinition["IBlockTypeIds"].RefreshOnDirty = true;
			#endregion

			var iblocks = BXIBlock.GetList(new BXFilter(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.In, Parameters.GetListInt("IBlockTypeIds"))), null);

			ParamsDefinition["IBlocks"].Values = iblocks.ConvertAll<BXParamValue>(x => new BXParamValue(x.Name, x.Id.ToString()));

			#region Sorting
			List<BXParamValue> sortingFields = new List<BXParamValue>();
			sortingFields.Add(new BXParamValue(GetMessageRaw("NotSelected"), string.Empty));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ElementID"), "ID"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.Sort"), "Sort"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.UpdateDate"), "UpdateDate"));
			sortingFields.Add(new BXParamValue(GetMessageRaw("Field.ElementName"), "Name"));


			ParamsDefinition["SortBy"].Values = sortingFields;

			#endregion
		}
	}

	public partial class IBlockListTemplate : BXComponentTemplate<IBlockListComponent>
	{
	}

	[FlagsAttribute]
	public enum IBlockListComponentError
	{
		None = 0x0,
		IBlockIsNotFound = 0x1
	}

	public class SectionWrapper
	{
		public BXIBlockSection Section { get; set; }

		public SectionWrapper(BXIBlockSection section)
		{
			if (section == null)
				throw new ArgumentNullException("section");

			this.Section = section;
		}
	}

	public class IBlockWrapper
	{

		public IBlockWrapper(BXIBlock iblock, IList<SectionWrapper> sections, 
			IBlockListComponent component, string iblockDetailUrl, string sectionUrlTemplate)
		{
			if (iblock == null)
				throw new ArgumentNullException("iblock");

			if (sections == null)
				throw new ArgumentNullException("sections");

			if (component == null)
				throw new ArgumentNullException("component");

			if (iblockDetailUrl == null)
				throw new ArgumentNullException("iblockDetailUrl");

			if (sectionUrlTemplate == null)
				throw new ArgumentNullException("sectionUrlTemplate");

			this.iblock = iblock;
			this.sections = new List<SectionWrapper>();
			this.sections.AddRange(sections);
			this.component = component;
			this.IBlockDetailUrl = iblockDetailUrl;
			this.SectionDetailUrlTemplate = sectionUrlTemplate;
		}

		BXIBlock iblock;
		List<SectionWrapper> sections;
		IBlockListComponent component;

		public string IBlockDetailUrl { get; set; }

		string SectionDetailUrlTemplate { get; set; }

		public bool IsSelected { get; set; }
		public bool IsCurrentSelected { get; set; }

		public string GetSectionUrl(int sectionId)
		{
			var replace = new BXParamsBag<object>();
			replace.Add("IBlockId", iblock.Id);
			replace.Add("SectionId", sectionId);
			replace.Add("IBlockCode", iblock.Code);
			return component.ResolveTemplateUrl(SectionDetailUrlTemplate, replace);
		}

		public BXIBlock IBlock 
		{
			get { return iblock; }
			set 
			{ 
				if (value == null) throw new ArgumentNullException("IBlock");
				iblock = value;
			}
		}

		

		public List<SectionWrapper> Sections
		{
			get { return sections ?? (sections = new List<SectionWrapper>()) ; }
		}

	}

}

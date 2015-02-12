using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using System.Collections.Generic;
using System.ComponentModel;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Js;
using Bitrix.DataLayer;

#region Attributes
[PersistChildren(false)]
[ParseChildren(true)]
#endregion
public partial class OperationsEdit : BXControl
{
	Dictionary<string, BXOperationsEditRoleInfo> roles;
	BXOperationsEditOperations operations;
	BXOperationsEditOperationState defaultOperationState = BXOperationsEditOperationState.Inherited;
	BXOperationsEditInheritedOperationState defaultInheritedOperationState = BXOperationsEditInheritedOperationState.Denied;
	protected OperationsState state;
	bool validate = true;
	bool showLegend;
	bool showLegendDontModify;
	bool showNotes = true;
	BXOperationsEditAllowedOperationState allowedStates = BXOperationsEditAllowedOperationState.All;
	LegendData legendData = new LegendData();

	#region Attributes
	[Browsable(false)]
	#endregion
	public Dictionary<string, BXOperationsEditRoleInfo> Roles
	{
		get
		{
			if (roles == null)
				roles = new Dictionary<string, BXOperationsEditRoleInfo>();
			return roles;
		}
	}
	#region Attributes
	[Browsable(false)]
	#endregion
	public BXOperationsEditOperations Operations
	{
		get
		{
			if (operations == null)
				operations = new BXOperationsEditOperations();
			return operations;
		}
	}
	#region Attributes
	[DefaultValue(BXOperationsEditOperationState.Inherited)]
	#endregion
	public BXOperationsEditOperationState DefaultOperationState
	{
		get
		{
			return defaultOperationState;
		}
		set
		{
			defaultOperationState = value;
		}
	}
	#region Attributes
	[DefaultValue(BXOperationsEditInheritedOperationState.Denied)]
	#endregion
	public BXOperationsEditInheritedOperationState DefaultInheritedOperationState
	{
		get
		{
			return defaultInheritedOperationState;
		}
		set
		{
			defaultInheritedOperationState = value;
		}
	}
	#region Attributes
	[Browsable(false)]
	#endregion
	public OperationsState State
	{
		get
		{
			if (state == null)
				state = new OperationsState(this);
			return state;
		}
	}
	#region Attributes
	[DefaultValue(true)]
	#endregion
	public bool ValidateBeforeRender
	{
		get
		{
			return validate;
		}
		set
		{
			validate = value;
		}
	}
	#region Attributes
	[Browsable(false)]
	#endregion
	public string JSName
	{
		get
		{
			return ClientID + "_js";
		}
	}
	#region Attributes
	[DefaultValue(false)]
	#endregion
	public bool ShowLegend
	{
		get
		{
			return showLegend;
		}
		set
		{
			showLegend = value;
		}
	}
	#region Attributes
	[DefaultValue(false)]
	#endregion
	public bool ShowLegendDontModify
	{
		get
		{
			return showLegendDontModify;
		}
		set
		{
			showLegendDontModify = value;
		}
	}
	#region Attributes
	[DefaultValue(true)]
	#endregion
	public bool ShowNotes
	{
		get
		{
			return showNotes;
		}
		set
		{
			showNotes = value;
		}
	}
	#region Attributes
	[DefaultValue(BXOperationsEditAllowedOperationState.All)]
	#endregion
	public BXOperationsEditAllowedOperationState AllowedStates
	{
		get
		{
			return allowedStates;
		}
		set
		{
			allowedStates = value;
		}
	}
	#region Attributes
	[PersistenceMode(PersistenceMode.Attribute)]
	#endregion
	public LegendData LegendText
	{
		get
		{
			return legendData;
		}
	}

	static int RoleClass(BXRole role)
	{
		switch (role.RoleId)
		{
			case 2:
				return 0;
			case 3:
				return 1;
			case 1:
				return 2;
			default:
				return 3;
		}
	}
	
	protected IDictionary BuildRolesData()
	{
		Dictionary<string, object> rolesData = new Dictionary<string, object>();
		if (roles != null)
			foreach (KeyValuePair<string, BXOperationsEditRoleInfo> r in roles)
			{
				Dictionary<string, object> roleData = new Dictionary<string, object>();
				roleData.Add("title", r.Value.Title);
				roleData.Add("description", r.Value.Description);
				roleData.Add("icon", r.Value.IconClass);

				Dictionary<string, object> operationsData = new Dictionary<string, object>();
				if (operations != null)
					foreach (string op in operations.Keys)
						if (r.Value.Operations.ContainsKey(op))
						{
							Dictionary<string, object> operationData = new Dictionary<string, object>();
							operationData.Add("state", (int)r.Value.Operations[op].State);
							operationData.Add("inheritedState", r.Value.Operations[op].InheritedState);
							if (r.Value.Operations[op].AllowDontModify)
								operationData.Add("allowDontModify", true);
							if (ShowNotes && !string.IsNullOrEmpty(r.Value.Operations[op].NoteHtml))
								operationData.Add("note", r.Value.Operations[op].NoteHtml);
							operationsData.Add(op, operationData);
						}
				roleData.Add("operations", operationsData);

				rolesData.Add(r.Key, roleData);
			}
		return rolesData;
	}
	protected IEnumerable BuildOperationsData()
	{
		List<object> operationsData = new List<object>();
		if (operations != null)
			foreach (BXOperationsEditOperationDescription op in operations)
			{
				Dictionary<string, object> operationData = new Dictionary<string, object>();
				if (op.IsSeparator)
					operationData.Add("separator", true);
				else
					operationData.Add("id", op.Id);
				operationData.Add("title", op.Title); //BXFileAuthorizationManager.GetActionTypeDisplayName(op)
				operationsData.Add(operationData);
			}
		return operationsData;
	}

	public void FillStandardRoles(bool nameAsKey)
	{
		BXRoleCollection roles = BXRoleManager.GetList(
			new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)),
			new BXOrderBy_old("RoleName", "Asc")
		);
		FillStandardRoles(roles, nameAsKey);
	}
	public void FillStandardRoles(List<BXRole> roles, bool nameAsKey)
	{
		roles.Sort(delegate(BXRole a, BXRole b)
		{
			int i = RoleClass(a).CompareTo(RoleClass(b));
			if (i == 0)
				return string.Compare(a.RoleName, b.RoleName, true);
			return i;
		});
		foreach (BXRole role in roles)
		{
			string key = nameAsKey ? role.RoleName : role.RoleId.ToString();
			Roles.Add(key, new BXOperationsEditRoleInfo(
				role.Title,
				string.IsNullOrEmpty(role.Comment) ? (role.RoleId <= 3 ? GetMessageRaw("RoleDescription.SystemRole") : GetMessageRaw("RoleDescription.CustomRole")) : role.Comment,
				role.RoleId <= 3 ? "extra-image" : null
			));
		}
	}
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		string state = IsPostBack ? Request.Form[UniqueID] : null;
		if (!string.IsNullOrEmpty(state))
			State.Load(state);
	}
	protected override void OnPreRender(EventArgs e)
	{
		BXPage.RegisterScriptInclude("~/bitrix/js/Main/security_ui.js");
		BXPage.RegisterThemeStyle("BXSecurityUI.css");
		ScriptManager.RegisterOnSubmitStatement(this, GetType(), ClientID, string.Format("document.getElementById('{0}_state').value = {1}.PersistState();", ClientID, JSName));
		base.OnPreRender(e);
	}
	protected override void Render(HtmlTextWriter writer)
	{
		if (ValidateBeforeRender && state != null)
			state.Validate(true);
		base.Render(writer);
	}

	public class OperationsState : IEnumerable<KeyValuePair<string, Dictionary<string, BXOperationsEditOperationState>>>
	{
		OperationsEdit owner;
		Dictionary<string, Dictionary<string, BXOperationsEditOperationState>> state = new Dictionary<string, Dictionary<string, BXOperationsEditOperationState>>();

		public Dictionary<string, BXOperationsEditOperationState> this[string role]
		{
			get
			{
				if (role == null)
					throw new ArgumentNullException("role");
				if (!state.ContainsKey(role))
					throw new InvalidOperationException(string.Format("State doesn't contain role '{0}'", role));
				return state[role];
			}
		}
		public BXOperationsEditOperationState this[string role, string operation]
		{
			get
			{
				if (role == null)
					throw new ArgumentNullException("role");
				if (operation == null)
					throw new ArgumentNullException("operation");
				if (!state.ContainsKey(role))
					throw new InvalidOperationException(string.Format("State doesn't contain role '{0}'", role));
				if (!state[role].ContainsKey(operation))
					throw new InvalidOperationException(string.Format("State for role '{0}' doesn't contain operation '{0}'", operation));
				return state[role][operation];
			}
			set
			{
				if (role == null)
					throw new ArgumentNullException("role");
				if (operation == null)
					throw new ArgumentNullException("operation");
				if (!state.ContainsKey(role))
					state.Add(role, new Dictionary<string, BXOperationsEditOperationState>());
				state[role][operation] = value;
			}
		}

		internal OperationsState(OperationsEdit owner)
		{
			this.owner = owner;
		}
		internal void Load(string serializedData)
		{
			if (serializedData == null)
				throw new ArgumentNullException("serializedData");
			IDictionary loaded = BXSerializer.Deserialize(serializedData) as IDictionary;
			if (loaded == null)
				throw new InvalidOperationException("State couldn't be loaded");
			state.Clear();
			foreach (DictionaryEntry e in loaded)
			{
				IDictionary opsLoaded = e.Value as IDictionary;
				if (opsLoaded == null)
					throw new InvalidOperationException(string.Format("No operations data for role '{0}'", e.Key));

				Dictionary<string, BXOperationsEditOperationState> ops = new Dictionary<string, BXOperationsEditOperationState>();
				state.Add(e.Key.ToString(), ops);
				foreach (DictionaryEntry o in opsLoaded)
					ops.Add(o.Key.ToString(), (BXOperationsEditOperationState)o.Value);
			}
		}
		public string ToJSON()
		{
			return BXJSUtility.BuildJSON(state);
		}

		public bool ContainsRole(string role)
		{
			if (role == null)
				throw new ArgumentNullException("role");
			return state.ContainsKey(role);
		}
		public void SetRoleDefault(string role)
		{
			SetRoleDefault(role, owner.DefaultOperationState);
		}
		public void SetRoleFromData(string role)
		{
			if (role == null)
				throw new ArgumentNullException("role");

			Dictionary<string, BXOperationsEditOperationState> ops = null;
			if (!state.TryGetValue(role, out ops))
				state.Add(role, ops = new Dictionary<string, BXOperationsEditOperationState>());

			ops.Clear();
			foreach (string op in owner.Operations.Keys)
			{
				BXOperationsEditRoleInfo roleInfo = null;
				BXOperationsEditOperationInfo operationInfo = null;
				if (owner.roles != null
					&& owner.roles.TryGetValue(role, out roleInfo)
					&& roleInfo.operations != null
					&& roleInfo.operations.TryGetValue(op, out operationInfo))
					ops.Add(op, operationInfo.State);
				else
					ops.Add(op, owner.DefaultOperationState);
			}
		}
		public void SetRoleDefault(string role, BXOperationsEditOperationState initialState)
		{
			if (role == null)
				throw new ArgumentNullException("role");

			Dictionary<string, BXOperationsEditOperationState> ops = null;
			if (!state.TryGetValue(role, out ops))
				state.Add(role, ops = new Dictionary<string, BXOperationsEditOperationState>());

			ops.Clear();
			foreach (string op in owner.Operations.Keys)
				ops.Add(op, initialState);

		}
		public void RemoveRole(string role)
		{
			if (role == null)
				throw new ArgumentNullException("role");
			state.Remove(role);
		}
		public void Clear()
		{
			state.Clear();
		}
		public void Validate(bool fillMissingOperations)
		{
			if (state.Count > 0 && owner.roles == null)
			{
				IEnumerator e = state.Keys.GetEnumerator();
				e.MoveNext();
				throw new InvalidOperationException(string.Format("Available roles list is empty, but state contains at least 1 role ('{0}')", e.Current));
			}
			foreach (KeyValuePair<string, Dictionary<string, BXOperationsEditOperationState>> role in state)
			{
				if (!owner.roles.ContainsKey(role.Key))
					throw new InvalidOperationException(string.Format("Role '{0}' is not in a list of available roles", role.Key));
				Dictionary<string, BXOperationsEditOperationState> ops = role.Value;
				if (ops.Count > 0 && owner.operations == null)
				{
					IEnumerator e = ops.Keys.GetEnumerator();
					e.MoveNext();
					throw new InvalidOperationException(string.Format("Available operations list is empty, but state for role '{0}' contains at least 1 operation ('{1}')", role.Key, e.Current));
				}


				foreach (string op in owner.operations.Keys)
				{
					if (!ops.ContainsKey(op))
						if (fillMissingOperations)
						{
							BXOperationsEditOperationState fillState = owner.DefaultOperationState;
							BXOperationsEditOperationInfo info;
							if (owner.roles[role.Key].operations != null && owner.roles[role.Key].operations.TryGetValue(op, out info))
								fillState = info.State;
							ops.Add(op, fillState);
						}
						else
							throw new InvalidOperationException(string.Format("Operation '{0}' for role '{1}' is not in a list of available operations", op, role.Key));
					if (ops[op] != BXOperationsEditOperationState.DontModify
					&& (owner.AllowedStates & (BXOperationsEditAllowedOperationState)(1 << ((int)ops[op] + 1))) == BXOperationsEditAllowedOperationState.None) //conversion hack from linear to power of 2
						throw new InvalidOperationException(string.Format("Operation state '{0}' is not allowed!", ops[op]));
				}
			}
		}


		#region IEnumerable<KeyValuePair<string,Dictionary<string,BXOperationsEditOperationState>>> Members

		public IEnumerator<KeyValuePair<string, Dictionary<string, BXOperationsEditOperationState>>> GetEnumerator()
		{
			return state.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
	public class LegendData
	{
		private string allow;
		private string deny;
		private string inheritAllow;
		private string inheritDeny;
		private string dontModify;

		public string Allow
		{
			get
			{
				return allow;
			}
			set
			{
				allow = value;
			}
		}
		public string Deny
		{
			get
			{
				return deny;
			}
			set
			{
				deny = value;
			}
		}
		public string InheritAllow
		{
			get
			{
				return inheritAllow;
			}
			set
			{
				inheritAllow = value;
			}
		}
		public string InheritDeny
		{
			get
			{
				return inheritDeny;
			}
			set
			{
				inheritDeny = value;
			}
		}
		public string DontModify
		{
			get
			{
				return dontModify;
			}
			set
			{
				dontModify = value;
			}
		}

		internal LegendData()
		{

		}
	}
}

public class BXOperationsEditRoleInfo
{
	internal Dictionary<string, BXOperationsEditOperationInfo> operations;
	string title;
	string description;
	string iconClass;

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
	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			description = value;
		}
	}
	public string IconClass
	{
		get
		{
			return iconClass;
		}
		set
		{
			iconClass = value;
		}
	}
	public Dictionary<string, BXOperationsEditOperationInfo> Operations
	{
		get
		{
			if (operations == null)
				operations = new Dictionary<string, BXOperationsEditOperationInfo>();
			return operations;
		}
	}

	public BXOperationsEditRoleInfo(string title, string description, string iconClass)
	{
		this.title = title;
		this.description = description;
		this.iconClass = iconClass;
	}
}

public class BXOperationsEditOperationInfo
{
	public BXOperationsEditOperationState State;
	public BXOperationsEditInheritedOperationState InheritedState;
	public bool AllowDontModify;
	public string NoteHtml;

	public BXOperationsEditOperationInfo()
		: this(BXOperationsEditOperationState.Inherited, BXOperationsEditInheritedOperationState.Denied)
	{

	}
	public BXOperationsEditOperationInfo(BXOperationsEditOperationState state, BXOperationsEditInheritedOperationState inheritedState)
	{
		this.State = state;
		this.InheritedState = inheritedState;
	}
}

public class BXOperationsEditOperationDescription
{
	private string title;
	private string id;

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
	public string Id
	{
		get
		{
			return id;
		}
	}
	public bool IsSeparator
	{
		get
		{
			return id == null;
		}
	}

	public BXOperationsEditOperationDescription(string title)
	{
		this.title = title;
	}
	public BXOperationsEditOperationDescription (string id, string title)
	{
		if (id == null)
			throw new ArgumentNullException("id");
		this.id = id;
		this.title = title;
	}
}

public class BXOperationsEditOperations : IList<BXOperationsEditOperationDescription>, IDictionary<string, string>
{
	List<BXOperationsEditOperationDescription> items = new List<BXOperationsEditOperationDescription>();
	Dictionary<string, BXOperationsEditOperationDescription> index = new Dictionary<string,BXOperationsEditOperationDescription>();

	
	public void AddSeparator(string title)
	{
		Add(new BXOperationsEditOperationDescription(title));
	}

	#region IList<BXOperationsEditOperationDescription> Members

	public int IndexOf(BXOperationsEditOperationDescription item)
	{
		return items.IndexOf(item);
	}

	public void Insert(int index, BXOperationsEditOperationDescription item)
	{
		items.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		items.RemoveAt(index);
	}

	public BXOperationsEditOperationDescription this[int index]
	{
		get
		{
			return items[index];
		}
		set
		{
			items[index] = value;
		}
	}

	#endregion

	#region ICollection<BXOperationsEditOperationDescription> Members

	public void Add(BXOperationsEditOperationDescription item)
	{
		if (item.Id != null)
			index.Add(item.Id, item);
		items.Add(item);
	}

	public void Clear()
	{
		items.Clear();
		index.Clear();
	}

	public bool Contains(BXOperationsEditOperationDescription item)
	{
		return items.Contains(item);
	}

	public void CopyTo(BXOperationsEditOperationDescription[] array, int arrayIndex)
	{
		items.CopyTo(array, arrayIndex);
	}

	public int Count
	{
		get
		{
			return items.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return ((IList<BXOperationsEditOperationDescription>)items).IsReadOnly;
		}
	}

	public bool Remove(BXOperationsEditOperationDescription item)
	{
		bool result = items.Remove(item);
		if (result && item.Id != null)
			index.Remove(item.Id);
		return result;
	}

	#endregion

	#region IEnumerable<BXOperationsEditOperationDescription> Members

	public IEnumerator<BXOperationsEditOperationDescription> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	#endregion

	#region IEnumerable Members

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)items).GetEnumerator();
	}

	#endregion

	#region IDictionary<string,string> Members

	public void Add(string key, string value)
	{
		BXOperationsEditOperationDescription item = new BXOperationsEditOperationDescription(key, value);
		index.Add(key, item);
		items.Add(item); 
	}

	public bool ContainsKey(string key)
	{
		return index.ContainsKey(key);
	}

	public ICollection<string> Keys
	{
		get
		{
			return index.Keys;
		}
	}

	public bool Remove(string key)
	{
		if (index.ContainsKey(key))
			items.Remove(index[key]);
		return index.Remove(key);
	}

	public bool TryGetValue(string key, out string value)
	{
		BXOperationsEditOperationDescription item;
		if (!index.TryGetValue(key, out item))
		{
			value = null;
			return false;
		}

		value = item.Title;
		return true;
	}

	public ICollection<string> Values
	{
		get
		{
			throw new NotSupportedException("The property Values is not supported.");
		}
	}

	public string this[string key]
	{
		get
		{
			if (!index.ContainsKey(key))
				throw new InvalidOperationException("No element with such key.");
			return index[key].Title;
		}
		set
		{
			if (index.ContainsKey(key))
				index[key].Title = value;
			else
				Add(key, value);
		}
	}

	#endregion

	#region ICollection<KeyValuePair<string,string>> Members

	public void Add(KeyValuePair<string, string> item)
	{
		throw new NotSupportedException("The method Add is not supported.");
	}

	public bool Contains(KeyValuePair<string, string> item)
	{
		throw new NotSupportedException("The method Contains is not supported.");
	}

	public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
	{
		throw new NotSupportedException("The method CopyTo is not supported.");
	}

	public bool Remove(KeyValuePair<string, string> item)
	{
		throw new NotSupportedException("The method Remove is not supported.");
	}

	#endregion

	#region IEnumerable<KeyValuePair<string,string>> Members

	IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
	{
		foreach (KeyValuePair<string, BXOperationsEditOperationDescription> op in index)
			yield return new KeyValuePair<string, string>(op.Key, op.Value.Title);
	}

	#endregion
}

public enum BXOperationsEditOperationState
{
	Inherited = -1,
	Denied = 0,
	Allowed,
	DontModify
}

public enum BXOperationsEditInheritedOperationState
{
	Denied = 0,
	Allowed
}

[Flags]
public enum BXOperationsEditAllowedOperationState
{
	None = 0,
	Inherited = 1,
	Denied = 2,
	Allowed = 4,
	All = Inherited | Denied | Allowed,
	AllButInherited = All ^ Inherited,
	AllButDenied = All ^ Denied,
	AllButAllowed = All ^ Allowed
}
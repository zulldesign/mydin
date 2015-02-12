using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using Bitrix.DataTypes;
using Bitrix.Services;
using Bitrix.Services.Js;
using Bitrix.UI.Wizards;
using System.Web.Configuration;
using System.Text;
using Bitrix.Security;

namespace Bitrix.UI
{
	public partial class AdminWizardHost : BXControl
	{
		protected BXWizard wizard;
		private InnerContext context;
		protected BXWizardResultView view;
		private BXParamsBag<object> innerState;
		protected bool doFinish;
		private bool getBehaviour;
		protected RenderMethod renderButtons;
		protected string storeState;
		

		private string url;
		public string Url
		{
			get
			{
				return url ?? "";
			}
			set
			{
				url = ProcessUrl(value);
			}
		}

		private string ProcessUrl(string value)
		{
			UriBuilder uri = new UriBuilder(new Uri(Request.Url, value));
			NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
			query.Remove("inplace");
			uri.Query = query.ToString();
			return uri.Uri.AbsoluteUri;
		}

		private string wizardPath;
		public string WizardPath
		{
			get
			{
				return wizardPath;
			}
			set
			{
				wizardPath = value;
			}
		}

		private string locale;
		public string Locale
		{
			get
			{
				return locale;
			}
			set
			{
				locale = value;
				BXLoc.CurrentLocale = value;
			}
		}

		private BXWizardNavigation navigation;
		public BXWizardNavigation Navigation
		{
			get { return navigation; }
		}

		public string WizardTitleHtml
		{
			get
			{
				return wizard != null ? Encode(wizard.Title) : null;
			}
		}

		public string WizardStepTitleHtml
		{
			get
			{
				return view != null ? Encode(view.TitleHtml) : null;
			}
		}

		public ICollection<string> WizardStepErrorHtml
		{
			get
			{
				return view != null ? view.ErrorHtml : null;
			}
		}

		private string wizardButtonsPlaceHolderId;
		public string WizardButtonsPlaceHolderId
		{
			get
			{
				return wizardButtonsPlaceHolderId;
			}
			set
			{
				wizardButtonsPlaceHolderId = value;
			}
		}

		private string wizardNavigationPlaceHolderId;
		public string WizardNavigationPlaceHolderId
		{
			get
			{
				return wizardNavigationPlaceHolderId;
			}
			set
			{
				wizardNavigationPlaceHolderId = value;
			}
		}

		public event EventHandler<AdminWizardHostFinishEventArgs> Finish;
		public event EventHandler<AdminWizardHostInitStateEventArgs> InitState;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			BXCsrfToken.ValidatePost();


			storeState = Request.Form["wizard$state"];
			if (Request["action_special$restart"] != null)
			{
				DeleteSession();
				getBehaviour = true;
			}
			else if (Request["action_special$retry"] != null)
				getBehaviour = true;

			bool initWizard = false;
			if (storeState != null)
				LoadSession(storeState);
			else if (!string.IsNullOrEmpty(WizardPath))
			{
				CreateSession();
				initWizard = true;
			}

			if (!doFinish)
				DoProcess(initWizard);
			else
				DoFinish();

			if (Request.QueryString["inplace"] != null)
			{
				using (HtmlTextWriter h = new HtmlTextWriter(Response.Output))
					Render(h);
				Response.End();
			}
		}

		private void DoProcess(bool initWizard)
		{
			if (context == null)
				return;

			wizard = new BXWizard(WizardPath, Locale, initWizard ? navigation : null);
			if (initWizard)
				wizard.Initialize(context);

			string step = innerState.GetString("step", wizard.FirstStep);
			string action = null;
			BXCommonBag parameters = null;
			if (string.Equals(Request.RequestType, "POST", StringComparison.OrdinalIgnoreCase) && !getBehaviour)
			{
				string transferedStep = Request.Form["step"];
				if (!string.IsNullOrEmpty(transferedStep) && !string.Equals(step, transferedStep, StringComparison.InvariantCultureIgnoreCase))
				{
					SetErrorView(GetMessageRaw("Error.Desync"));
					return;
				}

				foreach (string key in Request.Form.AllKeys)
				{
					if (key != null && key.StartsWith("action$", StringComparison.OrdinalIgnoreCase))
					{
						int start = "action$".Length;
						int i = key.IndexOf('$', start);
						if (i == -1)
						{
							action = key.Substring(start).ToLowerInvariant();
						}
						else
						{
							action = key.Substring(start, i - start).ToLowerInvariant();
							step = key.Substring(i).ToLowerInvariant();
						}
						break;
					}
				}
				parameters = new FormBag(Request);
			}
			
			if (action == null)
				action = innerState.GetString("action") ?? "";

			BXWizardResult result = wizard.Act(step, action, parameters, context);
			ProcessResult(step, action, result);
		}

		private void ProcessResult(string step, string action, BXWizardResult result)
		{
			innerState.Remove("step");
			innerState.Remove("action");
			if (result != null)
			{
				switch (result.Type)
				{
					case BXWizardResultType.HtmlView:
						{
							SetHtmlView((BXWizardResultHtmlView)result, RenderButtons);
							innerState["step"] = step;
							//innerState["action"] = action;
						}
						break;
					case BXWizardResultType.Navigate:
						{
							string to = null;
							switch (((BXWizardResultNavigate)result).Target)
							{
								case BXWizardResultNavigateTarget.Next:
									to = wizard.GetNextStep(step, context);
									break;
								case BXWizardResultNavigateTarget.Previous:
									to = wizard.GetPrevStep(step, context);
									break;
								case BXWizardResultNavigateTarget.First:
									to = wizard.FirstStep;
									break;
								case BXWizardResultNavigateTarget.Last:
									to = wizard.LastStep;
									break;
							}
							if (string.IsNullOrEmpty(to))
							{
								SetErrorView(string.Format(
									GetMessageRaw("Error.NavigationFailed"),
									((BXWizardResultNavigate)result).Target,
									step
								));
								
							}
							else 
							{
								ProcessResult(
									to,
									"",
									wizard.Act(to, "", null, context)
								);
							}
							return;
						}
						break;
					case BXWizardResultType.Action:
						{
							BXWizardResultAction ra = (BXWizardResultAction)result;
							ProcessResult(
								!string.IsNullOrEmpty(ra.Step) ? ra.Step : step,
								ra.Action ?? "",
								wizard.Act(
									!string.IsNullOrEmpty(ra.Step) ? ra.Step : step,
									ra.Action ?? "",
									ra.Parameters,
									context
								)
							);
							return;
						}
						break;
					case BXWizardResultType.Finish:
						{
							if (Request.QueryString["inplace"] != null)
							{
								doFinish = true;
								SaveSession();
								return;
							}
							DoFinish();
							return;
						}
					case BXWizardResultType.Error:
						{
							SetErrorView(((BXWizardResultError)result).HtmlMessage);
							return;
						}
				}
			}
			SaveSession();
		}

		private void DoFinish()
		{
			AdminWizardHostFinishEventArgs args = new AdminWizardHostFinishEventArgs(context.State);
			try
			{
				if (Finish != null)
					Finish(this, args);
				if (!args.CancelDefaultFinish)
				{
					BXWizardResultHtmlView view = new BXWizardResultHtmlView();
					view.TitleHtml = GetMessageRaw("Finish.Title");
					view.RenderMethod = delegate(HtmlTextWriter writer, Control control)
					{
						writer.Write(GetMessageRaw("Finish.Body"));
					};
					SetHtmlView(view, RenderButtons);
				}
			}
			finally
			{
				if (args.DeleteSession)
					DeleteSession();
			}
		}

		private void LoadSession(string state)
		{
			ObjectStateFormatter sf = new ObjectStateFormatter();
			using (StringReader sr = new StringReader((string)sf.Deserialize(state)))
			using (XmlReader xml = XmlReader.Create(sr))
				innerState = (BXParamsBag<object>)new XmlSerializer(typeof(BXParamsBag<object>)).Deserialize(xml);

			context = new InnerContext(this, innerState.Get<BXParamsBag<object>>("state") ?? new BXParamsBag<object>());
			navigation = innerState.Get<BXWizardNavigation>("navigation");
			doFinish = innerState.GetBool("finish");
		}

		private void SaveSession()
		{
			if (doFinish)
				innerState["finish"] = true;

			innerState["state"] = context.state;
			innerState["navigation"] = navigation;

			using (StringWriter s = new StringWriter())
			{
				new System.Xml.Serialization.XmlSerializer(typeof(BXWizardNavigation)).Serialize(s, navigation);
				innerState["navigation"] = navigation;
			}

			using (StringWriter sw = new StringWriter())
			{
				using (XmlWriter xml = XmlWriter.Create(sw))
					new XmlSerializer(innerState.GetType()).Serialize(xml, innerState);
				ObjectStateFormatter sf = new ObjectStateFormatter();
				storeState = sf.Serialize(sw.ToString());
			}
		}

		private void DeleteSession()
		{
			storeState = null;
		}

		private void CreateSession()
		{
			context = new InnerContext(this, new BXParamsBag<object>());
			innerState = new BXParamsBag<object>();
			navigation = new BXWizardNavigation();
			if (InitState != null)
				InitState(this, new AdminWizardHostInitStateEventArgs(context.State));
		}

		protected void RenderButtons(HtmlTextWriter writer, Control control)
		{
			if (view == null)
				return;
			
			
			for(int i = view.Buttons.Count - 1; i >= 0; i--)
			{
				BXWizardResultViewButton button = view.Buttons[i];

				if (string.IsNullOrEmpty(button.Action))
					continue;

				string cssClass = button.CssClass;
				writer.Write(@"<a class=""wizard-button");
				if (string.IsNullOrEmpty(button.CssClass))
				{
					writer.Write(" wizard-");
					HttpUtility.HtmlAttributeEncode(button.Action, writer);
					writer.Write("-button");
				}
				writer.Write(@""" href="""" onclick=""this.blur(); return WizardSubmit('");
				HttpUtility.HtmlAttributeEncode(JSEncode(button.Action), writer);
				if (!string.IsNullOrEmpty(button.Step))
				{
					writer.Write("$");
					HttpUtility.HtmlAttributeEncode(JSEncode(button.Step), writer);
				}
				writer.Write(@"');""><span>");
							
				string text = button.Text;
				if (string.IsNullOrEmpty(text))
					text = GetStandardButtonTitle(button.Action);
				HttpUtility.HtmlEncode(text, writer);

				writer.Write("</span></a>");
			}
		}
		protected void RenderFatalErrorButtons(HtmlTextWriter writer, Control control)
		{
			writer.Write(
@"<a 
	class=""wizard-button"" 
	href=""""
	onclick=""this.blur(); return WizardSubmit('action_special$restart', true);""
><span>{0}</span></a>",
				HttpUtility.HtmlAttributeEncode(GetMessage("ButtonTitle.Restart"))
			);
			writer.Write(
@"<a 
	class=""wizard-button"" 
	href=""""
	onclick=""this.blur(); return WizardSubmit('action_special$retry', true);""
><span>{0}</span></a>",
				HttpUtility.HtmlEncode(GetMessage("ButtonTitle.Retry"))
			);
		}
		protected void RenderNavigation(HtmlTextWriter writer, Control control)
		{
			if (navigation == null)
				return;

			BXWizardNavigationStep selected = null;
			foreach (BXWizardNavigationStep s in navigation)
			{
				if (s.Visible && s.Selected)
					selected = s;
			}
			bool done = true;
			bool first = true;
			int num = 0;
			writer.Write(@"<table width=""100%"" cellspacing=""0"" cellpadding=""0"" id=""menu"">");
			foreach (BXWizardNavigationStep s in navigation)
			{
				if (!s.Visible)
					continue;
				if (!first)
					writer.Write(@"<tr class=""menu-separator""><td colspan=""3""></td></tr>");

				first = false;
				num++;
				writer.Write(@"<tr ");
				if (done)
				{
					writer.Write(@"class=""");
					writer.Write(s.Selected ? "selected" : "done");
					writer.Write(@"""");

				}
				writer.Write(@"><td class=""menu-number"">");
				writer.Write(num);
				writer.Write(@"</td><td class=""menu-name"">");
				writer.Write(s.TitleHtml);
				writer.Write(@"</td><td class=""menu-end""></td></tr>");
				if (s == selected)
					done = false;
			}
			writer.Write(@"</table>");
		}


		private string GetStandardButtonTitle(string action)
		{
			if (string.Equals("next", action, StringComparison.OrdinalIgnoreCase))
				return GetMessageRaw("ButtonTitle.Next");
			else if (string.Equals("prev", action, StringComparison.OrdinalIgnoreCase))
				return GetMessageRaw("ButtonTitle.Prev");
			else if (string.Equals("finish", action, StringComparison.OrdinalIgnoreCase))
				return GetMessageRaw("ButtonTitle.Finish");
			else if (string.Equals("cancel", action, StringComparison.OrdinalIgnoreCase))
				return GetMessageRaw("ButtonTitle.Cancel");
			else
				return "";
		}

		private void SetHtmlView(BXWizardResultHtmlView htmlView, RenderMethod buttons)
		{
			view = htmlView;
			HtmlView.SetRenderMethodDelegate(htmlView.RenderMethod);
			if (!string.IsNullOrEmpty(wizardButtonsPlaceHolderId))
			{
				Control placeholder = NamingContainer.FindControl(wizardButtonsPlaceHolderId);
				if (placeholder != null)
					placeholder.SetRenderMethodDelegate(buttons);
				renderButtons = buttons;
			}

			if (!string.IsNullOrEmpty(wizardNavigationPlaceHolderId))
			{
				Control placeholder = NamingContainer.FindControl(wizardNavigationPlaceHolderId);
				if (placeholder != null)
					placeholder.SetRenderMethodDelegate(RenderNavigation);
			}
		}
		private void SetErrorView(string html)
		{
			BXWizardResultHtmlView view = new BXWizardResultHtmlView();
			view.TitleHtml = GetMessageRaw("Error.Title");
			view.RenderMethod = delegate(HtmlTextWriter writer, Control control)
			{
				writer.Write(html);
			};
			SetHtmlView(view, RenderFatalErrorButtons);
		}

		public string RenderBeginForm()
		{
			return
				@"<form action="""
				+ HttpUtility.HtmlAttributeEncode(Url)
				+ @""" method=""post"" enctype=""multipart/form-data"" id=""wizard_form"">";
		}

		public string RenderEndForm()
		{
			return "</form>";
		}

		class InnerContext : BXWizardContext
		{
			AdminWizardHost host;
			internal readonly BXParamsBag<object> state;
			private BXCommonBag bag;

			public InnerContext(AdminWizardHost host, BXParamsBag<object> state)
			{
				this.host = host;
				this.state = state;
				this.bag = new BXDictionaryBag(state);
			}

			public override BXCommonBag State
			{
				get { return bag; }
			}

			public override string Locale
			{
				get { return host.locale; }
			}

			public override BXWizardNavigation Navigation
			{
				get { return host.navigation; }
			}
		}
		class FormBag : BXCommonBag
		{
			NameValueCollection container;
			HttpFileCollection files;

			public FormBag(HttpRequest request)
			{
				this.container = request.Form;
				this.files = request.Files;
			}

			public override void Add(string key, object value)
			{
				throw new NotSupportedException();
			}
			public override void Clear()
			{
				throw new NotSupportedException();
			}
			public override int Count
			{
				get { return container.Count; }
			}
			public override bool IsReadOnly
			{
				get { return true; }
			}
			public override bool Remove(string key)
			{
				throw new NotSupportedException();
			}
			public override bool TryGetValue(string key, out object value)
			{
				string[] values = container.GetValues(key);
				if (values != null)
				{
					value = values.Length == 1 ? (object)values[0] : values;
					return true;

				}

				List<HttpPostedFile> fl = null;
				for (int i = 0; i < files.Count; i++)
				{
					if (!string.Equals(key, files.GetKey(i), StringComparison.InvariantCultureIgnoreCase))
						continue;
					if (fl == null)
						fl = new List<HttpPostedFile>();
					fl.Add(files.Get(i));
				}

				if (fl != null)
				{
					value = fl.Count == 1 ? (object)fl[0] : fl.ToArray();
					return true;
				}

				value = null;
				return false;
			}
			public override object this[string key]
			{
				get
				{
					object obj;
					if (!TryGetValue(key, out obj))
						return null;
					return obj;
				}
				set
				{
					throw new NotSupportedException();
				}
			}
			public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
			{
				for (int i = 0; i < container.Count; i++)
				{
					string[] values = container.GetValues(i);
					yield return new KeyValuePair<string, object>(container.GetKey(i), values.Length == 1 ? (object)values[0] : values);
				}
				if (files.Count != 0)
				{
					Dictionary<string, List<HttpPostedFile>> fd = new Dictionary<string, List<HttpPostedFile>>(StringComparer.InvariantCultureIgnoreCase);
					for (int i = 0; i < files.Count; i++)
					{
						string key = files.GetKey(i);
						List<HttpPostedFile> list;
						if (!fd.TryGetValue(key, out list))
							fd[key] = list = new List<HttpPostedFile>();
						list.Add(files.Get(i));
					}
					foreach (KeyValuePair<string, List<HttpPostedFile>> p in fd)
						yield return new KeyValuePair<string, object>(p.Key, p.Value.Count == 1 ? (object)p.Value[0] : p.Value.ToArray());
				}
			}

			public override bool TryGetString(string key, out string value)
			{
				value = container[key];
				return value != null;
			}
			public override bool TryGetValue<R>(string key, out R value)
			{
				if (typeof(R) == typeof(string[]) || typeof(R) == typeof(IEnumerable<string>))
				{
					value = (R)(object)container.GetValues(key);
					return value != null;
				}
				else if (typeof(R) == typeof(string))
				{
					value = (R)(object)container[key];
					return value != null;
				}
				else if (typeof(R) == typeof(HttpPostedFile[]) || typeof(R) == typeof(IEnumerable<HttpPostedFile>))
				{
					List<HttpPostedFile> files = new List<HttpPostedFile>();

					for (int i = this.files.Count - 1; i >= 0; i--)
					{
						if (string.Equals(this.files.GetKey(i), key, StringComparison.InvariantCultureIgnoreCase))
							files.Add(this.files.Get(i));
					}

					value = files.Count > 0 ? (R)(object)files.ToArray() : default(R);
					return value != null;
				}
				else if (typeof(R) == typeof(HttpPostedFile))
				{
					value = (R)(object)files[key];
					return value != null;
				}
				else
					return base.TryGetValue<R>(key, out value);
			}
		}
	}

	public class AdminWizardHostFinishEventArgs : EventArgs
	{
		internal AdminWizardHostFinishEventArgs(BXCommonBag state)
		{
			this.state = state;
		}

		private BXCommonBag state;
		public BXCommonBag State
		{
			get
			{
				return state;
			}
		}
		
		private bool cancelDefaultFinish;
		public bool CancelDefaultFinish
		{
			get
			{
				return cancelDefaultFinish;
			}
			set
			{
				cancelDefaultFinish = value;
			}
		}

		private bool deleteSession = true;
		public bool DeleteSession
		{
			get
			{
				return deleteSession;
			}
			set
			{
				deleteSession = value;
			}
		}
	}
	public class AdminWizardHostInitStateEventArgs : EventArgs
	{
		internal AdminWizardHostInitStateEventArgs(BXCommonBag state)
		{
			this.state = state;
		}

		private BXCommonBag state;
		public BXCommonBag State
		{
			get
			{
				return state;
			}
		}
	}
}
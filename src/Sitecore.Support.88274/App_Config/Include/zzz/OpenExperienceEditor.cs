using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ExperienceEditor.Utils;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Pipelines.HasPresentation;
using Sitecore.Publishing;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
  [Serializable]
  public class OpenExperienceEditor : Command
  {
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      NameValueCollection nameValueCollection = new NameValueCollection();
      bool flag = false;
      if (context.Items.Length == 1)
      {
        Item item = context.Items[0];
        nameValueCollection["uri"] = item.Uri.ToString();
        nameValueCollection.Add("sc_lang", item.Language.ToString());
        nameValueCollection.Add("sc_version", item.Version.Number.ToString(CultureInfo.InvariantCulture));
        if (HasPresentationPipeline.Run(item))
        {
          nameValueCollection.Add("sc_itemid", item.ID.ToString());
        }
        else
        {
          flag = true;
        }
      }
      ClientPipelineArgs clientPipelineArgs = new ClientPipelineArgs(nameValueCollection);
      if (!flag)
      {
        clientPipelineArgs.Result = "yes";
        clientPipelineArgs.Parameters.Add("needconfirmation", "false");
      }
      Context.ClientPage.Start(this, "Run", clientPipelineArgs);
    }

    public override CommandState QueryState(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      if ((UIUtil.IsIE() && UIUtil.GetBrowserMajorVersion() < 7) || !Settings.WebEdit.Enabled)
      {
        return CommandState.Hidden;
      }
      return base.QueryState(context);
    }

    protected void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      if (SheerResponse.CheckModified())
      {
        if (args.Parameters["needconfirmation"] == "false" || args.IsPostBack)
        {
          if (!(args.Result == "no"))
          {
            UrlString urlString = new UrlString("/");
            urlString.Add("sc_mode", "edit");
            if (!string.IsNullOrEmpty(args.Parameters["sc_itemid"]))
            {
              urlString.Add("sc_itemid", args.Parameters["sc_itemid"]);
            }
            if (!string.IsNullOrEmpty(args.Parameters["sc_version"]))
            {
              urlString.Add("sc_version", args.Parameters["sc_version"]);
            }
            SiteContext siteContext = null;
            if (!string.IsNullOrEmpty(args.Parameters["uri"]))
            {
              Item item = Database.GetItem(ItemUri.Parse(args.Parameters["uri"]));
              if (item == null)
              {
                SheerResponse.Alert("Item not found.");
                return;
              }
              siteContext = LinkManager.GetPreviewSiteContext(item);
            }
            siteContext = (siteContext ?? Factory.GetSite(Settings.Preview.DefaultSite));
            if (siteContext == null)
            {
              SheerResponse.Alert(Translate.Text("Site \"{0}\" not found", Settings.Preview.DefaultSite));
            }
            else
            {
              string value = args.Parameters["sc_lang"];
              if (string.IsNullOrEmpty(value))
              {
                value = WebEditUtility.ResolveContentLanguage(siteContext).ToString();
                urlString.Add("sc_lang", value);
              }
              if (!string.IsNullOrEmpty(args.Parameters["sc_lang"]))
              {
                urlString.Add("sc_lang", value);
              }
              urlString["sc_site"] = siteContext.Name;
              PreviewManager.RestoreUser();
              Context.ClientPage.ClientResponse.Eval("window.open('" + urlString + "', '_blank')");
            }
          }
        }
        else
        {
          SheerResponse.Confirm("The current item does not have a layout for the current device.\n\nDo you want to open the start Web page instead?");
          args.WaitForPostBack();
        }
      }
    }
  }
}
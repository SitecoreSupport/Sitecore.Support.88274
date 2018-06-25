using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ExperienceEditor.Utils;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Publishing;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using System;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
  [Serializable]
  public class OpenExperienceEditor : Sitecore.Shell.Applications.WebEdit.Commands.OpenExperienceEditor
  {    
    protected new void Run([NotNull] ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, nameof(args));

      if (!SheerResponse.CheckModified())
      {
        return;
      }

      if (args.Parameters["needconfirmation"] == "false" || args.IsPostBack)
      {
        var url = new UrlString("/");
        url.Add("sc_mode", "edit");

        if (!string.IsNullOrEmpty(args.Parameters["sc_itemid"]))
        {
          url.Add("sc_itemid", args.Parameters["sc_itemid"]);
        }

        if (!string.IsNullOrEmpty(args.Parameters["sc_version"]))
        {
          url.Add("sc_version", args.Parameters["sc_version"]);
        }

        SiteContext site = null;
        if (!string.IsNullOrEmpty(args.Parameters["uri"]))
        {
          Item item = Database.GetItem(ItemUri.Parse(args.Parameters["uri"]));
          if (item == null)
          {
            SheerResponse.Alert(Texts.ITEM_NOT_FOUND);
            return;
          }

          site = LinkManager.GetPreviewSiteContext(item);
        }

        site = site ?? Factory.GetSite(Settings.Preview.DefaultSite);
        if (site == null)
        {
          SheerResponse.Alert(Translate.Text(Texts.Site0NotFound, Settings.Preview.DefaultSite));
          return;
        }

        string langName = args.Parameters["sc_lang"];
        if (string.IsNullOrEmpty(langName))
        {
          langName = WebEditUtility.ResolveContentLanguage(site).ToString();
        }

        if (string.IsNullOrEmpty(args.Parameters["sc_lang"]))
        {
          url.Add("sc_lang", langName);
        }

        url["sc_site"] = site.Name;

        PreviewManager.RestoreUser();
        OpenEditor(url);
      }
      else
      {
        SheerResponse.Confirm(Texts.THE_CURRENT_ITEM_DOES_NOT_HAVE_A_LAYOUT_FOR_THE_CURRENT_DEVICE_DO_YOU_WANT_TO_OPEN_THE_START_WEB_PAGE);
        args.WaitForPostBack();
      }
    }
  }
}

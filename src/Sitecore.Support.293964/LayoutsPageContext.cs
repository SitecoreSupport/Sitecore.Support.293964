using System.Collections.Generic;
using System.Xml.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;
using Sitecore.XA.Foundation.PlaceholderSettings.Pipelines.GetXmlBasedLayoutDefinition;
using System.Linq;
using Sitecore.Sites;
using Sitecore.StringExtensions;
using Sitecore.Web;

namespace Sitecore.Support.XA.Foundation.PlaceholderSettings.Services
{
  public class LayoutsPageContext: Sitecore.XA.Foundation.PlaceholderSettings.Services.LayoutsPageContext
  {
    public override IList<Item> GetSxaPlaceholderItems(string layout, string placeholderKey, Item currentItem, ID deviceId)
    {
      var layoutArgs = new GetXmlBasedLayoutDefinitionArgs
      {
        ContextItem = currentItem,
        Result = XElement.Parse(layout)
      };

      #region fix
      string realSiteName =
        Sitecore.Web.WebUtil.ParseQueryString(System.Web.HttpContext.Current.Items["SC_FORM"].ToString())["scSite"];
      if (!realSiteName.IsNullOrEmpty())
      {
        SiteContext site = SiteContext.GetSite(realSiteName);
        if (site != null)
        {
          using (new SiteContextSwitcher(site))
          {
            new AddPartialDesignsPlaceholderSettings().Process(layoutArgs);
            var xElements1 = layoutArgs.Result.Descendants("d").Where(d => new ID(d.Attribute("id")?.Value) == deviceId);
            var pElements1 = xElements1.Descendants("p").Where(element => element.Attribute("key")?.Value == placeholderKey);

            var items1 = pElements1
              .Select(p => p.Attribute("md"))
              .Select(attr => currentItem.Database.GetItem(attr.Value)).ToList();

            return items1;
          }
        }
      }
      #endregion
      new AddPartialDesignsPlaceholderSettings().Process(layoutArgs);
      var xElements = layoutArgs.Result.Descendants("d").Where(d => new ID(d.Attribute("id")?.Value) == deviceId);
      var pElements = xElements.Descendants("p").Where(element => element.Attribute("key")?.Value == placeholderKey);

      var items = pElements
        .Select(p => p.Attribute("md"))
        .Select(attr => currentItem.Database.GetItem(attr.Value)).ToList();

      return items;
    }
  }
}
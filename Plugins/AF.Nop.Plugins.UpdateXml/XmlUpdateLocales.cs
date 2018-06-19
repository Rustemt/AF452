using Nop.Core.Plugins;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AF.Nop.Plugins.XmlUpdate
{
    public static class XmlUpdateLocales
    {
        public static void Install(BasePlugin plugin)
        {
            foreach (var d in Collection)
            {
                plugin.AddOrUpdatePluginLocaleResource(d.Key, d.Value);
            }
        }

        public static void Uninstall(BasePlugin plugin)
        {
            foreach (var d in Collection)
            {
                plugin.DeletePluginLocaleResource(d.Key);
            }
        }

        public static Dictionary<string, string> Collection
        {
            get
            {
                var list = new Dictionary<string, string>()
                {
                    {"AF.XmlUpdate.Provider", "Provider"},
                    {"AF.XmlUpdate.Provider.Hint", "Enter the provider name"},
                    {"AF.XmlUpdate.SourceUrl", "Source Url"},
                    {"AF.XmlUpdate.SourceUrl.Hint", "Enter the usl to get the XML file"},
                    {"AF.XmlUpdate.XmlRootNode", "Xml Root"},
                    {"AF.XmlUpdate.XmlRootNode.Hint", "The name of the root element in the XML file"},
                    {"AF.XmlUpdate.XmlItemNode", "Xml Item"},
                    {"AF.XmlUpdate.XmlItemNode.Hint", "The name of node that represnts the product data"},
                    {"AF.XmlUpdate.AuthType", "Authentication"},
                    {"AF.XmlUpdate.AuthType.Hint", "Choose the type of authentication if needed."},
                    {"AF.XmlUpdate.Enabled", "Enabled"},
                    {"AF.XmlUpdate.Enabled.Hint", "Check if you want to get data from this provider"},
                    {"AF.XmlUpdate.Username", "Username"},
                    {"AF.XmlUpdate.Username.Hint", "The username to use in Basic authentication"},
                    {"AF.XmlUpdate.Password", "Password"},
                    {"AF.XmlUpdate.Password.Hint", "The password to use in Basic authentication"},
                    {"AF.XmlUpdate.Properties", "Properties"},
                    {"AF.XmlUpdate.Properties.hint", "The map between the properties define in product and the correponding properties in the XML file"},
                    {"AF.XmlUpdate.AutoResetStock", "Auto Reset Stock"},
                    {"AF.XmlUpdate.AutoResetStock.hint", "Set the StockQuantity to 0 for products that are not included in the XML file"},
                    {"AF.XmlUpdate.AutoUnpublish", "Auto Unpublish"},
                    {"AF.XmlUpdate.AutoUnpublish.hint", "Unpublish products that are not included in the XML file"},
                    {"AF.XmlUpdate.UnpublishZeroStock","Unpublish 0 Stock Products" },
                    {"AF.XmlUpdate.UnpublishZeroStock.hint","If enabled, all products mentioned in the XML file will be unpublished if their StockQuantity is 0" },
                    {"AF.XmlUpdate.Provider.Add", "Add New Provider"},
                    {"AF.XmlUpdate.Save.Success", "The provider's information has been saved successfully"},
                    {"AF.XmlUpdate.Save.Failed", "Saving the information has been failed"},
                    {"AF.XmlUpdate.Provider.Update", "Update Provider Information"},
                    {"AF.XmlUpdate.Providers", "Providers"},
                    {"AF.XmlUpdate.Providers.Manage", "Manage Providers"},
                    {"AF.XmlUpdate.Import", "Import XML"},
                    {"AF.XmlUpdate.ProductProperties", "Product's Properties"}
                };
                return list;
            }
        }
    }
}

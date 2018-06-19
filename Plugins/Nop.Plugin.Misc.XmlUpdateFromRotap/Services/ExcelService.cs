using Nop.Plugin.Misc.XmlUpdateProducts.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.XmlUpdateProducts.Services
{
    class ExcelService : IExcelService
    {
        #region Fields
        
        #endregion

        #region Ctor

        public ExcelService()
        {
        }
        #endregion

        #region Utilties  
        #endregion

        #region Methods
        public virtual void BuildExcelFile(Stream stream, IList<ReportLine> reportLines)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // ok, we can run the real code of the sample now
            using (var xlPackage = new ExcelPackage(stream))
            {
                // uncomment this line if you want the XML written out to the outputDir
                //xlPackage.DebugMode = true; 

                // get handle to the existing worksheet
                var worksheet = xlPackage.Workbook.Worksheets.Add("Products");
                //Create Headers and format them 

                //SKU	Product	Stock	Publish
                var properties = new string[]
                {
                    "SKU",
                    "Stock Qty",
                    "Product",
                    "Stock",
                    "Publish Varyant",
                    "Publish Product",
                    "Price",
                };
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i];
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }


                int row = 2;
                foreach (var p in reportLines)
                {
                    int col = 1;

                    worksheet.Cells[row, col].Value = p.SKU;
                    col++;

                    worksheet.Cells[row, col].Value = p.StockQty;
                    col++;

                    worksheet.Cells[row, col].Value = p.Product;
                    col++;

                    worksheet.Cells[row, col].Value = p.Stock;
                    col++;

                    worksheet.Cells[row, col].Value = p.PublishV;
                    col++;

                    worksheet.Cells[row, col].Value = p.PublishP;
                    col++;

                    worksheet.Cells[row, col].Value = p.Price;
                    col++;
                    
                    row++;
                }



                // we had better add some document properties to the spreadsheet 

                // set some core property values
                //var storeName = _storeInformationSettings.StoreName;
                //var storeUrl = _storeInformationSettings.StoreUrl;
                //xlPackage.Workbook.Properties.Title = string.Format("{0} products", storeName);
                //xlPackage.Workbook.Properties.Author = storeName;
                //xlPackage.Workbook.Properties.Subject = string.Format("{0} products", storeName);
                //xlPackage.Workbook.Properties.Keywords = string.Format("{0} products", storeName);
                //xlPackage.Workbook.Properties.Category = "Products";
                //xlPackage.Workbook.Properties.Comments = string.Format("{0} products", storeName);

                // set some extended property values
                //xlPackage.Workbook.Properties.Company = storeName;
                //xlPackage.Workbook.Properties.HyperlinkBase = new Uri(storeUrl);

                // save the new spreadsheet
                xlPackage.Save();
            }
        }

        #endregion
    }
}

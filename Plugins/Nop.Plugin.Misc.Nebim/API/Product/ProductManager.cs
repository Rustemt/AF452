using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using NebimV3.ApplicationCommon;
using Nop.Services.ExportImport;
namespace Nop.Plugin.Misc.NebimIntegration.API
{
    public class ProductManager
    {
        public ProductManager(NebimIntegrationSettings nebimIntegrationSettings)
        {
            ConnectionManager.ConnectDB(nebimIntegrationSettings);
        }

        //AF
        internal void CreateUpdateProduct(
            string Barcode,
                    string ProductCode,/*sku*/
                    string ProductNameTR,
                    string ProductNameEN,
                    int ItemDimTypeCode,
                    IList<Tuple<string, string, string, string, string, string, string>> combinations,//renk kodu,renk adı, renk adı, boyut kodu,boyut adı,boyut adı
                    string UnitOfMeasureCode1,
                    string CountryCode,
                    string PurcVatRate,//tax rate
                    string SellVatRate,//tax rate
                    string ItemAccountGrCode,//tax rate
                    int[] ProductHierarchyLevelCodes,
                    IList<Tuple<byte, string, string, string>> attributes,//attr type code (1-20),attr code, attr name,attr name
                    decimal price1,
                    decimal price2,
                    decimal price3,
                    string currencyCode)
        {
            try
            {
                using (NebimV3.DataConnector.JoinedSqlTransactionScope Trans = new NebimV3.DataConnector.JoinedSqlTransactionScope())
                {
                    NebimV3.Items.Product Product = new NebimV3.Items.Product();
                    Product.ProductCode = ProductCode;
                    Product.ProductDescription = ProductNameTR;
                    //kampanya grubu
                    Product.PromotionGroupCode = null;//"Sezon"; //sezon ürünleri
                    #region hierarchy

                    //TODO: yoksa önce hiyerarşi yaratılmalıdır.
                    // Ürün hiyerarşisindeki yeri
                    int? hierarchyId = FindProductHierarchy(ProductHierarchyLevelCodes);
                    if (!hierarchyId.HasValue)
                    {
                        throw new Exception("CreateUpdateProduct=> kategori*hiyerarşi eşleşrtirilemedi. product code:" + ProductCode + "hierarchy levels:" + String.Join("-", ProductHierarchyLevelCodes.Select(x => x.ToString())));
                    }
                    Product.ProductHierarchyID = 0;//ProductHierarchyLevel03.HasValue? ProductHierarchyLevel03.Value:ProductHierarchyLevel02.HasValue?ProductHierarchyLevel02.Value:ProductHierarchyLevel03.Value;

                    #endregion hierarchy

                    //NoDimension = 0,
                    //OnlyColor = 1,
                    //ColorAndOne = 2,
                    //ColorAndTwo = 3,
                    //ColorAndThree = 4,
                    Product.ItemDimTypeCode = (byte)ItemDimTypeCode;   // Örn: Renk + Beden + Ton

                    // Vergi grubu
                    Product.ItemTaxGrCode = ItemAccountGrCode;
                    Product.SaveOrUpdate();

                    // Farklı dilde ürün açıklaması için
                    string LangCode = "EN";
                    Product.Descriptions.Load();
                    NebimV3.v3Entities.LanguageDescription LanguageDescription = Product.Descriptions.Where((NebimV3.v3Entities.LanguageDescription X) => NebimV3.Library.ObjectComparer.AreEqual(X.DataLanguageCode, LangCode)).FirstOrDefault();
                    if (LanguageDescription != null)
                    {
                        LanguageDescription.Description = ProductNameEN;
                        LanguageDescription.SaveOrUpdate();
                    }

                    #region variants
                    // Renk ve boyut ekleme
                    foreach (var comb in combinations)
                    {
                        switch (ItemDimTypeCode)
                        {
                            case 0:
                                CreateProductBarcode(Product, comb.Item7);
                                break;
                            case 1:
                                CreateColorIfNotExists(comb.Item1, comb.Item2, comb.Item3);
                                Product.Variants.Load();
                                //check if variant is already exists?
                                if (Product.Variants.Any(x => x.ColorCode == comb.Item1)) break;
                                Product.AddItemVariant(comb.Item1);
                                CreateProductBarcode(Product, comb.Item7, comb.Item1);
                                break;
                            case 2:
                                CreateColorIfNotExists(comb.Item1, comb.Item2, comb.Item3);
                                CreateItemDim1IfNotExists(comb.Item4, comb.Item5, comb.Item6);
                                Product.Variants.Load();
                                //check if variant is already exists?
                                if (Product.Variants.Any(x => (x.ColorCode == comb.Item1) && (x.ItemDim1Code == comb.Item4))) break;
                                Product.AddItemVariant(comb.Item1, comb.Item4);
                                CreateProductBarcode(Product, comb.Item7, comb.Item1, comb.Item4);
                                break;
                            default:
                                break;
                        }
                    }
                    #endregion variants

                    #region attributes

                    Product.Attributes.Load();
                    for (byte i = 0; i < attributes.Count; i++)
                    {
                        var attr = attributes[i];
                        if (string.IsNullOrWhiteSpace(attr.Item2))
                            continue;
                        if (Product.Attributes.Any(x => (x.AttributeCode == attr.Item2) && (x.AttributeTypeCode == attr.Item1)))
                            continue;

                        //this.CreateProductAttributeIfNotExists(attr.Item1, attr.Item2, attr.Item3, attr.Item4);
                        NebimV3.Items.ProductAttribute Attribute = new NebimV3.Items.ProductAttribute(Product);
                        Attribute.AttributeTypeCode = attr.Item1;
                        Attribute.AttributeCode = attr.Item2;
                        Attribute.SaveOrUpdate();
                        Product.Attributes.Add(Attribute);
                    }
                    #endregion attributes

                    this.CreateProductBasePrice(Product, currencyCode, price1, price2, price3);

                    Trans.Commit();

                }


            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }


        }

        internal void CreateUpdateProduct(
            string Barcode,
                   string ProductCode,/*sku*/
                   string ProductNameTR,
                   string ProductNameEN,
                   int ItemDimTypeCode,
                   IList<Tuple<string, string, string, string, string, string, string>> combinations,//renk kodu,renk adı, renk adı, boyut kodu,boyut adı,boyut adı
                   string UnitOfMeasureCode1,
                   string CountryCode,
                   string PurcVatRate,//tax rate
                   string SellVatRate,//tax rate
                   string ItemAccountGrCode,//tax rate
                   Dictionary<int, IList<int>> ProductHierarchyLevelCodes,
                   IList<Tuple<byte, string, string, string>> attributes,//attr type code (1-20),attr code, attr name,attr name
                   decimal price,
                   decimal price2,
                   decimal price3,
                   string currencyCode)
        {
            try
            {
                using (NebimV3.DataConnector.JoinedSqlTransactionScope Trans = new NebimV3.DataConnector.JoinedSqlTransactionScope())
                {
                    NebimV3.Items.Product Product = new NebimV3.Items.Product();
                    Product.ProductCode = ProductCode;
                    Product.ProductDescription = ProductNameTR;
                    //kampanya grubu
                    Product.PromotionGroupCode = null;//"Sezon"; //sezon ürünleri
                    #region hierarchy

                    // hiyerarşi nebim ile eklenebilir!
                    // Ürün hiyerarşisindeki yeri
                    int? hierarchyId = FindProductHierarchy(ProductHierarchyLevelCodes);
                    if (!hierarchyId.HasValue)
                    {
                        throw new Exception("CreateUpdateProduct=> product hierarchy Id does not exists. product code:" + ProductCode + "hierarchy levels:" + String.Join("-", ProductHierarchyLevelCodes.Select(x => x.Value)));
                    }
                    Product.ProductHierarchyID = hierarchyId.Value;//ProductHierarchyLevel03.HasValue? ProductHierarchyLevel03.Value:ProductHierarchyLevel02.HasValue?ProductHierarchyLevel02.Value:ProductHierarchyLevel03.Value;

                    #endregion hierarchy

                    //NoDimension = 0,
                    //OnlyColor = 1,
                    //ColorAndOne = 2,
                    //ColorAndTwo = 3,
                    //ColorAndThree = 4,
                    Product.ItemDimTypeCode = (byte)ItemDimTypeCode;   // Örn: Renk + Beden + Ton

                    // Vergi grubu
                    Product.ItemTaxGrCode = ItemAccountGrCode;
                    Product.UseInternet = true;
                    Product.UsePOS = true;
                    Product.SaveOrUpdate();

                    // Farklı dilde ürün açıklaması için
                    string LangCode = "EN";
                    Product.Descriptions.Load();
                    NebimV3.v3Entities.LanguageDescription LanguageDescription = Product.Descriptions.Where((NebimV3.v3Entities.LanguageDescription X) => NebimV3.Library.ObjectComparer.AreEqual(X.DataLanguageCode, LangCode)).FirstOrDefault();
                    if (LanguageDescription != null)
                    {
                        LanguageDescription.Description = ProductNameEN;
                        LanguageDescription.SaveOrUpdate();
                    }

                    #region variants
                    // Renk ve boyut ekleme
                    foreach (var comb in combinations)
                    {
                        switch (ItemDimTypeCode)
                        {
                            case 0:
                                CreateProductBarcode(Product, comb.Item7);
                                break;
                            case 1:
                                CreateColorIfNotExists(comb.Item1, comb.Item2, comb.Item3);
                                Product.Variants.Load();
                                //check if variant is already exists?
                                if (Product.Variants.Any(x => x.ColorCode == comb.Item1)) break;
                                Product.AddItemVariant(comb.Item1);
                                CreateProductBarcode(Product, comb.Item7, comb.Item1);
                                break;
                            case 2:
                                CreateColorIfNotExists(comb.Item1, comb.Item2, comb.Item3);
                                CreateItemDim1IfNotExists(comb.Item4, comb.Item5, comb.Item6);
                                Product.Variants.Load();
                                //check if variant is already exists?
                                if (Product.Variants.Any(x => (x.ColorCode == comb.Item1) && (x.ItemDim1Code == comb.Item4))) break;
                                Product.AddItemVariant(comb.Item1, comb.Item4);
                                CreateProductBarcode(Product, comb.Item7, comb.Item1, comb.Item4);  //eğer ürün daha önce kayıtlı ise hata verir
                                break;
                            default:
                                break;
                        }
                    }
                    #endregion variants

                    #region attributes

                    Product.Attributes.Load();
                    for (byte i = 0; i < attributes.Count; i++)
                    {
                        var attr = attributes[i];
                        if (string.IsNullOrWhiteSpace(attr.Item2))
                            continue;
                        if (Product.Attributes.Any(x => (x.AttributeCode == attr.Item2) && (x.AttributeTypeCode == attr.Item1)))
                            continue;

                        this.CreateProductAttributeIfNotExists(attr.Item1, attr.Item2, attr.Item3, attr.Item4);
                        NebimV3.Items.ProductAttribute Attribute = new NebimV3.Items.ProductAttribute(Product);
                        Attribute.AttributeTypeCode = attr.Item1;
                        Attribute.AttributeCode = attr.Item2;
                        Attribute.SaveOrUpdate();
                        Product.Attributes.Add(Attribute);
                    }
                    #endregion attributes

                    this.CreateProductBasePrice(Product, currencyCode, price, price2, price3);

                    Trans.Commit();

                }


            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }


        }

        private void CreateProductBarcode(NebimV3.Items.Product Product, string barcode, string colorCode = "", string itemDim1Code = "", string itemDim2Code = "", string itemDim3Code = "")
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return;
            Product.Barcodes.Load();
            if (Product.Barcodes.Any(x => x.Barcode == barcode))
                return;
            NebimV3.Items.ItemBarcode ib = new NebimV3.Items.ItemBarcode(Product);
            ib.BarcodeTypeCode = "Def"; // EAN13, DV vs... cdBarcodeType tablosundaki uygun Barkod Tiplerinden biri verilmeli.
            ib.Barcode = barcode;
            ib.ColorCode = colorCode;
            ib.ItemDim1Code = itemDim1Code;
            ib.ItemDim2Code = itemDim2Code;
            ib.ItemDim3Code = itemDim3Code;
            ib.UnitOfMeasureCode = "AD"; // Datada bulunan birim cinslerinden biri yazılabilir.
            ib.Qty = 1;
            ib.Save();
            Product.Barcodes.Add(ib);
        }


        //AF
        private void CreateColorIfNotExists(string colorCode, string colorDescriptionTR, string colorDescriptionEN)
        {
            NebimV3.Items.Color Color = new NebimV3.Items.Color();
            Color.ColorCode = colorCode;
            if (Color.ExistsInDB()) return;

            Color.ColorDescription = colorDescriptionTR;
            NebimV3.v3Entities.LanguageDescription LanguageDescription = Color.Descriptions.Where((NebimV3.v3Entities.LanguageDescription X) => NebimV3.Library.ObjectComparer.AreEqual(X.DataLanguageCode, "EN")).FirstOrDefault();
            if (LanguageDescription != null)
            {
                LanguageDescription.Description = colorDescriptionEN;
                LanguageDescription.SaveOrUpdate();
            }
            // Color.ColorHex = "#E0E0E0";
            Color.Save();

        }
        //AF
        private void CreateItemDim1IfNotExists(string code, string descriptionTR, string descriptionEN)
        {
            NebimV3.Items.ItemDim1 ItemDim1 = new NebimV3.Items.ItemDim1();
            ItemDim1.ItemDim1Code = code;
            if (ItemDim1.ExistsInDB()) return;
            ItemDim1.ItemDim1Description = descriptionTR;
            NebimV3.v3Entities.LanguageDescription LanguageDescription = ItemDim1.Descriptions.Where((NebimV3.v3Entities.LanguageDescription X) => NebimV3.Library.ObjectComparer.AreEqual(X.DataLanguageCode, "EN")).FirstOrDefault();
            if (LanguageDescription != null)
            {
                LanguageDescription.Description = descriptionEN;
                LanguageDescription.SaveOrUpdate();
            }
            ItemDim1.Save();

        }
        //AF
        private void CreateProductAttributeIfNotExists(byte attrTypeCode, string code, string descriptionTR, string descriptionEN)
        {
            // 1'den 20'ye kadar, Ürün özellik tanımlarından biri olmalı. Bunlar hali hazırda kayıtlıdır.
            if (attrTypeCode > 20) return;

            NebimV3.Attributes.ProductAttributeType AttType = NebimV3.Attributes.ProductAttributeType.CreateInstance<NebimV3.Attributes.ProductAttributeType>(attrTypeCode);

            NebimV3.Attributes.ProductAttribute Att = new NebimV3.Attributes.ProductAttribute(AttType);
            Att.AttributeCode = code;
            if (Att.ExistsInDB()) return;
            Att.AttributeDescription = descriptionTR;

            NebimV3.v3Entities.LanguageDescription LanguageDescription = Att.Descriptions.Where((NebimV3.v3Entities.LanguageDescription X) => NebimV3.Library.ObjectComparer.AreEqual(X.DataLanguageCode, "EN")).FirstOrDefault();
            if (LanguageDescription != null)
            {
                LanguageDescription.Description = descriptionEN;
                LanguageDescription.SaveOrUpdate();
            }

            Att.Save();

        }

        //AF
        //Özellike tipi güncellemesi- gerekli olacak mı?
        private void CreateProductAttributeTypeIfNotExists(byte index, byte code, string descriptionTR, string descriptionEN)
        {
            //// 1'den 20'ye kadar, Ürün özellik tanımlarından biri olmalı. Bunlar hali hazırda kayıtlıdır.
            //if (index > 20) return;

            //NebimV3.Attributes.ProductAttributeType AttType = NebimV3.Attributes.ProductAttributeType.CreateInstance<NebimV3.Attributes.ProductAttributeType>(index);

            //AttType.AttributeTypeCode = code;
            //AttType.Load();
            //if (Att.ExistsInDB()) return;
            //Att.AttributeDescription = descriptionTR;

            //NebimV3.v3Entities.LanguageDescription LanguageDescription = Att.Descriptions.Where((NebimV3.v3Entities.LanguageDescription X) => NebimV3.Library.ObjectComparer.AreEqual(X.DataLanguageCode, "EN")).FirstOrDefault();
            //if (LanguageDescription != null)
            //{
            //    LanguageDescription.Description = descriptionEN;
            //    LanguageDescription.SaveOrUpdate();
            //}

            //Att.Save();

        }

        //AF
        /// <summary>
        /// 1:satış fiyatı
        /// 2:satın alma fiyatı
        /// 3:ilk satış fiyatı
        /// </summary>
        /// <param name="Product"></param>
        /// <param name="currencyCode"></param>
        /// <param name="price1"></param>
        /// <param name="price2"></param>
        /// <param name="price3"></param>
        private void CreateProductBasePrice(NebimV3.Items.Product Product, string currencyCode, decimal price1 = 0, decimal price2 = 0, decimal price3 = 0)
        {
            using (NebimV3.DataConnector.JoinedSqlTransactionScope Trans = new NebimV3.DataConnector.JoinedSqlTransactionScope())
            {
                NebimV3.Items.ItemBasePrice ibp = new NebimV3.Items.ItemBasePrice(Product);
                ibp.BasePriceCode = (byte)NebimV3.Items.BasePriceCodes.RetailSalePrice;
                ibp.CurrencyCode = currencyCode; // USD, EUR vs...
                ibp.CountryCode = "TR";
                ibp.Price = price1;
                ibp.PriceDate = DateTime.Today; // Günün tarihini atıyoruz. Farklı bir tarih yazılmasına gerek yok.
                ibp.SaveOrUpdate();
                Product.BasePrices.Add(ibp);

                if (price2 > 0)
                {
                    ibp = new NebimV3.Items.ItemBasePrice(Product);
                    ibp.BasePriceCode = (byte)NebimV3.Items.BasePriceCodes.PurchasePrice;
                    ibp.CurrencyCode = currencyCode; // USD, EUR vs...
                    ibp.CountryCode = "TR";
                    ibp.Price = price2;
                    ibp.PriceDate = DateTime.Today; // Günün tarihini atıyoruz. Farklı bir tarih yazılmasına gerek yok.
                    ibp.SaveOrUpdate();
                    Product.BasePrices.Add(ibp);
                }
                if (price3 > 0)
                {
                    ibp = new NebimV3.Items.ItemBasePrice(Product);
                    ibp.BasePriceCode = (byte)NebimV3.Items.BasePriceCodes.FirstSalePrice;
                    ibp.CurrencyCode = currencyCode; // USD, EUR vs...
                    ibp.CountryCode = "TR";
                    ibp.Price = price3;
                    ibp.PriceDate = DateTime.Today; // Günün tarihini atıyoruz. Farklı bir tarih yazılmasına gerek yok.
                    ibp.SaveOrUpdate();
                    Product.BasePrices.Add(ibp);
                }

                Trans.Commit();
            }
        }

        private int? FindProductHierarchy(int[] hierarchLevelCodes)
        {
            int? productHierarchyId = null;
            try
            {
                NebimV3.DataConnector.SqlSelectStatement query = new NebimV3.DataConnector.SqlSelectStatement();
                query.TableNames.Add("dfProductHierarchy", false);
                query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("dfProductHierarchy", "ProductHierarchyID"));

                for (int i = 0; i < hierarchLevelCodes.Length; i++)
                {
                    // query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("dfProductHierarchy", "ProductHierarchyLevelCode0"+(i+1).ToString()));

                    query.Filter.AddCondition(
                      new NebimV3.DataConnector.BinaryCondition(
                          new NebimV3.DataConnector.PropertyCondition("dfProductHierarchy", "ProductHierarchyLevelCode0" + (i + 1).ToString()),
                          new NebimV3.DataConnector.ValueCondition(hierarchLevelCodes[i])
                          ));
                }

                HashSet<int> results = new HashSet<int>();

                using (System.Data.IDataReader reader = NebimV3.DataConnector.SqlStatmentExecuter.ExecuteSelect(query))
                {
                    while (reader.Read())
                    {
                        results.Add((int)(reader["ProductHierarchyID"]));
                    }
                }

                // if (results.Count > 1)  // Örnek olarak yaptik. Exception atmak yerine ne yapilmasi gerektigi uygulamaya göre degisebilir.
                //     throw new Exception("More than one record with the same B2C customer Id");

                if (results.Count == 0)
                    return null;

                productHierarchyId = results.LastOrDefault();
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }
            return productHierarchyId;
        }
        private int? FindProductHierarchy(Dictionary<int, IList<int>> hierarchLevelCodes)
        {
            int? productHierarchyId = null;
            if (hierarchLevelCodes == null || hierarchLevelCodes.Count == 0)
                return null;
            try
            {
                NebimV3.DataConnector.SqlSelectStatement query = new NebimV3.DataConnector.SqlSelectStatement();
                query.TableNames.Add("dfProductHierarchy", false);
                query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("dfProductHierarchy", "ProductHierarchyID"));

                for (int i = 0; i < hierarchLevelCodes.Count; i++)
                {
                    // query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("dfProductHierarchy", "ProductHierarchyLevelCode0"+(i+1).ToString()));
                    if (hierarchLevelCodes[i + 1] == null) continue;
                    List<NebimV3.DataConnector.ValueCondition> conditions = new List<NebimV3.DataConnector.ValueCondition>();
                    foreach (var code in hierarchLevelCodes[i + 1])
                    {
                        conditions.Add(new NebimV3.DataConnector.ValueCondition(code));
                    }
                    query.Filter.AddCondition(new NebimV3.DataConnector.InCondition(
                          new NebimV3.DataConnector.PropertyCondition("dfProductHierarchy", "ProductHierarchyLevelCode0" + (i + 1).ToString()),
                          conditions));
                }

                HashSet<int> results = new HashSet<int>();
                using (System.Data.IDataReader reader = NebimV3.DataConnector.SqlStatmentExecuter.ExecuteSelect(query))
                {
                    while (reader.Read())
                    {
                        results.Add((int)(reader["ProductHierarchyID"]));
                    }
                }

                // if (results.Count > 1)  // Örnek olarak yaptik. Exception atmak yerine ne yapilmasi gerektigi uygulamaya göre degisebilir.
                //     throw new Exception("More than one record with the same B2C customer Id");

                if (results.Count == 0)
                    return null;

                productHierarchyId = results.LastOrDefault();
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }
            return productHierarchyId;
        }

        internal IList<int> FindProductHierarchyLevelCode(string hierarchLevelName, int levelNumber)
        {
            try
            {
                NebimV3.DataConnector.SqlSelectStatement query = new NebimV3.DataConnector.SqlSelectStatement();
                query.TableNames.Add("cdProductHierarchyLevel", false);
                query.TableNames.Add("cdProductHierarchyLevelDesc", false);
                query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("cdProductHierarchyLevel", "ProductHierarchyLevelCode"));

                query.Filter = new NebimV3.DataConnector.GroupCondition();
                query.Filter.AddCondition(
                    new NebimV3.DataConnector.BinaryCondition(
                        new NebimV3.DataConnector.PropertyCondition("cdProductHierarchyLevel", "ProductHierarchyLevelCode"),
                        new NebimV3.DataConnector.PropertyCondition("cdProductHierarchyLevelDesc", "ProductHierarchyLevelCode")
                        ));

                query.Filter.AddCondition(
                  new NebimV3.DataConnector.BinaryCondition(
                      new NebimV3.DataConnector.PropertyCondition("cdProductHierarchyLevelDesc", "ProductHierarchyLevelDescription"),
                      new NebimV3.DataConnector.ValueCondition(hierarchLevelName)
                      ));

                query.Filter.AddCondition(
                    new NebimV3.DataConnector.BinaryCondition(
                        new NebimV3.DataConnector.PropertyCondition("cdProductHierarchyLevel", "LevelNumber"),
                        new NebimV3.DataConnector.ValueCondition(levelNumber)
                        ));


                HashSet<int> results = new HashSet<int>();

                using (System.Data.IDataReader reader = NebimV3.DataConnector.SqlStatmentExecuter.ExecuteSelect(query))
                {
                    while (reader.Read())
                    {
                        results.Add((int)(reader["ProductHierarchyLevelCode"]));
                    }
                }

                // if (results.Count > 1)  // Örnek olarak yaptik. Exception atmak yerine ne yapilmasi gerektigi uygulamaya göre degisebilir.
                //     throw new Exception("More than one record with the same B2C customer Id");

                if (results.Count == 0)
                {
                    throw new Exception("Kategori*hiyerarşi eşi bulunamadı. Kategori:" + hierarchLevelName);
                    //return null;
                }


                return results.ToList();
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }
        }


        //TODO: define parameters!
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LangCode" value="TR"></param>
        /// <param name="PriceGroup1Code" value="SZNF" remarks="ilk satış fiyatı"></param>
        /// <param name="PriceGroup2Code" value="PSF"  remarks="site satış fiyatı"></param>
        /// <param name="Warehouse1Code" value="K150" ></param>
        /// <param name="Warehouse2Code"></param>
        /// <param name="BarcodeTypeCode1" value="DEF" ></param>
        /// <param name="LastNDay"></param>
        /// <returns></returns>
        internal DataTable GetAllProducts(string LangCode, string PriceGroup1Code, string PriceGroup2Code, string Warehouse1Code, string Warehouse2Code, string BarcodeTypeCode1, int LastNDay)
        {
            var executer = new NebimV3.DataConnector.FnExecuter(NebimV3.DataConnector.FunctionType.TableValuedFunction);
            executer.FunctionName = "ProductPriceAndInventory_B2C";
            executer.Parameters.Add(string.IsNullOrWhiteSpace(LangCode) ? "" : LangCode);
            executer.Parameters.Add(string.IsNullOrWhiteSpace(PriceGroup1Code) ? "" : PriceGroup1Code);
            executer.Parameters.Add(string.IsNullOrWhiteSpace(PriceGroup2Code) ? "" : PriceGroup2Code);
            executer.Parameters.Add(string.IsNullOrWhiteSpace(Warehouse1Code) ? "" : Warehouse1Code);
            executer.Parameters.Add(string.IsNullOrWhiteSpace(Warehouse2Code) ? "" : Warehouse2Code);
            executer.Parameters.Add(string.IsNullOrWhiteSpace(BarcodeTypeCode1) ? "" : BarcodeTypeCode1);
            executer.Parameters.Add(LastNDay > 0 ? LastNDay : int.MaxValue);
            var table = executer.ExecuteQueryReturnDataTable();

            return table;

        }
    }
}

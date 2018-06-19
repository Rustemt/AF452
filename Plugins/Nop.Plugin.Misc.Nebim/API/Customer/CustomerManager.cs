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
    public class CustomerManager
    {
        NebimIntegrationSettings _nebimIntegrationSettings;
        public CustomerManager(NebimIntegrationSettings nebimIntegrationSettings)
        {
            _nebimIntegrationSettings = nebimIntegrationSettings;
            ConnectionManager.ConnectDB(nebimIntegrationSettings);
        }

        public string CreateUpdateCustomer(string description, string languageCode, string firstName, string lastName,
            List<string> emails, List<string> phones, string identityNum, byte genderCode = 3, string customerSegment = null, DateTime? registerDate=null)
        {
            try
            {
                NebimV3.CurrentAccounts.RetailCustomer customer = new NebimV3.CurrentAccounts.RetailCustomer();

                //check if customer exists by B2C id in description filed.
                string customerCode = null;
                if (_nebimIntegrationSettings.API_CustomerCodePrefix == null)
                {
                    customerCode = this.FindCustomerCodeByDescription(description);
                }
                else
                {
                    customerCode = GetCustomerCodeFromB2CId(description);
                }
                customer.RetailCustomerCode = customerCode;
                if (customer.ExistsInDB())
                    customer.Load();


                //lets keep "B2C customer id" in description
                customer.RetailCustomerDescription = description;
                customer.DataLanguageCode = languageCode;//TR,EN
                customer.FirstName = firstName;
                customer.LastName = lastName;
                if (registerDate.HasValue)
                    customer.AccountOpeningDate = registerDate.Value;
                
                customer.IdentityNum = identityNum;
                customer.PersonalInfo.GenderCode = genderCode;//1:erkek,2:kadın,3:bilimiyor
                //if (customerCode != null)
                //{
                customer.SaveOrUpdate();
                //}
                //else
                //{
                //    customer.Save();

                //    customerCode = customer.RetailCustomerCode;
                //}
                #region müşteri idsi özellik olarak ekleniyor

                //B2C customer id,
                //SetAttribute(1, description, customer);
                //müşteri seviyesi, müşteri seviyeleri açılmış olması gerekiyor. (müşteri özelliği 2 noyu kullnacağız)
                if(!string.IsNullOrWhiteSpace(customerSegment))
                    //SetExistingAttribute(2, customerSegment, customer);
                    SetAttribute(2, customerSegment, customer);

                #endregion müşteri idsi özellik olarak ekleniyor

                NebimV3.CurrentAccounts.CurrAccCommunication comm = null;
                customer.Communications.Load();
                #region CommunicationTypeCode
                //1	EN	Telephone
                //2	EN	Fax
                //3	EN	E-Mail
                //4	EN	Home Phone
                //5	EN	Office Phone
                //6	EN	Business Mobile
                //7	EN	Personal Mobile
                //1	TR	Telefon
                //2	TR	Fax
                //3	TR	E-Posta
                //4	TR	Ev Telefonu
                //5	TR	İş Telefonu
                //6	TR	İş Cep Telefonu
                //7	TR	Özel Cep Telefonu
                #endregion CommunicationTypeCode

                foreach (var email in emails)
                {

                    if (CommunicationExists(customer.Communications, email)) continue;
                    comm = new NebimV3.CurrentAccounts.CurrAccCommunication(customer);
                    comm.CommunicationTypeCode = _nebimIntegrationSettings.API_CommunicationTypeEmail;
                    comm.CommAddress = email;
                    comm.Save();
                    customer.Communications.Add(comm);
                }
                foreach (var phone in phones)
                {
                    if (CommunicationExists(customer.Communications, phone)) continue;
                    comm = new NebimV3.CurrentAccounts.CurrAccCommunication(customer);
                    comm.CommunicationTypeCode = _nebimIntegrationSettings.API_CommunicationTypePhone;
                    comm.CommAddress = phone;
                    comm.Save();
                    customer.Communications.Add(comm);

                }
                return customerCode; // Kaydederken otomatik olarak bir numara verilir. Bu numarayı kullanacağız.
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }
        }

        private bool CommunicationExists(NebimV3.v3Entities.v3ElementItemList<NebimV3.CurrentAccounts.CurrAccCommunication> communications, string commAddress)
        {
            foreach (NebimV3.CurrentAccounts.CurrAccCommunication c in communications)
            {
                if (NebimV3.Library.ObjectComparer.AreEqual(c.CommAddress, commAddress))
                {
                    return true;
                }
            }
            return false;
        }

        private static void SetAttribute(byte AttributeTypeCode,string AttributeCode, NebimV3.CurrentAccounts.RetailCustomer customer)
        {
            var foo = new NebimV3.Attributes.RetailCustomerAttribute(
                (NebimV3.Attributes.RetailCustomerAttributeType)
                (NebimV3.Attributes.AttributeTypeBase.CreateInstance
                    (typeof(NebimV3.Attributes.RetailCustomerAttributeType), AttributeTypeCode)));

            foo.AttributeCode = AttributeCode;
            foo.AttributeDescription = AttributeCode;
            foo.SaveOrUpdate();

            var att = new NebimV3.CurrentAccounts.AttributeRetailCustomer(customer);
            att.AttributeTypeCode = AttributeTypeCode;
            att.AttributeCode = foo.AttributeCode;
            att.SaveOrUpdate();
        }

        private static void SetExistingAttribute(byte AttributeTypeCode, string AttributeCode, NebimV3.CurrentAccounts.RetailCustomer customer)
        {
            var att = new NebimV3.CurrentAccounts.AttributeRetailCustomer(customer);
            att.AttributeTypeCode = AttributeTypeCode;
            att.AttributeCode = AttributeCode;
            att.SaveOrUpdate();
        }

        private string GetCustomerCodeFromB2CId(string id)
        {
            return _nebimIntegrationSettings.API_CustomerCodePrefix + id;
        }


        public Guid CreateCustomerAddressByCustomerCode(string customerCode, string type,
            string firstName, string lastName,
            string addressLine, string district, string city,
            string zipCode, string countryCode,
            string taxNumber, string taxOffice)
        {
            try
            {
                // DB'de kayıtlı bir müşterinin Müşteri Kodu vasıtasıyla yüklenmesi
                NebimV3.CurrentAccounts.RetailCustomer customer = new NebimV3.CurrentAccounts.RetailCustomer();
                if (!string.IsNullOrWhiteSpace(customerCode))
                {
                    customer.RetailCustomerCode = customerCode;
                    if (!customer.ExistsInDB())
                    {
                        throw new Exception("Customer does not exists. customer code:" + customerCode);
                    }
                    customer.Load();
                }

                NebimV3.CurrentAccounts.CurrAccPostalAddress address = new NebimV3.CurrentAccounts.CurrAccPostalAddress(customer);
                //TODO: il, ilçeyi ayır
                addressLine = string.Format(_nebimIntegrationSettings.AddressFormat, firstName, lastName, addressLine, district);
                address.AddressTypeCode = type; // 1:Ev adresi , 2: İş adresi
                address.Address = addressLine.Substring(0, Math.Min(200, addressLine.Length));
                address.ZipCode = zipCode == null ? null : zipCode.Substring(0, Math.Min(20, zipCode.Length));
                address.CountryCode = countryCode;
                //address.StateCode = "TR.00"; // Marmara
                address.CityCode = city; // TR.34 İstanbul

                address.TaxNumber = taxNumber == null ? null : taxNumber.Substring(0, Math.Min(10, taxNumber.Length));
                if(!string.IsNullOrWhiteSpace(taxOffice))
                    address.SetTaxOfficedescription(taxOffice.Substring(0, Math.Min(100, taxOffice.Length)));
                address.SetCitydescription(city);
                if (!string.IsNullOrWhiteSpace(district))
                address.SetDistrictdescription(district.Substring(0, Math.Min(100, district.Length)));
                address.SaveOrUpdate();
                customer.PostalAddresses.Add(address);
                return address.PostalAddressID;
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }

        }

        public Guid CreateCustomerAddressByCustomerDescription(string customerDescription, string type,
          string firstName, string lastName,
          string addressLine, string district, string city,
          string zipCode, string countryCode,
          string taxNumber, string taxOffice)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerDescription))
                    throw new Exception("Customer description (B2C Id) must be provided");

                NebimV3.CurrentAccounts.RetailCustomer customer = new NebimV3.CurrentAccounts.RetailCustomer();
                string customerCode = null;


                customerCode = FindCustomerCodeByDescription(customerDescription);
                customer.RetailCustomerCode = customerCode;
                if (!customer.ExistsInDB())
                {
                    throw new Exception("Customer does not exists. CustomerCode:" + customerCode);
                }
                return CreateCustomerAddressByCustomerCode(customerCode, type, firstName, lastName, addressLine, district, city, zipCode, countryCode, taxNumber, taxOffice);
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }

        }


        private string FindCustomerCodeByDescription(string description)
        {
            string customerCode = null;
            try
            {
                NebimV3.DataConnector.SqlSelectStatement query = new NebimV3.DataConnector.SqlSelectStatement();
                query.TableNames.Add("cdCurrAcc", false);
                query.TableNames.Add("cdCurrAccDesc", false);

                query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccCode"));
                query.Filter = new NebimV3.DataConnector.GroupCondition();
                query.Filter.AddCondition(
                    new NebimV3.DataConnector.BinaryCondition(
                        new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccCode"),
                        new NebimV3.DataConnector.PropertyCondition("cdCurrAccDesc", "CurrAccCode")
                        ));


                if (description != null)
                {
                    query.Filter.AddCondition(
                      new NebimV3.DataConnector.BinaryCondition(
                          new NebimV3.DataConnector.PropertyCondition("cdCurrAccDesc", "CurrAccDescription"),
                          new NebimV3.DataConnector.ValueCondition(description)
                          ));
                }



                query.Filter.AddCondition(
                    new NebimV3.DataConnector.BinaryCondition(
                        new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccTypeCode"),
                        new NebimV3.DataConnector.ValueCondition(NebimV3.ApplicationCommon.CurrAccTypes.RetailCustomer)
                        ));

                HashSet<string> results = new HashSet<string>();

                using (System.Data.IDataReader reader = NebimV3.DataConnector.SqlStatmentExecuter.ExecuteSelect(query))
                {
                    while (reader.Read())
                    {
                        results.Add((string)(reader["CurrAccCode"]));
                    }
                }

                // if (results.Count > 1)  // Örnek olarak yaptik. Exception atmak yerine ne yapilmasi gerektigi uygulamaya göre degisebilir.
                //     throw new Exception("More than one record with the same B2C customer Id");

                if (results.Count == 0)
                    return null;

                customerCode = results.LastOrDefault();
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;

            }
            return customerCode;
        }

        private string FindCustomerCode(string OfficeCode = null, string FirstName = null, string LastName = null, string PhoneNumber = null)
        {
            NebimV3.DataConnector.SqlSelectStatement query = new NebimV3.DataConnector.SqlSelectStatement();
            query.TableNames.Add("cdCurrAcc", false);
            query.TableNames.Add("prCurrAccCommunication", false);

            query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccCode"));

            query.Filter = new NebimV3.DataConnector.GroupCondition();
            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccCode"),
                    new NebimV3.DataConnector.PropertyCondition("prCurrAccCommunication", "CurrAccCode")
                    ));

            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccTypeCode"),
                    new NebimV3.DataConnector.PropertyCondition("prCurrAccCommunication", "CurrAccTypeCode")
                    ));

            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("prCurrAccCommunication", "CommunicationTypeCode"),
                    new NebimV3.DataConnector.ValueCondition("1")
                // DIKKAT! Buradaki "1" kodu örnektir. Uygulamanin kullanildigi database de hangi CommunicationType telefon bilgisini iceriyorsa, onun kodu gelmelidir.
                    ));

            if (PhoneNumber != null)
            {
                query.Filter.AddCondition(
                    new NebimV3.DataConnector.BinaryCondition(
                        new NebimV3.DataConnector.PropertyCondition("prCurrAccCommunication", "CommAddress"),
                        new NebimV3.DataConnector.ValueCondition(PhoneNumber)
                        ));
            }

            if (FirstName != null)
            {
                query.Filter.AddCondition(
                  new NebimV3.DataConnector.BinaryCondition(
                      new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "FirstName"),
                      new NebimV3.DataConnector.ValueCondition(FirstName)
                      ));
            }

            if (LastName != null)
            {
                query.Filter.AddCondition(
                    new NebimV3.DataConnector.BinaryCondition(
                        new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "LastName"),
                        new NebimV3.DataConnector.ValueCondition(LastName)
                        ));
            }

            if (OfficeCode != null)
            {
                query.Filter.AddCondition(
                    new NebimV3.DataConnector.BinaryCondition(
                        new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "OfficeCode"),
                        new NebimV3.DataConnector.ValueCondition(OfficeCode)
                        ));
            }

            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccTypeCode"),
                    new NebimV3.DataConnector.ValueCondition(NebimV3.ApplicationCommon.CurrAccTypes.Customer)
                    ));

            HashSet<string> results = new HashSet<string>();

            using (System.Data.IDataReader reader = NebimV3.DataConnector.SqlStatmentExecuter.ExecuteSelect(query))
            {
                while (reader.Read())
                {
                    results.Add((string)(reader["CurrAccCode"]));
                }
            }

            if (results.Count > 1)  // Örnek olarak yaptik. Exception atmak yerine ne yapilmasi gerektigi uygulamaya göre degisebilir.
                throw new Exception("More than one record with the same Name and Phone Number");

            if (results.Count == 0)
                return null;

            return results.FirstOrDefault();

        }

        private NebimV3.CurrentAccounts.RetailCustomer FindCustomer(string OfficeCode = null, string FirstName = null, string LastName = null, string PhoneNumber = null)
        {
            string customerCode = FindCustomerCode(OfficeCode, FirstName, LastName, PhoneNumber);

            if (string.IsNullOrWhiteSpace(customerCode))
                return null;


            NebimV3.CurrentAccounts.RetailCustomer customer = new NebimV3.CurrentAccounts.RetailCustomer();
            customer.RetailCustomerCode = customerCode;
            customer.Load();

            return customer;
        }

        private NebimV3.CurrentAccounts.RetailCustomer FindCustomerByPresenterCard(string CardNumber)
        {
            NebimV3.DataConnector.SqlSelectStatement query = new NebimV3.DataConnector.SqlSelectStatement();
            query.TableNames.Add("cdCurrAcc", false);
            query.TableNames.Add("prCustomerPresentCard", false);

            query.Parameters.Add(new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccCode"));

            query.Filter = new NebimV3.DataConnector.GroupCondition();
            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccCode"),
                    new NebimV3.DataConnector.PropertyCondition("prCustomerPresentCard", "CurrAccCode")
                    ));

            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccTypeCode"),
                    new NebimV3.DataConnector.PropertyCondition("prCustomerPresentCard", "CurrAccTypeCode")
                    ));

            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("prCustomerPresentCard", "CardNumber"),
                    new NebimV3.DataConnector.ValueCondition(CardNumber)
                    ));


            query.Filter.AddCondition(
                new NebimV3.DataConnector.BinaryCondition(
                    new NebimV3.DataConnector.PropertyCondition("cdCurrAcc", "CurrAccTypeCode"),
                    new NebimV3.DataConnector.ValueCondition(NebimV3.ApplicationCommon.CurrAccTypes.Customer)
                    ));

            HashSet<string> results = new HashSet<string>();

            using (System.Data.IDataReader reader = NebimV3.DataConnector.SqlStatmentExecuter.ExecuteSelect(query))
            {
                while (reader.Read())
                {
                    results.Add((string)(reader["CurrAccCode"]));
                }
            }

            if (results.Count > 1)  // Örnek olarak yaptik. Exception atmak yerine ne yapilmasi gerektigi uygulamaya göre degisebilir.
                throw new Exception("More than one record with the same Name and Phone Number");

            if (results.Count == 0)
                return null;

            NebimV3.CurrentAccounts.RetailCustomer customer = new NebimV3.CurrentAccounts.RetailCustomer();
            customer.RetailCustomerCode = results.FirstOrDefault();
            customer.Load();

            return customer;
        }

    }
}

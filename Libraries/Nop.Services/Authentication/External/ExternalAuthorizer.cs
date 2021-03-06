//Contributor:  Nicholas Mayne

using System;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Orders;

namespace Nop.Services.Authentication.External
{
    public partial class ExternalAuthorizer : IExternalAuthorizer
    {
        #region Fields

        private readonly IAuthenticationService _authenticationService;
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IWorkContext _workContext;
        private readonly CustomerSettings _customerSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        #endregion

        #region Ctor

        public ExternalAuthorizer(IAuthenticationService authenticationService,
            IOpenAuthenticationService openAuthenticationService,
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService, 
            IWorkContext workContext, CustomerSettings customerSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            IShoppingCartService shoppingCartService,
            IWorkflowMessageService workflowMessageService, LocalizationSettings localizationSettings)
        {
            this._authenticationService = authenticationService;
            this._openAuthenticationService = openAuthenticationService;
            this._customerService = customerService;
            this._customerRegistrationService = customerRegistrationService;
            this._workContext = workContext;
            this._customerSettings = customerSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._shoppingCartService = shoppingCartService;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
        }
        
        #endregion

        #region Utilities

        private bool RegistrationIsEnabled()
        {
            return _customerSettings.UserRegistrationType != UserRegistrationType.Disabled && !_externalAuthenticationSettings.AutoRegisterEnabled;
        }

        private bool AutoRegistrationIsEnabled()
        {
            return _customerSettings.UserRegistrationType != UserRegistrationType.Disabled && _externalAuthenticationSettings.AutoRegisterEnabled;
        }

        private bool AccountDoesNotExistAndUserIsNotLoggedOn(Customer userFound, Customer userLoggedIn)
        {
            return userFound == null && userLoggedIn == null;
        }

        private bool AccountIsAssignedToLoggedOnAccount(Customer userFound, Customer userLoggedIn)
        {
            return userFound.Id.Equals(userLoggedIn.Id);
        }

        private bool AccountAlreadyExists(Customer userFound, Customer userLoggedIn)
        {
            return userFound != null && userLoggedIn != null;
        }

        #endregion

        #region Methods

        public virtual AuthorizationResult Authorize(OpenAuthenticationParameters parameters)
        {
            var userFound = _openAuthenticationService.GetUser(parameters);

            var userLoggedIn = _workContext.CurrentCustomer.IsRegistered() ? _workContext.CurrentCustomer : null;

            if (AccountAlreadyExists(userFound, userLoggedIn))
            {
                if (AccountIsAssignedToLoggedOnAccount(userFound, userLoggedIn))
                {
                    // The person is trying to log in as himself.. bit weird
                    return new AuthorizationResult(OpenAuthenticationStatus.Authenticated);
                }

                var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                result.AddError("Account is already assigned");
                return result;
            }
            if (AccountDoesNotExistAndUserIsNotLoggedOn(userFound, userLoggedIn))
            {
                ExternalAuthorizerHelper.StoreParametersForRoundTrip(parameters);

                if (AutoRegistrationIsEnabled())
                {
                    #region Register user

                    var currentCustomer = _workContext.CurrentCustomer;
                    var details = new RegistrationDetails(parameters);
                    var randomPassword = CommonHelper.GenerateRandomDigitCode(20);


                    bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;

                    var customer = _customerService.GetCustomerByEmail(details.EmailAddress);
                    if (customer != null)
                    {
                        //migrate shopping cart
                        _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer);
                        _authenticationService.SignIn(customer, false);
                        var result = new AuthorizationResult(OpenAuthenticationStatus.AssociateOnLogon);
                        return result;
                    }
                    else
                    {

                    var registrationRequest = new CustomerRegistrationRequest(currentCustomer, details.EmailAddress,
                        _customerSettings.UsernamesEnabled ? details.UserName : details.EmailAddress, randomPassword, PasswordFormat.Clear, isApproved);
                    var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
                    if (registrationResult.Success)
                    {
                        //store other parameters (form fields)
                        if (!String.IsNullOrEmpty(details.FirstName))
                            _customerService.SaveCustomerAttribute(currentCustomer, SystemCustomerAttributeNames.FirstName, details.FirstName);
                        if (!String.IsNullOrEmpty(details.LastName))
                            _customerService.SaveCustomerAttribute(currentCustomer, SystemCustomerAttributeNames.LastName, details.LastName);
                    

                        userFound = currentCustomer;
                        _openAuthenticationService.AssociateExternalAccountWithUser(currentCustomer, parameters);
                        ExternalAuthorizerHelper.RemoveParameters();

                        //code below is copied from CustomerController.Register method

                        //authenticate
                        if (isApproved)
                            _authenticationService.SignIn(userFound ?? userLoggedIn, false);

                        //notifications
                        if (_customerSettings.NotifyNewCustomerRegistration)
                            _workflowMessageService.SendCustomerRegisteredNotificationMessage(currentCustomer, _localizationSettings.DefaultAdminLanguageId);

                        switch (_customerSettings.UserRegistrationType)
                        {
                            case UserRegistrationType.EmailValidation:
                                {
                                    //email validation message
                                    _customerService.SaveCustomerAttribute(currentCustomer, SystemCustomerAttributeNames.AccountActivationToken, Guid.NewGuid().ToString());
                                    _workflowMessageService.SendCustomerEmailValidationMessage(currentCustomer, _workContext.WorkingLanguage.Id);

                                    //result
                                    return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredEmailValidation);
                                }
                            case UserRegistrationType.AdminApproval:
                                {
                                    //result
                                    return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredAdminApproval);
                                }
                            case UserRegistrationType.Standard:
                                {
                                    //send customer welcome message
                                    _workflowMessageService.SendCustomerWelcomeMessage(currentCustomer, _workContext.WorkingLanguage.Id);

                                    //result
                                    return new AuthorizationResult(OpenAuthenticationStatus.AutoRegisteredStandard);
                                }
                            default:
                                break;
                        }
                    }
                    else
                    {
                        ExternalAuthorizerHelper.RemoveParameters();

                        var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                        foreach (var error in registrationResult.Errors)
                            result.AddError(string.Format(error));
                        return result;
                    }
                        }
                    #endregion
                       
                }
                else if (RegistrationIsEnabled())
                {
                    return new AuthorizationResult(OpenAuthenticationStatus.AssociateOnLogon);
                }
                else
                {
                    ExternalAuthorizerHelper.RemoveParameters();

                    var result = new AuthorizationResult(OpenAuthenticationStatus.Error);
                    result.AddError("Registration is disabled");
                    return result;
                }
            }
            if (userFound == null)
            {
                _openAuthenticationService.AssociateExternalAccountWithUser(userLoggedIn, parameters);
            }

            //migrate shopping cart
            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, userFound ?? userLoggedIn);
            //authenticate
            _authenticationService.SignIn(userFound ?? userLoggedIn, false);

            return new AuthorizationResult(OpenAuthenticationStatus.Authenticated);
        }

        #endregion
    }
}
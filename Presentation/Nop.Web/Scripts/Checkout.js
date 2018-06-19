$(function () {
    


    // select address type
    $(function () {
        $('div#terms-of-service div.selectAddressType a.button7').click(function (e) {
            e.preventDefault();
            $(this).toggleClass('checked'); //.siblings().removeClass('checked');
        });
    });

    //// sure terms of service is aggreed
    //$(function () {
    //    $('a#ConfirmOrderSubmit').click(function (e) {
    //        if ($('.terms-of-service .button7').hasClass('checked') == false) {
    //            $.fancybox(
	//	            '<h2>Hi!</h2><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam quis mi eu elit tempor facilisis id et neque</p>',
	//	            {
	//	                'autoDimensions'	: false,
	//	                'width'         		: 350,
	//	                'height'        		: 'auto',
	//	                'transitionIn'		: 'none',
	//	                'transitionOut'		: 'none'
	//	            }
	//            );
    //        }
    //        else {
    //            $('a.checkBtn').attr('href', 'Checkout/ConfirmOrder');
    //            $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/images/loader.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>');
    //        } //return false;
    //    });
    //});

    if ($('input[name="paymentMethod"]').val()=="Payments.PurchaseOrder") {
        // $("html, body").animate({ scrollTop: $('#ulOrderTotals').position().top + 10 }, "slow", function () { });
        if ($("#withOtherMethod a").attr('rdovalue') == 'Payments.PurchaseOrder') {
            $("#withCard a").removeClass("checked");
            $("#withOtherMethod a").addClass('checked');
        }
    }
    

    $('input#CardNumber').change(function () {
        var cardNumber = $(this).val();
        var data = { CCNo: cardNumber };
        if (cardNumber.length >= 6) {
            $.ajax({
                type: "POST",
                url: _getCCEffectiveType,
                data: JSON.stringify(data),
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    //cardType = data.CCType; alert(cardType);
                    if (typeof data.Installments === "undefined" || data.Installments.length <= 1) {
                        $('#dtInstallment').hide();
                        $('#ddInstallment').hide();
                        $('input#Installment').val('1');
                    }
                    else {
                        $('#dtInstallment').show();
                        $('#ddInstallment').show();
                        $('#ddInstallment').html(AFDropdown(data.Installments, 'Installment'));

                        InitAFDropdown($('#ddInstallment'));

                        //do joker vada toggle 
                        $('#ddInstallment div.selectBox li').click(function (e) {
                            e.stopPropagation();
                            var selectedItem = $(this).attr('data-value');

                            if (selectedItem == "1") {
                                $('#dtCCOption').hide();
                                $('#ddCCOption').hide();
                            }
                            else {
                                if ($('#ddCCOption').html() != '') {//check available options
                                    $('#dtCCOption').show();
                                    $('#ddCCOption').show();

                                } 
                            }
                        });

                    }
                    if (typeof data.CCOptions === "undefined" || data.CCOptions.length == 0) {
                        $('#dtCCOption').hide();
                        $('#ddCCOption').hide();
                        $('input#CCOption').val('');
                        $('#ddCCOption').html('');
                    }
                    else {
                        $('#ddCCOption').html(AFDropdown(data.CCOptions, 'CCOption'));
                        InitAFDropdown($('#ddCCOption'));
                    }



                }
            });
        }
    });

    //Use this block for manuel solution
    /*
    $('div.securityQuestion a').click(function () {
    var infoContainer = $(this).siblings('div.ccvInfo');
    if (infoContainer.is(":visible")) {
    infoContainer.hide();
    }
    else {
    infoContainer.fadeIn(300)
    }
    });
    */

    $("div.securityQuestion a").fancybox({});

    // select address type
    $('div#Adresses div.selectAddressType a.button7').live("click", function (e) {
        e.preventDefault();
        var $that = $(this);
        var $container = $(this).closest('div.addressSet');
        var addressId = $(this).attr('addressId');
        var isShipping = $container.attr('id') == 'adressInfo';
        var isBillingShippingSame = $('input[name="BillingAdressAction"]').val() == "S";
        var selectionType = "";
        if (!isShipping)
            selectionType = "Billing";
        else if (isBillingShippingSame)
            selectionType = "Both";
        else selectionType = "Shipping";

        $.ajax({
            type: "POST",
            url: _selectAddress,
            data: JSON.stringify({ addressId: addressId, selectionType: selectionType }),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                $that.addClass('checked').siblings().removeClass('checked');
                $container.find('.addressContainer').hide();
                $container.find('.addressContainer[addressId="' + addressId + '"]').show();

                //set hidden form input
                if (selectionType == "Shipping")
                    $('input[name="ShippingAddressId"]').val(addressId);
                else if (selectionType == "Billing")
                    $('input[name="BillingAddressId"]').val(addressId);
                else {
                    $('input[name="ShippingAddressId"]').val(addressId);
                    $('input[name="BillingAddressId"]').val(addressId);
                }
                if (selectionType != "Billing") {
                    $('ul#ulOrderTotals').html(data.OrderTotal);
                    $('div#ShippingMethod').html(data.ShippingMethod);
                }
            },
            beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
            complete: function () { $('div#loading').remove(); }
        });

    });

    //////////////
    $('a#addNewShippingAddress').click(function () {
        //hide current addresses
        $('#adressInfo .addressContainer').hide();
        $('#adressInfo .addressContainer[addressId = "0"]').show();
        //uncheck currentaddress title
        $('#adressInfo a.button7').removeClass('checked');
        $('input[name="ShippingAddressId"]').val('');
    });

    ////////////
    $('.clear').click(function () {
        ClearAddressForm($(this).closest('div.addressContainer'));
    });

    /////////////
    $('div.selectBox[name="AddressType"] li').click(function (e) {
        e.stopPropagation();
        ToggleAddressType($(this).attr('addressType') == 'P', $(this).closest('div.addressContainer'));
    });

    ////////////
    $('div.selectBox[name="AddressType"]').each(function () {
        ToggleAddressType($(this).find('li.on').attr('addressType') == 'P', $(this).closest('div.adddressContainer'));
    });

    //It is disabled for customer! for now!
    $('div.selectBox#shippingMethod').unbind('click');

    $('div.selectBox#shippingMethod li').click(function (e) {
        e.stopPropagation();
        var value = $(this).attr('data-value');

        $.ajax({
            type: "POST",
            url: _selectShippingMethod,
            data: JSON.stringify({ shippingoption: value }),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

            },
            beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
            complete: function () { $('div#loading').remove(); }
        });


    });

    $('a#checkoutPaymentInfoSubmit').click(function () {
        var valid = true;
        var message = "";
        var $form = $(this).closest('form');
        var $paymentForm = $('a[group="paymentMethod"].checked').closest('form');
        var $addressForm = $('form#CheckoutAddresses');
        if ($paymentForm.length > 0)
            valid = valid && $paymentForm.valid();
        //$addressForm.validate();
        $addressForm.valid();
        valid = valid && $addressForm.valid();
        var addressMessages = $addressForm.validate().errorList;
        if (addressMessages.length > 0) {
            for (var i = 0; i < addressMessages.length; i++) {
                message = message + addressMessages[i].message + '<br/>';
            }
        }
        if (valid) {

            $form.find('input').remove();

            //Add payment data
            $paymentForm.find('input').each(function () {
                $('<input>').attr({
                    type: 'hidden',
                    id: $(this).attr('id'),
                    name: $(this).attr('name'),
                    value: $(this).val()
                }).appendTo($form);
            });
            //Add address data
            $addressForm.find('input').each(function () {
                $('<input>').attr({
                    type: 'hidden',
                    id: $(this).attr('id'),
                    name: $(this).attr('name'),
                    value: $(this).val()
                }).appendTo($form);
            });
            //Add payment selection
            $('<input>').attr({
                type: 'hidden',
                id: 'paymentMethod',
                name: 'paymentMethod',
                value: $('input[name="paymentMethod"]').val()
            }).appendTo($form);

            $.ajax({
                type: "POST",
                url: $form.attr('action'),
                data: $form.serialize(),
                //dataType: 'json',
                success: function (data) {
                    //Valid operation
                    if (data.Status == 1) {
                        window.location.replace(data.RedirectUrl);
                    }
                    //exceptional case, redirect to aprropriate page
                    else if (data.Status == 2) {
                        window.location.replace(data.RedirectUrl);
                    }
                    //invalid checkout data
                    else {

                        $("html, body").animate({ scrollTop: $('div#paymentOptions').position().top }, "slow", function () { afFancy('', data.Messages, false); });
                        $('div#loading').remove();
                    }

                },
                beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
                complete: function () { }
            });

        }
        else {
            if (message != "") {
                $("html, body").animate({ scrollTop: $('#addressTitle').position().top + 10 }, "slow", function () { afFancy('', message, false); });

            }
            else { $("html, body").animate({ scrollTop: $('div#paymentOptions').position().top }, "slow"); }
            return false;
        }

    });

    //Billing address actions
    $('div.selectBox#BillingAdressAction li').click(function () {
        var value = $('input[name="BillingAdressAction"]').val();
        if (value == "S") {

            $('div#billingInfo .addressContainer').hide();
            $('div#billingInfo .selectAddressType').hide();
            $('input[name="BillingAddressId"]').val($('input[name="ShippingAddressId"]').val());
        }
        else if (value == "N") {
            $('div#billingInfo .selectAddressType').show();
            $('div#billingInfo .addressContainer').show();

            //unselect billing addresss
            $('#billingInfo .addressContainer').hide();

            $('#billingInfo .addressContainer[addressId="0"]').show();
            //uncheck currentaddress title
            $('#billingInfo a.button7').removeClass('checked');
            $('input[name="BillingAddressId"]').val('');
        }
        else {

            $('div#billingInfo .selectAddressType').show();
            //unselect billing addresss
            $('#billingInfo .addressContainer').hide();
            //select address
            //            $('#billingInfo a.button7').removeClass('checked');
            var addressId = $('#adressInfo a.button7.checked').attr('addressId');
            //            $('#billingInfo a.button7[addressId="' + addressId + '"]').click();
            //selectAddress(addressId);
            $('div#billingInfo div.selectAddressType a.button7[addressId="' + addressId + '"]').click();
        }
    });

    /////////////
    $('div.saveInfo input.save').click(function () {
        //conditional validation for address types.
        //        var $container = $(this).closest('form');
        //        var isEnterPrise = $container.find('div.selectBox[name="AddressType"] li.on').attr('addressType') == 'C';
        //        if (isEnterPrise) {
        //            $container.find('input[name="CivilNo"]').rules("remove");
        //            $container.find('input[name="Company"]').rules("add", { required: true });
        //            $container.find('input[name="TaxNo"]').rules("add", { required: true });
        //            $container.find('input[name="TaxOffice"]').rules("add", { required: true });
        //        }
        //        else {
        //            $container.find('input[name="CivilNo"]').rules("add", { required: true });
        //            $container.find('input[name="Company"]').rules("remove");
        //            $container.find('input[name="TaxNo"]').rules("remove");
        //            $container.find('input[name="TaxOffice"]').rules("remove");
        //        }

        if ($(this).closest('form').valid()) {
            SaveAddress($(this).closest('div.addressContainer'));
        }
    });

    $('.formCheckOut').submit(function () {
        return false;
    });

    //For promotinalcode at shopping-cart if trigged via link
    //$('a#applydiscountcouponcode').click(function () { $(this).closest('form').submit();});

});


function ToggleAddressType(isPersonal, $container) {
    if (isPersonal) {
        $container.find('div.personal').show();
        $container.find('div.company').hide();
    }
    else {
        $container.find('div.personal').hide();
        $container.find('div.company').show();
    }

    $container.find('input[name="IsEnterprise"]').val(!isPersonal);
}

function ClearAddressForm(container) {
    container.find('input[type="text"]').val('');
    container.find('textarea').val('');
    container.find('div.selectBox[name="AddressType"] li').first().click();
}

function UpdateAddressTitles(data, $container) {
    //TODO: mustafa do some exception cases!
    if (data.Status == 0) return;
    //new address is added.

    //var addressId = $('a.button7.checked').attr('addressId');

    //new address is added
    if (data.Status == 1) {
        $container.closest('div.addressSet').find('a.button7').removeClass('checked');
        $container.closest('div.addressSet').find('div.selectAddressType').append(
    '<a class="button7 checked" href="javascript:;" addressId="' + data.AddressId + '"><span>' + data.TitleHtml + '</span></a>');

        $container.closest('div.addressSet').append(data.Html);

        $container.closest('div.addressSet').find('div.selectAddressType a.button7[addressId="' + data.AddressId + '"]').click();
        //        $('div.shippingAddressContainer:visible');

    }
    //address is updated.
    else {
        // refresh address name (it may be updated.)
       $container.closest('div.addressSet').find('a.button7.checked span').html(data.TitleHtml);
       $container.closest('div.addressSet').find('h2.title strong').html($container.find('input[name="Name"]').val());
    }
}


function SaveAddress(container) {
        var $container = container;
        var id = $container.attr('addressId');
        var title = $container.find('input[name="Title"]').val();
        var name = $container.find('input[name="Name"]').val();
        var firstName = $container.find('input[name="FirstName"]').val();
        var lastName = $container.find('input[name="LastName"]').val();
        var address1 = $container.find('textarea[name="Address1"]').val();
        var city = $container.find('input[name="City"]').val();
        var stateProvinceId = $container.find('input[name=StateProvinceId]').val();
        var countryId = $container.find('input[name=CountryId]').val();
        var zipPostalCode = $container.find('input[name="ZipPostalCode"]').val();
        var phoneNumber = $container.find('input[name="PhoneNumber"]').val();
        var civilNo = '';
        var company = '';
        var taxNo = '';
        var taxOffice = '';
        var isEnterprise = false;
        var isShipping = $container.closest('div#adressInfo').length > 0;
        if ($container.find('div.selectBox[name="AddressType"] li.on').attr('addressType') == 'P') {
            civilNo = $container.find('input[name="CivilNo"]').val();
        }
        else {
            company = $container.find('input[name="Company"]').val();
            taxNo = $container.find('input[name="TaxNo"]').val();
            taxOffice = $container.find('input[name="TaxOffice"]').val();
            isEnterprise = true;
        }

        var model = {
            Id: id,
            Title: title,
            Name: name,
            FirstName: firstName,
            LastName: lastName,
            Company: company,
            Address1: address1,
            City: city,
            StateProvinceId: stateProvinceId,
            CountryId: countryId,
            ZipPostalCode: zipPostalCode,
            PhoneNumber: phoneNumber,
            TaxNo: taxNo,
            TaxOffice: taxOffice,
            CivilNo: civilNo,
            IsEnterprise: isEnterprise,
            DefaultShippingAddress:$container.find('input[name="DefaultShippingAddress"]').val(),
            DefaultBillingAddress:$container.find('input[name="DefaultBillingAddress"]').val()
        };
        $.ajax({
            type: "POST",
            url: isShipping ? _saveCheckoutShippingAddress : _saveCheckoutBillingAddress,
            data: JSON.stringify(model),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                UpdateAddressTitles(data, $container);
                if (data.Status == 0) {
                    afFancy('', data.Message, false);
                    return;
                }
                if (isShipping) {
                    $('ul#ulOrderTotals').html(data.OrderTotal);
                    $('div#ShippingMethod').html(data.ShippingMethod);
                }
                ClearAddressForm($('div.addressContainer[addressId="0"]'));
                //  $("html, body").animate({ scrollTop: $('#paymentOptions').position().top + 10 }, "slow", function () { afFancy('', data.Message, true); });
                
                if ($('input[name="paymentMethod"]').val()=="Payments.PurchaseOrder") {
                    $("html, body").animate({ scrollTop: $('#ulOrderTotals').position().top + 10 }, "slow", function () { });
                    if ($("#withCard a").attr('rdovalue') == 'Payments.CC') {
                        $("#withCard a").removeClass("checked");
                    }
                    if ($("#withOtherMethod a").attr('rdovalue') == 'Payments.PurchaseOrder') {
                        $("#withOtherMethod a").addClass('checked');
                    }

                } else {
                    $("html, body").animate({ scrollTop: $('#paymentOptions').position().top + 10 }, "slow", function () { });
                }
            },
            beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
            complete: function () {
                $('div#loading').remove();
                //change button style and text
                //$container.find('div.saveInfo input.save').addClass("selected");
                //$container.find('div.saveInfo input.save').val($container.find('input#savedText').val());
            }

        });
    }
    //@Url.Action("Payment3DRequest","Checkout")
    function Open3DGateOverlay(title, message, loadingContent) {
        var id = 'Gateway3DSecure';
            $.fancybox(
            '<div style="min-width: 610px; height: 800px; overflow: auto; position: relative;">' +
            '<div style="color:#fff;" id="' + id + '">' +
            '<p class="textCenter" style="text-transform:uppercase">' + title + '<p>' +
            '<p id="Gateway3DSecureClose" class="textCenter" style="text-transform:none;font-size:12px; cursor: hand; cursor: pointer; ">' + message + '<p></div>' +
            '<div id="Gateway3DSecureLoading" style="height:90%; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat;  ">' + loadingContent + '</div>' +
            '<iframe id="Gateway3DSecureFrame" style=" background-color:#fff; display:none;" src="/Checkout/Payment3DRequest" width="600px" height="650px"></iframe></div>',
            {
                'autoDimensions': false,
                'width': 'auto',
                'height': 'auto',
                'transitionIn': 'none',
                'transitionOut': 'none',
                'showCloseButton':'true'
//                'modal': 'false'
//                'hideOnOverlayClick':'false',
//                'hideOnContentClick':'false',
//                'overlayShow':'true',
//                'enableEscapeButton':'false',

                
            }
		);

           

            var int = self.setInterval(function () {
                try {
                    $('iframe#Gateway3DSecureFrame').contents().find("html");
                }
                catch (err) {
                    $('iframe#Gateway3DSecureFrame').show();
                    $('#Gateway3DSecureLoading').hide();
                    window.clearInterval(int);
                }
            },
            200);

            //if iframe loading can not be detected show it anyway.
            setTimeout(function(){
             $('iframe#Gateway3DSecureFrame').show();
                    $('#Gateway3DSecureLoading').hide();
                    window.clearInterval(int);

            },10000);


            $('#Gateway3DSecureClose').click(function f() { $.fancybox.close(); });

        }


        
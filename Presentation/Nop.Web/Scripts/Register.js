$(document).ready(function () {

    $('a#Register').click(function () {
        $('div#selectBoxGender input#Gender').val($('div#selectBoxGender li.on').attr('option'));



        $(this).closest('form').submit();


    });

    $('input.continue').click(function () {

        $('input#RememberMe').val($('input#RememberMe').prev('a').hasClass('checked'));


    });

    // Campanya sayfası
    $('#CampaignRegister').click(function () {

        $('div#selectBoxGender input#Gender').val($('div#selectBoxGender li.on').attr('option'));

        if ($('input#terms-conditions-agreement').is(':checked')) {
            $(this).closest('from').submit();
        }
        else {
            afFancy('', _policyCheckedMessage, false);
            return false;
        }

    });


    $('#CampaignConvoke').click(function () {
        $(this).closest('form').submit();
    });

    //Menu newsletter registeration
    $('div#headerTop form#RegisterNewsLetter').submit(function (e) {
        return false;
    });

    $('div#headerTop input#newsletter').click(function (e) {

        if ($(this).closest('form').valid()) {
            $.ajax({
                url: "/Customer/RegisterCustomerNewsletter",
                data: $(this).closest('form').serialize(),

                success: function (data) {
                    if (data.Success) {
                        afFancy(data.header, data.message, data.Success);
                       
                        e.preventDefault();
                        e.stopPropagation();
                        $('li#umEmailSignUp div.popupClose a').parent().parent().parent().stop().slideUp({ duration: 175, easing: 'easeInOutCirc', complete: function () { $(this).parent().removeClass('on'); } });
                    }
                    else {
                        afFancy(data.header, data.message, data.Success);
                    }
                },
                complete: function () {
                    $('form#RegisterNewsLetter').clearForm();
                }
            });
        }

    });

    //landing page registration

    $('form#LandingRegisterationForm').submit(function (e) {
        return false;
    });
    //TODO:
    $('form#LandingRegisterationForm a.button1').click(function (e) {

        if ($(this).closest('form').valid()) {
            $.ajax({
                url: "/Customer/RegisterCustomerNewsletter",
                data: $(this).closest('form').serialize(),

                success: function (data) {
                    if (data.Success) {
                        afFancy(data.header, data.message, data.Success);
                        setCookie('registerationAsked', 100, 365);
                    }
                    else {
                        afFancy(data.header, data.message, data.Success);
                    }
                },
                complete: function () {
                    $('div#newsletterRegister').hide();
                }
            });
        }

    });

    $('div#NewsLetterSubscription div.popupClose').click(function () {

        $('div#NewsLetterSubscription').hide();

    });

    $('div.saveInfo.clearfix#subscribeNewsletter a').click(function () {
        if ($('form#formInfo').valid()) {
            $.ajax({
                url: "/Customer/RegisterCustomerNewsletter",
                data: $(this).closest('form').serialize(),
                success: function (data) {
                    if (data.success) {

                        $('div.saveInfo.clearfix#unSubscribeNewsletter').show();
                        $('div.saveInfo.clearfix#subscribeNewsletter').hide();
                        afFancy(data.header, data.message, data.Success);
                    }
                    else {
                        $('div.saveInfo.clearfix#unSubscribeNewsletter').hide();
                        $('div.saveInfo.clearfix#subscribeNewsletter').show();
                        afFancy(data.header, data.message, data.Success);
                    }

                },
                complete: function () {
                    $('div.saveInfo.clearfix#unSubscribeNewsletter').show();
                    $('div.saveInfo.clearfix#subscribeNewsletter').hide();
                }
            });
        }
    });

    $('div.saveInfo.clearfix#unSubscribeNewsletter a').click(function () {
        if ($('form#formInfo').valid()) {
            $.ajax({
                url: "/Customer/UnRegisterCustomerNewsletter",
                data: $(this).closest('form').serialize(),
                success: function (data) {
                    if (data.success) {
                        $('div.saveInfo.clearfix#unSubscribeNewsletter').hide();
                        $('div.saveInfo.clearfix#subscribeNewsletter').show();
                        afFancy(data.header, data.message, data.Success);
                    }
                    else {
                        $('div.saveInfo.clearfix#unSubscribeNewsletter').show();
                        $('div.saveInfo.clearfix#subscribeNewsletter').hide();
                        afFancy(data.header, data.message, data.Success);
                    }

                },
                complete: function () {
                    $('div.saveInfo.clearfix#unSubscribeNewsletter').hide();
                    $('div.saveInfo.clearfix#subscribeNewsletter').show();
                }
            });
        }
    });

    $('div.saveInfo a#SaveInfo').click(function () {
        if ($(this).closest('form').valid()) {
            $(this).closest('form').submit();
        }
    });

    $('div#accountInformation a#ResetInfo').click(function () {
        $(this).closest('form').clearForm();
    });

    $('form#frmRegisterNewsLetter').submit(function (e) {
        return false;
    });

    $('input#btnNewsLetter').click(function (e) {
        var emailReg = /^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})?$/;
        var email = $("#frmRegisterNewsLetter #Email").val();
        if (email != ''
            && email != $("#frmRegisterNewsLetter #Email").prop("defaultValue")
            && emailReg.test(email)
            ) {
            $.ajax({
                url: "/Customer/RegisterCustomerNewsletter",
                data: $(this).closest('form').serialize(),

                success: function (data) {
                    if (data.Success) {
                        afFancy(data.header, data.message, data.Success);

                        e.preventDefault();
                        e.stopPropagation();
                        //$('li#umEmailSignUp div.popupClose a').parent().parent().parent().stop().slideUp({ duration: 175, easing: 'easeInOutCirc', complete: function () { $(this).parent().removeClass('on'); } });
                    }
                    else {
                        afFancy(data.header, data.message, data.Success);
                    }
                },
                complete: function () {
                    $('form#frmRegisterNewsLetter').clearForm();
                }
            });
        }
        else
            afFancy('', $("#frmRegisterNewsLetter #Email").attr("errorMessage"), false)
    });

});

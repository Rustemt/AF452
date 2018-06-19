$(function () {
    $('div.addressContainer div.rules a.checkbox').unbind('click');
    InitAddressForms();
    //ADDRESSTYPE
   
    
});


function InitAddressForms() {


    //CHECKBOX
    $('div.addressContainer div.rules').click(function () {
        $(this).find('a.checkbox').toggleClass('checked');
        var defaultValue = $(this).find('a.checkbox').hasClass('checked');
        var form = $(this).closest('form');
        if (form.attr('id') == 'formShipping') {
            form.find('input#DefaultShippingAddress').val(defaultValue);
        }
        if (form.attr('id') == 'formBilling') {
            form.find('input#DefaultBillingAddress').val(defaultValue);
        }
    });

    // SELECTBOX

    $('div.selectBox').unbind('click').click(function () {
        var that = $(this);
        if (!that.hasClass('on')) { that.addClass('on').children('ul').stop().slideDown({ duration: 150, easing: 'easeInOutQuad' }); }
        else { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
    }).hover(function () { }, function () {
        var that = $(this);
        if (that.hasClass('on')) { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
    });

    $('div.selectBox ul li').unbind('click').click(function (e) {
        e.stopPropagation();
        var that = $(this);
        that.addClass('on').siblings().removeClass('on');
        that.parent().prev().text(that.text());
        that.parent().parent().find(':hidden').val(that.attr('data-value'));
        that.parent().parent().trigger('click');
    });

    //ADDRESS SELECT 
    $('div.selectBox#addressIdBilling li').click(function () {
        var id = $(this).attr('data-value');
        var uri = _getAddressBilling + '/' + id;
        if (id == 0) {
            $('#formBilling').clearForm();
            $('input#Id').val('0');
            $('input#billingAddressButtonAdd').show();
            $('input#billingAddressButtonEdit').hide();
            return;
        }
        else {
            $.ajax({ url: uri,
                type: "GET",
                cache:false,
                success: function (data) {
                    $('div#addressBillingContainer').html(data);
                },
                beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
                complete: function (data) {
                    $('div#loading').remove();
                    InitAddressForms();
                }
            });
        }
    });


    //ADDRESS SELECT 
    $('div.selectBox#addressIdShipping li').click(function (e) {
        var id = $(this).attr('data-value');
        if (id == 0) {
            $('#formShipping').clearForm();
            $('input#Id').val('0');
            $('input#shippingAddressButtonAdd').show();
            $('input#shippingAddressButtonEdit').hide();
            return;
        }
        else {
            var uri = _getAddressShipping + '/' + id;
            $.ajax({ url: uri,
                type: "GET",
                cache: false,
                success: function (data) {
                    $('div#addressShippingContainer').html(data);
                },
                beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
                complete: function (data) {
                    $('div#loading').remove();
                    InitAddressForms();
                }
            });
        }
    });

    //COUNTRY SELECT
    $('div.selectBox#CountryId li').click(function (e) {
        e.stopPropagation();
        var selectedItem = $(this).attr('data-value');
        var $country = $(this).closest('div.addressContainer').find('input[name="CountryId"]');
        $country.val($(this).attr('data-value'));
        var ddlStates = $(this).closest('div.addressContainer').find('div.selectBox#StateProvinceId');
        $.ajax({
            cache: false,
            type: "GET",
            url: _getStatesByCountryId,//'/Country/GetStatesByCountryId',
            data: "countryId=" + selectedItem + "&addEmptyStateIfRequired=true",
            success: function (data) {
                ddlStates.find('span').html(data[0].name);
                var $ul = ddlStates.find('ul');
                $ul.html('');
                $.each(data, function (id, option) {
                    $ul.append('<li value="' + option.id + '">' + option.name + '</li>');
                    //                    $ul.append($('<option></option>').val(option.id).html(option.name));
                });
                $ul.find('li').first().addClass('on');
                ddlStates.find('input').val(data[0].id);
                SetupSelectBox(ddlStates);
            },
            beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve states.');
            },
            complete: function () { $('div#loading').remove(); }
        });

    });

    $('div#addressBillingContainer div#AddressType li').click(function () {
        if ($(this).attr('addressType') == 'P') {
            $('div#personal').show();
            $('div#company').hide();
            $('div#addressBillingContainer input#IsEnterprise').val('False');
        }
        else {
            $('div#personal').hide();
            $('div#company').show();
            $('div#addressBillingContainer input#IsEnterprise').val('True');
        }
    });

}










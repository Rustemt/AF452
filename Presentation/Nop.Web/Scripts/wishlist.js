$(function () {
    InitWishList();
    InitPriceOffer();

    $('div.whislistSelectBarLeft a#removeAll').click(function () {
        var ids = '-';
        $('div.wishedItemLeft div.date a.checked').each(function () {
            ids = ids + $(this).attr('group').substring(12, $(this).attr('group').length) + '-';
        });
        $.ajax({
            type: "POST",
            url: _deleteWishListCartItemCollection,
            data: "id=" + ids + "",
            success: function (data) {
                $('div.wishedItemsContainer').html(data);
                InitChecboxes($('div.wishedItemsContainer'));
                InitWishList();
                location.reload();
            },
            error: function (data) {

            },
            complete: function () { }
        });
    });


    //SEND PRICE FORM MAIL
    //    $('div.wishedItemsContainer a#priceOfferLink').click(function () {
    //        var $form = $(this).closest('form');
    //        if ($form.valid()) {
    //            $.ajax({
    //                type: "POST",
    //                url: $(this).closest('form').attr('action'),
    //                data: $(this).closest('form').serialize(),
    //                success: function (data) {
    //                    if (data) {
    //                        $('div#sendPrice #successResult').show();
    //                        $('div#sendPrice #errorResult').hide();
    //                        $('div#sendPrice #formDivision').hide();
    //                    }
    //                    else {

    //                        $('div#sendPrice #successResult').hide();
    //                        $('div#sendPrice #errorResult').show();
    //                        $('div#sendPrice #formDivision').hide();
    //                    }
    //                },
    //                complete: function () {
    //                }
    //            });
    //        }
    //    });



    //


    $('div.selectBox ul#wishListSelectBox li').click(function () {

        var sortType = $(this).attr('value');
        $.ajax({
            url: _sortWishListby,
            data: "id=" + sortType + "",
            beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
            success: function (data) { $('div.wishedItemsContainer').html(data); InitChecboxes($('div.wishedItemsContainer')); InitWishList(); },
            complete: function () { $('div#loading').remove(); }
        });
    });


    $('div#wishListFooter a#buySelected').click(function () {
        var ids = '-';
        $('div.wishedItemsContainer div.date a.checked').each(function () {
            ids = ids + $(this).attr('group').substring(12, $(this).attr('group').length) + '-';
        });
        $.ajax({
            type: "POST",
            url: _buySelectedUrl,
            data: "id=" + ids + "", beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); }, 
            success: function (data) {
                if (data.success) {
                    UpdateCartWish(data.html)
                }
                else {
                    afFancyByList(data.message, '', false);
                }
            },
            complete: function () { $('div#loading').remove(); }
        });
    });

//    $('form#ProductSendEmailForm a#WishListSendEmail').click(function () {
//        //var result = $('form#RegisterNewsLetter').validate();
//        if ($('form#ProductSendEmailForm').valid()) {
//            $.ajax({
//                type: "POST",
//                url: $(this).closest('form').attr('action'),
//                data: $(this).closest('form').serialize(),
//                success: function (data) {
//                    if (data.Success) {
//                        $('#ProductSendEmailSuccess').show();
//                        $('#ProductSendEmailError').hide();
//                    }
//                    else {
//                        $('#ProductSendEmailSuccess').hide();
//                        $('#ProductSendEmailError').show();
//                        if (data.Message)
//                            $('#ProductSendEmailError p.message').html(data.Message);
//                    }
//                },
//                complete: function () {
//                }
//            });
//        }

//    });




    $('div#whislistHeader a#sendEmailLink').fancybox({});
    $('div#sendWishListEmail a#WishListSendEmail').click(function () {
        if ($(this).closest('form').valid()) {
            $.ajax({
                type: "POST",
                url: $(this).closest('form').attr('action'),
                data: $(this).closest('form').serialize(),
                beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
                success: function (data) { afFancy(data.header, data.message, data.success); },
                complete: function () { $('div#loading').remove(); }
            });
        }
    });
});

//Shopping cart
function UpdateCartWish(data) {
    if (data != '') {
        $('li#umShopBag').html(data);
        $('li#umShopBag').addClass('on').children('div:last').stop().slideDown({ duration: 175, easing: 'easeInOutCirc' });
        $('html, body').animate({ scrollTop: 0 }, { duration: 1000, easing: 'easeInOutCirc' });
        setTimeout(
         "$('li#umShopBag').children('div:last').stop().slideUp({ duration: 175, easing: 'easeInOutCirc', complete: function () { $(this).parent().removeClass('on'); $(this).attr('style', ''); } })",
        5000);
    }
}

function InitWishList() {

    //OFFER PRICE BUTTON CLICK

    if (IsGuest()) { 
     $('a#wishListOffer').fancybox({});
    }


    $('ul.bottomButtons a#wishListOffer').click(function () {
        OpenPriceOffer($(this).attr('product-id'), $(this).attr('product-name'), $(this).attr('product-img'));
         
        if (!IsGuest()) {
             $('div#sendPrice a#priceOfferLink').click();
        }
       
        $('div#sendPrice #formDivision').show();
        return false;
    });

    //Select All
    $('a.cbSelectAll').click(function () {
        $(this).hasClass('checked') ? $('div.wishedItemLeft div.date .checkbox').addClass('checked') : $('div.wishedItemLeft div.date .checkbox').removeClass('checked');
    });

    $('ul.bottomButtons a.button1').click(function () {
        //if clicked button is offer button dont work
        var dataVal = $(this).attr('data-val');
        if (dataVal == 'offer')
            return;
        // var uri = _addtoCartFromWishlist; dataVal;
        $.ajax({
            type: "POST",
            url: _addtoCartFromWishlist,
            data: "id=" + dataVal + "",
            cache: false,
            success: function (data) {
                if (data.success) {
                    UpdateCartWish(data.html);
                }
                else {
                    afFancy(data.message, '', false);
                }
            },
            beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
            complete: function () { $('div#loading').remove(); }
        });
    });

    $('ul.bottomButtons a.button6').click(function () {
        var modelId = $(this).attr('data-val');
        var comment = $('div.wishedItemRight textarea#' + modelId).val();
        //var uri = 'update/wishitem/' + modelId + '/' + comment;
        //{ name: 'fooId', value: 'fooValue', args: args };

        $.ajax({
            type: "POST",
            url: _updateWishlist,
            data: "id=" + modelId + "&comment=" + comment + "",
            success: function (data) {
                if (data.success) {
                    afFancy(data.header, "", true);
                }
                else {
                    afFancy(data.header, "", false);
                }
            },
            complete: function () { }
        });
    });
    $('div.wishedItemRight a.button2').click(function () {
        $.ajax({
            type: "POST",
            url: _deleteWishListCartItem,
            data: "id=" + $(this).attr('data-val') + "",
            cache: false,
            success: function (data) {
                if (data.count == 0) {
                    location.reload();
                }
                else {
                    $('div.wishedItemsContainer').html(data.output);
                    InitChecboxes($('div.wishedItemsContainer'));
                    InitWishList();
                }
            }
        });
    });


}


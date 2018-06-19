$(function () {


    InitAttributeSelections();
    SetVariantSelection();
    InitAddToBag();
    InitProductSendEmail();
    InitAddToWishList();
    InitPriceOffer();
    InitPriceOfferPopup();
    if (IsGuest()) {
        $('a#offerLink').fancybox({});
    }



    $('a#offerLink').click(function () {
        OpenPriceOffer($(this).attr('product-id'), $(this).attr('product-name'), $(this).attr('product-img'));
        if (!IsGuest()) {
            $('div#sendPrice a#priceOfferLink').click();
        }
        return false;
    });


    //Diynamicly set product description div.
    var height = $('div#productRightTop').height();
    var descHeight = 614 - height;
    $('div#productDesc').height(descHeight);


    // product info
    (function () {
        checkBind($('div.productPicLeft a').attr('href').toString());
        $('ul.productPicRight li').not('.on').css({ opacity: 0.5 });
        $('ul.productPicRight li').hover(function () { $(this).stop().animate({ opacity: 1 }); }, function () { $(this).not('.on').stop().animate({ opacity: 0.5 }); });
        $('ul.productPicRight li a').click(function (e) {
            e.preventDefault();
            var that = $(this);
            //if (that.parent().hasClass('on')) { return; }
            that.parent().addClass('on').css('opacity', 1).siblings().removeClass('on').css('opacity', 0.5);
            //$('div.productPicLeft a img').attr('src', $(this).attr('href')).parent().attr('href', $(this).attr('href'));
            var bigPic = that.attr('rel');
            var pic = that.attr('href');
            $('div.productPicLeft').html('<a href="' + bigPic + '"><img src="' + pic + '" width="470" height="612" /></a>');
            $('div#product div.fullScreen a').attr('href', bigPic);
            $('.fullScreenZoom:not(".hiddenVariant")').find('ul.fullScreenZoomThumbs li:eq(' + that.parent().prevAll().length + ') a').trigger('click');
            checkBind(that.attr('href').toString());

        });
    })();

    // product detail scroll
    (function () {
        $('div#productDesc').jScrollPane()
    })();

    // find similar slider
    (function () {
        var sliderContainer = $('div#findSimilarSliderInner ul');
        var sliderContainerWidth = sliderContainer.children().length * 95;
        sliderContainer.css({ width: sliderContainerWidth + 'px', left: 0 });

        var animating = false;
        $('div#findSimilarSliderLeft a').click(function (e) {
            e.preventDefault();
            if (animating) return;
            if (sliderContainer.position().left == 0) return;
            //sliderContainer.stop().animate({ left: 0 }, { duration: 300, easing: 'easeInOutQuad' });
            animating = true;
            sliderContainer.stop().animate({ left: '' + (95 + sliderContainer.position().left) + 'px' }, { duration: 200, easing: 'easeInOutQuad', complete: function () { animating = false; } });
        });

        $('div#findSimilarSliderRight a').click(function (e) {
            e.preventDefault();
            if (animating) return;
            if (sliderContainerWidth + sliderContainer.position().left - 95 * 3 == 0)
                return;
            if (sliderContainerWidth <= 95 * 3)
                return;
            //sliderContainer.stop().animate({ left: '-' + (sliderContainerWidth / 2) + 'px' }, { duration: 300, easing: 'easeInOutQuad' });
            animating = true;
            sliderContainer.stop().animate({ left: (sliderContainer.position().left - 95) + 'px' }, { duration: 200, easing: 'easeInOutQuad', complete: function () { animating = false; } });
        });

    })();

    // full screen image
    //(function(){ $('div#product div.fullScreen a').prettyPhoto({ allow_resize: true, default_width: 800, deeplinking: false, social_tools: '', modal: false }); })();

    // size chart
    (function () {
        var sizeChartButton = $('div#productColorSize div.sizeChart span.sizeChartButton');
        var sizeChartCloseButton = $('div#productColorSize div.sizeChart div.detail span.close');

        sizeChartButton.click(function () {
            var that = $(this).parent();
            if (!that.hasClass('on')) { that.addClass('on').children(':last').fadeIn(300); }
            else { that.removeClass('on').children(':last').hide(); }
        });

        sizeChartCloseButton.click(function () { sizeChartButton.trigger('click'); });
    })();

    // send email
    (function () {
        var sendEmailButton = $('div#shareProduct div.sendEmail em.button');

        //sendEmailButton.click(function(){
        //	var that = sendEmailButton.parent();
        //	if(!that.hasClass('on')){ that.addClass('on').children('div.detail').fadeIn(300); }
        //	else { that.removeClass('on').children('div.detail').hide(); }
        //});

        //$('div#shareProduct div.sendEmail div.detail span.close').click(function(){ sendEmailButton.trigger('click'); });

        sendEmailButton.parent().hover(function () {
            var that = $(this);
            that.addClass('on').children('div.detail').fadeIn(300);
        }, function () {
            var that = $(this);
            that.removeClass('on').children('div.detail').hide();
        });

    })();

    // full screen
    (function () {
        var zoomWindow = $('div.fullScreenZoom');
        var zoomWindowImage = $('div.fullScreenZoomPic');
        var zoomWindowClose = $('div.fullScreenZoomClose');
        var thumbs = $('ul.fullScreenZoomThumbs li');
        thumbs.not('.on').css({ opacity: '0.6' });

        var currentWidth = 0;
        var currentHeight = 0;

        if (zoomWindow.length < 1) { return; }

        $(window).resize(function () {
            currentWidth = $(window).width();
            currentHeight = $(window).height();

            zoomWindow.css({ width: currentWidth + 'px', height: currentHeight + 'px' });
            zoomWindowImage.css({ width: currentWidth + 'px', height: currentHeight + 'px' });
        }).trigger('resize');

        zoomWindowImage.mousemove(function (e) {
            var image = zoomWindowImage.children('img');
            var position = parseInt((e.pageY / (currentHeight - 5)) * (image.height() - currentHeight), 0);
            image.css({ top: '-' + position + 'px' });
        });

        zoomWindowClose.click(function () { zoomWindow.fadeOut(150, function () { $(this).attr('style', ''); }); $('div#footer, div#container, div#header').show(); });

        thumbs.hover(function () { if ($(this).hasClass('on')) { return; } $(this).stop().animate({ opacity: 1 }, 300); }, function () { if ($(this).hasClass('on')) { return; } $(this).stop().animate({ opacity: 0.6 }, 300); });
        thumbs.children('a').click(function (e) {
            e.preventDefault();
            var that = $(this);
            that.parent().addClass('on').attr('style', '').stop().siblings().removeClass('on').stop().animate({ opacity: 0.6 }, 300);
            var image = zoomWindowImage.children('img');
            image.attr('src', that.attr('href'));
        });

        $('div#product div.fullScreen a').click(function (e) {
            if (!bigpicture) return false;
            e.preventDefault();
            $('div#footer, div#container, div#header').hide();
            $(window).trigger('resize');
            zoomWindow.fadeIn(300);
        })

    })();

    //Price offer tooltip
        (function () {
            $('#productPriceOffer').bind('click', function () { $('.offerTooltip').fadeOut(300); });
        })();
});

//shows or hides buy button
function showCallForPriceButton(callForPrice) {
    if (callForPrice) {
        $('div#productAddToRight').hide();
        $('div#quantity').hide();
        $('div#productPriceOffer').show();
    }
    else {
        $('div#productAddToRight').show();
        $('div#quantity').show();
        $('div#productPriceOffer').hide();
    }
}


//mustafa todo: may be re-coded.
function InitAttributeSelections() {
    var variantId = $('#variantId').val();
    $('div.color').hide();
    $('div.color[parentId="0"]').show();
    $('div.selectBox ul li').click(function (e) {
        showCallForPriceButton($(this).hasClass('callPrice'));
        e.stopPropagation();
        var valueId = $(this).attr('valueId');
        var level = $(this).closest('div.color').attr('level');
        var quantity = $(this).attr('quantity');
        $('div.color').each(function f() {
            if ($(this).closest('div.color').attr('level') > level) {
                $(this).closest('div.color').hide();
            }
        });
        if (level == 1) {
            var vairantId = $(this).attr('ownerVariantId');
            $('#variantId').val($(this).attr('ownerVariantId'));
            SetVariantSelection();
        }
        $('div.color[parentId="' + valueId + '"]').show();
        $('div.color[parentId="' + valueId + '"]').find('li').first().click();
        $('div.color[parentId="' + valueId + '"]').find('li').first().parent().parent().trigger('click');

    });
    $('div.color[parentId="0"] li[ownerVariantId="' + variantId + '"]').first().click();
    $('div.color[parentId="0"] li[ownerVariantId="' + variantId + '"]').first().parent().parent().trigger('click');

}

//aaa


//updates page for the selected variant data.
function SetVariantSelection() {
    var variantId = $('#variantId').val();
    $('[variantId]').addClass('hiddenVariant');
    $('[variantId="' + variantId + '"]').removeClass('hiddenVariant');
    $('.productPic:not(".hiddenVariant") ul.productPicRight li a').first().click();
}



function AddToCart(container) {

    var productId = $('input#productId').val();
    var variantId = $('input#variantId').val();
    var quantity = parseInt($('div#priceStock[variantId="' + variantId + '"] div.quantityRight div.selectBox li.on').text());
    var recipientName = $("input[id $='RecipientName']").val();
    var recipientEmail = $("input[id $='RecipientEmail']").val();
    var yourName = $("input[id $='SenderName']").val();
    var yourEmail = $("input[id $='SenderEmail']").val();
    var message = $("textarea[id $='_Message']").val();

    if (isNaN(quantity)) quantity = 1;
    var shoppingCartType = 1;
    var attributes = new Array();
    $('div#productColorSize div.selectBox li.on[ownerVariantId="' + variantId + '"]').each(function f() {
        attributes.push({
            ProductVariantAttributeValueId: $(this).attr('valueId'),
            ProductAttributeId: $(this).closest('div.color').attr('attributeId')
        });
    });
    var model = {
        ProductId: productId,
        VariantId: variantId,
        Quantity: quantity,
        ShoppingCartType: shoppingCartType,
        Attributes: attributes,
        RecipientName: recipientName,
        RecipientEmail: recipientEmail,
        YourName: yourName,
        YourEmail: yourEmail,
        Message: message
    };

    $.ajax({
        type: "POST",
        url: _addToCartUrl,
        data: JSON.stringify(model),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            UpdateCart(data);
        },
        beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
        complete: function () { $('div#loading').remove(); }

    });

}
function AddToList() {

//    var productId = $('input#productId').val();
//    var variantId = $('input#variantId').val();
//    alert(variantId);
//    var quantity = parseInt($('div.quantityRight div.selectBox li.on').text());
//    if (isNaN(quantity)) quantity = 1;
//    var shoppingCartType = 1;
//    var attributes = new Array();
//    $('div#productColorSize div.selectBox li.on[ownerVariantId="' + variantId + '"]').each(function f() {
//        attributes.push({
//            ProductVariantAttributeValueId: $(this).attr('valueId'),
//            ProductAttributeId: $(this).closest('div.color').attr('attributeId')
//        });
//    });
//    var model = {
//        ProductId: productId,
//        VariantId: variantId,
//        Quantity: quantity,
//        ShoppingCartType: shoppingCartType,
//        Attributes: attributes
//    };

    var productId = $('input#productId').val();
    var variantId = $('input#variantId').val();
    var quantity = parseInt($('div#priceStock[variantId="' + variantId + '"] div.quantityRight div.selectBox li.on').text());
    if (isNaN(quantity)) quantity = 1;
    var shoppingCartType = 1;
    var attributes = new Array();
    $('div#productColorSize div.selectBox li.on[ownerVariantId="' + variantId + '"]').each(function f() {
        attributes.push({
            ProductVariantAttributeValueId: $(this).attr('valueId'),
            ProductAttributeId: $(this).closest('div.color').attr('attributeId')
        });
    });
    
    var model = {
        ProductId: productId,
        VariantId: variantId,
        Quantity: quantity,
        ShoppingCartType: shoppingCartType,
        Attributes: attributes
    };

    $.ajax({
        type: "POST",
        url: _addToWishList,
        data: JSON.stringify(model),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            if (data.isGuest) {
                window.location.replace(data.href);
            }
            else if (data.success) {
                afFancyWishList(data.Message, data.click, data.href, data.success);
            }
            else {
                afFancyWishList(data.Message, data.click, data.href, data.success);
            }
        },
        beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
        complete: function () { $('div#loading').remove(); }

    });

}
function InitAddToBag() {
    $('div#productAddToRight input.button1').not('.soldOut').click(function f(e) {e.stopPropagation(); AddToCart($(''));
    });
}

function InitAddToWishList() {
    $('div#productAddToLeft a.button2').click(function f(e) { e.stopPropagation(); AddToList(); });
}



function InitFB() {
    $('a#shareFB').click(function () {
        $('.fb-like').click();
    });
}


function fullScreenBind() {


}
var bigpicture = false;
function checkBind(imgUrl) {
    var newImg = new Image();
    newImg.src = imgUrl;
    if (newImg.height > 0) {
        if (newImg.height > 612 && newImg.width > 470) {
            bindjQZoom();
            bigpicture = true;
        }

        else {
            $('div.productPicLeft a').attr('href', 'javascript:;');
            $('div.productPicLeft a').css('cursor', 'default');
            bigpicture = false;
        }
    }
    else {

        newImg.onload = function () {
            var dizi = [newImg.height, newImg.width];
            if (newImg.height > 612 && newImg.width > 470) {
                bindjQZoom();
                bigpicture = true;
            }
            else {
                $('div.productPicLeft a').attr('href', 'javascript:;');
                $('div.productPicLeft a').css('cursor', 'default');
                bigpicture = false;
            }
            if (!bigpicture) {
                $('div.fullScreen a').hide();
            }
            else {
                //göster
                $('div.fullScreen a').show();
            }

        };
    }
    if (!bigpicture) {
        $('div.fullScreen a').hide();
    }
    else {
        //göster
        $('div.fullScreen a').show();
    }




}

function bindjQZoom() {
    var options = {
        zoomType: 'standard',
        lens: true,
        preloadImages: true,
        alwaysOn: false,
        zoomWidth: 467,
        zoomHeight: 610,
        xOffset: 1,
        yOffset: 0,
        position: 'right',
        title: false
    };
    $('div.productPicLeft a').jqzoom(options);
    $('.zoomWindow').live('mouseover', function () { $(this).hide(); });
}

function InitProductSendEmail() {
    $('form#ProductSendEmailForm a#ProductSendEmail').click(function() {
        //var result = $('form#RegisterNewsLetter').validate();
        if ($('form#ProductSendEmailForm').valid()) {
            $('form#ProductSendEmailForm input#VariantId').val($('#variantId').val());
            $.ajax({
                type: "POST",
                url: $(this).closest('form').attr('action'),
                data: $(this).closest('form').serialize(),
                success: function(data) {
                    if (data.Success) {
                        afFancy(data.Message, "", true);
                        $("input#YourName").val("");
                        $("input#YourEmailAddress").val("");
                        $("input#FriendName").val("");
                        $("input#FriendEmail").val("");
                        $("textarea#PersonalMessage").val("");
                    } else {
                        afFancy(data.Message, "", false);

                        if (data.Message)
                            $('#ProductSendEmailError p.message').html(data.Message);
                    }
                },
                complete: function() {
                }
            });
        } else {
            return false;
        }
    });



}


function PopupPriceOffer() {


    setCookieHour('priceOfferAskedSession', 1, 23);
    var count = getCookie('priceOfferAsked');
    if (typeof count === "undefined") {
        count = 0;
    }
    count++;
    setCookie('priceOfferAsked', count, 365);
    document.cookie = 'priceOfferAsked=' + count;
    var as = $("#priceOfferToolTip").val();
    // teklif iste
    (function () {
        $('#productPriceOffer').jSpot({ text: as });
    })();

}

function InitPriceOfferPopup() {
    // teklif iste otomatik actirma
    var priceOfferAsked = getCookie('priceOfferAsked');
    var priceOfferAskedSession = getCookie('priceOfferAskedSession');

    if ((typeof priceOfferAsked === "undefined" || priceOfferAsked < 3) && (typeof priceOfferAskedSession === "undefined" || priceOfferAskedSession == 0)) {
        window.setTimeout("PopupPriceOffer();", 1500);
    }
    else
        $('.offerTooltip').show();
}

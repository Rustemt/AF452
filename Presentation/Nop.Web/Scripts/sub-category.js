$(function () {

    InitAttributeSelections();
    InitSorting();
    InitAddToBag();
    InitPriceOffer();
    if (IsGuest()) {
        $("div.buttons a.button1[data-val='offer']").fancybox({});
    }
    (function () {
        $(document).click(function () {
            $('.selection').unbind();
            $('.yourselectiontooltip').fadeOut(300);
        });
    })();
    // toggle price
    (function () {
        $('div#content div.tools a.togglePrice').click(function (e) {
            e.preventDefault();
            var that = $(this);
            var container = that.parent();

            that.toggleClass('on');
            if (that.hasClass('on')) {
                that.html('' + $('input#PriceOn').val() + '&nbsp;');
                $('div#content div.thumbListContainer div.thumb:not(".list")>div.text').slideDown({ duration: 300, easing: 'easeInOutQuad', complete: function () { $(this).parent().addClass('text'); } });
            } else {
                that.text($('input#PriceOff').val());
                $('div#content div.thumbListContainer div.thumb:not(".list")>div.text').slideUp({ duration: 300, easing: 'easeInOutQuad', complete: function () { $(this).parent().removeClass('text'); } });
            }
        });
    })();

    // price slider

    (function () {
        $('div#content div.tools div.priceSlider').slider({ range: "min",
            value: parseInt($('input#slider-Value').val()),
            min: parseInt($('input#slider-Min').val()),
            max: parseInt($('input#slider-Max').val()),
            step: parseInt($('input#slider-Step').val()),
            slide: function (e, ui) {
                var value = ui.value;
                $('div#content div.tools div.priceSliderValue strong').text(value);
            },
            stop: function () {
                $('input#slider-Value').val($('div#content div.tools div.priceSliderValue strong').text());
                SearchAndFilterProducts(false, 'P')
            }
        });
    })();

    // change grid size
    (function () {
        $('div#content div.tools ul.chooseGrid li a').click(function (e) {
            e.preventDefault();
            $(this).parent().addClass('on').siblings().removeClass('on');
            SearchAndFilterProducts(false, 'G');
            if ($(this).parent().attr('id') == 'grid3')
                $('div#togglePrice').hide();
            else
                $('div#togglePrice').show();
        });
    })();

    // list view
    InitProductList();

    // hr scroller
    InitScroll();

    // selection filter
    InitYourSelection();

    //open prþce info
    $('div#content div.tools a.togglePrice').click();



    //page scroll down
    $('input[type="hidden"]#pageNumber').val(1);
    $('input[type="hidden"]#hasMore').val($('#viewAll').length > 0);
    $(window).scroll(function () {
        if ($(window).scrollTop() + 150 > $(document).height() - $(window).height()) {
            if ($('ul.chooseGrid li.on').attr('id') == 'grid3') return;
            if (ajaxExecuting) return;
            if ($('input[type="hidden"]#hasMore').val().toLowerCase() == 'true')
                SearchAndFilterProducts(true, 'paging');
        }
    });
    InitYourSelectionPopup();


});

function InitScroll() {

    var count = $('div#content div.thumbListContainer div.thumb.list').length;
    //    if ((count % 3) == 2) { count++; }
    //    if ((count % 3) == 2) { count += 2; }
    if ((count % 3) == 1) { count += 2; }
    else if ((count % 3) == 2) { count += 1; }
    var scrollContent = $('div#scrollLayout');
    var scrollMask = $('div#scrollMask');
    scrollContent.css('width', Math.max(((count * 157) / 3), (6 * 157)) + 'px');

    var scroller = $('div#scrollBar span');

    var rePositionScrollBar = function (left, speed) {
        if (!speed) { var speed = 300; }
        var position = (left / (scrollContent.width() - scrollMask.width())) * (scroller.parent().width() - scroller.width());

        scroller.animate({ left: position + 'px' }, { duration: speed, easing: 'easeInOutCirc' });
        //console.log('scroller:' + position);
    };

    var scroll = function (left) {
        var position = parseInt((left / (scroller.parent().width() - scroller.width())) * (scrollContent.width() - scrollMask.width()), 0);
        scrollContent.css("left", '-' + position + "px");
    };

    var reAlign = function (left) {
        var position = parseInt((left / (scroller.parent().width() - scroller.width())) * (scrollContent.width() - scrollMask.width()), 0);
        var itemWidth = 157;
        var mod = position % itemWidth;
        if (mod === 0) { return; }
        //console.log(mod);

        // exclude the diff from position (its easier than it looks with mod ;)
        if (mod > (itemWidth / 2)) {
            scrollContent.stop().animate({ left: '-' + (position + (itemWidth - mod)) + "px" }, { duration: 300, easing: 'easeInOutCirc' });
            rePositionScrollBar((position + (itemWidth - mod)));
        } else {
            scrollContent.stop().animate({ left: '-' + (position - mod) + "px" }, { duration: 300, easing: 'easeInOutCirc' });
            //console.log((position - (itemWidth - mod)));
            rePositionScrollBar(position - mod);
        }
    };

    scroller.draggable({
        axis: 'x',
        containment: 'parent',
        drag: function (e, ui) { scroll(ui.position.left); },
        stop: function (e, ui) { reAlign(ui.position.left); }
    });

    $('div#thumbScrollButtons a.left').click(function (e) {
        e.preventDefault();
        //debugger;
        if (scrollContent.queue("fx").length > 0) { return; }
        var position = parseInt(scrollContent.css('left'), 0);

        if ((position * -1) < 1) { return; }
        var goToPosition = (position + (157 * 6));
        if (goToPosition > -1) { goToPosition = 0; }
        scrollContent.stop().animate({ left: goToPosition + 'px' }, { duration: 750, easing: 'easeInOutCirc' });
        rePositionScrollBar(goToPosition * -1, 750);
    });

    $('div#thumbScrollButtons a.right').click(function (e) {
        e.preventDefault();

        if (scrollContent.queue("fx").length > 0) { return; }
        var position = parseInt(scrollContent.css('left'), 0);
        var lastPosition = (scrollContent.width() - (157 * 6)) * -1;
        if ((position) <= lastPosition) {
            
            // horizantal scroll load content
            if ($('input[type="hidden"]#hasMore').val().toLowerCase() != 'true')
                return;  
            SearchAndFilterProducts(true, 'paging');
         }
        var goToPosition = (position - (157 * 6));
        if (lastPosition > goToPosition) { goToPosition = lastPosition; }
        //alert(lastPosition + '\n' + goToPosition);
        scrollContent.stop().animate({ left: goToPosition + 'px' }, { duration: 750, easing: 'easeInOutCirc' });
        rePositionScrollBar(goToPosition * -1, 750);
    });
}

function InitSorting() {
    $('div.selectBox ul.chooseOrder li').click(function () { SearchAndFilterProducts(false, 'O'); });

//    $('a#viewAll').click(function () {
//        $('input[type="hidden"]#pageIndex').val(0);
//        $('input[type="hidden"]#pageSize').val(10000);
//        SearchAndFilterProducts(true, '');
//    });

    $('a#viewAll-bottom').click(function () { $('a#viewAll').click(); });

}

function RenderGrid1(model) {

    // $('div.thumbListContainerLoading').addClass('thumbListContainer');
    var previousItemCount = $('div#content div.thumbListContainer div.thumb').length;
    if (model.PageNumber == 1) {
        if (model.ViewMode == 'grid3') {

            var html = '<div id="thumbScrollButtons" class="clearfix">' +
                    '<a href="#" class="left">left</a> <a href="#" class="right">right</a>' +
                    '<div id="scrollMask">' +
                        '<div id="scrollLayout">' +
                            '<div class="thumbListContainer clearfix">' +
                             model.Html +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
                '<div id="scrollBar">' +
                    '<span>&nbsp;</span></div>';

            $('#ProductsContainer').html(html);
        }
        else {
            $('#ProductsContainer').html(' <div class="thumbListContainer clearfix"></div><div id="productsLoadingNotifier" style="background: url(/_img/products_loading.gif) #f3f3f3 center center no-repeat; display:none; height:40px;"></div>');
            $('#ProductsContainer div.thumbListContainer').html(model.Html);
        }

    }

    else
        $('div.thumbListContainer').append(model.Html);

    var $items = $('div#content div.thumbListContainer div.thumb');
    if ($items.length > previousItemCount && model.PageNumber!=1) {
        $items = $items.slice(-($items.length - previousItemCount));
    }
    InitProductList($items);
    InitScroll();
    InitAddToBag($items);
    //   InitPriceOffer();
    if (IsGuest()) {
        $items.find("div.buttons a.button1[data-val='offer']").fancybox({});
    }

    InitAttributeSelections($items);

    //open info area
    if ($('div#content div.tools a.togglePrice').hasClass('on')) {
        $items.filter(':not(".list")').find('div.text').slideDown({ duration: 300, easing: 'easeInOutQuad', complete: function () { $(this).parent().addClass('text'); } });
    } else {
        $items.filter(':not(".list")').find('div.text').slideUp({ duration: 300, easing: 'easeInOutQuad', complete: function () { $(this).parent().removeClass('text'); } });
    }



    $('a#viewAll-bottom').click(function () { $('a#viewAll').click(); });

}



//function InitProductList() {
//    //$('div#content div.thumbListContainer div.thumb div.panel span.close').bind('click', function(){ $(this).parent().stop().fadeOut(300, function(){ $(this).attr('style', ''); }); });
//    $('div#content div.thumbListContainer div.thumb').hover(
//			function (e) { $(this).css('zIndex', 100).children('div.panel').stop().fadeTo(300,1); },
//			function () { $(this).css('zIndex', 1).children('div.panel').stop().fadeTo(300, 0); }
//        );
//}

function InitProductList($items) {
    //$('div#content div.thumbListContainer div.thumb div.panel span.close').bind('click', function(){ $(this).parent().stop().fadeOut(300, function(){ $(this).attr('style', ''); }); });
    if (typeof $items === 'undefined') {
        $items = $('div#content div.thumbListContainer div.thumb');
    }
    $items.hover(
			function (e) { $(this).css('zIndex', 100).children('div.panel').stop().fadeTo(300, 1); },
			function () { $(this).css('zIndex', 1).children('div.panel').stop().fadeTo(300, 0); }
        );
}



//from product boxes
function InitAttributeSelections($items) {
    if (typeof $items === 'undefined') {
        $items = $('div#content div.thumbListContainer div.thumb');
    }
    $items.find('div.options dl').hide();
    $items.find('div.options dl[parentId="0"]').show();
    $items.find('div.selectBox ul li').click(function (e) {
        e.stopPropagation();
        var $container = $(this).closest('div.thumb');
        var valueId = $(this).attr('valueId');
        //set selected VariantId.
        if ($(this).attr('ownerVariantId')) {
            $container.find('input[name="VariantId"]').val($(this).attr('ownerVariantId'));
        }
        var level = $(this).closest('div.options dl').attr('level');
        $container.find('div.options dl').each(function f() {
            if ($(this).closest('div.options dl').attr('level') > level) {
                $(this).closest('div.options dl').hide();
            }
        });
        $container.find('div.options dl[parentId="' + valueId + '"]').show();
        $container.find('div.options dl[parentId="' + valueId + '"]').find('li').first().click();
        $container.find('div.options dl[parentId="' + valueId + '"]').find('li').first().parent().parent().trigger('click');
    });

    $items.find('div.options dl[parentId="0"]').each(function f() {
        $(this).find('li.on').first().click();
        $(this).find('li.on').first().parent().parent().trigger('click');

    });


}

function InitSelectBox() {
    $('.thumbListContainer div.selectBox').click(function () {
        var that = $(this);
        if (!that.hasClass('on')) { that.addClass('on').children('ul').stop().slideDown({ duration: 150, easing: 'easeInOutQuad' }); }
        else { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
    }).hover(function () { }, function () {
        var that = $(this);
        if (that.hasClass('on')) { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
    });

    $('.thumbListContainer div.selectBox ul li').click(function (e) {
        e.stopPropagation();
        var that = $(this);
        that.addClass('on').siblings().removeClass('on');
        that.parent().prev().text(that.text());
        that.parent().parent().trigger('click');
    });
}

var ajaxExecuting = false;
function SearchAndFilterProducts(updatePrice, trigger) {
    var specifications = new Array();
    var attributes = new Array();
    var manufacturers = new Array();
    var categories = new Array();
    var filterItems = $('div#content div.yourSelection ul.selections li');
    for (var i = 0; i < filterItems.length; i++) {
        if ($(filterItems[i]).attr('rel') != null) {
            var value = $(filterItems[i]).attr('rel');
            var parent = $(filterItems[i]).attr('parentId');
            if (value.substring(0, 4) === 'item') {
                specifications.push({ OptionId: $(filterItems[i]).attr('rel').substring(4), AttributeId: parent });
            }
            else if (value.substring(0, 5) === 'brand') {
                manufacturers.push({ Id: $(filterItems[i]).attr('rel').substring(5) });
            }
            else if (value.substring(0, 4) === 'attr') {
                attributes.push({ OptionId: $(filterItems[i]).attr('rel').substring(4), AttributeId: parent });
            }
            else if (value.substring(0, 3) === 'cat') {
                categories.push({ Id: $(filterItems[i]).attr('rel').substring(3) });
            }
        }
    }


    var priceFilters = new Array();
    //TODO: mustafa clean up eg. priceFilters
    priceFilters.push({ From: "0", To: $('div.priceSliderValue strong').text() });

    context = { categoryId: $('input[type="hidden"]#categoryId').val(),
        manufacturerId: $('input[type="hidden"]#manufacturerId').val(),
        ViewMode: $('ul.chooseGrid li.on').attr('id'),
        OrderBy: $('ul.chooseOrder li.on').attr('value'),
        PriceRangeFilter: { Items: priceFilters },
        SpecificationFilter: { AlreadyFilteredItems: specifications },
        ProductAttributeFilter: { AlreadyFilteredItems: attributes },
        ManufacturerFilter: { AlreadyFilteredItems: manufacturers },
        CategoryFilter: { AlreadyFilteredItems: categories },
        PriceRangeSliderFilter: { Item: { Value: $('input#slider-Value').val()} },
        PageSize: 0,//$('input[type="hidden"]#pageSize').val(),
        PageNumber: trigger == 'paging' ? parseInt($('input[type="hidden"]#pageNumber').val()) + 1 : 1,
        FilterTrigging: trigger,
        newsItemId: $('input[type="hidden"]#newsItemId').val()

    };

    //  $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>');
    $.ajax({
        type: "POST",
        url: _applyUrl,
        data: JSON.stringify(context),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            //set paging data
            $('input[type="hidden"]#pageNumber').val(data.PageNumber);
            $('input[type="hidden"]#hasMore').val(data.HasMore);

            RenderGrid1(data);

            //your selection begin
            var content = $('div#content div.yourSelection ul.selections').html();
            $('div.yourSelection').html(data.SelectionHtml);

            ////keep current filtering item coloumn
            // if (!(typeof filteringId === "undefined"))
            //     $('div#' + filteringId).html(filteringHtml);
            ////

            InitYourSelection();
            if (trigger == 'C' || trigger == 'M' || trigger == 'S' || trigger == 'A') {
                $('div#content div.yourSelection div.selection span').click();
                //$('div#content div.yourSelection ul.selections').html(content);
                if ($('div#content div.yourSelection ul.selections li').length > 1) { $('div#content div.yourSelection ul.selections').show(); }
            }
            else if (trigger == 'P') {
                //$('div#content div.yourSelection ul.selections').html(content);
            }
            //your selection end

            //price slider begin
            if (updatePrice) {
                //$('div#content div.tools div.priceSlider').html('');

                $('div#content div.tools div.priceSlider').slider('option', 'max', data.PriceMax);
                $('div#content div.tools div.priceSlider').slider('option', 'min', data.PriceMin);
                $('div#content div.tools div.priceSlider').slider('option', 'step', data.PriceStep);
                $('div#content div.tools div.priceSlider').slider('value', data.PriceValue);
                $('div#content div.tools div.priceSliderValue strong').text(data.PriceValue);
            }
            //price slider end 
            if (data.HasMore)
                $('#viewAll').show();
            else
                $('#viewAll').hide();
            InitSelectBox();
           

        },
        beforeSend: function () {
            ajaxExecuting = true;
            if (trigger == 'paging') {
                $('div#productsLoadingNotifier').show();
            }
            else {
                $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>');
            }
        },
        complete: function () {
        ajaxExecuting = false;
            if (trigger == 'paging') {
                $('div#productsLoadingNotifier').hide();
            }
            else {
                $('div#loading').remove(); 
            }
        }
    });

}

function AddToCart(container) {

    var productId = container.find('input[name="ProductId"]').val();
    var variantId = container.find('input[name="VariantId"]').val();
    var quantity = parseInt(container.find('div.quantity li.on').text());
    if (isNaN(quantity)) quantity = 1;
    var shoppingCartType = 1;
    var attributes = new Array();
    container.find('div.options div.selectBox li.on[ownerVariantId="' + variantId + '"]').each(function f() {
        attributes.push({
            ProductVariantAttributeValueId: $(this).attr('valueId'),
            ProductAttributeId: $(this).closest('dl').attr('attributeId')
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
        url: _addToCartUrl,
        data: JSON.stringify(model),
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            //window.scrollTo(0,0);
            //$('html, body').animate({ scrollTop: 0 }, { duration: 1000, easing: 'easeInOutCirc', function () { UpdateCart(data); } });
            $("html, body").animate({ scrollTop: 0 }, "slow", function () { UpdateCart(data); });

        },
        beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
        complete: function () { $('div#loading').remove(); }
    });

}

//function InitPriceOffer() {

//    $("div.buttons a.button1[data-val='offer']").fancybox({});
//    $('.submit a#priceOfferLink').click(function () {
//        var $form = $(this).closest('form');
//        if ($form.valid()) {
//            $.ajax({
//                type: "POST",
//                url: $(this).closest('form').attr('action'),
//                data: $(this).closest('form').serialize(),
//                success: function (data) {
//                    if (data.Success) {
//                        $('div#sendPrice #formDivision').hide();
//                        afFancy(data.Message, data.SubMessage, true);
//                    }
//                    else {
//                        $('div#sendPrice #formDivision').hide();
//                        afFancy(data.Message, data.SubMessage, false);
//                    }
//                },
//                complete: function () {
//                }
//            });

//        }
//    });
//}

//function InitAddToBag() {

//    $('div.buttons a.button1').click(function f(e) {
//        //Price Offer
//        if ($(this).attr('data-val') == "offer") {
//           
//            if (!IsGuest()) {
//                OpenPriceOffer($(this).attr('product-id'), $(this).attr('product-name'), $(this).attr('product-img'));
//                $('div#sendPrice a#priceOfferLink').click();
//            }
//            else {
//                OpenPriceOffer($(this).attr('product-id'), $(this).attr('product-name'), $(this).attr('product-img'));
//            }
//            return false;
//        }


//        //add to bag
//        e.stopPropagation();
//        if ($(this).hasClass('soldout')) return;
//        //if add to bag is clicked first time, display options menu
//        var optionsVisible = $(this).closest('div.panel').hasClass('quickBuy');
//        if (!optionsVisible) {
//            $(this).closest('div.panel').addClass('quickBuy');
//            $(this).closest('div.panel').find('div.options').show();
//            //hide product detail button
//            $(this).closest('div.buttons').find('.button2').hide();
//        }
//        else {
//            AddToCart($(this).closest('div.thumb'));
//        }
//    });
//}
function InitAddToBag($items) {
    if (typeof $items === 'undefined') {
        $items = $('div#content div.thumbListContainer div.thumb');
    }
    $items.find('div.buttons a.button1').click(function f(e) {
        //Price Offer
        if ($(this).attr('data-val') == "offer") {

            if (!IsGuest()) {
                OpenPriceOffer($(this).attr('product-id'), $(this).attr('product-name'), $(this).attr('product-img'));
                $('div#sendPrice a#priceOfferLink').click();
            }
            else {
                OpenPriceOffer($(this).attr('product-id'), $(this).attr('product-name'), $(this).attr('product-img'));
            }
            return false;
        }


        //add to bag
        e.stopPropagation();
        if ($(this).hasClass('soldout')) return;
        //if add to bag is clicked first time, display options menu
        var optionsVisible = $(this).closest('div.panel').hasClass('quickBuy');
        if (!optionsVisible) {
            $(this).closest('div.panel').addClass('quickBuy');
            $(this).closest('div.panel').find('div.options').show();
            //hide product detail button
            $(this).closest('div.buttons').find('.button2').hide();
        }
        else {
            AddToCart($(this).closest('div.thumb'));
        }
    });
}

function InitYourSelection() {

    // show / hide selection menu
    $('div#content div.yourSelection div.selection span').click(function () {
        var that = $(this).parent();
        var menu = that.children('div');
        var windowWidth = $(window).width();
        var itemWidth = 940;
        var bg = menu.children('div.bg');
        var leftPosition = ((windowWidth - itemWidth) / 2);

        bg.css({
            height: menu.height(),
            width: windowWidth,
            left: '-' + leftPosition + 'px'
        });

        // if (!that.hasClass('on')) { that.addClass('on').children('div').fadeIn(350, ResizeYourSlectionColumns); }
        if (!that.hasClass('on')) {
            //that.addClass('on').children('div').hide();
            that.addClass('on').children('div').css('top', '28px');
            //that.addClass('on').children('div').fadeIn(350);
            that.addClass('on').children('div').show();
        }
        else { that.removeClass('on').children('div').stop().fadeOut(250, function () { $(this).attr('style', ''); }); }

    });

    // checkbox
    $('div#content div.yourSelection div.selection div.selectionMenu div.selectionMenuInner div.item ul li a').click(function (e) {
        e.preventDefault();
        var that = $(this);
        that.toggleClass('checked');
        if (that.hasClass('checked')) {
            $('div#content div.yourSelection ul.selections').show().prepend('<li parentId="' + that.attr('parentId') + '" rel="' + that.attr('class').replace('checked', '').replace(' ', '') + '"><span>' + that.text() + '</span></li>');
        } else {
            $('div#content div.yourSelection ul.selections li[rel="' + that.attr('class') + '"]').remove();
            if ($('div#content div.yourSelection ul.selections li').length < 2) { $('div#content div.yourSelection ul.selections').hide(); }
        }

        filteringHtml = $(this).closest('div.item').html();
        filteringId = $(this).closest('div.item').attr('id');
        SearchAndFilterProducts(true, 'S');

    });

    // clear all button
    $('div#content div.yourSelection div.selection div.selectionMenu div.selectionMenuInner a#clearSelections').click(function (e) {
        e.preventDefault();
        $('div#content div.yourSelection div.selection div.selectionMenu div.selectionMenuInner div.item ul li a').removeClass('checked');
        $('div#content div.yourSelection ul.selections li.clearAll').prevAll().remove();
        if ($('div#content div.yourSelection ul.selections li').length < 2) { $('div#content div.yourSelection ul.selections').hide(); }

        SearchAndFilterProducts(true, 'C');
    });
    // AF
    //Apply button
    $('div#content div.yourSelection div.selection div.selectionMenu div.selectionMenuInner a#apply').click(function (e) {
        e.preventDefault();
        // SearchAndFilterProducts(true);
        $('div#content div.yourSelection div.selection span').click();

    });

    // clear all button at filter section
    $('div#content div.yourSelection ul.selections li.clearAll').click(function (e) {
        e.preventDefault();
        $(this).prevAll().remove();
        $('div#content div.yourSelection div.selection div.selectionMenu div.selectionMenuInner div.item ul li a').removeClass('checked');
        if ($('div#content div.yourSelection ul.selections li').length < 2) { $('div#content div.yourSelection ul.selections').hide(); }

        SearchAndFilterProducts(true, 'R');
    });

    // filter section self remove
    $('div#content div.yourSelection ul.selections li span').click(function () {
        var that = $(this).parent();
        that.remove();
        $('div#content div.yourSelection div.selection div.selectionMenu div.selectionMenuInner div.item ul li a[class*="' + that.attr('rel') + '"]').removeClass('checked');
        if ($('div#content div.yourSelection ul.selections li').length < 2) { $('div#content div.yourSelection ul.selections').hide(); }
        SearchAndFilterProducts(true, 'R');
    });

    //close filter section on mouseleave when there is no selection.
    $('div.selectionMenu').mouseleave(function () {
        // check thare is a selected filter option
        if (ajaxExecuting) return;
        if ($('li.clearAll').length > 0) {
            if ($('div#content div.yourSelection div.selection').hasClass('on')) {
                $('div#content div.yourSelection div.selection span').click();
            }
        }
    });


    ResizeYourSlectionColumns();


}

//Resize columns //do it row specific!
function ResizeYourSlectionColumns() {
    var height = 0;
    var count = 0;
    var items = new Array();
    var columnCount = 6;

    $('div#content div.selectionMenu div.item').each(function () {
        height = Math.max(height, $(this).height());

        items.push($(this));
        count++;
        if (count % columnCount == 0) {
            for (var i = 0; i < columnCount; i++) {
                items[i].height(height);
            }
            items = new Array();
            height = 0;
        }
        //alert(height);
    });
    //       $('div#content div.selectionMenu div.item').each(function () {
    //           $(this).height(height);
    //       });
}
function PopupYourSelection() {
    setCookieHour('yourSelectionAskedSession', 1, 23);
    var count = getCookie('yourSelectionAsked');
    if (typeof count === "undefined") {
        count = 0;
    }
    count++;
    setCookie('yourSelectionAsked', count, 365);
    document.cookie = 'yourSelectionAsked=' + count;
    var as = $("#yourSelectionToolTip").val();
    (function () {
        $('.selection').jSpot2({ text: as});
    })();

}

function InitYourSelectionPopup() {
    var yourSelectionAsked = getCookie('yourSelectionAsked');
    var yourSelectionAskedSession = getCookie('yourSelectionAskedSession');

  if ((typeof yourSelectionAsked === "undefined" || yourSelectionAsked < 3) && (typeof yourSelectionAskedSession === "undefined" || yourSelectionAskedSession == 0)) {
   window.setTimeout("PopupYourSelection();", 1500);
    }
     else
     $('.yourselectiontooltip').show();
}
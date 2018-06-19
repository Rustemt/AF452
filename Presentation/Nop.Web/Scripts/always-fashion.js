$(function () {
    $.ajaxSetup({
        //            beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
        //            complete: function () { $('div#loading').remove(); },
        error: function (Data) {
            $('div#loading').remove();
            afFancy(Data.header, Data.messages, false);
        }

    });

    // popup menus
    (function () {
        $('ul#navigation li.popup').hover(function () { $(this).children('div').stop().slideDown({ duration: 175, easing: 'easeInOutCirc' }); }, function () { $(this).children('div').stop().slideUp({ duration: 175, easing: 'easeInOutCirc', complete: function () { $(this).attr('style', ''); } }); });
        // $('div#searchBox').hover(function () { $(this).addClass('on').children('div.searchBoxHover').stop().fadeIn(300); }, function () { $(this).removeClass('on').children('div.searchBoxHover').stop().fadeOut(200, function () { $(this).attr('style', ''); }); });
        $('div#searchBox').hover(function () { $(this).addClass('on'); }, function () { if (!$(this).children('div.searchBoxHover').is(':visible')) $(this).removeClass('on'); });
        $('div#searchBox a').click(function () {
            if (!$(this).closest('div').children('div.searchBoxHover').is(':visible')) $(this).closest('div').addClass('on').children('div.searchBoxHover').stop().fadeIn(300, function(){$('.searchBoxInput input').focus();}); 
            else $(this).closest('div').removeClass('on').children('div.searchBoxHover').stop().fadeOut(200, function () { });
        });

        $('li#umShopBag').hover(function () {
            $(this).addClass('on').children('div:last').stop().slideDown({ duration: 175, easing: 'easeInOutCirc' });
            // shop bag scroll
            $('div#shopBagScroll').jScrollPane();

        }, function () {
            $(this).children('div:last').stop().slideUp({ duration: 175, easing: 'easeInOutCirc', complete: function () { $(this).parent().removeClass('on'); $(this).attr('style', ''); } });
        });

        $('#umEmailSignUp').click(function () {
            $(this).addClass('on').children('div:last').stop().slideDown({ duration: 175, easing: 'easeInOutCirc' });
        });

        $('li#umEmailSignUp div.popupClose a').click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            $(this).parent().parent().parent().stop().slideUp({ duration: 175, easing: 'easeInOutCirc', complete: function () { $(this).parent().removeClass('on'); } });
            //     $(this).stop().slideUp({ duration: 175, easing: 'easeInOutCirc', complete: function () { $(this).parent().removeClass('on'); } });
        });
    })();

    // announcement close
    (function () {
        var announcementArea = $('div#announcement');
        $('div#announcement dl dd a').click(function (e) {

            //hide announcement for the session.
            document.cookie = "news.DisplayCount=100";
            e.preventDefault();
            announcementArea.stop().animate({ opacity: 0 }, 350, function () { $(this).children('dl').remove(); });
        });
    } ());

    // promo toggle
    (function () {
        var toggleButton = $('div#promoInner div.toggleButton a');
        var promoSections = $('div#promoInner div.section');
        toggleButton.click(function (e) {
            e.preventDefault();
            var promoArea = $('div#promo');
            if (!promoArea.hasClass('collapsed')) {
                toggleButton.hide();
                promoSections.stop().fadeTo(600, 0);
                promoArea.stop().animate({ height: '1px' }, { duration: 600, easing: 'easeInOutCirc', complete: function () { promoArea.addClass('collapsed'); toggleButton.show(); } });
            } else {
                toggleButton.hide();
                promoSections.stop().fadeTo(600, 1);
                promoArea.stop().animate({ height: '138px' }, { duration: 600, easing: 'easeInOutCirc', complete: function () { promoArea.removeClass('collapsed'); toggleButton.show(); } });
            }
        });

        setTimeout(function () { toggleButton.trigger('click'); }, 1500);
    })();

    // category select
    (function () {
        $('ul#innerMenu li').hover(function (e) {
            var that = $(this);

            if (that.find('li').length == 0) return;

            that.addClass('on').children('ul').stop().slideDown({ duration: 200, easing: 'easeInOutCirc' });
        }, function () {
            var that = $(this);
            that.removeClass('on').children('ul').stop().slideUp({ duration: 200, easing: 'easeInOutCirc', complete: function () { $(this).attr('style', ''); } });
        });
    })();

    // goto top()
    (function () {
        $('div#contentRight div.gotoTop a').click(function (e) {
            e.preventDefault();
            $('html, body').animate({ scrollTop: 0 }, { duration: 1000, easing: 'easeInOutCirc' });
        });
    })();

    //Help Page goto top
    (function () {
        $('ul#leftMenu >li >a').click(function (e) {
            var a = $(this).attr('data-contentId');
            goToByScroll(a);
        });
    })();

    // misc selectbox
    (function () {
        $('div#content div.dropDownSection div.dropDownSectionInner select').css({ visibility: 'visible', opacity: 0 }).change(function () {
            $(this).prev().text($(this).val());
        }).trigger('change');
    })();


    // gift finder
    (function () {
        $('div#selectGiftType dl dt img').css({ opacity: 0, visibility: 'visible' });
        $('div#selectGiftType dl').hover(function () {
            var that = $(this);
            that.find('dt img').stop().animate({ opacity: 1 }, 300);
            that.find('dd span').stop().animate({ top: 0 }, { duration: 300, easing: 'easeInOutCirc' });
        }, function () {
            var that = $(this);
            that.find('dt img').stop().animate({ opacity: 0 }, 300);
            that.find('dd span').stop().animate({ top: '-67px' }, { duration: 300, easing: 'easeInOutCirc' });
        });
    })();

    // selectbox
    (function () {
        $('div.selectBox').click(function () {
            var that = $(this);
            if (!that.hasClass('on')) { that.addClass('on').children('ul').stop().slideDown({ duration: 150, easing: 'easeInOutQuad' }); }
            else { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
        }).hover(function () { }, function () {
            var that = $(this);
            if (that.hasClass('on')) { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
        });

        $('div.selectBox ul li').click(function (e) {
            e.stopPropagation();
            var that = $(this);
            that.addClass('on').siblings().removeClass('on');
            that.parent().prev().text(that.text());
            that.parent().parent().find(':hidden').val(that.attr('data-value'));
            that.parent().parent().trigger('click');
        });
    })();

    // checkbox
    (function () {
        $('div#content a.checkbox').click(function (e) {
            e.preventDefault();
            var mode = $(this).attr('mode');
            if (mode == 'single')// radio button
            {
                //find all related checkboxes and turn of them
                $('a.checkbox[group="' + $(this).attr('group') + '"]').removeClass('checked');
                $(this).addClass('checked');
                //there shall be a hidden imput name=id=groupname
                $('input[name="' + $(this).attr('group') + '"]').val($(this).attr('rdoValue'));
            }
            else if (mode == 'all') {
                $(this).toggleClass('checked');
                $('a.checkbox[group="' + $(this).attr('group') + '"]').attr('class', $(this).attr('class'));
                //$('input[name="' + $(this).attr('group') + '"]').val($(this).hasClass('checked'));
            }
            else//checkbox
            {
                $(this).toggleClass('checked');
                $('input[name="' + $(this).attr('group') + '"]').val($(this).hasClass('checked'));
            }

        });
    })();

    // recipient
    (function () {
        $('ul.webOrder li.order div.Recipient a').click(function (e) {
            e.preventDefault();
            $(this).next().toggle();
        });

        $('ul.webOrder li.order div.Recipient div.RecipientDetails a.close').click(function (e) {
            e.preventDefault();
            e.stopPropagation();
            $(this).parent().prev().trigger('click');
        });
    })();

    // left navigation toggle
    (function () {
        $('ul#leftMenu li a').click(function (e) {
            var that = $(this).parent();
            if (that.children('ul').length < 1) { return; }
            that.toggleClass('open');
        });
    })();

    // search input
    (function () {
        $('div.searchBoxInput input').focus(function () {
            var that = $(this);
            if (that.val() == that.attr('title')) { that.val(''); }
        });
        $('div.searchBoxInput input').blur(function () {
            var that = $(this);
            if (that.val() == '') { that.val(that.attr('title')); }
        });
    })();
    // langselect  currency
    (function () {
        $('div#footer a#language').click(function (e) {
            e.preventDefault();
            var that = $(this);

            that.toggleClass('on');
            if (that.hasClass('on')) { $('div#footer ul#langList').css({ width: that.outerWidth() + 'px', display: 'block' }); }
            else { $('div#footer ul#langList').css({ display: 'none' }); }

        });

        $('div#footer a#currency').click(function (e) {
            e.preventDefault();
            var that = $(this);

            that.toggleClass('on');
            if (that.hasClass('on')) { $('div#footer ul#curList').css({ width: that.outerWidth() + 'px', display: 'block' }); }
            else { $('div#footer ul#curList').css({ display: 'none' }); }

        });
    })();


    if ($("a#customerLink").length > 0) {

        $("a#customerLink").fancybox({});
    }
    
    if ($("a#TopicDetailsSalesAgreements").length > 0) {

        $("a#TopicDetailsSalesAgreements").fancybox({});
    }



    //Global ajax events
    //    $('body').ajaxStart(function () {
    //        $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>');
    //    });

    //    $('body').ajaxComplete(function () {
    //        $('div#loading').remove();
    //    });

    InitRegistrationPopup();

    SetProvinceUpdate();

});



function ShowMessage(Messages) {

    var str = '';
    if (Messages.length == 0) return;
    for (var i = 0; i < Messages.length; i++) {
        str += Messages[i] + '\n';
    }
    afFancy('', str, false);

}

//Shopping cart
function UpdateCart(data) {

    if (data.Messages.length > 0) {
        ShowMessage(data.Messages);

    }
    else if (data.Html != '') {
        $('li#umShopBag').html(data.Html);
        $('.umShopBag_next_pop').stop().animate({ height: '395px' }, { duration: 300 });
        if ($('li#umShopBag ul.popupShopBag').length > 4) {
            $('div#shopBagScroll').jScrollPane();
        }
        setTimeout(function () { $('.umShopBag_next_pop').stop().animate({ height: '0' }, { duration: 400 }) }, 5000);
        //Çantanýn 5 sn süre ile açýk kalmasý
    }

    if ($('.scroll-pane .list_info') != null && $('.scroll-pane .list_info').length == 0)
        $('.scroll-pane').css('height', '0px');
    if ($('.scroll-pane .list_info') != null && $('.scroll-pane .list_info').length == 1)
        $('.scroll-pane').css('height', '116px');
    if ($('.scroll-pane .list_info') != null && $('.scroll-pane .list_info').length == 2)
        $('.scroll-pane').css('height', '232px');

    $('.scroll-pane').jScrollPane({
        autoReinitialise: true
    });
}

//TODO: get url from server side.
function SetProvinceUpdate() {

    $('div.selectBox#CountryId li').click(function (e) {
        e.stopPropagation();
        var selectedItem = $(this).attr('data-value');
        var ddlStates = $(this).closest('div.addressContainer').find('div.selectBox#StateProvinceId');
        $.ajax({
            cache: false,
            type: "GET",
            url: _getStatesByCountryId, //'/Country/GetStatesByCountryId',
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
            error: function (xhr, ajaxOptions, thrownError) {
                alert('Failed to retrieve states.');
            }
        });


    });

}


// item is the div.selectBox
function SetupSelectBox($item) {
    $item.find('ul li').click(function (e) {
        e.stopPropagation();
        var that = $(this);
        that.addClass('on').siblings().removeClass('on');
        that.parent().prev().text(that.text());
        that.parent().parent().find(':hidden').val(that.attr('value'));
        that.parent().parent().trigger('click');
    });
}

$.fn.clearForm = function () {
    return this.each(function () {
        var type = this.type, tag = this.tagName.toLowerCase();
        if (tag == 'form')
            return $(':input', this).clearForm();
        if (type == 'text' || type == 'password' || tag == 'textarea')
            this.value = '';
        else if (type == 'checkbox' || type == 'radio')
            this.checked = false;
        else if (tag == 'select')
            this.selectedIndex = -1;

    });
};

//(function () {
//    $('div#contentRight div.gotoTop a').click(function (e) {
//        e.preventDefault();
//        $('html, body').animate({ scrollTop: 0 }, { duration: 1000, easing: 'easeInOutCirc' });
//    });
//})();

//function gotoId(id) {
//    alert('gotoId');
//    $(id).animate.animate({ scrollTop: 0 }, { duration: 1000, easing: 'easeInOutCirc' });
//}



//AF Fancy function.
function afFancy(message, detailedMessage, isSuccess) {
    var picLink;
    piclink = isSuccess ? '/_img/popupthanks.png' : '/_img/error_password.png';
    var id = isSuccess ? 'messageSent' : 'messageNotSent';
    $.fancybox(
            '<div style="min-width: 500px; height: auto; overflow: auto; position: relative;">' +
            '<div id="' + id + '">' +
            '<p class="textCenter" style="text-transform:uppercase"><img src="' + piclink + '" width="22" height="22" alt="" /></p>' +
            '<p class="textCenter" style="text-transform:uppercase">' + message + '<p>' +
            '<p class="textCenter" style="text-transform:none;font-size:12px; ">' + detailedMessage + '<p></div></div>',
            {
                'autoDimensions': false,
                'width': 'auto',
                'height': '300px',
                'transitionIn': 'none',
                'transitionOut': 'none'
            }
        );

}

function afFancyByList(message, detailedMessage, isSuccess) {
    var msList = "";
    $.each(message, function (i, value) {
        msList += "<p class=\"textCenter\" style=\"text-transform:uppercase\">" + value + "</p>";
    });
    var picLink;
    piclink = isSuccess ? '/_img/popupthanks.png' : '/_img/error_password.png';
    var id = isSuccess ? 'messageSent' : 'messageNotSent';
    $.fancybox(
            '<div style="width: 500px; height: auto; overflow: auto; position: relative;">' +
            '<div id="' + id + '">' +
            '<p class="textCenter" style="text-transform:uppercase"><img src="' + piclink + '" width="22" height="22" alt="" /></p>' +
            msList +
            '<p class="textCenter" style="text-transform:none;font-size:12px; ">' + detailedMessage + '<p></div></div>',
            {
                'autoDimensions': false,
                'width': 'auto',
                'height': '300px',
                'transitionIn': 'none',
                'transitionOut': 'none'
            }
        );

}

//AF Fancy function.
function afFancyWishList(message, click, href, isSuccess) {
    var picLink;
    var id;
    var color;

    piclink = isSuccess ? '/_img/popupthanks.png' : '/_img/error_password.png';
    id = isSuccess ? 'messageSent' : 'messageNotSent';
    color = isSuccess ? '#669933' : '#c66';

    $.fancybox(
            '<div style="width: 500px; height: auto; overflow: auto; position: relative;">' +
            '<div id="' + id + '">' +
            '<p class="textCenter"><img src="' + piclink + '" width="32" height="34" alt="" /></p>' +
            '<p class="textCenter" style="text-transform:uppercase">' +
            ' ' + message + '</p>' +
            '<br/><a style="color:' + color + '" class="textCenter"  href="' + href + '"><p class="textCenter" style="text-transform:uppercase;font-size:11px">' + click + '</p></a></div></div>',
            {
                'autoDimensions': false,
                'width': 'auto',
                'height': '200px',
                'transitionIn': 'none',
                'transitionOut': 'none'
            }
        );

}

function goToByScroll(id) {
    if (id == "ContactUs")
        return;
    var op = jQuery.browser.opera ? jQuery("html") : jQuery("html, body");
    op.animate({ scrollTop: jQuery("#" + id).offset().top }, 'slow');
}


function InitChecboxes($container) {
    $container.find('a.checkbox').click(function (e) {
        e.preventDefault();
        var mode = $(this).attr('mode');
        if (mode == 'single')// radio button
        {
            //find all related checkboxes and turn of them
            $('a.checkbox[group="' + $(this).attr('group') + '"]').removeClass('checked');
            $(this).addClass('checked');
            //there shall be a hidden imput name=id=groupname
            $('input[name="' + $(this).attr('group') + '"]').val($(this).attr('rdoValue'));
        }
        //            else if (mode == 'all') {
        //                $(this).toggleClass('checked');
        //                $('a.checkbox[group="' + $(this).attr('group') + '"]').attr('class', $(this).attr('class'));
        //                //$('input[name="' + $(this).attr('group') + '"]').val($(this).hasClass('checked'));
        //            }
        else//checkbox
        {
            $(this).toggleClass('checked');
            $('input[name="' + $(this).attr('group') + '"]').val($(this).hasClass('checked'));
        }

    });
}

function OpenPriceOffer(productId, productName, productSrc) {
    $('div#sendPrice input#ProductId').val(productId);
    $('div#sendPrice input#ProductId').attr('readonly', 'true');
    $('div#sendPrice input#ProductName').val(productName);
    $('div#sendPrice input#ProductName').attr('readonly', 'true');
    $('div#sendPrice img#productImg').attr('src', productSrc);
    $('div#sendPrice input#Phone').val('');
    $('div#sendPrice textarea#Enquiry').val('');
    $('div#sendPrice #successResult').hide();
    $('div#sendPrice #errorResult').hide();
    $('p#productNameTop').text(productName);
}
function InitPriceOffer() {
    $('div#sendPrice a#priceOfferLink').click(function () {
        var $form = $(this).closest('form');
        if ($form.valid()) {
            $.ajax({
                type: "POST",
                url: $(this).closest('form').attr('action'),
                data: $(this).closest('form').serialize(),
                success: function (data) {
                    if (data.Success) {
                        //$('div#sendPrice #formDivision').hide();
                        afFancy(data.Message, data.SubMessage, true);
                    }
                    else {
                        // $('div#sendPrice #formDivision').hide();
                        afFancy(data.Message, data.SubMessage, false);
                    }
                },
                complete: function () {
                }
            });
        }
    });
}
function IsGuest() {
    return $('li#umLogin').length > 0;
}

function getCookie(c_name) {
    var i, x, y, ARRcookies = document.cookie.split(";");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == c_name) {
            return unescape(y);
        }
    }
}

function setCookie(c_name, value, exdays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value = escape(value) + ((exdays == null) ? "" : "; expires=" + exdate.toUTCString());
    document.cookie = c_name + "=" + c_value + '; path=/';
}
function setCookieHour(c_name, value, exhours) {
    var now = new Date();
    var time = now.getTime();
    time += 3600 * 1000 * exhours;
    now.setTime(time);
    var c_value = escape(value) + ((exhours == null) ? "" : "; expires=" + now.toUTCString());
    document.cookie = c_name + "=" + c_value + '; path=/';
}


//language selection

//function InitlanguageSelection() { 
//    $('ul.langList li').click(function f(){
//   
//   var id=$(this).attr('id');
//     $.ajax({
//        url: _languageSelectedUrl,
//        data: {customerlanguage : id},
//        success: function (data) {
//          location.reload();
//        }
//       
//    });


//    });

//}


function AFDropdown(list, id) {
    var css = '';
    var caption = '';
    var innerHtml = '';
    var value = '';
    for (var i = 0; i < list.length; i++) {
        css = '';
        if (list[i].Selected) {
            css = 'on';
            caption = list[i].Text;
            value = list[i].Value;
        }
        innerHtml += '<li data-value="' + list[i].Value + '" class="' + css + '">' + list[i].Text + '</li>';
    }
    var html = '<div class="selectBox"><span>' + caption + '</span><ul style="">';
    html += innerHtml;
    html += '</ul> <input id="' + id + '" name="' + id + '" value="' + value + '" type="hidden"/> </div>';

    return html;
}
function InitAFDropdown($container) {
    $container.find('div.selectBox').click(function () {
        var that = $(this);
        if (!that.hasClass('on')) { that.addClass('on').children('ul').stop().slideDown({ duration: 150, easing: 'easeInOutQuad' }); }
        else { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
    }).hover(function () { }, function () {
        var that = $(this);
        if (that.hasClass('on')) { that.children('ul').stop().slideUp({ duration: 150, easing: 'easeInOutQuad', complete: function () { that.removeClass('on').children('ul').attr('style', ''); } }); }
    });

    $container.find('div.selectBox ul li').click(function (e) {
        e.stopPropagation();
        var that = $(this);
        that.addClass('on').siblings().removeClass('on');
        that.parent().prev().text(that.text());
        that.parent().parent().find(':hidden').val(that.attr('data-value'));
        that.parent().parent().trigger('click');
    });

}

function PopupRegistration() {

    if ($('#newsletterRegister').length == 0) return;
    setCookieHour('registerationAskedSession', 1,1);
    var count = getCookie('registerationAsked');
    if (typeof count === "undefined") {
        count = 0;
    }
    count++;
    setCookie('registerationAsked', count, 365);
    //document.cookie = 'registerationAsked=' + count;
    $('div#newsletterRegister').show();
    $('<a />').attr({ href: '#newsletterRegister' }).fancybox({
       'padding' : '0px',
        onClosed: function () { $('div#newsletterRegister').hide(); }

    }).trigger('click');    

}

// e bulten kayit
function InitRegistrationPopup() {
    // ebulten otomatik actirma
    var registerationAsked = getCookie('registerationAsked');
    var registerationAskedSession = getCookie('registerationAskedSession');

    if (IsGuest() && (typeof registerationAsked === "undefined" || registerationAsked <= 3) && (typeof registerationAskedSession === "undefined" || registerationAskedSession == 0)) {
        window.setTimeout(
          "PopupRegistration();", 1000); // each(function () { $(this).click(); });
        //.trigger('click');
    }
}

function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}


$(function () {
    $('.currentPageInput').keypress(function (e) {
        if (!e) e = window.event;
        var keyCode = e.keyCode || e.which;
        var pn = $(this).val();
        if (keyCode == '13' && pn != "" && !isNaN(parseFloat(pn)) && isFinite(pn)) {
            var tp = $(this).attr('data-totalpages');
            if (parseInt(pn) <= parseInt(tp)) {
                var a = $(this).attr('data-href');

                var searchMask = "pagenumber";
                var regEx = new RegExp(searchMask, "ig");
                var replaceMask = "pagenumber";
                a = a.replace(regEx, replaceMask);

                a = a.replace("?pagenumber=", "").replace("&pagenumber=", "");
                if (a.indexOf("?") > 0)
                    a = a + "&pagenumber=";
                else
                    a = a + "?pagenumber=";
                window.location = a + pn;
                e.preventDefault();
                return false;
            }
        }
    });
});
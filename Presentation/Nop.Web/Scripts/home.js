$(function () {

    // main page slider
    (function () {
        var sliderContainer = $('div#mainSliderContainer');
        var slider = sliderContainer.children('ul');
        var slides = slider.children('li');

        var spotlightContainer = $('div.mainSpotLight');
        var spotlightItemContainer = spotlightContainer.children('div.spotLightItems');
        var spotlightItems = spotlightItemContainer.children('div.item');

        // otomatik dondurme
        var sliderInterval = "";
        sliderInterval = setTimeout(function () {
            $('ul.spotlightButtons li.next a').trigger('click');
        }, 5000);

        $(window).resize(function () {
            var pageWidth = $(window).width();

            sliderContainer.css({ width: pageWidth + 'px' });
            slides.css({ width: pageWidth + 'px' });
            slider.css({ width: (slides.length * pageWidth) + 'px', left: '-' + (slider.children('li.current').position().left) + 'px' });

        }).trigger('resize');

        $('ul.spotlightButtons li.next a').click(function (e) {
            e.preventDefault();

            clearTimeout(sliderInterval);

            var currSlide = slider.children('li.current');
            if (currSlide.nextAll().length < 1) {
                currSlide = slider.children('li:first');
                currSlide.addClass('current').siblings().removeClass('current');
                slider.css('left', '0');
            }

            //debugger;
            currSlide.removeClass('current').next().addClass('current');
            currSlide = currSlide.next();
            slider.clearQueue().delay(350).animate({ left: '-' + currSlide.position().left + 'px' }, 500);

            // spolight section
            spotlightContainer.clearQueue().animate({ left: 0, opacity: 0 }, 350, function () {
                var that = $(this);
                var currItem = spotlightItemContainer.children('div.item.current');
                if (currItem.nextAll().length > 0) { currItem = currItem.next(); }
                else { currItem = spotlightItemContainer.children('div.item:first'); }

                currItem.addClass('current').siblings().removeClass('current');
                that.css({ left: '560px' }).clearQueue().delay(400).animate({ opacity: 1 }, 350, function () { that.removeAttr('style'); });
            });

            sliderInterval = setTimeout(function () {
                $('ul.spotlightButtons li.next a').trigger('click');
            }, 5000);
        });

        $('ul.spotlightButtons li.prev a').click(function (e) {
            e.preventDefault();

            clearTimeout(sliderInterval);

            var currSlide = slider.children('li.current');
            if (currSlide.prevAll().length < 1) {
                var lastItem = slider.children(':last');
                slider.css('left', '-' + lastItem.position().left + 'px');

                currSlide = lastItem;
                currSlide.addClass('current').siblings().removeClass('current');
            }

            //debugger;
            currSlide.removeClass('current').prev().addClass('current');
            currSlide = currSlide.prev();
            slider.clearQueue().delay(350).animate({ left: '-' + currSlide.position().left + 'px' }, { duration: 500, easing: 'easeInOutCirc' });

            // spolight section
            spotlightContainer.clearQueue().animate({ opacity: 0 }, { duration: 350, easing: 'easeInOutCirc', complete: function () {
                var that = $(this);
                var currItem = spotlightItemContainer.children('div.item.current');
                if (currItem.prevAll().length > 0) { currItem = currItem.prev(); }
                else { currItem = spotlightItemContainer.children('div.item:last'); }

                currItem.addClass('current').siblings().removeClass('current');
                that.css({}).clearQueue().delay(400).animate({ opacity: 1 }, 350, function () { that.removeAttr('style'); });
            } 
            });

            //e.preventDefault();
            //var currSlide = slider.children('li.current');
            //if(currSlide.prevAll().length > 0){
            //debugger;
            //	currSlide.removeClass('current').prev().addClass('current');
            //	currSlide = currSlide.prev();
            //	slider.stop().animate({ left: '-' + currSlide.position().left + 'px' }, { duration: 500, easing: 'easeInOutCirc' });
            //} else {
            //	var lastItem = slider.children(':last');
            //	slider.css('left', '-' + lastItem.position().left + 'px');
            //	lastItem.addClass('current').siblings().removeClass('current');
            //	$(this).trigger('click');
            //}

            sliderInterval = setTimeout(function () {
                $('ul.spotlightButtons li.next a').trigger('click');
            }, 5000);
        });

    })();

    // e bulten kayit
    (function () {
        // ebulten otomatik actirma
        //$("<a />").attr({ href: '#newsletterRegister' }).fancybox({}).trigger('click');	
    })();

    // home page resize
//    (function () {
//        var headerHeight = 110,
//		footerHeight = 110,
//		containers = $('#main, #mainSliderContainer, #mainSliderContainer ul li');

//        $(window).resize(function (e) {
//            var pageHeight = $(window).height(),
//			resizedValue = (pageHeight - headerHeight - footerHeight);
//            resizedValue = resizedValue > 577 ? resizedValue : 577;
//            containers.css({ height: resizedValue + 'px' });
//        }).trigger('resize');
//    })();
});
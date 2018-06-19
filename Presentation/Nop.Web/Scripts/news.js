$(function () {

    // slider
    (function () {
        var prevSlideButton = $('#newsPageSliderPrev')
        var nextSlideButton = $('#newsPageSliderNext');

        $('#newsPageSlider .counter li').bind('click', function () {
            var that = $(this),
			sliderContents = $('#newsPageSlider .content ul li');
            if (that.hasClass('on')) { return; }
            that.addClass('on').siblings().removeClass('on');

            $('#newsPageSlider .content ul').stop().animate({ left: '-' + $(sliderContents[that.prevAll().length]).position().left + 'px' }, { easing: 'easeInOutQuart', duration: 1000 });
        });

        prevSlideButton.bind('click', function () {
            var selectedSlide = $('#newsPageSlider .counter li.on');
            if (selectedSlide.prevAll().length > 0) { selectedSlide.prev().trigger('click'); }
            else { selectedSlide.siblings(':last').trigger('click'); }
        });

        nextSlideButton.bind('click', function () {
            var selectedSlide = $('#newsPageSlider .counter li.on');
            if (selectedSlide.nextAll().length > 0) { selectedSlide.next().trigger('click'); }
            else { selectedSlide.siblings(':first').trigger('click'); }
        });

        $('#newsPageSlider').hover(function () {
            prevSlideButton.fadeIn(200); nextSlideButton.fadeIn(200);
        }, function () {
            prevSlideButton.fadeOut(200); nextSlideButton.fadeOut(200);
        });
    })();

    // accordion
    (function () {
        $('#content .newsPageAccordion .item h5').bind('click', function () {
            var that = $(this);
            if (that.parent().hasClass('on')) { that.next().stop().slideUp(850, function () { $(this).css({ height: 'auto' }).parent().removeClass('on'); }); }
            else { that.next().stop().slideDown(850).parent().addClass('on').siblings().children('.content').stop().slideUp(850, function () { $(this).css({ height: 'auto' }).parent().removeClass('on'); }); }
        });
    })();

    // bottom slider
    (function () {
        var sliderContainer = $('#newsBottomSliderContent');
        var slider = $('#newsBottomSliderContent ul');
        slider.children('li:first').addClass('on');
        var sliderContainerWidth = sliderContainer.children().length * 170;

        $('#newsBottomSliderRight').bind('click', function () {
            var current = slider.children('li.on');
            var nextChild = $(slider.children()[current.prevAll().length + 1]);
            var hede = $(slider.children()[current.prevAll().length + 3]);
            if (hede.length == 0) return;
            nextChild.addClass('on').siblings().removeClass('on');
          //  if (sliderContainerWidth <= 170 * 3)
            //    return;
            if (!nextChild.length > 0) { return; }
            slider.stop().animate({ left: '-' + nextChild.position().left + 'px' }, { easing: 'easeInOutQuart', duration: 200 });
        });

        $('#newsBottomSliderLeft').bind('click', function () {
            var current = slider.children('li.on');
            var nextChild = $(slider.children()[current.prevAll().length - 1]);
            nextChild.addClass('on').siblings().removeClass('on');
            if (!nextChild.length > 0) { return; }
            slider.stop().animate({ left: '-' + nextChild.position().left + 'px' }, { easing: 'easeInOutQuart', duration: 200 });
        });
    })();

    //fb like
    (function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_US/all.js#xfbml=1";
        fjs.parentNode.insertBefore(js, fjs);
    } (document, 'script', 'facebook-jssdk'));


    !function (d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0]; if (!d.getElementById(id)) {
            js = d.createElement(s); js.id = id; js.src = "//platform.twitter.com/widgets.js";
            fjs.parentNode.insertBefore(js, fjs);
        } 
    } (document, "script", "twitter-wjs");


    (function () {
        var po = document.createElement('script'); po.type = 'text/javascript'; po.async = true;
        po.src = 'https://apis.google.com/js/plusone.js';
        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(po, s);
    })();

});
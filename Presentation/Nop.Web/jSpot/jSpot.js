// jQuery Spotlight Plugin written by Orhan Ekici at 27.08.2012 { v 1.0 }

(function ($) {

    $.fn.jSpot = function (options) {
        var defaults = {
            tooltip: true,
            text: 'Enter a text via plugin call or set tooltip as false',
            opacity: '0.5',
            unbindResizeAfterClose: true // use as false if you have other binded resize actions
        };

        var options = $.extend(defaults, options);

        return this.each(function () {
            var that = $(this),
			itemWidth = that.width(),
			itemHeight = that.height(),
			currWindow = $(window);
            currBody = $('body');

            // check if area is already spotted
            if (that.hasClass('jSpotted')) { return; }

            // add class to control later
            that.addClass('jSpotted');
            // define spotlight areas
            var spotlightLeftArea = $('<div />'),
			spotlightRightArea = $('<div />'),
			spotlightTopArea = $('<div />'),
			spotlightBottomArea = $('<div />'),
			spotLightAreaItems = [spotlightLeftArea, spotlightTopArea, spotlightBottomArea, spotlightRightArea];

            // close jSpot method
            var closejSpot = function () {
                if (options.unbindResizeAfterClose) {
                    // handle window again to avoid if its altered by body at resize control
                    $(window).unbind('resize');
                }

                for (var i = 0, len = spotLightAreaItems.length; i < len; i++) {
                    // unbind click handlers for unused elements
                    spotLightAreaItems[i].unbind('click');
                    spotLightAreaItems[i].fadeOut('250', function () { $(this).remove(); });
                }

                if (options.tooltip) {
                    spotlightTooltip.fadeOut('250', function () { $(this).remove(); });
                }

                that.removeClass('jSpotted');
                $('.offerTooltip').hide();
            };

            // add class and styles and bind closing method
            for (var i = 0, len = spotLightAreaItems.length; i < len; i++) {
                //spotLightAreaItems[i].addClass('jSpotArea').css({ opacity: options.opacity }).appendTo(currBody);
                spotLightAreaItems[i].bind('click', closejSpot);
            }

            // define tooltip
            if (options.tooltip) {
                var spotlightTooltip = $('<div />');
                spotlightTooltip.addClass('jSpotTooltip');
                spotlightTooltip.append('<p>' + options.text + '</p>');
                spotlightTooltip.appendTo(currBody);
                spotlightTooltip.bind('click', closejSpot);
            }

            // main resize event
            currWindow.bind('resize', function () {
                // check if body or window is larger
                if (currBody.height() > currWindow.height()) {
                    currWindow = currBody;
                }

                var itemLeftPosition = parseFloat(that.offset().left, 10),
				itemTopPosition = parseInt(that.offset().top, 10)
                currWindowWidth = currWindow.width(),
				currWindowHeight = currWindow.height();

                spotlightLeftArea.css({
                    width: itemLeftPosition + 'px',
                    height: (itemTopPosition + itemHeight) + 'px',
                    left: '0',
                    top: '0'
                });

                spotlightTopArea.css({
                    width: (currWindowWidth - itemLeftPosition) + 'px',
                    height: (itemTopPosition) + 'px',
                    left: itemLeftPosition + 'px',
                    top: '0'
                });

                spotlightBottomArea.css({
                    width: (itemLeftPosition + itemWidth) + 'px',
                    height: (currWindowHeight - (itemTopPosition + itemHeight)) + 'px',
                    left: '0',
                    top: (itemTopPosition + itemHeight) + 'px'
                });

                spotlightRightArea.css({
                    width: (currWindowWidth - (itemLeftPosition + itemWidth)) + 'px',
                    height: (currWindowHeight - itemTopPosition) + 'px',
                    left: (itemLeftPosition + itemWidth) + 'px',
                    top: itemTopPosition + 'px'
                });

                if (options.tooltip) {
                    spotlightTooltip.css({
                        left: ((itemLeftPosition + (itemWidth / 2)) - (spotlightTooltip.width() / 2)) + 'px',
                        top: (itemTopPosition + 29) + 'px'
                    });
                }

            }).trigger('resize'); // trigger once to set areas
        });
    };
    $.fn.jSpot2 = function (options) {
        var defaults = {
            tooltip: true,
            text: 'Enter a text via plugin call or set tooltip as false',
            opacity: '0.5',
            unbindResizeAfterClose: true // use as false if you have other binded resize actions
        };

        var options = $.extend(defaults, options);

        return this.each(function () {
            var that = $(this),
			itemWidth = that.width(),
			itemHeight = that.height(),
			currWindow = $(window);
            currBody = $('body');

            // check if area is already spotted
            if (that.hasClass('jSpotted')) { return; }

            // add class to control later
            that.addClass('jSpotted');
            // define spotlight areas
            var spotlightLeftArea = $('<div />'),
			spotlightRightArea = $('<div />'),
			spotlightTopArea = $('<div />'),
			spotlightBottomArea = $('<div />'),
			spotLightAreaItems = [spotlightLeftArea, spotlightTopArea, spotlightBottomArea, spotlightRightArea];

            // close jSpot method
            var closejSpot = function () {
                if (options.unbindResizeAfterClose) {
                    // handle window again to avoid if its altered by body at resize control
                    $(window).unbind('resize');
                }

                for (var i = 0, len = spotLightAreaItems.length; i < len; i++) {
                    // unbind click handlers for unused elements
                    spotLightAreaItems[i].unbind('click');
                    spotLightAreaItems[i].fadeOut('250', function () { $(this).remove(); });
                }

                if (options.tooltip) {
                    spotlightTooltip.fadeOut('250', function () { $(this).remove(); });
                }

                that.removeClass('jSpotted');
                $('.offerTooltip').hide();
            };

            // add class and styles and bind closing method
            for (var i = 0, len = spotLightAreaItems.length; i < len; i++) {
               // spotLightAreaItems[i].addClass('jSpotArea').css({ opacity: options.opacity }).appendTo(currBody);
                spotLightAreaItems[i].bind('click', closejSpot);
            }

            // define tooltip
            if (options.tooltip) {
                var spotlightTooltip = $('<div />');
                spotlightTooltip.addClass('jSpotTooltip2');
                spotlightTooltip.append('<p>' + options.text + '</p>');
                spotlightTooltip.appendTo(currBody);
                spotlightTooltip.bind('click', closejSpot);
            }

            // main resize event
            currWindow.bind('resize', function () {
                // check if body or window is larger
                if (currBody.height() > currWindow.height()) {
                    currWindow = currBody;
                }

                var itemLeftPosition = parseFloat(that.offset().left, 10),
				itemTopPosition = parseInt(that.offset().top, 10)
                currWindowWidth = currWindow.width(),
				currWindowHeight = currWindow.height();

                spotlightLeftArea.css({
                    width: itemLeftPosition + 'px',
                    height: (itemTopPosition + itemHeight - 7) + 'px',
                    left: '0',
                    top: '0'
                });

                spotlightTopArea.css({
                    width: (currWindowWidth - itemLeftPosition) + 'px',
                    height: (itemTopPosition - 2) + 'px',
                    left: itemLeftPosition + 'px',
                    top: '0'
                });

                spotlightBottomArea.css({
                    width: (itemLeftPosition + itemWidth) + 'px',
                    height: (currWindowHeight - (itemTopPosition + itemHeight)) + 'px',
                    left: '0',
                    top: (itemTopPosition + itemHeight -7) + 'px'
                });

                spotlightRightArea.css({
                    width: (currWindowWidth - (itemLeftPosition + itemWidth)) + 'px',
                    height: (currWindowHeight - itemTopPosition) + 'px',
                    left: (itemLeftPosition + itemWidth) + 'px',
                    top: itemTopPosition - 2 + 'px'
                });

                if (options.tooltip) {
                    spotlightTooltip.css({
                        left: ((itemLeftPosition + (itemWidth / 2)) - (spotlightTooltip.width() / 2)) + 'px',
                        top: (itemTopPosition + 29) + 'px'
                    });
                }

            }).trigger('resize'); // trigger once to set areas
        });
    };
})(jQuery);
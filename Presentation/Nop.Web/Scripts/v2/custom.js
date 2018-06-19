$(window).load(function() {
    setTimeout(function() {
        $('.header_top_out').animate({
            'margin-top': 0,
            'opacity': 1
        });
    }, 1300);
    $('.umShopBag_next ').hover(function() {
        $(this).addClass('active')
        $('.umShopBag_next_pop').stop().animate( {height:'395px' },{ duration:300 } );
    },function(){
		 $(this).removeClass('active')
        setTimeout(function() {$('.umShopBag_next_pop').stop().animate( {height:'0' },{ duration:400 } )}, 250);
		});
    
    $('.input_text1').click(function(e) {
        $(this).animate({width:'125px',
        } );
        $(this).css({
            'border-bottom': '1px solid #000', 
        })
        event.stopPropagation(e);
    });
    $(document).on('click', function(e) {
        if ($(e.target).closest('.input_text1').length === 0) {
            $('.input_text1').animate({
                'width': '40px'
            }).css({
                'border-bottom': '1px solid transparent'
            });
        }
    });
    $('.close_pop').click(function() {
        $('.umShopBag_next_pop').slideUp()
        $('.subtrigger').removeClass('active');
    });
    $("#countries,#currency ").msDropdown();
    $('.banner_slider').carouFredSel({
		responsive: true,
        scroll: {
            fx: 'crossfade',
			pauseOnHover: true
        },
        prev: '.leftarrow',
        next: '.rightarrow',
        pagination: "#pager2",
        mousewheel: false,
        swipe: {
            onMouse: true,
            onTouch: true
        }
    });
    $('#foo0').carouFredSel({
        scroll: {
            fx: 'crossfade',
			pauseOnHover: true
        },
        prev: '#prev2',
        next: '#next2',
        items: {
            visible: {
                min: 1,
                max: 1
            }
        },
        mousewheel: false,
        swipe: {
            onMouse: true,
            onTouch: true
        }
    });
    $('#foo1').carouFredSel({
        responsive: true,
        width: '100%',
        auto: false,
        scroll: {
            duration: 1200,
        },
        prev: '.slider_arrow_left',
        next: '.slider_arrow_right',
        items: {
            visible: {
                min: 5,
                max: 5
            }
        },
        mousewheel: false,
        swipe: {
            onMouse: true,
            onTouch: true
        },
    });
    $('#foo2').carouFredSel({
        auto: false,
        prev: '#prev5',
        next: '#next5',
        mousewheel: false,
        scroll: {
            duration: 1200,
        },
        swipe: {
            onMouse: true,
            onTouch: true
        }
    });
    $('#foo3').carouFredSel({
        responsive: true,
        scroll: {
            fx: 'crossfade'
        }
    });

    if ($('.scroll-pane .list_info') != null && $('.scroll-pane .list_info').length == 0)
        $('.scroll-pane').css('height', '0px');
    if ($('.scroll-pane .list_info') != null && $('.scroll-pane .list_info').length == 1)
        $('.scroll-pane').css('height', '116px');
    if ($('.scroll-pane .list_info') != null && $('.scroll-pane .list_info').length == 2)
        $('.scroll-pane').css('height', '232px');

    $('.scroll-pane').jScrollPane({
        autoReinitialise: true
    });
    $('.tab_nav li a, .subnav_box h3 a').hover(function () {
        var lsrc = $(this).attr('data-icon');
        $('#catlivebox' + $(this).attr('data-cat')).attr('src', lsrc);
    }, function () {
        $('#catlivebox' + $(this).attr('data-cat')).attr('src', '/_img/p.png');
    });
    $('.tab_nav5 li a').hover(function() {
        var lsrc = $(this).attr('data-icon');
        $('#manlivebox' + $(this).attr('data-cat')).attr('src', lsrc);
    }, function () {
        $('#manlivebox' + $(this).attr('data-cat')).attr('src', '/_img/p.png');
    });
    //$('#languge').toggle(function() {
//        $(this).addClass('active');
//        $('#languge_bar').slideDown();
//    }, function() {
//        $(this).removeClass('active');
//        $('#languge_bar').slideUp();
//    });
   // $('#currency_').toggle(function() {
//        $(this).addClass('active');
//        $('#frank-bar').slideDown()
//    }, function() {
//        $(this).removeClass('active');
//        $('#frank-bar').slideUp()
//    });
	$('#lang').hover(function(){
		$('#languge').addClass('active');
		$('#languge_bar').stop().slideDown()
		},
		function(){
		$('#languge').removeClass('active');
		$('#languge_bar').stop().slideUp()
		}
		);
	$('.currency_').hover(function(){
		$('#currency-select').addClass('active');
		$('#frank-bar').stop().slideDown()
		},
		function(){
		$('#currency-select').removeClass('active');
		$('#frank-bar').stop().slideUp()
		}
		);
		
});

$(document).ready(function(e) {
	$('.overlay').css({'opacity':'0'})
   	$('.overlay').hover(function(){
		$(this).stop().animate({opacity:1})	
	},
	function(){
		$(this).stop().animate({opacity:0})	
	});
	
	$('.top_product').hover(function(){
		$(this).find('.product_text').stop().animate({bottom:0})	
	},
	function(){
		$(this).find('.product_text').stop().animate({bottom:-116})
	});
	
	$('.top_product').hover(function(){
		$(this).find('.product_text.odd').stop().animate({bottom:0})	
	},
	function(){
		$(this).find('.product_text.odd').stop().animate({bottom:-65})
	});
	
	$('.post_pic_cont a.link2 span').css({'background-position':'-178px -227px'})
	$('.post_pic_cont a').hover(function(){
		$(this).find('span').stop().animate({'background-position': '-152px -227px'})	
	},
	function(){
		$(this).find('span').stop().animate({'background-position': '-178px -227px'})
	});
	
	$('.product_text.odd').css({'bottom':'-65px'});
   	$('.top_product2').mouseenter(function(){
		$(this).find('.product_text.odd').stop().animate({bottom:0})
	});
	$('.top_product2').mouseleave(function(){
		$(this).find('.product_text.odd').stop().animate({bottom:-65})
	});
	$('.product_text.second').css({'bottom':'-70px'});
   	$('.gift_slider ul li').mouseenter(function(){
		$(this).find('.product_text.second').stop().animate({bottom:0})
	});
	$('.gift_slider ul li').mouseleave(function(){
		$(this).find('.product_text.second').stop().animate({bottom:-70})
	});
});


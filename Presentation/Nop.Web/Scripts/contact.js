$(function () {
    $("a#customerLink").click(function () {
        $('div#customerCare #formDivision').show();
        $('div#customerCare #successResult').hide();
        $('div#customerCare #errorResult').hide();
    });
    
    $("a#TopicDetailsSalesAgreements").click(function () {
        $('div#SalesAgreements #formDivision').show();
    });

    $('a#customerSupportLink').click(function () {
        if ($(this).closest('form').valid()) {
            $.ajax({
                type: "POST",
                url: $(this).closest('form').attr('action'),
                data: $(this).closest('form').serialize(),
                success: function (data) {
                    if (data.success) {
                        afFancy(data.header, data.message, true)
                    }
                    else {
                        afFancy(data.header, false)
                    }
                },
                complete: function () {
                }
            });
        }
        else {
            $(this).closest('form')
            return false;
        }

    });

    $('a#contactUsLink').click(function () {
        if ($(this).closest('form').valid()) {
            $.ajax({
                type: "POST",
                url: $(this).closest('form').attr('action'),
                data: $(this).closest('form').serialize(),
                success: function (data) {
                    if (data.success) {
                        afFancy(data.header, data.message, true)
                    }
                    else {
                        afFancy(data.header, data.message, false)
                        afFancy(data.header, data.message, false)
                    }
                },
                complete: function () {
                }
            });
        }
        else {
            $(this).closest('form')
            return false;
        }

    });

    $("a#detailEditCustomer").click(function () {

        if ($(this).attr("data") == 'edit') {
            $("div#overviewDetail").hide();
            $("div#detailEdit").show();
            $(this).attr("data", "back")
        }
        else {
            $(this).attr("data", "edit")
            $("div#overviewDetail").show();
            $("div#detailEdit").hide();
        }

    });

    $('ul.webOrderCapsules li.webOrderDetails a.button2').click(function () {

        $(this).closest('form').submit();
    });

    //    $('input#returnRequest').click(function () {
    //        alert('asd');
    //        if ($(this).closest('form').valid()) {
    //            $.ajax({
    //                type: "POST",
    //                url: $(this).closest('form').attr('action'),
    //                data: $(this).closest('form').serialize(),
    //                beforeSend: function () { $('body').append('<div id="loading" style="position:fixed; top:0px;left:0px; width:100%; height:100%; z-index:1000; opacity:0.70; background: url(/_img/loading.gif) #f3f3f3 center center no-repeat; overflow:hidden; "></div>'); },
    //                success: function (data) { afFancy(data.header, data.message, data.success); },
    //                complete: function () { $('div#loading').remove(); }
    //            });
    //        }
    //    });

    $("a#inline").fancybox({
        'hideOnContentClick': true
    });

    function formatTitle(title, currentArray, currentIndex, currentOpts) {
        return '<div id="tip7-title"><span><a href="javascript:;" onclick="$.fancybox.close();"><img src="/data/closelabel.gif" /></a></span>' + (title && title.length ? '<b>' + title + '</b>' : '') + 'Image ' + (currentIndex + 1) + ' of ' + currentArray.length + '</div>';
    }

    //    $("a#fancyLink").fancybox('<h2>Hi!</h2><p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam quis mi eu elit tempor facilisis id et neque</p>')

});



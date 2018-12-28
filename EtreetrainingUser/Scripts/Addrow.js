var nm = 1;
$(document).ready(function () {
   
    
    $('.removefst').hide();
  
    $('.addfst').click(function () {
      
        var $newRow = $('#mainrow').clone().removeAttr('id');
        $('.addfst', $newRow).removeClass('addfst').addClass('add').val('+');
        $('.removefst', $newRow).removeClass('removefst').addClass('remove').val('-').removeClass('btn-success').addClass('btn-danger');
        $('.remove', $newRow).show();
        $('#orderdetailsItems').append($newRow.addClass('remove' + nm).addClass('nwdv'));
       
        //$('.remove' + nm).find('.pcs').val('');
        //$('.remove' + nm).find('.wt').val('');
        //$('.remove' + nm).find('.stkin').val('');
        //$('.remove' + nm).find('.dscnt').val('');
        //$('.remove' + nm).find('.ttl').val('');
        //$('.remove' + nm).find('.amt').val('');
        //$('.remove' + nm).find('.amaftrtx').val('');

        //$('.remove' + nm).find('.nm').val('');
        //$('.remove' + nm).find('.txrt').val('');
        //$('.remove' + nm).find('.hsn').val('');
        //$('.remove' + nm).find('.dt').val('');
        //$('.remove' + nm).find('.unt').val('');
      
        nm = nm + 1;
    })
 
    $('#orderdetailsItems').on('click', '.add', function () {
     
        var $newRow = $('#mainrow').clone().removeAttr('id');
        $('.addfst', $newRow).removeClass('addfst').addClass('add').val('+');
        $('.removefst', $newRow).removeClass('removefst').addClass('remove').val('-').removeClass('btn-success').addClass('btn-danger');
        $('.remove', $newRow).show();
        $('#orderdetailsItems').append($newRow.addClass('remove' + nm).addClass('nwdv'));

        $('.remove' + nm).find('.pcs').val('');
        $('.remove' + nm).find('.wt').val('');
        $('.remove' + nm).find('.stkin').val('');
        $('.remove' + nm).find('.dscnt').val('');
        $('.remove' + nm).find('.ttl').val('');
        $('.remove' + nm).find('.amt').val('');
        $('.remove' + nm).find('.amaftrtx').val('');

        $('.remove' + nm).find('.nm').val('');
        $('.remove' + nm).find('.txrt').val('');
        $('.remove' + nm).find('.hsn').val('');
        $('.remove' + nm).find('.dt').val('');
        $('.remove' + nm).find('.unt').val('');
        nm = nm + 1;
    })

    $('#orderdetailsItems').on('click', '.remove', function () {
      

        $(this).parents('div.nwdv').remove();
    });
   
});
$('#orderdetailsItems').on('change', '.pp', function () {
    var prdcttkn1 = $(this).parents('div.nwdv').find('.pp').val();
    var x = $(this).parents('div.nwdv').find('.hsn');
    var y = $(this).parents('div.nwdv').find('.txrt');
    $.ajax
        ({
            type: 'POST',
            url: 'http://localhost:7298/ProductsForSale/GetProducts/',
            dataType: 'json',
            data: { 'id': prdcttkn1 },
            success: function (data) {
                x.val(data.HSN_SAC_Code);
                y.val(parseFloat(data.Tax_rate).toFixed(2));
            },
            error: function (ex) {
                var r = jQuery.parseJSON(response.responseText);
                alert("Message: " + r.Message);
            }
          
        });
   
});
$('#orderdetailsItems').on('change','.sllon', function () {
    var stkin1 = $(this).parents('div.nwdv').find('.stkin').val();
    var sllupn = $(this).parents('div.nwdv').find('.sllon').val();
 
    if (sllupn == "Pieces") {
        $(this).parents('div.nwdv').find('.wt').hide();
        $(this).parents('div.nwdv').find('.pcs').show();
        $(this).parents('div.nwdv').find('.unt').val(sllupn);

        $(this).parents('div.nwdv').find('.stkin').val((stkin1 * 1).toFixed(0));
    } else if (sllupn == "Weight") {
        $(this).parents('div.nwdv').find('.pcs').hide();
        $(this).parents('div.nwdv').find('.wt').show();
        $(this).parents('div.nwdv').find('.unt').val(sllupn);
        $(this).parents('div.nwdv').find('.stkin').val((stkin1 * 1).toFixed(3));
    } else if (sllupn == "Plate") {
        $(this).parents('div.nwdv').find('.wt').hide();
        $(this).parents('div.nwdv').find('.pcs').show();
        $(this).parents('div.nwdv').find('.unt').val(sllupn);
        $(this).parents('div.nwdv').find('.stkin').val((stkin1 * 1).toFixed(0));
    } else {

    }


});
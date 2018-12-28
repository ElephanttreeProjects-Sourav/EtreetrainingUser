
$('#submit').click(function () {
   
    var isAllValid = true;

    //validate order items

    var list = [];
    //var stocklist = [];
    //var errorItemCount = 0;
    var orderItemMain = {
        Course_name: $('#mainrow').find('.crsnm').text(),
        Chapter_number: $('#mainrow').find('.chno').val(),
        Title: $('#mainrow').find('.ttl').val(),
        Description: $('#mainrow').find('.des').val()
        
        
    }

    list.push(orderItemMain);

    $('#orderdetailsItems div.nwdv').each(function (index, ele) {
        var i = index + 1;
        if ($('.remove' + i).find('.chno').val() != "") {
            var orderItemnwdv = {
                Course_name: $('.remove' + i).find('.crsnm').text(),
                Chapter_number: $('.remove' + i).find('.chno').val(),
                Title: $('.remove' + i).find('.ttl').val(),
                Description: $('.remove' + i).find('.des').val(),

               
            }
        }
        list.push(orderItemnwdv);


    });

    if (isAllValid) {

        $(this).val('Please wait...');
        //alert($('.remove' + 1).find('.chno').val());
      
        $.ajax({
            type: 'POST',
            url: 'https://localhost:44305/Home/Doc_upload',
            data: JSON.stringify(list),
            contentType: 'application/json',
            success: function (data) {
           
                if (data.status) {
                    alert('Successfully added your chapters. Thank you.');
                    //here we will clear the form
                    list = [];
                    
                        document.location.reload(true);
                   
                }
                else {
                    alert(data.error);
                }
                $('#submit').val('Save');
            },
            error: function (error) {
                alert("error");
                $('#submit').val('Save');
            }
        });

    }

});

$('#submitforvideo').click(function () {

    var isAllValid = true;

    //validate order items

    var list = [];
    //var stocklist = [];
    //var errorItemCount = 0;
    var orderItemMain = {
        Course_name: $('#mainrow').find('.crsnm').text(),
        Chapter_number: $('#mainrow').find('.chno').val(),
        Title: $('#mainrow').find('.ttl').val(),
        Description: $('#mainrow').find('.des').val()


    }

    list.push(orderItemMain);

    $('#orderdetailsItems div.nwdv').each(function (index, ele) {
        var i = index + 1;
        if ($('.remove' + i).find('.chno').val() != "") {
            var orderItemnwdv = {
                Course_name: $('.remove' + i).find('.crsnm').text(),
                Chapter_number: $('.remove' + i).find('.chno').val(),
                Title: $('.remove' + i).find('.ttl').val(),
                Description: $('.remove' + i).find('.des').val(),


            }
        }
        list.push(orderItemnwdv);


    });

    if (isAllValid) {

        $(this).val('Please wait...');
        //alert($('.remove' + 1).find('.chno').val());

        $.ajax({
            type: 'POST',
            url: 'https://localhost:44305/Home/Doc_upload',
            data: JSON.stringify(list),
            contentType: 'application/json',
            success: function (data) {

                if (data.status) {
                    alert('Successfully added your chapters. Thank you.');
                    //here we will clear the form
                    list = [];

                    document.location.reload(true);

                }
                else {
                    alert(data.error);
                }
                $('#submit').val('Save');
            },
            error: function (error) {
                alert("error");
                $('#submit').val('Save');
            }
        });

    }

});
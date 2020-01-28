// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    //---------------------- NOTIF ---------------------------------
    


    //--------------------------------------------------------------


    var numberOfNotif = $('span.numberNotif');
    var bodey = $('body');

    if (numberOfNotif.length > 0) {
        $.when(GetNumberNotif()).done(function (output) {
            numberOfNotif.text(output);
        });
    }

    //----------------------- TRY-----------------------------------

    

    //---------------------- ADDRESS CHANGE -------------------------

    var muncitySelect = $('#muncityFilter');
    var barangaySelect = $('#barangay');
    var muncityId = 0;
    muncitySelect.on('change', function () {
        muncityId = muncitySelect.val();
        if (muncityId != '') {
            $.when(GetBarangayFiltered(muncityId)).done(function (output) {
                barangaySelect.empty()
                    .append($('<option>', {
                        value: '',
                        text: 'Select Barangay...'
                    }));
                jQuery.each(output, function (i, item) {
                    barangaySelect.append($('<option>', {
                        value: item.id,
                        text: item.description
                    }));
                });
            });
        }
        else {
            barangaySelect.empty()
                .append($('<option>', {
                    value: '',
                    text: 'Select Barangay...'
                }));
        }
    });

    //---------------------- ADDRESS CHANGE -------------------------

    //---------------------- ACCEPT MODAL ---------------------------
    

    //---------------------- ACCEPT MODAL ---------------------------

    var secondPlaceHolder = $('#modal-second-placeholder');

    //---------------------- MODALS ---------------------------------

    var placeholderElement = $('#modal-placeholders');


    var smallModal = $('#small-modal');
    var smallContent = $('#small-content');
    var largeModal = $('#large-modal')
    var largeContent = $('#large-content')

    $('a[data-toggle="small-modal"]').click(function (event) {
        var url = $(this).data('url');
        CallSmallModal(url);
    });


    $('button[data-toggle="small-modal"]').click(function (event) {
        var url = $(this).data('url');
        CallSmallModal(url);
    });

    $('a[data-toggle="large-modal"]').click(function (event) {
        var url = $(this).data('url');
        CallLargeModel(url);
    });

    function CallLargeModel(url) {
        $.get(url).done(function (data) {
            largeModal.modal('show');
            largeContent.empty();
            largeContent.html(data);
        });
    }

    function CallSmallModal(url) {
        $.get(url).done(function (data) {
            smallModal.modal('show');
            smallContent.empty();
            smallContent.html(data);
        });
    }

    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        console.log(url);
        $.get(url).done(function (data) {
            placeholderElement.empty();
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
        });
    });

    $('a[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeholderElement.empty();
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
        });
    });
    //small modal sumbit
    smallModal.on('click', 'button[data-save="modal"]', function (event) {
        event.preventDefault();
        var form = smallModal.find('form');
        var actionUrl = form.attr('action');
        var dataToSend = form.serialize();
        $.post(actionUrl, dataToSend).done(function (data) {
            var newBody = $(smallContent, data);
            smallContent.replaceWith(newBody);
            var validation = $('span.text-danger').text();
            if (validation == '') {
                //placeholderElement.find('.modal').modal('hide');
                //location.reload();
            }
        });
    });
    //large modal submit
    largeModal.on('click', 'button[data-save="modal"]', function (event) {
        event.preventDefault();
        console.log('here');
        var form = largeModal.find('form');
        var actionUrl = form.attr('action');
        var dataToSend = form.serialize();
        $.post(actionUrl, dataToSend).done(function (data) {
            var newBody = $(largeContent, data);
            largeContent.replaceWith(newBody);
            var validation = $('span.text-danger').text();
            if (validation == '') {
                placeholderElement.find('.modal').modal('hide');
                //location.reload();
            }
        });
    });


    placeholderElement.on('click', 'button[data-save="modal"]', function (event) {
        event.preventDefault();
        var form = placeholderElement.find('.modal').find('form');
        var formId = placeholderElement.find('.modal').attr('id');
        console.log(formId);
        var actionUrl = form.attr('action');
        console.log(actionUrl);
        var dataToSend = form.serialize();
        $.post(actionUrl, dataToSend).done(function (data) {
            var newBody = $('.modal-body', data);
            placeholderElement.find('.modal-body').replaceWith(newBody);
            var validation = $('span.text-danger').text();
            if (validation == '') {
                //placeholderElement.find('.modal').modal('hide');
                //location.href = '/Home/Index';
                if (formId == 'update-doctor-modal') {
                    location.href = '/Support/ManageUsers'
                }
                else if (formId == 'change-password-modal') {
                    location.reload();
                }
                else if (formId == 'reco-modal') {
                    var input = placeholderElement.find('.modal-footer').find('form').find('.code').val();
                    console.log(input);
                }
            }
        });
    });

    //---------------------- MODALS ---------------------------------

    //-------------------- CHANGE LOGIN STATUS ----------------------------

    var onDuty = $('button#btn-on-duty');
    var offDuty = $('button#btn-off-duty');
    var container = $('div#login-status-modal');

    onDuty.on('click', function () {
        SetLoginStatus('onDuty');
        container.modal('hide');
        location.reload();
    });

    offDuty.on('click', function () {
        SetLoginStatus('offDuty');
        container.modal('hide');
        location.reload();
    });



    //-------------------- CHANGE LOGIN STATUS ----------------------------

    //-------------------- DASHBOARD ----------------------------

    var bar_chart = $('#barChart');

    if ($('div.chart').length > 0) {
        $.when(GetDashboardValues()).done(function (output) {
            var areaChartData = {
                labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                datasets: [
                    {
                        label: 'Accepted',
                        backgroundColor: '#26B99A',
                        borderColor: 'rgba(60,141,188,0.8)',
                        pointRadius: false,
                        pointColor: '#3b8bba',
                        pointStrokeColor: 'rgba(60,141,188,1)',
                        pointHighlightFill: '#fff',
                        pointHighlightStroke: 'rgba(60,141,188,1)',
                        data: [output.accepted[0], output.accepted[1], output.accepted[2], output.accepted[3], output.accepted[4], output.accepted[5], output.accepted[6], output.accepted[7], output.accepted[8], output.accepted[9], output.accepted[10], output.accepted[11]]
                    },
                    {
                        label: 'Redirected',
                        backgroundColor: '#03586A',
                        borderColor: 'rgba(210, 214, 222, 1)',
                        pointRadius: false,
                        pointColor: 'rgba(210, 214, 222, 1)',
                        pointStrokeColor: '#c1c7d1',
                        pointHighlightFill: '#fff',
                        pointHighlightStroke: 'rgba(220,220,220,1)',
                        data: [output.redirected[0], output.redirected[1], output.redirected[2], output.redirected[3], output.redirected[4], output.redirected[5], output.redirected[6], output.redirected[7], output.redirected[8], output.redirected[9], output.redirected[10], output.redirected[11]]
                    },
                ]
            };
            var barChartCanvas = bar_chart.get(0).getContext('2d');
            var barChartData = jQuery.extend(true, {}, areaChartData);
            var temp0 = areaChartData.datasets[0];
            var temp1 = areaChartData.datasets[1];
            barChartData.datasets[0] = temp0;
            barChartData.datasets[1] = temp1;

            var barChartOptions = {
                responsive: true,
                maintainAspectRatio: false,
                datasetFill: false,
                beginAtZero: true,
                scales:
                {
                    xAxes:
                        [{
                            display: true
                        }],
                    yAxes:
                        [{
                            display: true,
                            ticks:
                            {
                                beginAtZero: true,
                                steps: 10,
                                stepValue: 5,
                                max: output.max
                            }
                        }],
                },
            };

            var barChart = new Chart(barChartCanvas, {
                type: 'bar',
                data: barChartData,
                options: barChartOptions
            });
        });
    }

    //-------------------- END -----------------------------------


});






//----------------------- FUNCTIONS --------------------------

function getDepartmentFiltered() {
    var urls = "/NoReload/FilterDepartment?facilityId=" + facilityId;
    return $.ajax({
        url: urls,
        type: 'get',
        async: true
    });
}



function getMdFiltered() {
    var urls = "/NoReload/FilterUser?facilityId=" + facilityId + "&departmentId=" + departmentId;
    return $.ajax({
        url: urls,
        type: 'get',
        async: true
    });
}

function GetDashboardValues() {
    var urlss = "/NoReload/DashboardValues?level=" + "doctor";
    return $.ajax({
        url: urlss, 
        type: 'get',
        async: true
    });
}

function SetLoginStatus(status) {
    var urlss = "/NoReload/ChangeLoginStatus?status=" + status;
    $.ajax({
        url: urlss,
        tpye: 'get',
        async: true
    });
}

function GetNumberNotif() {
    var urlss = "/NoReload/NumberNotif";
    return $.ajax({
        url: urlss,
        tpye: 'get',
        async: true
    })
}

function GetBarangayFiltered(id) {
    var urls = "/NoReload/FilteredBarangay?muncityId=" + id;
    return $.ajax({
        url: urls,
        type: 'get',
        async: true
    });
}


//----------------------- FUNCTIONS --------------------------
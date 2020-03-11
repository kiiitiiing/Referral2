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

    if (numberOfNotif.length > 0) {
        $.when(GetNumberNotif()).done(function (output) {
            numberOfNotif.text(output);
        });
    }

    var supReport = $('span.supReport');

    if (supReport.length > 0) {
        $.when(GetSupportReport()).done(function (output) {
            supReport.text("");
            supReport.text(output);
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

   

    //---------------------- MODALS ---------------------------------

    var placeholderElement = $('#modal-placeholders');

    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeholderElement.empty();
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');

            var formId = placeholderElement.find('.modal').attr('id');

            if (formId == 'reco-modal') {
                console.log('reco loaded');
                $('div.direct-chat-messages').animate({ scrollTop: $('#last-item').position().top }, 0);
            }
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


    placeholderElement.on('click', 'button[data-save="modal"]', function (event) {
        event.preventDefault();
        var form = placeholderElement.find('.modal').find('form');
        var formId = placeholderElement.find('.modal').attr('id');
        var actionUrl = form.attr('action');
        var dataToSend = form.serialize();
        console.log(actionUrl);
        $.post(actionUrl, dataToSend).done(function (data) {
            var newBody = $('.modal-body', data);
            placeholderElement.find('.modal-body').replaceWith(newBody);
            var validation = $('span.text-danger').text();
            $('.select2').select2({
                theme: 'bootstrap4'
            });
            if (formId == 'reco-modal') {
                console.log('chat sent');
                $('#chat').val('');
                $('div.direct-chat-messages').animate({ scrollTop: $('#last-item').position().top }, 0);
            }
            else if (validation == '') {
                placeholderElement.find('.modal').modal('hide');
                if (formId == 'update-doctor-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'updateDoctor');
                    location.reload();
                }
                else if (formId == 'change-password-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'passWordChanged');
                    location.reload();
                }
                else if (formId == 'reco-modal') {
                    var input = placeholderElement.find('.modal-footer').find('form').find('.code').val();
                    console.log(input);
                }
                else if (formId == 'switch-user-modal') {
                    location.href = '/Home/Index';
                }
                else if (formId == 'add-doctor-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'addDoctor');
                    location.reload();
                }
                else if (formId == 'add-support-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'addSupport');
                    location.reload();
                }
                else if (formId == 'update-support-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'updateSupport');
                    location.reload();
                }
                else if (formId == 'add-facility-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'addFacility');
                    location.reload();
                } 
                else if (formId == 'update-facility-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'updateFacility');
                    location.reload();
                }
                else if (formId == 'issues-modal') {
                    //placeholderElement.find('.modal').modal('hide');
                    sessionStorage.setItem('onReload', 'updatedIssues');
                    location.reload();
                }
            }
        });
    });

    //---------------------- MODALS ---------------------------------

    //-------------------- REMOVE FACILITY CONFIRMATION -------------------

    


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

    

    //-------------------- END -----------------------------------


});






//----------------------- FUNCTIONS --------------------------

function DashboardAPI(level) {

    var bar_chart = $('#barChart');

    if ($('div.chart').length > 0) {
        $.when(GetDashboardValues(level)).done(function (output) {
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
                        data: output.accepted
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
                        data: output.redirected
                    },
                ]
            };
            var barChartCanvas = bar_chart.get(0).getContext('2d');
            var barChartData = jQuery.extend(true, {}, areaChartData);
            var temp0 = areaChartData.datasets[0];
            var temp1 = areaChartData.datasets[1];
            barChartData.datasets[0] = temp0;
            barChartData.datasets[1] = temp1;

            var barChart = new Chart(barChartCanvas, {
                type: 'bar',
                data: barChartData
            });
        });
    }
}

function Test() {
    console.log('wat');
}

function getDepartmentFiltered() {
    var urls = "/NoReload/FilterDepartment?facilityId=" + facilityId;
    return $.ajax({
        url: urls,
        type: 'get',
        async: true
    });
}

function ChangePasswordSuccess(type,message) {
    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "1000",
        "hideDuration": "1500",
        "timeOut": "1500",
        "extendedTimeOut": "1500",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
    Command: toastr[type](message)
}



function getMdFiltered() {
    var urls = "/NoReload/FilterUser?facilityId=" + facilityId + "&departmentId=" + departmentId;
    return $.ajax({
        url: urls,
        type: 'get',
        async: true
    });
}

function GetDashboardValues(level) {
    var urlss = "/NoReload/DashboardValues?level=" + level;
    return $.ajax({
        url: urlss, 
        type: 'get',
        async: true
    });
}

function GetAvailableDepartments(id) {
    var url = "/NoReload/AvailableDepartments?facilityId=" + id;
    return $.ajax({
        url: url,
        type: 'get',
        async: true
    });
}

function RemoveFacility(id) {
    var url = "/Admin/RemoveFacility?id=" + id;
    $.ajax({
        url: url,
        tpye: 'get',
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

function CallRequest(code) {
    var urlss = "/Remarks/RequestCall?code=" + code;
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
    });
}

function GetSupportReport() {
    var url = "/NoReload/SupportReport";
    return $.ajax({
        url: url,
        type: 'get',
        async: true
    });
}

function UpdateRecoCount(code) {
    var url = "/NoReload/RecoCount?code=" + code;
    $.ajax({
        url: url,
        type: 'get',
        async: true,
        success: function (output) {
            $('#reco_' + code).html(output);
        }
    });
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
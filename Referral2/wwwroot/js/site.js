// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {

    var pregnantIdBtn = $('.pregId');


    pregnantIdBtn.on('click', function () {
        console.log('hellos');
    })

    var placeholderElement = $('#modal-placeholder');


    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
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

    var testDiv = $('#test_div');

    var testButton = $('#test_button')

    testButton.click(function () {
        testDiv.html('hello');
    });

    //-------------------- DASHBOARD ----------------------------

    var bar_chart = $('#barChart');

    if ($('div.chart').length > 0) {
        $.when(GetDashboardValues()).done(function (output) {
            console.log(output);
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
            $('#notificationModal').modal('show');
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

function GetDashboardValues() {
    var urlss = "/NoReload/DashboardValues?level=" + "doctor";
    return $.ajax({
        url: urlss,
        type: 'get',
        async: true
    });
}

//----------------------- FUNCTIONS --------------------------
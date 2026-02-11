$(function () {
    var palette = [
        '#2e7d32', '#1565c0', '#c62828', '#e65100', '#6a1b9a',
        '#00838f', '#4e342e', '#ad1457', '#283593', '#558b2f',
        '#d84315', '#1a237e', '#004d40', '#bf360c', '#311b92'
    ];

    var azMonths = ['Yan', 'Fev', 'Mar', 'Apr', 'May', 'İyn', 'İyl', 'Avq', 'Sen', 'Okt', 'Noy', 'Dek'];

    var rangePicker = flatpickr('#dashDateRange', {
        mode: 'range',
        dateFormat: 'Y-m-d',
        altInput: true,
        altFormat: 'd.m.Y',
        locale: 'az',
        allowInput: true
    });

    var charts = {};

    function getDateParams() {
        var params = {};
        if (rangePicker.selectedDates.length) {
            params.from = rangePicker.formatDate(rangePicker.selectedDates[0], 'Y-m-d');
            if (rangePicker.selectedDates.length > 1)
                params.to = rangePicker.formatDate(rangePicker.selectedDates[1], 'Y-m-d');
        }
        return params;
    }

    function downloadChart(canvasId) {
        var canvas = document.getElementById(canvasId);
        if (!canvas) return;
        var tmpCanvas = document.createElement('canvas');
        tmpCanvas.width = canvas.width;
        tmpCanvas.height = canvas.height;
        var ctx = tmpCanvas.getContext('2d');
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, tmpCanvas.width, tmpCanvas.height);
        ctx.drawImage(canvas, 0, 0);
        var link = document.createElement('a');
        link.download = canvasId + '.png';
        link.href = tmpCanvas.toDataURL('image/png');
        link.click();
    }

    $(document).on('click', '.download-btn', function () {
        var canvasId = $(this).data('canvas');
        if (canvasId) downloadChart(canvasId);
    });

    function destroyChart(key) {
        if (charts[key]) { charts[key].destroy(); charts[key] = null; }
    }

    function hBarChart(canvasId, key, labels, counts, extraOpts) {
        destroyChart(key);
        var opts = {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            layout: { padding: { right: 40 } },
            plugins: {
                legend: { display: false },
                datalabels: {
                    anchor: 'end',
                    align: 'right',
                    clip: false,
                    font: { size: 11, weight: '600' },
                    color: '#333'
                }
            },
            scales: {
                x: { display: false, beginAtZero: true },
                y: { ticks: { font: { size: 11 } }, grid: { display: false } }
            }
        };
        if (extraOpts) $.extend(true, opts, extraOpts);

        charts[key] = new Chart(document.getElementById(canvasId), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    data: counts,
                    backgroundColor: palette.slice(0, labels.length),
                    borderRadius: 4,
                    barThickness: labels.length > 10 ? 14 : 18
                }]
            },
            options: opts,
            plugins: [ChartDataLabels]
        });
    }

    function formatMonthLabel(raw) {
        var parts = raw.split('-');
        return azMonths[parseInt(parts[1], 10) - 1] + ' ' + parts[0];
    }

    function loadDashboard() {
        var params = getDateParams();
        var qs = $.param(params);
        var url = '/Home/GetDashboardData' + (qs ? '?' + qs : '');

        $.getJSON(url, function (d) {
            $('#statTotal').text(d.totalCount.toLocaleString());
            $('#statTopSecret').text(d.topSecretCount.toLocaleString());
            $('#statSecret').text(d.secretCount.toLocaleString());
            $('#statThisMonth').text(d.thisMonthCount.toLocaleString());

            renderByBase(d.byBase);
            renderTrend(d.monthlyTrend);
            renderByRank(d.byRank);
            renderByExecutor(d.byExecutor);
            renderPrivacy(d.topSecretCount, d.secretCount);
            renderStatus(d.statusCounts);
        });

        $('#baseFilterSelect, #rankFilterSelect').select2({
            width: '200px',
            placeholder: 'Seçin',
            allowClear: true,
            closeOnSelect: false,
            language: { noResults: function () { return 'Nəticə tapılmadı'; } }
        }).each(function () {
            if (!$(this).val() || !$(this).val().length) {
                $(this).val(null).trigger('change');
            }
        });

        var baseId = $('#baseFilterSelect').val();
        if (baseId) loadBaseDetail(baseId);

        var rankId = $('#rankFilterSelect').val();
        if (rankId) loadRankDetail(rankId);
    }

    function renderByBase(items) {
        hBarChart('chartByBase', 'byBase',
            items.map(function (x) { return x.label; }),
            items.map(function (x) { return x.count; })
        );
    }

    function renderByRank(items) {
        hBarChart('chartByRank', 'byRank',
            items.map(function (x) { return x.label; }),
            items.map(function (x) { return x.count; })
        );
    }

    function renderByExecutor(items) {
        hBarChart('chartByExecutor', 'byExecutor',
            items.map(function (x) { return x.label; }),
            items.map(function (x) { return x.count; })
        );
    }

    function renderTrend(items) {
        var labels = items.map(function (x) { return formatMonthLabel(x.label); });
        var counts = items.map(function (x) { return x.count; });

        destroyChart('trend');

        charts['trend'] = new Chart(document.getElementById('chartTrend'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    type: 'line',
                    data: counts,
                    borderColor: '#1565c0',
                    backgroundColor: 'transparent',
                    tension: 0.3,
                    pointRadius: 5,
                    pointBackgroundColor: '#1565c0',
                    order: 0
                }, {
                    type: 'bar',
                    data: counts,
                    backgroundColor: 'rgba(21, 101, 192, 0.15)',
                    borderRadius: 4,
                    barThickness: counts.length > 12 ? 20 : 32,
                    order: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    datalabels: {
                        display: function (ctx) { return ctx.datasetIndex === 0; },
                        anchor: 'end',
                        align: 'top',
                        offset: 4,
                        font: { size: 11, weight: '600' },
                        color: '#1565c0'
                    }
                },
                scales: {
                    x: {
                        ticks: { font: { size: 10 }, maxRotation: 45 },
                        grid: { display: false }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { font: { size: 11 }, precision: 0 },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    }
                }
            },
            plugins: [ChartDataLabels]
        });
    }

    function renderPrivacy(topSecret, secret) {
        destroyChart('privacy');

        charts['privacy'] = new Chart(document.getElementById('chartPrivacy'), {
            type: 'doughnut',
            data: {
                labels: ['Tam məxfi', 'Məxfi'],
                datasets: [{
                    data: [topSecret, secret],
                    backgroundColor: ['#c62828', '#e65100'],
                    borderWidth: 0,
                    spacing: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '60%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { font: { size: 12 }, padding: 16 }
                    },
                    datalabels: {
                        font: { size: 16, weight: '700' },
                        color: '#fff',
                        formatter: function (val) { return val || ''; }
                    }
                }
            },
            plugins: [ChartDataLabels]
        });
    }

    function renderStatus(s) {
        destroyChart('status');

        var labels = [
            'Ümumi qəbul',
            'DTX-a göndərilən',
            'Rəsmiləşdirilən',
            'İmtina edilən',
            'Geri qaytarılan'
        ];
        var counts = [s.total, s.sentToDtx, s.formalized, s.rejected, s.sentBack];
        var colors = ['#1565c0', '#6a1b9a', '#2e7d32', '#c62828', '#e65100'];

        charts['status'] = new Chart(document.getElementById('chartStatus'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    data: counts,
                    backgroundColor: colors,
                    borderRadius: 6,
                    barThickness: 36
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                layout: { padding: { top: 20 } },
                plugins: {
                    legend: { display: false },
                    datalabels: {
                        anchor: 'end',
                        align: 'top',
                        offset: 2,
                        font: { size: 13, weight: '700' },
                        color: '#333'
                    }
                },
                scales: {
                    x: { ticks: { font: { size: 11 } }, grid: { display: false } },
                    y: {
                        beginAtZero: true,
                        ticks: { font: { size: 11 }, precision: 0 },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    }
                }
            },
            plugins: [ChartDataLabels]
        });
    }

    function loadBaseDetail(baseId) {
        if (!baseId) { destroyChart('baseDetail'); return; }
        var params = getDateParams();
        params.id = baseId;
        $.getJSON('/Home/GetBaseBreakdown?' + $.param(params), function (d) {
            renderDetailChart('chartBaseDetail', 'baseDetail', d);
        });
    }

    function loadRankDetail(rankId) {
        if (!rankId) { destroyChart('rankDetail'); return; }
        var params = getDateParams();
        params.id = rankId;
        $.getJSON('/Home/GetRankBreakdown?' + $.param(params), function (d) {
            renderDetailChart('chartRankDetail', 'rankDetail', d);
        });
    }

    function renderDetailChart(canvasId, key, d) {
        destroyChart(key);

        var trendLabels = d.trend.map(function (x) { return formatMonthLabel(x.label); });
        var trendCounts = d.trend.map(function (x) { return x.count; });
        var breakdownLabels = d.breakdown.map(function (x) { return x.label; });
        var breakdownCounts = d.breakdown.map(function (x) { return x.count; });

        if (!trendLabels.length && !breakdownLabels.length) return;

        var allLabels = breakdownLabels.length ? breakdownLabels : trendLabels;
        var allCounts = breakdownLabels.length ? breakdownCounts : trendCounts;

        if (breakdownLabels.length) {
            hBarChart(canvasId, key, allLabels, allCounts);
        } else {
            charts[key] = new Chart(document.getElementById(canvasId), {
                type: 'bar',
                data: {
                    labels: allLabels,
                    datasets: [{
                        data: allCounts,
                        backgroundColor: 'rgba(21, 101, 192, 0.2)',
                        borderRadius: 4,
                        barThickness: allCounts.length > 12 ? 20 : 32
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    layout: { padding: { top: 20 } },
                    plugins: {
                        legend: { display: false },
                        datalabels: {
                            anchor: 'end', align: 'top', offset: 2,
                            font: { size: 11, weight: '600' }, color: '#1565c0'
                        }
                    },
                    scales: {
                        x: { ticks: { font: { size: 10 }, maxRotation: 45 }, grid: { display: false } },
                        y: { beginAtZero: true, ticks: { precision: 0 }, grid: { color: 'rgba(0,0,0,0.05)' } }
                    }
                },
                plugins: [ChartDataLabels]
            });
        }
    }

    $('#baseFilterSelect').on('change', function () { loadBaseDetail($(this).val()); });
    $('#rankFilterSelect').on('change', function () { loadRankDetail($(this).val()); });

    $('#dashApply').on('click', loadDashboard);

    $('#dashReset').on('click', function () {
        rangePicker.clear();
        loadDashboard();
    });

    loadDashboard();
});
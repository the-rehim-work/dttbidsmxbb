window.dttbidsmxbb = window.dttbidsmxbb || {};

window.dttbidsmxbb.initLogsTable = function (config) {
    var token = $('input[name="__RequestVerificationToken"]').val();

    return $('#logsTable').DataTable({
        processing: true,
        serverSide: true,
        paging: true,
        scrollX: false,
        autoWidth: false,
        pageLength: 50,
        lengthMenu: [[25, 50, 100, 200], [25, 50, 100, 200]],
        dom: '<"row mb-3"<"col-sm-12 col-md-3"l><"col-sm-12 col-md-9"f>>rt<"row mt-3"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        language: {
            search: '',
            searchPlaceholder: 'Axtar...',
            lengthMenu: '_MENU_ sətir',
            info: '_START_-_END_ / _TOTAL_',
            infoEmpty: '0 nəticə',
            infoFiltered: '(ümumi: _MAX_)',
            zeroRecords: 'Heç bir nəticə tapılmadı',
            paginate: { previous: '‹', next: '›' }
        },
        ajax: {
            url: config.url,
            type: 'POST',
            beforeSend: function (xhr) {
                if (token) xhr.setRequestHeader('RequestVerificationToken', token);
            },
            error: function (xhr) {
                console.error('DataTable error:', xhr.status, xhr.responseText);
            }
        },
        order: config.defaultOrder,
        columns: config.columns,
        initComplete: function () {
            $('#tableLoader').hide();
        }
    });
};

window.dttbidsmxbb.formatTimestamp = function (timestamp) {
    if (!timestamp) return '—';
    var date = new Date(timestamp);
    if (isNaN(date)) return timestamp;
    var day = String(date.getDate()).padStart(2, '0');
    var month = String(date.getMonth() + 1).padStart(2, '0');
    var year = date.getFullYear();
    var hours = String(date.getHours()).padStart(2, '0');
    var minutes = String(date.getMinutes()).padStart(2, '0');
    return day + '.' + month + '.' + year + ' ' + hours + ':' + minutes;
};

window.dttbidsmxbb.formatJson = function (value) {
    if (!value) return '(yoxdur)';
    try {
        var parsed = typeof value === 'string' ? JSON.parse(value) : value;
        return JSON.stringify(parsed, null, 2);
    } catch (e) {
        return String(value);
    }
};
window.dttbidsmxbb = window.dttbidsmxbb || {};

window.dttbidsmxbb.initLogsTable = function (config) {
    const defaults = {
        processing: false,
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
            paginate: {
                previous: '‹',
                next: '›'
            }
        },
        ajax: {
            url: config.url,
            type: 'POST',
            beforeSend: function (xhr) {
                const token = window.dttbidsmxbb.getToken();
                if (token) xhr.setRequestHeader('RequestVerificationToken', token);
            },
            error: function (xhr) {
                console.error('DataTable error:', xhr);
            }
        },
        order: config.defaultOrder || [[config.columns.length - 1, 'desc']],
        columns: config.columns,
        initComplete: function () {
            $('#tableLoader').hide();
            $('#logsTable').show();
            if (config.initComplete) {
                config.initComplete.call(this);
            }
        }
    };

    return $('#logsTable').DataTable(defaults);
};

window.dttbidsmxbb.formatTimestamp = function (timestamp) {
    if (!timestamp) return '—';
    const date = new Date(timestamp);
    if (isNaN(date)) return timestamp;

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return `${day}.${month}.${year} ${hours}:${minutes}`;
};

window.dttbidsmxbb.formatJson = function (value) {
    if (!value) return '(yoxdur)';

    try {
        const parsed = typeof value === 'string' ? JSON.parse(value) : value;
        return JSON.stringify(parsed, null, 2);
    } catch (e) {
        return String(value);
    }
};
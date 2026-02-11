$(function () {
    if (typeof logConfig === 'undefined') return;

    var token = window.dttbidsmxbb.getToken();

    function fmtTimestamp(d) {
        if (!d) return '';
        var dt = new Date(d);
        if (isNaN(dt)) return d;
        var dd = String(dt.getDate()).padStart(2, '0');
        var mm = String(dt.getMonth() + 1).padStart(2, '0');
        var yy = dt.getFullYear();
        var hh = String(dt.getHours()).padStart(2, '0');
        var mi = String(dt.getMinutes()).padStart(2, '0');
        var ss = String(dt.getSeconds()).padStart(2, '0');
        return dd + '.' + mm + '.' + yy + ' ' + hh + ':' + mi + ':' + ss;
    }

    var columnDefs = {
        audit: [
            { data: 'userFullName', width: '150px' },
            { data: 'action', width: '120px' },
            { data: 'entityName', width: '120px' },
            { data: 'entityId', width: '80px' },
            { data: 'timestamp', width: '160px', render: fmtTimestamp },
            {
                data: null,
                orderable: false,
                width: '70px',
                render: function (data) {
                    if (!data.oldValues && !data.newValues) return '-';
                    return '<button class="btn btn-info btn-sm text-white detail-btn" data-row-id="' + data.id + '">Bax</button>';
                }
            }
        ],
        auth: [
            { data: 'username', width: '200px' },
            {
                data: 'success',
                width: '100px',
                render: function (d) {
                    return d
                        ? '<span class="badge bg-success">Uğurlu</span>'
                        : '<span class="badge bg-danger">Uğursuz</span>';
                }
            },
            { data: 'ipAddress', width: '150px' },
            { data: 'timestamp', width: '160px', render: fmtTimestamp }
        ],
        event: [
            { data: 'userFullName', defaultContent: '-', width: '150px' },
            { data: 'method', width: '80px' },
            { data: 'path', width: '300px' },
            { data: 'statusCode', width: '80px' },
            { data: 'timestamp', width: '160px', render: fmtTimestamp }
        ]
    };

    var columns = columnDefs[logConfig.type];
    if (!columns) return;

    var sortCol = columns.length - (logConfig.type === 'audit' ? 2 : 1);

    var table = $('#logsTable').DataTable({
        processing: true,
        serverSide: true,
        paging: true,
        scrollX: true,
        autoWidth: false,
        order: [[sortCol, 'desc']],
        pageLength: 25,
        language: {
            search: 'Axtar:',
            lengthMenu: '_MENU_ sətir',
            info: '_TOTAL_ nəticədən _START_ - _END_',
            infoEmpty: 'Nəticə yoxdur',
            infoFiltered: '(ümumi _MAX_)',
            zeroRecords: 'Nəticə tapılmadı',
            processing: 'Yüklənir...',
            paginate: { previous: '‹', next: '›' }
        },
        ajax: {
            url: logConfig.url,
            type: 'POST',
            beforeSend: function (xhr) {
                if (token) xhr.setRequestHeader('RequestVerificationToken', token);
            }
        },
        columns: columns,
        initComplete: function () { $('#tableLoader').fadeOut(150); }
    });

    if (logConfig.type === 'audit') {
        $('#logsTable').on('click', '.detail-btn', function () {
            var rowData = table.row($(this).closest('tr')).data();
            if (!rowData) return;
            $('#oldValues').text(formatJson(rowData.oldValues));
            $('#newValues').text(formatJson(rowData.newValues));
            new bootstrap.Modal('#detailModal').show();
        });
    }

    function formatJson(val) {
        if (!val) return '(yoxdur)';
        if (typeof val === 'object') {
            try { return JSON.stringify(val, null, 2); }
            catch (e) { return String(val); }
        }
        try { return JSON.stringify(JSON.parse(val), null, 2); }
        catch (e) { return val; }
    }
});
$(function () {
    var token = $('input[name="__RequestVerificationToken"]').val();

    var dateRangeMap = {
        f_sentDateRange: { from: 'f_sentDateFrom', to: 'f_sentDateTo' },
        f_receivedDateRange: { from: 'f_receivedDateFrom', to: 'f_receivedDateTo' },
        f_assignmentDateRange: { from: 'f_assignmentDateFrom', to: 'f_assignmentDateTo' },
        f_formalizationSentDateRange: { from: 'f_sendAwayDateFrom', to: 'f_sendAwayDateTo' },
        f_formalizationDateRange: { from: 'f_formalizationDateFrom', to: 'f_formalizationDateTo' }
    };

    var filterPickers = {};
    Object.keys(dateRangeMap).forEach(function (id) {
        var el = document.getElementById(id);
        if (!el) return;
        filterPickers[id] = flatpickr(el, {
            mode: 'range',
            dateFormat: 'Y-m-d',
            altInput: true,
            altFormat: 'd.m.Y',
            locale: 'az',
            allowInput: false
        });
    });

    var nullableDateIds = ['f_formalizationSentDateRange', 'f_formalizationDateRange'];

    $('#filterPanel .filter-select2').select2({
        width: '100%',
        placeholder: 'Hamısı',
        allowClear: true,
        closeOnSelect: false,
        language: { noResults: function () { return 'Nəticə tapılmadı'; } }
    }).each(function () {
        if (!$(this).val() || !$(this).val().length) {
            $(this).val(null).trigger('change');
        }
    });

    var multiSelectIds = [
        'f_senderMilitaryBaseIds', 'f_militaryBaseIds',
        'f_militaryRankIds', 'f_executorIds', 'f_privacyLevels'
    ];

    var textInputIds = [
        'f_sentSerialNumberQuery', 'f_receivedSerialNumberQuery',
        'f_regardingPositionQuery', 'f_positionQuery', 'f_firstnameQuery'
    ];

    var nullPairs = [
        { query: 'f_lastnameQuery', nullSel: 'f_lastnameNull' },
        { query: 'f_fathernameQuery', nullSel: 'f_fathernameNull' },
        { query: 'f_formalizationSentSerialQuery', nullSel: 'f_formalizationSentSerialNull' },
        { query: 'f_formalizationSerialQuery', nullSel: 'f_formalizationSerialNull' },
        { query: 'f_rejectionInfoQuery', nullSel: 'f_rejectionInfoNull' },
        { query: 'f_sentBackInfoQuery', nullSel: 'f_sentBackInfoNull' },
        { query: 'f_noteQuery', nullSel: 'f_noteNull' }
    ];

    var nullDatePairs = [
        { picker: 'f_formalizationSentDateRange', nullSel: 'f_formalizationSentDateNull' },
        { picker: 'f_formalizationDateRange', nullSel: 'f_formalizationDateNull' }
    ];

    function getFilterData() {
        var data = {};

        multiSelectIds.forEach(function (id) {
            var vals = $('#' + id).val();
            if (vals && vals.length) data[id] = vals.join(',');
        });

        Object.keys(dateRangeMap).forEach(function (id) {
            var isNullable = nullableDateIds.indexOf(id) !== -1;
            if (isNullable) {
                var pair = nullDatePairs.find(function (p) { return p.picker === id; });
                if (pair) {
                    var nullVal = $('#' + pair.nullSel).val();
                    if (nullVal) {
                        data[pair.nullSel] = nullVal;
                        return;
                    }
                }
            }
            var fp = filterPickers[id];
            if (fp && fp.selectedDates.length) {
                var map = dateRangeMap[id];
                data[map.from] = fp.formatDate(fp.selectedDates[0], 'Y-m-d');
                if (fp.selectedDates.length > 1) {
                    data[map.to] = fp.formatDate(fp.selectedDates[1], 'Y-m-d');
                }
            }
        });

        textInputIds.forEach(function (id) {
            var val = $.trim($('#' + id).val());
            if (val) data[id] = val;
        });

        nullPairs.forEach(function (pair) {
            var nullVal = $('#' + pair.nullSel).val();
            if (nullVal) {
                data[pair.nullSel] = nullVal;
            } else {
                var el = $('#' + pair.query);
                var val = $.trim(el.val());
                if (val) data[pair.query] = val;
            }
        });

        return data;
    }

    function countActiveFilters() {
        var count = Object.keys(getFilterData()).length;
        var badge = $('#activeFilterCount');
        if (count > 0) {
            badge.text(count).removeClass('d-none');
        } else {
            badge.addClass('d-none');
        }
    }

    var table = $('#informationsTable').DataTable({
        processing: true,
        serverSide: true,
        searching: false,
        paging: true,
        scrollX: true,
        autoWidth: false,
        dom: 'rtip',
        ajax: {
            url: '/Informations/Load',
            type: 'POST',
            data: function (d) {
                d.showDeleted = $('#showDeletedToggle').is(':checked');
                var filters = getFilterData();
                for (var key in filters) {
                    d[key] = filters[key];
                }
            },
            beforeSend: function (xhr) {
                if (token) xhr.setRequestHeader('RequestVerificationToken', token);
            }
        },
        order: [[5, 'desc']],
        pageLength: 25,
        lengthMenu: [10, 25, 50, 100],
        fixedColumns: { start: 0, end: 1 },
        columns: [
            { data: 'senderMilitaryBase', render: function (d) { return d ? d.name : ''; } },
            { data: 'militaryBase', render: function (d) { return d ? d.name : ''; } },
            { data: 'sentSerialNumber' },
            { data: 'sentDate' },
            { data: 'receivedSerialNumber' },
            { data: 'receivedDate' },
            { data: 'militaryRank', render: function (d) { return d ? d.name : ''; } },
            { data: 'regardingPosition' },
            { data: 'position' },
            { data: 'lastname', defaultContent: '' },
            { data: 'firstname' },
            { data: 'fathername', defaultContent: '' },
            { data: 'assignmentDate' },
            { data: 'privacyLevel', render: function (d) { return d === 1 ? 'Tam məxfi' : 'Məxfi'; } },
            { data: 'sendAwaySerialNumber', defaultContent: '' },
            { data: 'sendAwayDate', defaultContent: '' },
            { data: 'executor', render: function (d) { return d ? d.fullInfo : ''; } },
            { data: 'formalizationSerialNumber', defaultContent: '' },
            { data: 'formalizationDate', defaultContent: '' },
            { data: 'rejectionInfo', defaultContent: '' },
            { data: 'sentBackInfo', defaultContent: '' },
            { data: 'note', defaultContent: '' },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    if (data.deletedAt) {
                        return '<button class="btn btn-success btn-sm restore-btn" data-id="' + data.id + '">Bərpa</button>';
                    }
                    return '<a href="/Informations/Edit/' + data.id + '" class="btn btn-warning btn-sm">Redaktə</a> ' +
                        '<button class="btn btn-danger btn-sm delete-btn" data-id="' + data.id + '">Sil</button>';
                }
            }
        ],
        createdRow: function (row, data) {
            if (data.deletedAt) {
                $(row).addClass('table-danger').css('opacity', '0.6');
            }
        },
        initComplete: function () { $('#tableLoader').fadeOut(150); }
    });

    $('#showDeletedToggle').on('change', function () {
        table.ajax.reload();
    });

    $('#applyFilters').on('click', function () {
        countActiveFilters();
        table.ajax.reload();
    });

    $('#resetFilters').on('click', function () {
        $('#filterPanel .filter-select2').val(null).trigger('change');

        $('#filterPanel .null-filter-select').val('');

        $('#filterPanel .text-filter-input').val('');
        $('#filterPanel textarea').val('');

        Object.values(filterPickers).forEach(function (fp) { fp.clear(); });

        countActiveFilters();
        table.ajax.reload();
    });

    var deleteId = null;
    var deleteModal = new bootstrap.Modal('#deleteConfirmModal');

    $('#informationsTable').on('click', '.delete-btn', function () {
        deleteId = $(this).data('id');
        deleteModal.show();
    });

    $('#confirmDeleteBtn').on('click', function () {
        if (!deleteId) return;
        $.ajax({
            url: '/Informations/Delete',
            type: 'POST',
            data: { id: deleteId, __RequestVerificationToken: token },
            success: function (res) {
                deleteModal.hide();
                deleteId = null;
                if (res.success) {
                    if (window.dttbidsmxbb && window.dttbidsmxbb.toast)
                        window.dttbidsmxbb.toast('success', res.message);
                    table.ajax.reload(null, false);
                } else {
                    if (window.dttbidsmxbb && window.dttbidsmxbb.toast)
                        window.dttbidsmxbb.toast('error', res.message);
                    else alert(res.message);
                }
            },
            error: function () {
                deleteModal.hide();
                alert('Xəta baş verdi');
            }
        });
    });

    $('#informationsTable').on('click', '.restore-btn', function () {
        var id = $(this).data('id');
        if (!confirm('Bu məlumatı bərpa etmək istəyirsiniz?')) return;
        $.ajax({
            url: '/Informations/Restore',
            type: 'POST',
            data: { id: id, __RequestVerificationToken: token },
            success: function (res) {
                if (res.success) {
                    if (window.dttbidsmxbb && window.dttbidsmxbb.toast)
                        window.dttbidsmxbb.toast('success', res.message);
                    table.ajax.reload(null, false);
                } else {
                    if (window.dttbidsmxbb && window.dttbidsmxbb.toast)
                        window.dttbidsmxbb.toast('error', res.message);
                    else alert(res.message);
                }
            }
        });
    });

    // TODO: Import logic — will be wired separately
});
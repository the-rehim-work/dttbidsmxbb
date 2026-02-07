$(function () {
    var token = $('input[name="__RequestVerificationToken"]').val() ||
        $('form input[name="__RequestVerificationToken"]').val();

    var filterPickers = {};
    $('.flatpickr-filter').each(function () {
        filterPickers[this.id] = flatpickr(this, {
            dateFormat: 'Y-m-d',
            altInput: true,
            altFormat: 'd.m.Y',
            locale: 'az',
            allowInput: true
        });
    });

    function initMultiSelect2(selector = '.filter-select2') {
        $(selector).select2({
            width: '100%',
            placeholder: 'Hamısı',
            allowClear: true,
            closeOnSelect: false,
            language: {
                noResults: () => 'Nəticə tapılmadı'
            }
        });

        $(selector).each(function () {
            if (!$(this).val() || !$(this).val().length) {
                $(this).val(null).trigger('change');
            }
        });
    }
    initMultiSelect2();

    function getFilterData() {
        var data = {};

        const multiSelects = [
            'f_militaryBaseIds', 'f_senderMilitaryBaseIds',
            'f_militaryRankIds', 'f_executorIds', 'f_privacyLevels'
        ];

        multiSelects.forEach(function (id) {
            var vals = $('#' + id).val();
            if (vals && vals.length) data[id] = vals.join(',');
        });

        var dateFields = [
            'f_sentDateFrom', 'f_sentDateTo',
            'f_receivedDateFrom', 'f_receivedDateTo',
            'f_assignmentDateFrom', 'f_assignmentDateTo',
            'f_sendAwayDateFrom', 'f_sendAwayDateTo',
            'f_formalizationDateFrom', 'f_formalizationDateTo'
        ];
        dateFields.forEach(function (id) {
            var fp = filterPickers[id];
            if (fp && fp.selectedDates.length)
                data[id] = fp.formatDate(fp.selectedDates[0], 'Y-m-d');
        });

        var nullFields = [
            'f_rejectionInfoNull', 'f_sentBackInfoNull',
            'f_noteNull', 'f_lastnameNull', 'f_fathernameNull'
        ];
        nullFields.forEach(function (id) {
            var val = $('#' + id).val();
            if (val) data[id] = val;
        });

        return data;
    }

    function countActiveFilters() {
        var count = 0;
        var fd = getFilterData();
        count = Object.keys(fd).length;
        var badge = $('#activeFilterCount');
        if (count > 0) {
            badge.text(count).removeClass('d-none');
        } else {
            badge.addClass('d-none');
        }
        return count;
    }

    var table = $('#informationsTable').DataTable({
        processing: true,
        serverSide: true,
        searching: false,
        paging: true,
        scrollX: true,
        autoWidth: false,
        dom: 'Brtip',
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
        fixedColumns: {
            start: 0,
            end: 1
        },
        language: {
            processing: 'Yüklənir...',
            lengthMenu: '_MENU_ sətir göstər',
            zeroRecords: 'Nəticə tapılmadı',
            info: '_TOTAL_ nəticədən _START_ - _END_ göstərilir',
            infoEmpty: 'Nəticə yoxdur',
            infoFiltered: '(cəmi _MAX_ nəticədən)',
            search: 'Axtar:',
            paginate: { first: 'İlk', last: 'Son', next: '›', previous: '‹' }
        },
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
            { data: 'sendAwaySerialNumber' },
            { data: 'sendAwayDate' },
            { data: 'executor', render: function (d) { return d ? d.fullInfo : ''; } },
            { data: 'formalizationSerialNumber' },
            { data: 'formalizationDate' },
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
        initComplete: () => $('#tableLoader').fadeOut(150)
    });

    $('#showDeletedToggle').on('change', function () {
        table.ajax.reload();
    });

    $('#applyFilters').on('click', function () {
        countActiveFilters();
        table.ajax.reload();
    });

    $('#resetFilters').on('click', function () {
        $('select[multiple]', '#filterPanel').each(function () {
            $(this).val([]);
        });
        $('select:not([multiple])', '#filterPanel').each(function () {
            $(this).val('');
        });
        Object.values(filterPickers).forEach(function (fp) {
            fp.clear();
        });
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

    var importModal = new bootstrap.Modal('#importModal');

    $('#importBtn').on('click', function () {
        var fileInput = document.getElementById('importFile');
        if (!fileInput.files.length) {
            alert('Fayl seçin');
            return;
        }

        var formData = new FormData();
        formData.append('file', fileInput.files[0]);
        formData.append('useAsDb', $('#useAsDbCheck').is(':checked'));
        formData.append('__RequestVerificationToken', token);

        var btn = $(this);
        btn.prop('disabled', true).text('Yüklənir...');

        $.ajax({
            url: '/Import/Upload',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                var resultDiv = $('#importResult').removeClass('d-none');
                if (res.success) {
                    resultDiv.html('<div class="alert alert-success">' + res.message + '</div>');
                    table.ajax.reload();
                } else {
                    var html = '<div class="alert alert-warning">' + res.message + '</div>';
                    if (res.errors && res.errors.length) {
                        html += '<div class="table-responsive" style="max-height:200px; overflow:auto;">';
                        html += '<table class="table table-sm table-bordered"><thead><tr><th>Sətir</th><th>Sahə</th><th>Xəta</th></tr></thead><tbody>';
                        res.errors.forEach(function (e) {
                            html += '<tr><td>' + e.row + '</td><td>' + e.field + '</td><td>' + e.message + '</td></tr>';
                        });
                        html += '</tbody></table></div>';
                    }
                    resultDiv.html(html);
                    if (res.importedRows > 0) table.ajax.reload();
                }
            },
            error: function () {
                $('#importResult').removeClass('d-none').html('<div class="alert alert-danger">Xəta baş verdi</div>');
            },
            complete: function () {
                btn.prop('disabled', false).text('İdxal et');
            }
        });
    });

    $('#importModal').on('hidden.bs.modal', function () {
        $('#importFile').val('');
        $('#useAsDbCheck').prop('checked', false);
        $('#importResult').addClass('d-none').html('');
    });
});
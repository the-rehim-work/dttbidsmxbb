$(function () {
    var token = $('input[name="__RequestVerificationToken"]').val();

    var columnNames = [
        'Göndərən hərbi hissə', 'Hərbi hissə', 'Göndərilmə №', 'Göndərilmə tarixi',
        'Daxil olma №', 'Daxil olma tarixi', 'Rütbə', 'Rəsmiləşdirildiyi vəzifə',
        'Vəzifə', 'Soyad', 'Ad', 'Ata adı', 'Təyin olunma tarixi',
        'Buraxılış forması', 'DTX-a göndərilmə №', 'DTX-a göndərilmə tarixi',
        'İcraçı', 'Vərəqə №', 'Vərəqə tarixi', 'İmtina', 'Geri qaytarılma', 'Qeyd'
    ];

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
            dateFormat: 'd-m-Y',
            altInput: true,
            altFormat: 'd.m.Y',
            locale: 'az',
            allowInput: true
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

    function getVisibleColumnIndices() {
        var indices = [];
        for (var i = 0; i < 22; i++) {
            if (table.column(i).visible()) indices.push(i);
        }
        return indices;
    }

    function fmtDate(d) {
        if (!d) return '';
        var parts = d.split('-');
        if (parts.length === 3) return parts[2] + '.' + parts[1] + '.' + parts[0];
        return d;
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
        fixedColumns: {
            start: 0,
            end: 1
        },
        columns: [
            { data: 'senderMilitaryBase', width: '150px', render: function (d) { return d ? d.name : ''; } },
            { data: 'militaryBase', width: '150px', render: function (d) { return d ? d.name : ''; } },
            { data: 'sentSerialNumber', width: '100px' },
            { data: 'sentDate', width: '150px', render: fmtDate },
            { data: 'receivedSerialNumber', width: '100px' },
            { data: 'receivedDate', width: '150px', render: fmtDate },
            { data: 'militaryRank', width: '150px', render: function (d) { return d ? d.name : ''; } },
            { data: 'regardingPosition', width: '250px' },
            { data: 'position', width: '250px' },
            { data: 'lastname', defaultContent: '', width: '200px' },
            { data: 'firstname', width: '200px' },
            { data: 'fathername', defaultContent: '', width: '200px' },
            { data: 'assignmentDate', width: '150px', render: fmtDate },
            { data: 'privacyLevel', width: '100px', render: function (d) { return d === 1 ? 'Tam məxfi' : 'Məxfi'; } },
            { data: 'sendAwaySerialNumber', defaultContent: '', width: '100px' },
            { data: 'sendAwayDate', defaultContent: '', width: '150px', render: fmtDate },
            { data: 'executor', width: '250px', render: function (d) { return d ? d.fullInfo : ''; } },
            { data: 'formalizationSerialNumber', defaultContent: '', width: '100px' },
            { data: 'formalizationDate', defaultContent: '', width: '150px', render: fmtDate },
            { data: 'rejectionInfo', defaultContent: '', width: '300px' },
            { data: 'sentBackInfo', defaultContent: '', width: '300px' },
            { data: 'note', defaultContent: '', width: '200px' },
            {
                data: null,
                orderable: false,
                searchable: false,
                width: '150px',
                render: function (data) {
                    if (data.deletedAt) {
                        return '<button class="btn btn-success btn-sm restore-btn opacity-100" data-id="' + data.id + '">Bərpa</button>';
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
        initComplete: function () {
            $('#tableLoader').fadeOut(150);
            buildColVisMenu();
        }
    });

    function buildColVisMenu() {
        var $menu = $('#colVisMenu');
        $menu.empty();
        for (var i = 0; i < 22; i++) {
            var visible = table.column(i).visible();
            var $item = $('<label class="dropdown-item d-flex align-items-center gap-2" style="font-size:0.85rem;cursor:pointer;"></label>');
            var $cb = $('<input type="checkbox" class="form-check-input colvis-cb" data-col="' + i + '"' + (visible ? ' checked' : '') + '>');
            $item.append($cb).append(document.createTextNode(columnNames[i]));
            $menu.append($item);
        }
    }

    $(document).on('change', '.colvis-cb', function () {
        var col = $(this).data('col');
        table.column(col).visible($(this).is(':checked'));
    });

    $('#colVisMenu').on('click', function (e) {
        e.stopPropagation();
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

    function submitExportForm(action) {
        var $form = $('#exportForm');
        $form.attr('action', '/Export/' + action);
        $form.find('input:not([name="__RequestVerificationToken"])').remove();

        var filters = getFilterData();
        for (var key in filters) {
            $form.append($('<input type="hidden">').attr('name', key).val(filters[key]));
        }

        var visCols = getVisibleColumnIndices();
        $form.append($('<input type="hidden">').attr('name', 'visibleColumns').val(visCols.join(',')));

        $form.submit();
    }

    $('#exportExcel').on('click', function () { submitExportForm('Excel'); });
    $('#exportPdf').on('click', function () { submitExportForm('Pdf'); });
    $('#exportWord').on('click', function () { submitExportForm('Word'); });

    $('#exportPrint').on('click', function () {
        var $form = $('<form method="post" target="_blank" style="display:none;"></form>');
        $form.attr('action', '/Export/Print');
        $form.append($('<input type="hidden">').attr('name', '__RequestVerificationToken').val(token));

        var filters = getFilterData();
        for (var key in filters) {
            $form.append($('<input type="hidden">').attr('name', key).val(filters[key]));
        }

        var visCols = getVisibleColumnIndices();
        $form.append($('<input type="hidden">').attr('name', 'visibleColumns').val(visCols.join(',')));

        $('body').append($form);
        $form.submit();
        $form.remove();
    });

    $('#backupExportBtn').on('click', function () {
        submitExportForm('Backup');
    });

    $('input[name="backupMode"]').on('change', function () {
        if ($('#modeClean').is(':checked')) {
            $('#backupCleanWarning').removeClass('d-none');
        } else {
            $('#backupCleanWarning').addClass('d-none');
        }
    });

    $('#backupImportBtn').on('click', function () {
        var fileInput = document.getElementById('backupFile');
        if (!fileInput.files.length) {
            window.dttbidsmxbb.toast('error', 'Fayl seçin.');
            return;
        }

        var mode = $('input[name="backupMode"]:checked').val();
        if (mode === 'clean' && !confirm('Mövcud bütün aktiv məlumatlar silinəcək. Davam etmək istəyirsiniz?')) return;

        var fd = new FormData();
        fd.append('file', fileInput.files[0]);
        fd.append('mode', mode);
        fd.append('__RequestVerificationToken', token);

        var $btn = $(this);
        $btn.prop('disabled', true).text('Yüklənir...');

        $.ajax({
            url: '/Import/Backup',
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                $btn.prop('disabled', false).text('İdxal et');
                showBackupResult(res);
                if (res.importedRows > 0) table.ajax.reload(null, false);
            },
            error: function () {
                $btn.prop('disabled', false).text('İdxal et');
                window.dttbidsmxbb.toast('error', 'Xəta baş verdi');
            }
        });
    });

    function showBackupResult(res) {
        var $el = $('#backupImportResult').removeClass('d-none');
        var cls = res.success ? 'alert-success' : 'alert-warning';
        var html = '<div class="alert ' + cls + ' py-2 mb-0" style="font-size:0.85rem;">';
        html += '<strong>' + res.message + '</strong>';
        if (res.errors && res.errors.length) {
            html += '<ul class="mt-2 mb-0">';
            res.errors.slice(0, 20).forEach(function (e) {
                html += '<li>Sətir ' + e.row + ' — ' + e.field + ': ' + e.message + '</li>';
            });
            if (res.errors.length > 20) html += '<li>...və ' + (res.errors.length - 20) + ' daha</li>';
            html += '</ul>';
        }
        html += '</div>';
        $el.html(html);
    }

    $('#importBtn').on('click', function () {
        var fileInput = document.getElementById('importFile');
        if (!fileInput.files.length) {
            window.dttbidsmxbb.toast('error', 'Fayl seçin.');
            return;
        }

        var useAsDb = $('#useAsDbCheck').is(':checked');
        if (useAsDb && !confirm('Mövcud bütün məlumatlar silinəcək. Davam etmək istəyirsiniz?')) return;

        var fd = new FormData();
        fd.append('file', fileInput.files[0]);
        fd.append('useAsDb', useAsDb);
        fd.append('__RequestVerificationToken', token);

        var $btn = $(this);
        $btn.prop('disabled', true).text('Yüklənir...');

        $.ajax({
            url: '/Import/Upload',
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                $btn.prop('disabled', false).text('İdxal et');
                showImportResult(res);
                if (res.importedRows > 0) table.ajax.reload(null, false);
            },
            error: function () {
                $btn.prop('disabled', false).text('İdxal et');
                window.dttbidsmxbb.toast('error', 'Xəta baş verdi');
            }
        });
    });

    function showImportResult(res) {
        var $el = $('#importResult').removeClass('d-none');
        var cls = res.success ? 'alert-success' : 'alert-warning';
        var html = '<div class="alert ' + cls + ' py-2 mb-0" style="font-size:0.85rem;">';
        html += '<strong>' + res.message + '</strong>';
        if (res.errors && res.errors.length) {
            html += '<ul class="mt-2 mb-0">';
            res.errors.slice(0, 20).forEach(function (e) {
                html += '<li>Sətir ' + e.row + ' — ' + e.field + ': ' + e.message + '</li>';
            });
            if (res.errors.length > 20) html += '<li>...və ' + (res.errors.length - 20) + ' daha</li>';
            html += '</ul>';
        }
        html += '</div>';
        $el.html(html);
    }

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
});
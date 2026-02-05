document.addEventListener("DOMContentLoaded", function () {
    const $parent = $('#column_filters');
    const isAdmin = window.XNB_CONFIG?.isAdmin ?? false;
    const ENTITY = 'ChaparMobileApps';

    function initSelect2Filters() {
        [
            { id: '#militaryRank_list', placeholder: 'Rütbə' },
            { id: '#mbase_list', placeholder: 'Hərbi hissə' },
            { id: '#createdBy_list', placeholder: 'Bazaya əlavə edən' }
        ].forEach(cfg => {
            const $el = $(cfg.id);
            if (!$el.length) return;
            $el.select2({
                width: '100%',
                dropdownParent: $parent,
                placeholder: cfg.placeholder,
                allowClear: true
            });
        });
    }

    function initDatePickers() {
        ['#createdDate'].forEach(sel => {
            const el = document.querySelector(sel);
            if (!el) return;
            flatpickr(el, {
                mode: "range",
                dateFormat: "Y-m-d",
                allowInput: true,
                locale: {
                    ...flatpickr.l10ns.az,
                    rangeSeparator: " … "
                }
            });
        });
    }

    function readRange(id, mode) {
        const el = document.getElementById(id);
        const fp = el?._flatpickr;
        if (!fp?.selectedDates?.length) return { start: null, end: null };

        const a = fp.selectedDates[0];
        const b = fp.selectedDates[1] || a;
        const pad = n => String(n).padStart(2, '0');
        const ymd = dt => `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}`;

        if (mode === 'daySpan') {
            return { start: `${ymd(a)}T00:00:00`, end: `${ymd(b)}T23:59:59` };
        }
        return { start: ymd(a), end: ymd(b) };
    }

    function getFilters() {
        const cre = readRange('createdDate', 'daySpan');

        return {
            MilitaryRankId: $('#militaryRank_list').val() || null,
            MBaseId: $('#mbase_list').val() || null,
            CreatedByAppUserId: $('#createdBy_list').val() || null,  // ADD THIS
            FullNameQuery: $('#fullName_filter').val()?.trim() || null,
            NoteQuery: $('#note_filter').val()?.trim() || null,
            CreatedAtStart: cre.start,
            CreatedAtEnd: cre.end
        };
    }

    function getFiltersForDT(d) {
        const f = getFilters();
        d['col.MilitaryRankId'] = f.MilitaryRankId;
        d['col.MBaseId'] = f.MBaseId;
        d['col.CreatedByAppUserId'] = f.CreatedByAppUserId;  // ADD THIS
        d['col.FullNameQuery'] = f.FullNameQuery;
        d['col.NoteQuery'] = f.NoteQuery;
        d['col.CreatedAtStart'] = f.CreatedAtStart;
        d['col.CreatedAtEnd'] = f.CreatedAtEnd;
    }

    initSelect2Filters();
    initDatePickers();

    const table = $('#table_data').DataTable({
        serverSide: true,
        processing: true,
        searching: false,
        paging: true,
        order: [[3, 'desc']],
        pageLength: 25,
        lengthMenu: [5, 10, 25, 50, 100],
        scrollX: true,
        autoWidth: false,
        fixedColumns: {
            start: 0,
            end: 1
        },
        dom: 'Brtip',
        ajax: {
            url: '/ChaparMobileApps/Load',
            type: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            data: getFiltersForDT
        },
        columns: [
            { data: 'militaryRankName', width: '120px' },
            { data: 'fullName', width: '200px' },
            { data: 'mBaseName', width: '150px' },
            { data: 'createdAt', width: '150px' },
            { data: 'createdBy', width: '200px' },
            { data: 'note', width: '250px' },
            {
                data: null,
                orderable: false,
                searchable: false,
                width: '160px',
                render: function (_, __, row) {
                    if (!row.canEdit || !isAdmin) return '';
                    return `<div class="d-flex gap-1 justify-content-center">
                        <a href="/ChaparMobileApps/Edit/${row.id}" class="btn btn-secondary btn-sm">Düzəliş et</a>
                        <a href="/ChaparMobileApps/Delete/${row.id}" class="btn btn-danger btn-sm">Sil</a>
                    </div>`;
                }
            }
        ],
        columnDefs: [{ targets: '_all', className: 'dt-center' }],
        buttons: [
            {
                text: 'Çap',
                className: 'btn btn-secondary btn-sm mx-1',
                action: () => XnbExport.print(ENTITY, table, getFilters)
            },
            {
                text: 'Word',
                className: 'btn btn-secondary btn-sm mx-1',
                action: () => XnbExport.toWord(ENTITY, table)
            },
            {
                text: 'PDF',
                className: 'btn btn-secondary btn-sm mx-1',
                action: () => XnbExport.toPdf(ENTITY, table)
            },
            {
                extend: 'colvis',
                text: 'Sütunlar',
                className: 'btn btn-secondary btn-sm mx-1',
                columns: ':not(:last-child)'
            }
        ],
        language: {
            paginate: { previous: "<", next: ">" },
            emptyTable: "Cədvəldə məlumat yoxdur",
            info: "_START_ - _END_ məlumat göstərilir, cəmi: _TOTAL_ məlumat",
            infoFiltered: "(filter tətbiq olundu, ümumi say: _MAX_)",
            infoEmpty: "Məlumat yoxdur, cədvəl boşdur",
            lengthMenu: "_MENU_ nəticələri göstər",
            loadingRecords: "Yüklənir...",
            processing: "İşlənilir...",
            zeroRecords: "Uyğun nəticə tapılmadı"
        },
        initComplete: () => $('#tableLoader').fadeOut(150)
    });

    $('#filtersSearchBtn').on('click', e => {
        e.preventDefault();
        table.ajax.reload();
    });

    $('#filtersResetBtn').on('click', e => {
        e.preventDefault();
        $('#militaryRank_list, #mbase_list, #createdBy_list').val(null).trigger('change');
        $('#fullName_filter, #note_filter').val('');
        document.getElementById('createdDate')?._flatpickr?.clear();
        table.ajax.reload();
    });
});
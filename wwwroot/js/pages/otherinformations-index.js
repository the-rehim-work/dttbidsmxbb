document.addEventListener("DOMContentLoaded", function () {
    const $parent = $('#column_filters');
    const isAdmin = window.XNB_CONFIG?.isAdmin ?? false;
    const ENTITY = 'OtherInformations';

    function initSelect2Filters() {
        [
            { id: '#executorUser_list', placeholder: 'İcraçı' },
            { id: '#informationType_list', placeholder: 'Növü' },
            { id: '#mbase_list', placeholder: 'Hərbi hissə' },
            { id: '#sentToMBase_list', placeholder: 'Göndərildiyi hərbi hissə' },
            { id: '#createdBy_list', placeholder: 'Bazaya əlavə edən' },
            { id: '#modifiedBy_list', placeholder: 'Düzəliş edən' }
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
        ['#detectionDate', '#createdDate', '#modifiedDate'].forEach(sel => {
            const el = document.querySelector(sel);
            if (!el) return;
            flatpickr(el, {
                mode: "range",
                dateFormat: "Y-m-d",
                allowInput: true,
                locale: "az"
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
        const det = readRange('detectionDate', 'date');
        const cre = readRange('createdDate', 'daySpan');
        const mod = readRange('modifiedDate', 'daySpan');

        return {
            ExecutorAppUserId: $('#executorUser_list').val() || null,
            InformationTypeId: $('#informationType_list').val() || null,
            MBaseId: $('#mbase_list').val() || null,
            SentToMBaseId: $('#sentToMBase_list').val() || null,
            CreatedByAppUserId: $('#createdBy_list').val() || null,
            ModifiedByAppUserId: $('#modifiedBy_list').val() || null,
            ExecutionStatusQuery: $('#executionStatus_filter').val()?.trim() || null,
            ResultQuery: $('#result_filter').val()?.trim() || null,
            NoteQuery: $('#note_filter').val()?.trim() || null,
            ContentQuery: $('#content_filter').val()?.trim() || null,
            SentToWhomQuery: $('#sentToWhom_filter').val()?.trim() || null,
            DetectionDateStart: det.start,
            DetectionDateEnd: det.end,
            CreatedAtStart: cre.start,
            CreatedAtEnd: cre.end,
            ModifiedAtStart: mod.start,
            ModifiedAtEnd: mod.end
        };
    }

    function getFiltersForDT(d) {
        const f = getFilters();
        d['col.ExecutorAppUserId'] = f.ExecutorAppUserId;
        d['col.InformationTypeId'] = f.InformationTypeId;
        d['col.MBaseId'] = f.MBaseId;
        d['col.SentToMBaseId'] = f.SentToMBaseId;
        d['col.CreatedByAppUserId'] = f.CreatedByAppUserId;
        d['col.ModifiedByAppUserId'] = f.ModifiedByAppUserId;
        d['col.ExecutionStatusQuery'] = f.ExecutionStatusQuery;
        d['col.ResultQuery'] = f.ResultQuery;
        d['col.NoteQuery'] = f.NoteQuery;
        d['col.ContentQuery'] = f.ContentQuery;
        d['col.SentToWhomQuery'] = f.SentToWhomQuery;
        d['col.DetectionDateStart'] = f.DetectionDateStart;
        d['col.DetectionDateEnd'] = f.DetectionDateEnd;
        d['col.CreatedAtStart'] = f.CreatedAtStart;
        d['col.CreatedAtEnd'] = f.CreatedAtEnd;
        d['col.ModifiedAtStart'] = f.ModifiedAtStart;
        d['col.ModifiedAtEnd'] = f.ModifiedAtEnd;
    }

    initSelect2Filters();
    initDatePickers();

    const columns = [
        { data: 'executorFullName', width: '150px' },
        { data: 'executionStatus', width: '700px', className: 'wrap-text' },
        { data: 'detectionDate', width: '100px' },
        { data: 'informationTypeName', width: '120px' },
        { data: 'mBaseName', width: '120px' },
        { data: 'content', width: '350px' },
        { data: 'sentToWhom', width: '120px' },
        { data: 'sentToMBaseName', width: '120px' },
        { data: 'result', width: '200px' },
        { data: 'note', width: '200px' },
        { data: 'createdAt', width: '130px' },
        { data: 'createdBy', width: '130px' },
        { data: 'modifiedAt', width: '130px' },
        { data: 'modifiedBy', width: '130px' },
        {
            data: null,
            orderable: false,
            searchable: false,
            width: '160px',
            render: function (_, __, row) {
                let btns = '';
                if (row.canEdit) {
                    btns += `<a href="/OtherInformations/Edit/${row.id}" class="btn btn-secondary btn-sm">Düzəliş et</a> `;
                }
                if (row.canDelete) {
                    btns += `<a href="/OtherInformations/Delete/${row.id}" class="btn btn-danger btn-sm">Sil</a>`;
                }
                if (!btns) return '';
                return `<div class="d-flex gap-1 justify-content-center">${btns}</div>`;
            }
        }
    ];

    const table = $('#table_data').DataTable({
        serverSide: true,
        processing: true,
        searching: false,
        paging: true,
        order: [[10, 'desc']],
        pageLength: 25,
        lengthMenu: [5, 10, 25, 50, 100],
        scrollX: true,
        autoWidth: false,
        fixedColumns: { start: 0, end: 1 },
        dom: 'Brtip',
        ajax: {
            url: '/OtherInformations/Load',
            type: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            data: getFiltersForDT
        },
        columns: columns,
        columnDefs: [{ targets: '_all', className: 'dt-center' }],
        buttons: [
            { text: 'Çap', className: 'btn btn-secondary btn-sm mx-1', action: () => XnbExport.print(ENTITY, table, getFilters) },
            { text: 'Word', className: 'btn btn-secondary btn-sm mx-1', action: () => XnbExport.toWord(ENTITY, table, getFilters) },
            { text: 'PDF', className: 'btn btn-secondary btn-sm mx-1', action: () => XnbExport.toPdf(ENTITY, table, getFilters) },
            { extend: 'colvis', text: 'Sütunlar', className: 'btn btn-secondary btn-sm mx-1', columns: ':not(:last-child)' }
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
        $('#executorUser_list, #informationType_list, #mbase_list, #sentToMBase_list, #createdBy_list, #modifiedBy_list').val(null).trigger('change');
        $('#executionStatus_filter, #result_filter, #note_filter, #content_filter, #sentToWhom_filter').val('');
        ['detectionDate', 'createdDate', 'modifiedDate'].forEach(id => {
            document.getElementById(id)?._flatpickr?.clear();
        });
        table.ajax.reload();
    });

    $('#column_filters').on('keydown', 'input.filter-el, textarea.filter-el', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            $('#filtersSearchBtn').trigger('click');
        }
    });
});
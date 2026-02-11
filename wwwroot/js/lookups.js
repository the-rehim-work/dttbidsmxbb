var editId = null;
var deleteId = null;
var upsertModal = null;
var deleteModal = null;

$(function () {
    upsertModal = new bootstrap.Modal('#upsertModal');
    deleteModal = new bootstrap.Modal('#deleteModal');

    $('#lookupTable').DataTable({
        paging: true,
        searching: true,
        ordering: true,
        pageLength: 25,
        autowidth: false,
        order: [[0, 'asc']],
        language: {
            search: 'Axtar:',
            lengthMenu: '_MENU_ sətir',
            info: '_TOTAL_ nəticədən _START_ - _END_',
            infoEmpty: 'Nəticə yoxdur',
            infoFiltered: '(ümumi _MAX_)',
            zeroRecords: 'Nəticə tapılmadı',
            paginate: { previous: '‹', next: '›' }
        },
        columnDefs: [
            { orderable: false, searchable: false, width: '150px', targets: -1 }
        ],
        initComplete: function () { $('#tableLoader').fadeOut(150); }
    });

    $('#lookupTable').on('click', '.edit-btn', function () {
        var btn = $(this);
        editId = btn.data('id');
        $('#editId').val(editId);
        $('#nameInput').val(btn.data('name'));
        $('#upsertTitle').text(lookupConfig.entityName.charAt(0).toUpperCase() + lookupConfig.entityName.slice(1) + ' redaktə et');
        upsertModal.show();
    });

    $('#lookupTable').on('click', '.delete-btn', function () {
        deleteId = $(this).data('id');
        deleteModal.show();
    });

    $('#confirmDeleteBtn').on('click', function () {
        if (!deleteId) return;
        var $btn = $(this);
        $btn.prop('disabled', true);

        window.dttbidsmxbb.post(lookupConfig.deleteUrl, { id: deleteId }).done(function (res) {
            $btn.prop('disabled', false);
            deleteModal.hide();
            if (res.success) {
                window.dttbidsmxbb.toast('success', res.message);
                location.reload();
            } else {
                window.dttbidsmxbb.toast('error', res.message);
            }
        }).fail(function () {
            $btn.prop('disabled', false);
            deleteModal.hide();
            window.dttbidsmxbb.toast('error', 'Xəta baş verdi');
        });
    });

    $('#upsertModal').on('shown.bs.modal', function () {
        $('#nameInput').trigger('focus');
    });

    $('#nameInput').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            save();
        }
    });
});

function openCreate() {
    editId = null;
    $('#editId').val('');
    $('#nameInput').val('');
    $('#upsertTitle').text('Yeni ' + lookupConfig.entityName);
}

function save() {
    var val = $.trim($('#nameInput').val());
    if (!val) {
        window.dttbidsmxbb.toast('error', 'Dəyər daxil edin.');
        return;
    }

    var url = editId ? lookupConfig.updateUrl : lookupConfig.createUrl;
    var data = {};
    data[lookupConfig.fieldName] = val;
    if (editId) data.id = editId;

    var $btn = $('#saveBtn');
    $btn.prop('disabled', true);

    window.dttbidsmxbb.post(url, data).done(function (res) {
        $btn.prop('disabled', false);
        if (res.success) {
            upsertModal.hide();
            window.dttbidsmxbb.toast('success', res.message);
            location.reload();
        } else {
            window.dttbidsmxbb.toast('error', res.message);
        }
    }).fail(function () {
        $btn.prop('disabled', false);
        window.dttbidsmxbb.toast('error', 'Xəta baş verdi');
    });
}
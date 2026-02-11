var editUserId = null;
var deleteUserId = null;
var upsertModal = null;
var deleteModal = null;
var resetModal = null;

$(function () {
    upsertModal = new bootstrap.Modal('#upsertUserModal');
    deleteModal = new bootstrap.Modal('#deleteUserModal');
    resetModal = new bootstrap.Modal('#resetPasswordModal');

    $('#usersTable').DataTable({
        paging: true,
        searching: true,
        ordering: true,
        pageLength: 25,
        autoWidth: false,
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
            { orderable: false, searchable: false, width: '200px', targets: -1 }
        ],
        initComplete: function () { $('#tableLoader').fadeOut(150); }
    });

    $('#usersTable').on('click', '.edit-btn', function () {
        var btn = $(this);
        editUserId = btn.data('id');
        $('#upsertUserTitle').text('İstifadəçi redaktə et');
        $('#passwordGroup').hide();
        $('#userId').val(editUserId);
        $('#fullName').val(btn.data('fullname'));
        $('#userName').val(btn.data('username')).prop('disabled', true);
        $('#role').val(btn.data('role'));
        upsertModal.show();
    });

    $('#usersTable').on('click', '.delete-btn', function () {
        deleteUserId = $(this).data('id');
        deleteModal.show();
    });

    $('#usersTable').on('click', '.reset-btn', function () {
        $('#resetUserId').val($(this).data('id'));
        $('#newPassword').val('');
        $('#confirmNewPassword').val('');
        resetModal.show();
    });

    $('#confirmDeleteUserBtn').on('click', function () {
        if (!deleteUserId) return;
        var $btn = $(this);
        $btn.prop('disabled', true);

        window.dttbidsmxbb.post('/Users/Delete', { id: deleteUserId }).done(function (res) {
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

    $('#saveUserBtn').on('click', saveUser);

    $('#resetPasswordBtn').on('click', resetPassword);

    $('#upsertUserModal').on('shown.bs.modal', function () {
        $('#fullName').trigger('focus');
    });
});

function openCreateUser() {
    editUserId = null;
    $('#upsertUserTitle').text('Yeni istifadəçi');
    $('#userId').val('');
    $('#fullName').val('');
    $('#userName').val('').prop('disabled', false);
    $('#password').val('');
    $('#confirmPassword').val('');
    $('#role').val('');
    $('#passwordGroup').show();
}

function saveUser() {
    var data = {
        id: editUserId || '',
        fullName: $.trim($('#fullName').val()),
        userName: $.trim($('#userName').val()),
        role: $('#role').val()
    };

    if (!data.fullName || !data.userName) {
        window.dttbidsmxbb.toast('error', 'Ad və istifadəçi adı daxil edin.');
        return;
    }

    if (!data.role) {
        window.dttbidsmxbb.toast('error', 'Rol seçin.');
        return;
    }

    if (!editUserId) {
        data.password = $('#password').val();
        data.confirmPassword = $('#confirmPassword').val();
        if (!data.password) {
            window.dttbidsmxbb.toast('error', 'Şifrə daxil edin.');
            return;
        }
        if (data.password !== data.confirmPassword) {
            window.dttbidsmxbb.toast('error', 'Şifrələr uyğun gəlmir.');
            return;
        }
    }

    var url = editUserId ? '/Users/Update' : '/Users/Create';
    var $btn = $('#saveUserBtn');
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

function resetPassword() {
    var pw = $('#newPassword').val();
    var cpw = $('#confirmNewPassword').val();

    if (!pw) {
        window.dttbidsmxbb.toast('error', 'Yeni şifrə daxil edin.');
        return;
    }
    if (pw !== cpw) {
        window.dttbidsmxbb.toast('error', 'Şifrələr uyğun gəlmir.');
        return;
    }

    var $btn = $('#resetPasswordBtn');
    $btn.prop('disabled', true);

    window.dttbidsmxbb.post('/Users/ResetPassword', {
        id: $('#resetUserId').val(),
        newPassword: pw,
        confirmNewPassword: cpw
    }).done(function (res) {
        $btn.prop('disabled', false);
        if (res.success) {
            resetModal.hide();
            window.dttbidsmxbb.toast('success', res.message);
        } else {
            window.dttbidsmxbb.toast('error', res.message);
        }
    }).fail(function () {
        $btn.prop('disabled', false);
        window.dttbidsmxbb.toast('error', 'Xəta baş verdi');
    });
}
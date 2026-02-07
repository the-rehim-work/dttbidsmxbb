(function () {
    'use strict';

    var ns = window.dttbidsmxbb = window.dttbidsmxbb || {};

    ns.getToken = function () {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    };

    ns.toast = function (type, message) {
        var tpl = document.getElementById('toastTemplate');
        var container = document.getElementById('toastContainer');
        if (!tpl || !container) return;

        var clone = tpl.content.cloneNode(true);
        var toastEl = clone.querySelector('.toast');
        var icon = clone.querySelector('.toast-icon');
        var title = clone.querySelector('.toast-title');
        var body = clone.querySelector('.toast-body');

        body.textContent = message;

        switch (type) {
            case 'success':
                title.textContent = 'Uğurlu';
                icon.textContent = '✓';
                icon.style.color = '#198754';
                toastEl.style.borderLeft = '4px solid #198754';
                break;
            case 'error':
                title.textContent = 'Xəta';
                icon.textContent = '✕';
                icon.style.color = '#dc3545';
                toastEl.style.borderLeft = '4px solid #dc3545';
                break;
            case 'warning':
                title.textContent = 'Diqqət';
                icon.textContent = '⚠';
                icon.style.color = '#ffc107';
                toastEl.style.borderLeft = '4px solid #ffc107';
                break;
            default:
                title.textContent = 'Məlumat';
                icon.textContent = 'ℹ';
                icon.style.color = '#0d6efd';
                toastEl.style.borderLeft = '4px solid #0d6efd';
                break;
        }

        container.appendChild(toastEl);
        var bsToast = new bootstrap.Toast(toastEl);
        bsToast.show();

        toastEl.addEventListener('hidden.bs.toast', function () {
            toastEl.remove();
        });
    };

    ns.post = function (url, data) {
        data = data || {};
        data.__RequestVerificationToken = ns.getToken();
        return $.ajax({
            url: url,
            type: 'POST',
            data: data
        });
    };

    if ($.fn.dataTable) {
        $.extend(true, $.fn.dataTable.defaults, {
            language: {
                processing: '<div class="d-flex align-items-center gap-2"><div class="spinner-border spinner-border-sm text-primary"></div> Yüklənir...</div>',
                lengthMenu: '_MENU_ sətir göstər',
                zeroRecords: 'Nəticə tapılmadı',
                info: '_TOTAL_ nəticədən _START_ - _END_ göstərilir',
                infoEmpty: 'Nəticə yoxdur',
                infoFiltered: '(cəmi _MAX_ nəticədən)',
                search: 'Axtar:',
                paginate: { first: 'İlk', last: 'Son', next: '›', previous: '‹' }
            }
        });
    }

    var pwdModal = document.getElementById('changeMyPasswordModal');
    var pwdBody = document.getElementById('changeMyPasswordBody');

    if (pwdModal && pwdBody) {
        var pwdHtml =
            '<div class="mb-3">' +
            '<label class="form-label"><span class="text-danger">*</span> Cari şifrə</label>' +
            '<input type="password" id="currentPassword" class="form-control" autocomplete="current-password">' +
            '</div>' +
            '<div class="mb-3">' +
            '<label class="form-label"><span class="text-danger">*</span> Yeni şifrə</label>' +
            '<input type="password" id="newPassword" class="form-control" autocomplete="new-password">' +
            '</div>' +
            '<div class="mb-3">' +
            '<label class="form-label"><span class="text-danger">*</span> Yeni şifrəni təsdiqlə</label>' +
            '<input type="password" id="confirmNewPassword" class="form-control" autocomplete="new-password">' +
            '</div>';

        pwdModal.addEventListener('show.bs.modal', function () {
            if (!pwdBody.innerHTML.trim()) pwdBody.innerHTML = pwdHtml;
        });

        pwdModal.addEventListener('shown.bs.modal', function () {
            var el = document.getElementById('currentPassword');
            if (el) el.focus();
        });

        pwdModal.addEventListener('hidden.bs.modal', function () {
            pwdBody.innerHTML = '';
        });

        document.getElementById('saveMyPasswordBtn').addEventListener('click', function () {
            var cur = document.getElementById('currentPassword').value;
            var nw = document.getElementById('newPassword').value;
            var cnf = document.getElementById('confirmNewPassword').value;

            if (!cur) { ns.toast('error', 'Cari şifrəni daxil edin'); return; }
            if (!nw || nw.length < 4) { ns.toast('error', 'Yeni şifrə minimum 4 simvol olmalıdır'); return; }
            if (nw !== cnf) { ns.toast('error', 'Yeni şifrələr uyğun gəlmir'); return; }

            var btn = this;
            btn.disabled = true;

            ns.post('/Auth/ChangeMyPassword', {
                currentPassword: cur,
                newPassword: nw,
                confirmPassword: cnf
            }).done(function (res) {
                if (res.success) {
                    bootstrap.Modal.getInstance(pwdModal).hide();
                    ns.toast('success', res.message);
                } else {
                    ns.toast('error', res.message || 'Xəta baş verdi');
                }
            }).fail(function () {
                ns.toast('error', 'Xəta baş verdi');
            }).always(function () {
                btn.disabled = false;
            });
        });
    }

})();
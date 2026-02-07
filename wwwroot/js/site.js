(function () {
    const $modal = $('#changeMyPasswordModal');
    if (!$modal.length) return;

    const modal = bootstrap.Modal.getOrCreateInstance($modal[0]);

    $modal.on('shown.bs.modal', function () {
        $('#currentPassword').focus();
    });

    $modal.on('hidden.bs.modal', function () {
        $('#currentPassword, #newPassword, #confirmNewPassword').val('');
    });

    $('#saveMyPasswordBtn').on('click', async function () {
        const currentPassword = $('#currentPassword').val();
        const newPassword = $('#newPassword').val();
        const confirmNewPassword = $('#confirmNewPassword').val();

        if (!currentPassword) {
            alert('Cari şifrəni daxil edin');
            return;
        }
        if (!newPassword || newPassword.length < 6) {
            alert('Yeni şifrə minimum 6 simvol olmalıdır');
            return;
        }
        if (newPassword !== confirmNewPassword) {
            alert('Yeni şifrələr uyğun gəlmir');
            return;
        }

        $(this).prop('disabled', true);

        try {
            const res = await fetch('/Auth/ChangeMyPassword', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ currentPassword, newPassword })
            });
            const json = await res.json();

            if (json.success) {
                modal.hide();
                alert('Şifrə uğurla dəyişdirildi');
            } else {
                alert(json.message || 'Xəta baş verdi');
            }
        } catch {
            alert('Xəta baş verdi');
        } finally {
            $(this).prop('disabled', false);
        }
    });

    const modalEl = document.getElementById('changeMyPasswordModal');
    const bodyEl = document.getElementById('changeMyPasswordBody');

    if (!modalEl || !bodyEl) return;

    const template = `
        <div class="mb-3">
            <label class="form-label"><span class="text-danger">*</span> Cari şifrə</label>
            <input type="password" id="currentPassword" class="form-control" autocomplete="current-password">
        </div>
        <div class="mb-3">
            <label class="form-label"><span class="text-danger">*</span> Yeni şifrə</label>
            <input type="password" id="newPassword" class="form-control" autocomplete="new-password">
        </div>
        <div class="mb-3">
            <label class="form-label"><span class="text-danger">*</span> Yeni şifrəni təsdiqlə</label>
            <input type="password" id="confirmNewPassword" class="form-control" autocomplete="new-password">
        </div>
        <div class="alert alert-danger d-none" id="changePwdErr"></div>
    `;

    modalEl.addEventListener('show.bs.modal', function () {
        if (!bodyEl.innerHTML.trim()) bodyEl.innerHTML = template;
        const first = modalEl.querySelector('#currentPassword');
        if (first) setTimeout(() => first.focus(), 0);
    });

    modalEl.addEventListener('hidden.bs.modal', function () {
        bodyEl.innerHTML = '';
    });

})();
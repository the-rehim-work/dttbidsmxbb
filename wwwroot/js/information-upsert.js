$(function () {
    var token = window.dttbidsmxbb.getToken();
    var quickAddModal = new bootstrap.Modal('#quickAddModal');

    $('.select2-lookup').each(function () {
        var $el = $(this);
        var $group = $el.closest('.input-group');

        if ($group.length) {
            $el.select2({
                placeholder: 'Seçin...',
                allowClear: true,
                width: 'resolve',
                dropdownParent: $el.closest('.col-md-6, .col-md-4, .col-md-3, .col-md-2')
            });
        } else {
            $el.select2({
                placeholder: 'Seçin...',
                allowClear: true,
                width: '100%'
            });
        }
    });

    flatpickr('.flatpickr-date', {
        dateFormat: 'd-m-Y',
        altInput: true,
        altFormat: 'd.m.Y',
        locale: 'az',
        allowInput: true
    });

    $('.quick-add-btn').on('click', function () {
        var type = $(this).data('type');
        var target = $(this).data('target');
        $('#quickAddType').val(type);
        $('#quickAddTarget').val(target);
        $('#quickAddValue').val('');

        var titles = {
            base: 'Yeni hərbi hissə',
            rank: 'Yeni rütbə',
            executor: 'Yeni icraçı'
        };
        $('#quickAddTitle').text(titles[type] || 'Əlavə et');
        quickAddModal.show();
    });

    $('#quickAddSaveBtn').on('click', function () {
        var type = $('#quickAddType').val();
        var target = $('#quickAddTarget').val();
        var value = $.trim($('#quickAddValue').val());

        if (!value) {
            window.dttbidsmxbb.toast('error', 'Dəyər daxil edin.');
            return;
        }

        var $btn = $(this);
        $btn.prop('disabled', true);

        $.ajax({
            url: '/Lookups/QuickAdd',
            type: 'POST',
            data: {
                type: type,
                value: value,
                __RequestVerificationToken: token
            },
            success: function (res) {
                $btn.prop('disabled', false);
                if (!res.success) {
                    window.dttbidsmxbb.toast('error', res.message);
                    return;
                }

                var $target = $('#' + target);
                var newOption = new Option(res.name, res.id, true, true);
                $target.append(newOption).trigger('change');

                if (type === 'base') {
                    var otherId = target === 'SenderMilitaryBaseId' ? 'MilitaryBaseId' : 'SenderMilitaryBaseId';
                    var $other = $('#' + otherId);
                    if (!$other.find('option[value="' + res.id + '"]').length) {
                        $other.append(new Option(res.name, res.id, false, false));
                    }
                }

                quickAddModal.hide();
                window.dttbidsmxbb.toast('success', res.message);
            },
            error: function () {
                $btn.prop('disabled', false);
                window.dttbidsmxbb.toast('error', 'Xəta baş verdi');
            }
        });
    });

    $('#quickAddModal').on('shown.bs.modal', function () {
        $('#quickAddValue').trigger('focus');
    });

    $('#quickAddValue').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#quickAddSaveBtn').trigger('click');
        }
    });
});
const XnbExport = (function () {
    function getVisibleColumns(table) {
        const visible = [];
        table.columns().every(function (idx) {
            if (this.visible() && idx < table.columns().count() - 1) {
                const key = $(this.header()).data('key');
                if (key) visible.push(key);
            }
        });
        return visible;
    }

    function getVisibleHeaders(table) {
        const headers = [];
        table.columns(':visible:not(:last-child)').every(function () {
            headers.push(this.header().innerText.trim());
        });
        return headers;
    }

    function buildFilterParams(filterFn) {
        const params = {};
        const filters = filterFn();
        Object.keys(filters).forEach(key => {
            const val = filters[key];
            if (val !== null && val !== undefined && val !== '') {
                params[`col.${key}`] = val;
            }
        });
        return params;
    }

    function downloadBlob(blob, filename) {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
    }

    return {
        print: function (entity, table, filterFn) {
            const params = new URLSearchParams();
            const filters = filterFn();
            Object.keys(filters).forEach(key => {
                const val = filters[key];
                if (val !== null && val !== undefined && val !== '') {
                    params.set(key, val);
                }
            });
            const visible = getVisibleColumns(table);
            if (visible.length > 0) {
                params.set('columns', visible.join(','));
            }
            const url = `/Export/Print/${entity}?${params.toString()}`;
            window.open(url, '_blank');
        },

        toWord: function (entity, table, filterFn) {
            const filters = buildFilterParams(filterFn);
            const columns = getVisibleColumns(table);
            const headers = getVisibleHeaders(table);

            fetch('/Export/Word', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Entity: entity,
                    Headers: headers,
                    Columns: columns,
                    Filters: filters
                })
            })
                .then(r => {
                    if (!r.ok) throw new Error(`HTTP ${r.status}`);
                    return r.blob();
                })
                .then(blob => downloadBlob(blob, `${entity}_${new Date().toISOString().slice(0, 10)}.docx`))
                .catch(err => alert('Export failed: ' + err.message));
        },

        toPdf: function (entity, table, filterFn) {
            const filters = buildFilterParams(filterFn);
            const columns = getVisibleColumns(table);
            const headers = getVisibleHeaders(table);

            fetch('/Export/Pdf', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    Entity: entity,
                    Headers: headers,
                    Columns: columns,
                    Filters: filters
                })
            })
                .then(r => {
                    if (!r.ok) throw new Error(`HTTP ${r.status}`);
                    return r.blob();
                })
                .then(blob => downloadBlob(blob, `${entity}_${new Date().toISOString().slice(0, 10)}.pdf`))
                .catch(err => alert('Export failed: ' + err.message));
        }
    };
})();
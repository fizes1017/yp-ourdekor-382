let adminManagersCache = [];

document.addEventListener('DOMContentLoaded', () => {
    if (!AuthManager.requireAuth()) {
        return;
    }

    const user = AuthManager.getCurrentUser();
    if (!user || user.role !== 'Admin') {
        window.location.href = 'index.html';
        return;
    }

    loadManagers();
    setupAuditFilters();
    setupReport();
});

async function loadManagers() {
    const tbody = document.querySelector('#adminManagersTable tbody');
    try {
        const response = await apiRequest('/admin/managers', 'GET', null, true);
        adminManagersCache = response || [];

        // Заполняем селект для фильтра аудита
        const auditSelect = document.getElementById('auditManagerSelect');
        if (auditSelect) {
            auditSelect.innerHTML = '<option value="">Все менеджеры</option>';
        }

        tbody.innerHTML = '';
        adminManagersCache.forEach(manager => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${manager.fullName}</td>
                <td>${manager.email}</td>
                <td>${manager.phone}</td>
                <td>${manager.role}</td>
                <td>${manager.createdAt}</td>
                <td>
                    <button class="btn btn-small btn-primary" onclick="editManager(${manager.id})">Редактировать</button>
                    <button class="btn btn-small btn-secondary" onclick="resetManagerPassword(${manager.id})">Сброс пароля</button>
                </td>
            `;
            tbody.appendChild(tr);

            if (auditSelect) {
                const opt = document.createElement('option');
                opt.value = manager.id;
                opt.textContent = manager.fullName;
                auditSelect.appendChild(opt);
            }
        });

        renderReportManagersList();
    } catch (error) {
        console.error('Ошибка загрузки менеджеров', error);
        tbody.innerHTML = `<tr><td colspan="6" class="empty-state">Ошибка загрузки менеджеров: ${error.message || ''}</td></tr>`;
    }
}

function showModal(title, message, type = 'info') {
    const overlay = document.getElementById('modalOverlay');
    const modal = document.getElementById('modal');
    const modalTitle = document.getElementById('modalTitle');
    const modalMessage = document.getElementById('modalMessage');
    const modalButtons = document.getElementById('modalButtons');

    modalTitle.textContent = title;
    modalMessage.innerHTML = message;

    modal.classList.remove('modal-success', 'modal-error', 'modal-warning');
    modal.classList.add('modal-' + type);

    modalButtons.innerHTML = '<button class="modal-btn modal-btn-primary" onclick="closeModal()">OK</button>';

    overlay.classList.add('show');
}

function closeModal() {
    const overlay = document.getElementById('modalOverlay');
    if (overlay) {
        overlay.classList.remove('show');
    }
}

function editManager(id) {
    const manager = adminManagersCache.find(m => m.id === id);
    if (!manager) return;

    const formHtml = `
        <div class="form-group">
            <label>ФИО</label>
            <input type="text" id="mgrFullName" value="${manager.fullName || ''}">
        </div>
        <div class="form-group">
            <label>Email</label>
            <input type="email" id="mgrEmail" value="${manager.email || ''}">
        </div>
        <div class="form-group">
            <label>Телефон</label>
            <input type="text"
                   id="mgrPhone"
                   class="js-phone-mask"
                   placeholder="+7 (999) 123-45-67"
                   maxlength="18"
                   value="${typeof formatPhoneDisplay === 'function'
                        ? formatPhoneDisplay(manager.phone || '')
                        : (manager.phone || '')}">
        </div>
        <div class="form-group">
            <label>Роль</label>
            <select id="mgrRole">
                <option value="Manager"${manager.role === 'Manager' ? ' selected' : ''}>Manager</option>
                <option value="Admin"${manager.role === 'Admin' ? ' selected' : ''}>Admin</option>
            </select>
        </div>
        <hr>
        <div class="form-group">
            <label>Сброс пароля</label>
            <input type="password" id="mgrNewPassword" placeholder="Новый пароль">
        </div>
        <div class="form-group">
            <input type="password" id="mgrConfirmPassword" placeholder="Подтверждение пароля">
        </div>
    `;

    const overlay = document.getElementById('modalOverlay');
    const modal = document.getElementById('modal');
    const modalTitle = document.getElementById('modalTitle');
    const modalMessage = document.getElementById('modalMessage');
    const modalButtons = document.getElementById('modalButtons');

    modalTitle.textContent = `Редактирование менеджера`;
    modalMessage.innerHTML = formHtml;

    modal.classList.remove('modal-success', 'modal-error', 'modal-warning');
    modal.classList.add('modal-info');

    modalButtons.innerHTML = `
        <button class="modal-btn modal-btn-secondary" onclick="closeModal()">Отмена</button>
        <button class="modal-btn modal-btn-primary" onclick="saveManagerChanges(${id})">Сохранить</button>
    `;

    overlay.classList.add('show');

    // Подключаем маску телефона к полю в модальном окне
    const phoneInput = document.getElementById('mgrPhone');
    if (phoneInput && typeof applyPhoneMask === 'function') {
        applyPhoneMask(phoneInput);
        if (phoneInput.value && typeof formatPhoneDisplay === 'function') {
            phoneInput.value = formatPhoneDisplay(phoneInput.value);
        }
    }
}

async function saveManagerChanges(id) {
    const fullName = document.getElementById('mgrFullName')?.value.trim();
    const email = document.getElementById('mgrEmail')?.value.trim();
    const phoneInput = document.getElementById('mgrPhone')?.value.trim();
    const role = document.getElementById('mgrRole')?.value;
    const newPassword = document.getElementById('mgrNewPassword')?.value;
    const confirmPassword = document.getElementById('mgrConfirmPassword')?.value;

    const body = {};
    if (fullName) body.fullName = fullName;
    if (email) body.email = email;

    if (phoneInput) {
        if (typeof getPhoneRaw === 'function') {
            const phoneRaw = getPhoneRaw(phoneInput);
            if (!phoneRaw) {
                showModal('Ошибка', 'Введите корректный номер телефона в формате +7 (XXX) XXX-XX-XX', 'error');
                return;
            }
            body.phone = phoneRaw;
        } else {
            body.phone = phoneInput;
        }
    }
    if (role) body.role = role;

    try {
        if (Object.keys(body).length > 0) {
            await apiRequest(`/admin/managers/${id}`, 'PUT', body, true);
        }

        if (newPassword || confirmPassword) {
            if (!newPassword || !confirmPassword) {
                showModal('Ошибка', 'Укажите и новый пароль, и подтверждение', 'error');
                return;
            }
            if (newPassword !== confirmPassword) {
                showModal('Ошибка', 'Пароли не совпадают', 'error');
                return;
            }
            await apiRequest(`/admin/managers/${id}/reset-password`, 'POST', {
                newPassword,
                confirmPassword
            }, true);
        }

        closeModal();
        await loadManagers();
    } catch (error) {
        showModal('Ошибка', error.message || 'Ошибка при сохранении данных менеджера', 'error');
    }
}

function setupAuditFilters() {
    const btn = document.getElementById('loadAuditBtn');
    if (!btn) return;
    btn.addEventListener('click', loadMaterialAudit);
}

async function loadMaterialAudit() {
    const tbody = document.querySelector('#materialsAuditTable tbody');
    const managerId = document.getElementById('auditManagerSelect')?.value;
    const from = document.getElementById('auditFrom')?.value;
    const to = document.getElementById('auditTo')?.value;

    const params = new URLSearchParams();
    if (managerId) params.append('managerId', managerId);
    if (from) params.append('from', from);
    if (to) params.append('to', to);

    try {
        const data = await apiRequest(`/admin/material-changes?${params.toString()}`, 'GET', null, true);
        tbody.innerHTML = '';
        if (!data || data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="empty-state">Нет данных за выбранный период</td></tr>';
            return;
        }

        data.forEach(row => {
            const tr = document.createElement('tr');
            const detailsShort = row.details && row.details.length > 200
                ? row.details.substring(0, 197) + '...'
                : (row.details || '');
            tr.innerHTML = `
                <td>${new Date(row.timestamp).toLocaleString()}</td>
                <td>${row.userFullName || ''}</td>
                <td>${row.action}</td>
                <td>${row.entityId}</td>
                <td title="${row.details || ''}">${detailsShort}</td>
            `;
            tbody.appendChild(tr);
        });
    } catch (error) {
        console.error('Ошибка загрузки аудита материалов', error);
        tbody.innerHTML = `<tr><td colspan="5" class="empty-state">Ошибка: ${error.message || ''}</td></tr>`;
    }
}

function setupReport() {
    const btn = document.getElementById('generateReportBtn');
    if (!btn) return;
    btn.addEventListener('click', generateManagerReport);

    const searchInput = document.getElementById('reportManagerSearch');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            renderReportManagersList(searchInput.value.trim());
        });
    }
}

function renderReportManagersList(filter = '') {
    const list = document.getElementById('reportManagersList');
    if (!list) return;

    const search = filter.toLowerCase();
    list.innerHTML = '';

    adminManagersCache
        .filter(m =>
            !search ||
            (m.fullName && m.fullName.toLowerCase().includes(search)) ||
            (m.email && m.email.toLowerCase().includes(search)))
        .forEach(m => {
            const row = document.createElement('div');
            row.style.display = 'flex';
            row.style.alignItems = 'center';
            row.style.gap = '8px';
            row.style.marginBottom = '4px';
            row.innerHTML = `
                <input type="checkbox" id="mgr_${m.id}" name="reportManagerCheckbox" value="${m.id}">
                <label for="mgr_${m.id}">${m.fullName || ''} (${m.email})</label>
            `;
            list.appendChild(row);
        });
}

async function generateManagerReport() {
    const checkboxes = document.querySelectorAll('input[name="reportManagerCheckbox"]:checked');
    const fromInput = document.getElementById('reportFrom');
    const toInput = document.getElementById('reportTo');

    if (!fromInput || !toInput) return;

    const selected = Array.from(checkboxes).map(o => parseInt(o.value, 10)).filter(Boolean);
    if (selected.length === 0) {
        alert('Выберите хотя бы одного менеджера');
        return;
    }

    if (!fromInput.value || !toInput.value) {
        alert('Выберите период (с и по дату)');
        return;
    }

    const from = new Date(fromInput.value);
    const to = new Date(toInput.value);
    if (to < from) {
        alert('Дата "по" не может быть меньше даты "с"');
        return;
    }

    try {
        const body = {
            managerIds: selected,
            from: from.toISOString(),
            to: to.toISOString()
        };

        const url = `${API_BASE_URL}/admin/manager-report`;
        const token = TokenManager.getToken();
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                ...(token ? { 'Authorization': `Bearer ${token}` } : {})
            },
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            const text = await response.text();
            throw new Error(text || `HTTP ${response.status}`);
        }

        const blob = await response.blob();
        const downloadUrl = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = downloadUrl;
        a.download = `report-managers-${fromInput.value}-${toInput.value}.pdf`;
        a.click();
        URL.revokeObjectURL(downloadUrl);

        const hint = document.getElementById('reportHint');
        if (hint) hint.style.display = 'block';
    } catch (error) {
        console.error('Ошибка формирования отчета', error);
        alert(error.message || 'Ошибка формирования отчета');
    }
}


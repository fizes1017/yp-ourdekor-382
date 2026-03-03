// Load profile data and user calculations on page load
document.addEventListener('DOMContentLoaded', () => {
    loadProfile();
    loadMyCalculations();
});

let calculationsCache = [];

async function loadProfile() {
    try {
        const profile = await ProfileAPI.get();

        document.getElementById('fullName').value = profile.fullName || '';
        document.getElementById('email').value = profile.email || '';
        document.getElementById('phone').value = (typeof formatPhoneDisplay === 'function' ? formatPhoneDisplay(profile.phone || '') : (profile.phone || ''));
        document.getElementById('role').value = profile.role || '';

        if (profile.createdAt) {
            const dateStr = typeof profile.createdAt === 'string'
                ? profile.createdAt
                : `${profile.createdAt.year}-${String(profile.createdAt.month).padStart(2, '0')}-${String(profile.createdAt.day).padStart(2, '0')}`;
            document.getElementById('createdAt').value = dateStr;
        }
    } catch (error) {
        console.error('Error loading profile:', error);
        showModal('Ошибка', error.message || 'Ошибка при загрузке профиля', 'error');
    }
}

async function handleProfileSave(event) {
    event.preventDefault();

    try {
        const fullName = document.getElementById('fullName').value.trim();
        const phoneRaw = (typeof getPhoneRaw === 'function' ? getPhoneRaw(document.getElementById('phone').value) : document.getElementById('phone').value.trim());

        await ProfileAPI.update({
            fullName: fullName || null,
            phone: phoneRaw || null
        });

        showModal('Успешно', 'Профиль успешно обновлен', 'success');

        const user = TokenManager.getUser() || {};
        user.fullName = fullName || user.fullName;
        user.phone = phoneRaw || user.phone;
        TokenManager.setUser(user);
    } catch (error) {
        console.error('Error updating profile:', error);
        showModal('Ошибка', error.message || 'Ошибка при обновлении профиля', 'error');
    }
}

async function handleChangePassword(event) {
    event.preventDefault();

    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (!currentPassword || !newPassword || !confirmPassword) {
        showModal('Внимание', 'Пожалуйста, заполните все поля', 'warning');
        return;
    }

    if (newPassword.length < 6) {
        showModal('Внимание', 'Новый пароль должен содержать минимум 6 символов', 'warning');
        return;
    }

    if (newPassword !== confirmPassword) {
        showModal('Внимание', 'Новый пароль и подтверждение не совпадают', 'warning');
        return;
    }

    try {
        await ProfileAPI.changePassword({
            currentPassword,
            newPassword,
            confirmPassword
        });

        showModal('Успешно', 'Пароль успешно изменен', 'success');

        document.getElementById('currentPassword').value = '';
        document.getElementById('newPassword').value = '';
        document.getElementById('confirmPassword').value = '';
    } catch (error) {
        console.error('Error changing password:', error);
        showModal('Ошибка', error.message || 'Ошибка при смене пароля', 'error');
    }
}

function formatDate(calc) {
    if (!calc.calculatedAt) return '';
    if (typeof calc.calculatedAt === 'string') return calc.calculatedAt;
    return `${calc.calculatedAt.year}-${String(calc.calculatedAt.month).padStart(2, '0')}-${String(calc.calculatedAt.day).padStart(2, '0')}`;
}

function toggleCalculationDetails(calcId) {
    const detailsRow = document.getElementById(`calc-details-${calcId}`);
    const toggleBtn = document.getElementById(`toggle-btn-${calcId}`);
    if (!detailsRow || !toggleBtn) return;

    const isHidden = detailsRow.style.display === 'none' || !detailsRow.style.display;
    detailsRow.style.display = isHidden ? 'table-row' : 'none';
    toggleBtn.textContent = isHidden ? '▲ Свернуть' : '▼ Просмотреть';
}

async function loadMyCalculations() {
    const tbody = document.getElementById('calculationsTableBody');
    tbody.innerHTML = '<tr><td colspan="5">Загрузка...</td></tr>';

    try {
        const calculations = await ProfileAPI.getMyCalculations();
        calculationsCache = calculations || [];

        if (!calculations || calculations.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="5" class="empty-state">
                        <p>Расчеты не найдены</p>
                    </td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = '';

        calculations.forEach(calc => {
            const dateStr = formatDate(calc);
            const itemsCount = calc.items ? calc.items.length : 0;
            const totalAmount = calc.totalAmount != null ? (typeof calc.totalAmount.toFixed === 'function' ? calc.totalAmount.toFixed(2) : calc.totalAmount) : '0.00';

            const mainRow = document.createElement('tr');
            mainRow.className = 'calc-main-row';
            mainRow.innerHTML = `
                <td>
                    <button type="button" class="btn btn-small" id="toggle-btn-${calc.id}" onclick="toggleCalculationDetails(${calc.id})">▼ Просмотреть</button>
                </td>
                <td>${dateStr}</td>
                <td>${totalAmount}</td>
                <td>${itemsCount}</td>
                <td>
                    <button type="button" class="btn btn-primary btn-small" onclick="openKpModal(${calc.id})">Создать КП</button>
                </td>
            `;
            tbody.appendChild(mainRow);

            const detailsRow = document.createElement('tr');
            detailsRow.id = `calc-details-${calc.id}`;
            detailsRow.className = 'calc-details-row';
            detailsRow.style.display = 'none';

            let itemsHtml = '';
            if (calc.items && calc.items.length > 0) {
                itemsHtml = `
                    <td colspan="5" class="calc-details-cell">
                        <div class="calc-details-inner">
                            <table class="calc-items-table">
                                <thead>
                                    <tr>
                                        <th>Артикул</th>
                                        <th>Название</th>
                                        <th>Ед.</th>
                                        <th>Кол-во</th>
                                        <th>Цена, руб</th>
                                        <th>Сумма, руб</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${calc.items.map(item => `
                                        <tr>
                                            <td>${item.materialArticle || ''}</td>
                                            <td>${item.materialName || ''}</td>
                                            <td>${item.unit || ''}</td>
                                            <td>${item.quantity != null ? (typeof item.quantity === 'number' ? item.quantity : parseFloat(item.quantity)).toFixed(3) : ''}</td>
                                            <td>${item.unitPrice != null ? (typeof item.unitPrice === 'number' ? item.unitPrice : parseFloat(item.unitPrice)).toFixed(2) : ''}</td>
                                            <td>${item.totalPrice != null ? (typeof item.totalPrice === 'number' ? item.totalPrice : parseFloat(item.totalPrice)).toFixed(2) : ''}</td>
                                        </tr>
                                    `).join('')}
                                </tbody>
                            </table>
                        </div>
                    </td>
                `;
            } else {
                itemsHtml = '<td colspan="5" class="calc-details-cell"><p>Нет позиций</p></td>';
            }
            detailsRow.innerHTML = itemsHtml;
            tbody.appendChild(detailsRow);
        });
    } catch (error) {
        console.error('Error loading calculations:', error);
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="empty-state">
                    <p style="color: red;">Ошибка при загрузке расчетов: ${error.message || 'Неизвестная ошибка'}</p>
                </td>
            </tr>
        `;
    }
}

function showModal(title, message, type = 'info') {
    const overlay = document.getElementById('modalOverlay');
    const modal = document.getElementById('modal');
    const modalTitle = document.getElementById('modalTitle');
    const modalMessage = document.getElementById('modalMessage');
    const modalButtons = document.getElementById('modalButtons');

    modalTitle.textContent = title;
    modalMessage.textContent = message;

    modal.classList.remove('modal-success', 'modal-error', 'modal-warning');
    modal.classList.add('modal-' + type);

    modalButtons.innerHTML = '<button class="modal-btn modal-btn-primary" onclick="closeModal()">OK</button>';

    overlay.classList.add('show');
}

function closeModal() {
    const overlay = document.getElementById('modalOverlay');
    overlay.classList.remove('show');
}

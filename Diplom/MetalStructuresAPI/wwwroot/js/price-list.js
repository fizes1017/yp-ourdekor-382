let editingMaterialId = null;
let allMaterials = [];
let articleDropdownOpen = false;
let unitDropdownOpen = false;

// Load materials on page load
document.addEventListener('DOMContentLoaded', () => {
    loadMaterials();
    
    // Allow Enter key to save
    const formInputs = document.querySelectorAll('.form-group input');
    formInputs.forEach(input => {
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                saveMaterial();
            }
        });
    });
    
    // Close dropdowns when clicking outside
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.combobox-wrapper')) {
            closeArticleDropdown();
            closeUnitDropdown();
        }
    });
    
    // Filter dropdown on input
    document.getElementById('article').addEventListener('input', (e) => {
        filterArticleDropdown(e.target.value);
        if (e.target.value && !articleDropdownOpen) {
            openArticleDropdown();
        }
    });
    
    document.getElementById('unit').addEventListener('input', (e) => {
        filterUnitDropdown(e.target.value);
        if (e.target.value && !unitDropdownOpen) {
            openUnitDropdown();
        }
    });
    
    // Open dropdown on focus
    document.getElementById('article').addEventListener('focus', () => {
        if (!articleDropdownOpen) {
            openArticleDropdown();
        }
    });
    
    document.getElementById('unit').addEventListener('focus', () => {
        if (!unitDropdownOpen) {
            openUnitDropdown();
        }
    });
});

function filterMaterials() {
    const input = document.getElementById('materialsSearch');
    if (!input) {
        return;
    }
    const query = input.value.trim().toLowerCase();
    if (!query) {
        displayMaterials(allMaterials);
        return;
    }
    const filtered = allMaterials.filter(m =>
        (m.article && m.article.toLowerCase().includes(query)) ||
        (m.name && m.name.toLowerCase().includes(query))
    );
    displayMaterials(filtered);
}

function resetMaterialsFilter() {
    const input = document.getElementById('materialsSearch');
    if (input) {
        input.value = '';
    }
    displayMaterials(allMaterials);
}

// Load all materials from API
async function loadMaterials() {
    try {
        allMaterials = await MaterialsAPI.getAll();
        displayMaterials(allMaterials);
        updateSelectOptions();
    } catch (error) {
        showModal('Ошибка', 'Ошибка при загрузке материалов: ' + error.message, 'error');
    }
}

// Update dropdown options for article and unit
function updateSelectOptions() {
    updateArticleDropdown();
    updateUnitDropdown();
}

// Update article dropdown
function updateArticleDropdown() {
    const dropdown = document.getElementById('articleDropdown');
    const articles = [...new Set(allMaterials.map(m => m.article))].sort();
    
    dropdown.innerHTML = '';
    articles.forEach(article => {
        const item = document.createElement('div');
        item.className = 'combobox-dropdown-item';
        item.textContent = article;
        item.onclick = () => {
            document.getElementById('article').value = article;
            closeArticleDropdown();
        };
        dropdown.appendChild(item);
    });
}

// Update unit dropdown
function updateUnitDropdown() {
    const dropdown = document.getElementById('unitDropdown');
    const units = [...new Set(allMaterials.map(m => m.unit))].sort();
    
    dropdown.innerHTML = '';
    units.forEach(unit => {
        const item = document.createElement('div');
        item.className = 'combobox-dropdown-item';
        item.textContent = unit;
        item.onclick = () => {
            document.getElementById('unit').value = unit;
            closeUnitDropdown();
        };
        dropdown.appendChild(item);
    });
}

// Filter article dropdown
function filterArticleDropdown(searchTerm) {
    const dropdown = document.getElementById('articleDropdown');
    const items = dropdown.querySelectorAll('.combobox-dropdown-item');
    const searchLower = searchTerm.toLowerCase();
    
    items.forEach(item => {
        const text = item.textContent.toLowerCase();
        if (text.includes(searchLower)) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// Filter unit dropdown
function filterUnitDropdown(searchTerm) {
    const dropdown = document.getElementById('unitDropdown');
    const items = dropdown.querySelectorAll('.combobox-dropdown-item');
    const searchLower = searchTerm.toLowerCase();
    
    items.forEach(item => {
        const text = item.textContent.toLowerCase();
        if (text.includes(searchLower)) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// Toggle article dropdown
function toggleArticleDropdown() {
    if (articleDropdownOpen) {
        closeArticleDropdown();
    } else {
        openArticleDropdown();
    }
}

function openArticleDropdown() {
    const dropdown = document.getElementById('articleDropdown');
    dropdown.classList.add('show');
    articleDropdownOpen = true;
    closeUnitDropdown();
}

function closeArticleDropdown() {
    const dropdown = document.getElementById('articleDropdown');
    dropdown.classList.remove('show');
    articleDropdownOpen = false;
}

// Toggle unit dropdown
function toggleUnitDropdown() {
    if (unitDropdownOpen) {
        closeUnitDropdown();
    } else {
        openUnitDropdown();
    }
}

function openUnitDropdown() {
    const dropdown = document.getElementById('unitDropdown');
    dropdown.classList.add('show');
    unitDropdownOpen = true;
    closeArticleDropdown();
}

function closeUnitDropdown() {
    const dropdown = document.getElementById('unitDropdown');
    dropdown.classList.remove('show');
    unitDropdownOpen = false;
}

// Display materials in table
function displayMaterials(materials) {
    const tbody = document.getElementById('materialsTableBody');
    tbody.innerHTML = '';

    if (!materials || materials.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="empty-state"><p>Нет материалов. Добавьте первый материал.</p></td></tr>';
        return;
    }

    materials.forEach(material => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${material.article}</td>
            <td>${material.name}</td>
            <td>${material.price}</td>
            <td>${material.unit}</td>
            <td class="action-buttons">
                <button class="btn btn-edit" onclick="editMaterial(${material.id})">Изменить</button>
                <button class="btn btn-danger" onclick="deleteMaterial(${material.id})">Удалить</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

// Save material (create or update)
async function saveMaterial() {
    const article = document.getElementById('article').value.trim();
    const name = document.getElementById('name').value.trim();
    const price = parseFloat(document.getElementById('price').value);
    const unit = document.getElementById('unit').value.trim();

    // Validation
    if (!article || !name || !price || !unit) {
        showModal('Внимание', 'Пожалуйста, заполните все поля', 'warning');
        return;
    }

    if (isNaN(price) || price <= 0) {
        showModal('Внимание', 'Цена должна быть положительным числом', 'warning');
        return;
    }

    const materialData = {
        article: article,
        name: name,
        price: price,
        unit: unit
    };

    try {
        if (editingMaterialId) {
            // Update existing material
            await MaterialsAPI.update(editingMaterialId, materialData);
            showModal('Успешно', 'Материал успешно обновлен', 'success');
        } else {
            // Create new material
            await MaterialsAPI.create(materialData);
            showModal('Успешно', 'Материал успешно добавлен', 'success');
        }

        clearForm();
        loadMaterials();
    } catch (error) {
        showModal('Ошибка', 'Ошибка при сохранении материала: ' + error.message, 'error');
    }
}

// Edit material
async function editMaterial(id) {
    try {
        const material = await MaterialsAPI.getById(id);
        
        document.getElementById('article').value = material.article;
        document.getElementById('name').value = material.name;
        document.getElementById('price').value = material.price;
        document.getElementById('unit').value = material.unit;

        editingMaterialId = id;

        // Scroll to form
        document.querySelector('.form-section').scrollIntoView({ behavior: 'smooth' });
    } catch (error) {
        showModal('Ошибка', 'Ошибка при загрузке материала: ' + error.message, 'error');
    }
}

// Delete material
async function deleteMaterial(id) {
    showConfirmModal('Подтверждение удаления', 'Вы уверены, что хотите удалить этот материал?', 
        async () => {
            try {
                await MaterialsAPI.delete(id);
                showModal('Успешно', 'Материал успешно удален', 'success');
                loadMaterials();
            } catch (error) {
                showModal('Ошибка', 'Ошибка при удалении материала: ' + error.message, 'error');
            }
        }
    );
}

// Clear form
function clearForm() {
    document.getElementById('article').value = '';
    document.getElementById('name').value = '';
    document.getElementById('price').value = '';
    document.getElementById('unit').value = '';
    editingMaterialId = null;
    closeArticleDropdown();
    closeUnitDropdown();
}

// Modal functions
function showModal(title, message, type = 'info') {
    const overlay = document.getElementById('modalOverlay');
    const modal = document.getElementById('modal');
    const modalTitle = document.getElementById('modalTitle');
    const modalMessage = document.getElementById('modalMessage');
    const modalButtons = document.getElementById('modalButtons');
    
    modalTitle.textContent = title;
    modalMessage.textContent = message;
    
    // Remove previous type classes
    modal.classList.remove('modal-success', 'modal-error', 'modal-warning');
    modal.classList.add('modal-' + type);
    
    modalButtons.innerHTML = '<button class="modal-btn modal-btn-primary" onclick="closeModal()">OK</button>';
    
    overlay.classList.add('show');
}

function showConfirmModal(title, message, onConfirm) {
    const overlay = document.getElementById('modalOverlay');
    const modal = document.getElementById('modal');
    const modalTitle = document.getElementById('modalTitle');
    const modalMessage = document.getElementById('modalMessage');
    const modalButtons = document.getElementById('modalButtons');
    
    modalTitle.textContent = title;
    modalMessage.textContent = message;
    
    // Remove previous type classes
    modal.classList.remove('modal-success', 'modal-error', 'modal-warning');
    modal.classList.add('modal-warning');
    
    modalButtons.innerHTML = `
        <button class="modal-btn modal-btn-secondary" onclick="closeModal()">Отмена</button>
        <button class="modal-btn modal-btn-primary" onclick="confirmAction()">OK</button>
    `;
    
    // Store confirm callback
    window.confirmCallback = () => {
        closeModal();
        onConfirm();
    };
    
    overlay.classList.add('show');
}

function confirmAction() {
    if (window.confirmCallback) {
        window.confirmCallback();
        window.confirmCallback = null;
    }
}

function closeModal() {
    const overlay = document.getElementById('modalOverlay');
    overlay.classList.remove('show');
    window.confirmCallback = null;
}

// Экспортируем функции в глобальную область, чтобы обработчики в HTML работали корректно
window.filterMaterials = filterMaterials;
window.resetMaterialsFilter = resetMaterialsFilter;
window.saveMaterial = saveMaterial;
window.clearForm = clearForm;
window.editMaterial = editMaterial;
window.deleteMaterial = deleteMaterial;


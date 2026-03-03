let selectedMaterials = new Map(); // Map<materialId, {material, quantity}>

// Load materials on page load (if any saved)
document.addEventListener('DOMContentLoaded', () => {
    updateCalculatorTable();
    calculateTotal();
});

// Search materials by name (but API will search by article)
let searchTimeout;
function searchMaterials() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        const searchTerm = document.getElementById('searchInput').value.trim();
        if (searchTerm.length >= 2) {
            showSearchResults();
            performSearch(searchTerm);
        } else {
            document.getElementById('searchResults').style.display = 'none';
        }
    }, 300);
}

// Perform search - get all materials and filter by name or article on client side
// User can search by name or article
async function performSearch(searchTerm) {
    try {
        // Get all materials (could be optimized later with server-side filtering)
        const allMaterials = await MaterialsAPI.getAll();
        
        // Filter by name or article on client side
        // Search is case-insensitive and partial match
        const searchLower = searchTerm.toLowerCase();
        const filtered = allMaterials.filter(material => 
            material.name.toLowerCase().includes(searchLower) ||
            material.article.toLowerCase().includes(searchLower)
        );

        displaySearchResults(filtered);
    } catch (error) {
        console.error('Search error:', error);
        const resultsDiv = document.getElementById('searchResultsList');
        resultsDiv.innerHTML = '<p style="color: red;">Ошибка при поиске материалов: ' + error.message + '</p>';
    }
}

// Show search results
function showSearchResults() {
    const searchTerm = document.getElementById('searchInput').value.trim();
    if (searchTerm.length >= 2) {
        performSearch(searchTerm);
        document.getElementById('searchResults').style.display = 'block';
    }
}

// Display search results
function displaySearchResults(materials) {
    const resultsDiv = document.getElementById('searchResultsList');
    resultsDiv.innerHTML = '';

    if (!materials || materials.length === 0) {
        resultsDiv.innerHTML = '<p>Материалы не найдены</p>';
        return;
    }

    materials.forEach(material => {
        const itemDiv = document.createElement('div');
        itemDiv.className = 'search-result-item';
        itemDiv.innerHTML = `
            <p><strong>${material.name}</strong></p>
            <p>Артикул: ${material.article} | Цена: ${material.price} руб. | Ед.: ${material.unit}</p>
        `;
        itemDiv.onclick = () => addMaterialToCalculator(material);
        resultsDiv.appendChild(itemDiv);
    });
}

// Add material to calculator
function addMaterialToCalculator(material) {
    if (selectedMaterials.has(material.id)) {
        // Material already added, just increase quantity
        const existing = selectedMaterials.get(material.id);
        existing.quantity += 1;
    } else {
        // Add new material with quantity 1
        selectedMaterials.set(material.id, {
            material: material,
            quantity: 1
        });
    }

    // Hide search results
    document.getElementById('searchResults').style.display = 'none';
    document.getElementById('searchInput').value = '';

    updateCalculatorTable();
    calculateTotal();
}

// Update calculator table
function updateCalculatorTable() {
    const tbody = document.getElementById('calculatorTableBody');
    tbody.innerHTML = '';

    if (selectedMaterials.size === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="empty-state"><p>Добавьте материалы для расчета стоимости</p></td></tr>';
        return;
    }

    selectedMaterials.forEach((item, materialId) => {
        const material = item.material;
        const quantity = item.quantity;
        const totalPrice = material.price * quantity;

        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${material.article}</td>
            <td>${material.name}</td>
            <td>${material.price}</td>
            <td>${material.unit}</td>
            <td>
                <div class="quantity-controls">
                    <button class="quantity-btn" onclick="changeQuantity(${materialId}, -1)">-</button>
                    <input type="number" class="quantity-input" value="${quantity}" min="0.001" step="0.001" 
                           onchange="setQuantity(${materialId}, parseFloat(this.value))"
                           onblur="validateQuantity(${materialId}, this)">
                    <button class="quantity-btn" onclick="changeQuantity(${materialId}, 1)">+</button>
                </div>
            </td>
            <td>
                <button class="btn btn-danger" onclick="removeMaterial(${materialId})">Удалить</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

// Change quantity
function changeQuantity(materialId, delta) {
    const item = selectedMaterials.get(materialId);
    if (item) {
        const newQuantity = Math.max(0.001, item.quantity + delta);
        item.quantity = newQuantity;
        
        // Remove if quantity is 0
        if (newQuantity <= 0.001) {
            selectedMaterials.delete(materialId);
        }
        
        updateCalculatorTable();
        calculateTotal();
    }
}

// Set quantity directly
function setQuantity(materialId, value) {
    const quantity = parseFloat(value);
    if (isNaN(quantity) || quantity <= 0) {
        showModal('Внимание', 'Количество должно быть положительным числом', 'warning');
        return;
    }

    const item = selectedMaterials.get(materialId);
    if (item) {
        item.quantity = quantity;
        updateCalculatorTable();
        calculateTotal();
    }
}

// Validate quantity on blur
function validateQuantity(materialId, inputElement) {
    const quantity = parseFloat(inputElement.value);
    const item = selectedMaterials.get(materialId);
    
    if (isNaN(quantity) || quantity <= 0) {
        if (item) {
            inputElement.value = item.quantity;
        } else {
            inputElement.value = '1';
        }
        showModal('Внимание', 'Количество должно быть положительным числом', 'warning');
    }
}

// Remove material from calculator
function removeMaterial(materialId) {
    showConfirmModal('Подтверждение удаления', 'Удалить материал из расчета?', 
        () => {
            selectedMaterials.delete(materialId);
            updateCalculatorTable();
            calculateTotal();
        }
    );
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

// Calculate total
function calculateTotal() {
    let total = 0;
    
    selectedMaterials.forEach((item) => {
        const itemTotal = item.material.price * item.quantity;
        total += itemTotal;
    });

    document.getElementById('totalAmount').textContent = total.toFixed(2);
}

// Save calculation
async function saveCalculation() {
    if (selectedMaterials.size === 0) {
        showModal('Внимание', 'Добавьте материалы для расчета', 'warning');
        return;
    }

    try {
        const items = Array.from(selectedMaterials.entries()).map(([materialId, item]) => ({
            materialId: materialId,
            quantity: item.quantity
        }));

        console.log('Saving calculation with items:', items);
        const response = await CalculationsAPI.create({ items });
        console.log('Calculation saved successfully:', response);
        
        const savedCalcId = response && response.id;
        
        if (savedCalcId) {
            showConfirmModal(
                'Расчет сохранен',
                'Хотите создать коммерческое предложение для этого расчета?',
                () => {
                    clearCalculatorAfterSave();
                    closeModal();
                    if (typeof openKpModal === 'function') {
                        openKpModal(savedCalcId);
                    }
                },
                () => {
                    clearCalculatorAfterSave();
                    closeModal();
                }
            );
        } else {
            showModal('Успешно', 'Расчет успешно сохранен', 'success');
            clearCalculatorAfterSave();
        }
    } catch (error) {
        console.error('Error saving calculation:', error);
        const errorMessage = error.message || 'Неизвестная ошибка при сохранении расчета';
        showModal('Ошибка', 'Ошибка при сохранении расчета: ' + errorMessage, 'error');
    }
}

function clearCalculatorAfterSave() {
    selectedMaterials.clear();
    updateCalculatorTable();
    calculateTotal();
    document.getElementById('searchInput').value = '';
}

function showConfirmModal(title, message, onConfirm, onCancel) {
    const overlay = document.getElementById('modalOverlay');
    const modal = document.getElementById('modal');
    const modalTitle = document.getElementById('modalTitle');
    const modalMessage = document.getElementById('modalMessage');
    const modalButtons = document.getElementById('modalButtons');
    
    modalTitle.textContent = title;
    modalMessage.textContent = message;
    
    modal.classList.remove('modal-success', 'modal-error', 'modal-warning');
    modal.classList.add('modal-warning');
    
    modalButtons.innerHTML = `
        <button class="modal-btn modal-btn-secondary" onclick="confirmCancelAction()">Нет</button>
        <button class="modal-btn modal-btn-primary" onclick="confirmOkAction()">Да</button>
    `;
    
    window._confirmOkCallback = onConfirm;
    window._confirmCancelCallback = onCancel;
    
    overlay.classList.add('show');
}

function confirmOkAction() {
    if (window._confirmOkCallback) {
        window._confirmOkCallback();
        window._confirmOkCallback = null;
        window._confirmCancelCallback = null;
    }
    closeModal();
}

function confirmCancelAction() {
    if (window._confirmCancelCallback) {
        window._confirmCancelCallback();
    }
    window._confirmOkCallback = null;
    window._confirmCancelCallback = null;
    closeModal();
}

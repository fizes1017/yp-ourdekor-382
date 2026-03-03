// Shared КП (commercial proposal) modal logic - used by profile.html and calculator.html

function openKpModal(calculationId) {
    window.currentKpCalculationId = calculationId;
    const form = document.getElementById('kpForm');
    if (form) {
        form.reset();
    }
    const overlay = document.getElementById('kpModalOverlay');
    if (overlay) overlay.classList.add('show');
}

function closeKpModal() {
    const overlay = document.getElementById('kpModalOverlay');
    if (overlay) overlay.classList.remove('show');
    window.currentKpCalculationId = null;
}

async function submitKpForm(event) {
    event.preventDefault();
    const calcId = window.currentKpCalculationId;
    if (!calcId) {
        closeKpModal();
        return;
    }

    const companyEl = document.getElementById('kpCustomerCompany');
    const personEl = document.getElementById('kpCustomerPerson');
    const phoneEl = document.getElementById('kpCustomerPhone');
    const emailEl = document.getElementById('kpCustomerEmail');
    const addressEl = document.getElementById('kpCustomerAddress');
    const commentsEl = document.getElementById('kpComments');

    const data = {
        calculationId: calcId,
        customerCompany: (companyEl && companyEl.value.trim()) || '',
        customerPerson: (personEl && personEl.value.trim()) || '',
        customerPhone: (phoneEl && (typeof getPhoneRaw === 'function' ? getPhoneRaw(phoneEl.value) : phoneEl.value.trim())) || '',
        customerEmail: (emailEl && emailEl.value.trim()) || '',
        customerAddress: (addressEl && addressEl.value.trim()) || null,
        comments: (commentsEl && commentsEl.value.trim()) || null
    };

    const btn = event.target ? event.target.querySelector('button[type="submit"]') : null;
    if (btn) {
        btn.disabled = true;
        btn.textContent = 'Формирование...';
    }

    try {
        const blob = await createCommercialProposalPdf(data);
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const safeName = (data.customerCompany || 'КП').replace(/["/\\?*]/g, '-');
        a.download = `КП_${safeName}_${new Date().toISOString().slice(0, 10)}.pdf`;
        a.click();
        URL.revokeObjectURL(url);

        if (typeof showModal === 'function') {
            showModal('Успешно', 'Коммерческое предложение успешно сформировано и скачано', 'success');
        }
        closeKpModal();
    } catch (error) {
        console.error('Error creating КП:', error);
        if (typeof showModal === 'function') {
            showModal('Ошибка', error.message || 'Ошибка при формировании КП', 'error');
        }
    } finally {
        if (btn) {
            btn.disabled = false;
            btn.textContent = 'Сформировать PDF';
        }
    }
}

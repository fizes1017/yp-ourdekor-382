// API configuration
const API_BASE_URL = window.location.origin + '/api';

// Token management
const TokenManager = {
    getToken: () => localStorage.getItem('authToken'),
    setToken: (token) => localStorage.setItem('authToken', token),
    removeToken: () => localStorage.removeItem('authToken'),
    getUser: () => {
        const userStr = localStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    },
    setUser: (user) => localStorage.setItem('user', JSON.stringify(user)),
    removeUser: () => localStorage.removeItem('user'),
    isAuthenticated: () => !!TokenManager.getToken()
};

// API helper functions
async function apiRequest(endpoint, method = 'GET', body = null, requireAuth = false) {
    const url = `${API_BASE_URL}${endpoint}`;
    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json',
        },
    };

    // Add authorization header if token exists or auth is required
    const token = TokenManager.getToken();
    if (token || requireAuth) {
        if (!token) {
            throw new Error('Требуется авторизация');
        }
        options.headers['Authorization'] = `Bearer ${token}`;
    }

    if (body) {
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(url, options);
        
        // Handle 401 Unauthorized - redirect to login
        if (response.status === 401) {
            TokenManager.removeToken();
            TokenManager.removeUser();
            if (requireAuth) {
                redirectToLogin();
            }
            throw new Error('Сессия истекла. Пожалуйста, войдите снова.');
        }
        
        if (!response.ok) {
            let errorMessage = `HTTP error! status: ${response.status}`;
            try {
                const errorData = await response.json();
                errorMessage = errorData.message || errorData.title || errorMessage;
                
                // Log full error for debugging
                console.error('API Error:', {
                    status: response.status,
                    statusText: response.statusText,
                    error: errorData
                });
            } catch {
                // If response is not JSON, try to get text
                try {
                    const text = await response.text();
                    if (text) {
                        errorMessage = text.substring(0, 200); // Limit error message length
                    }
                } catch {
                    // Use default error message
                }
            }
            throw new Error(errorMessage);
        }

        // Handle 204 No Content
        if (response.status === 204) {
            return null;
        }

        return await response.json();
    } catch (error) {
        console.error('API request failed:', error);
        throw error;
    }
}

// Redirect to login
function redirectToLogin() {
    const currentPage = window.location.pathname;
    if (!currentPage.includes('login.html') && !currentPage.includes('register.html')) {
        window.location.href = 'login.html?redirect=' + encodeURIComponent(currentPage);
    }
}

// Auth API
const AuthAPI = {
    register: (registerData) => apiRequest('/auth/register', 'POST', registerData),
    login: (loginData) => apiRequest('/auth/login', 'POST', loginData),
};

// Profile API
const ProfileAPI = {
    get: () => apiRequest('/profile', 'GET', null, true),
    update: (profileData) => apiRequest('/profile', 'PUT', profileData, true),
    changePassword: (passwordData) => apiRequest('/profile/change-password', 'POST', passwordData, true),
    getMyCalculations: () => apiRequest('/profile/calculations', 'GET', null, true),
};

// Materials API (requires auth)
const MaterialsAPI = {
    getAll: () => apiRequest('/materials', 'GET', null, true),
    getById: (id) => apiRequest(`/materials/${id}`, 'GET', null, true),
    create: (material) => apiRequest('/materials', 'POST', material, true),
    update: (id, material) => apiRequest(`/materials/${id}`, 'PUT', material, true),
    delete: (id) => apiRequest(`/materials/${id}`, 'DELETE', null, true),
};

// Calculations API (requires auth)
const CalculationsAPI = {
    getAll: () => apiRequest('/calculations', 'GET', null, true),
    getById: (id) => apiRequest(`/calculations/${id}`, 'GET', null, true),
    create: (calculation) => apiRequest('/calculations', 'POST', calculation, true),
    delete: (id) => apiRequest(`/calculations/${id}`, 'DELETE', null, true),
};

// Commercial Proposals API - creates КП and returns PDF for download
async function createCommercialProposalPdf(data) {
    const token = TokenManager.getToken();
    if (!token) throw new Error('Требуется авторизация');

    const response = await fetch(`${API_BASE_URL}/CommercialProposals`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
    });

    if (response.status === 401) {
        TokenManager.removeToken();
        TokenManager.removeUser();
        redirectToLogin();
        throw new Error('Сессия истекла');
    }

    if (!response.ok) {
        let errorMessage = 'Ошибка при создании КП';
        try {
            const text = await response.text();
            try {
                const err = JSON.parse(text);
                errorMessage = (err.message || err.detail || err.title || errorMessage);
            } catch {
                if (text) {
                    var plain = text.replace(/<[^>]+>/g, ' ').trim();
                    if (plain.length > 0) errorMessage = plain.length <= 450 ? plain : plain.substring(0, 447) + '...';
                }
            }
        } catch (e) {
            console.error('createCommercialProposalPdf error', e);
        }
        throw new Error(errorMessage);
    }

    return await response.blob();
}



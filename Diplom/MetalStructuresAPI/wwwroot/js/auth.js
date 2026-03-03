// Authentication management
const AuthManager = {
    // Check if user is authenticated
    checkAuth: () => {
        return TokenManager.isAuthenticated();
    },

    // Login
    login: async (email, password) => {
        try {
            const response = await AuthAPI.login({ email, password });
            TokenManager.setToken(response.token);
            TokenManager.setUser(response.user);
            return response;
        } catch (error) {
            throw error;
        }
    },

    // Register
    register: async (email, phone, fullName, password, confirmPassword) => {
        try {
            const response = await AuthAPI.register({
                email,
                phone,
                fullName,
                password,
                confirmPassword
            });
            
            if (response && response.token && response.user) {
                TokenManager.setToken(response.token);
                TokenManager.setUser(response.user);
                return response;
            } else {
                throw new Error('Неполный ответ от сервера');
            }
        } catch (error) {
            console.error('Registration error:', error);
            throw error;
        }
    },

    // Logout
    logout: () => {
        TokenManager.removeToken();
        TokenManager.removeUser();
        window.location.href = 'index.html';
    },

    // Get current user
    getCurrentUser: () => {
        return TokenManager.getUser();
    },

    // Require authentication - redirect if not logged in
    requireAuth: () => {
        if (!AuthManager.checkAuth()) {
            redirectToLogin();
            return false;
        }
        return true;
    }
};

// Check authentication on page load for protected pages
function checkAuthOnLoad() {
    if (!AuthManager.checkAuth()) {
        redirectToLogin();
    }
}






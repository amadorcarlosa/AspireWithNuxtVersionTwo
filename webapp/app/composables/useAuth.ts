export const useAuth = () => {
  const login = (returnUrl: string = '/') => {
    window.location.href = `/api/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  };

  const logout = () => {
    window.location.href = '/api/auth/logout';
  };

  const getUser = async () => {
    try {
      return await $fetch('/api/auth/user');
    } catch (e) {
      return null;
    }
  };

  return { login, logout, getUser };
};
